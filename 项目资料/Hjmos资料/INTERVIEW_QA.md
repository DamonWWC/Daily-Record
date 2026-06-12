# 智慧地铁管控平台 — 面试题与参考答案

---

## 一、架构设计类

### Q1：请介绍一下项目的整体架构设计

**A：** 项目是基于 Prism MVVM 框架的模块化 WPF 桌面应用，整体分为四层：

1. **Shell 层**（HjmosApplication）：负责应用启动、DI 容器初始化、模块目录加载、主窗口/菜单创建、全局异常捕获
2. **模块层**（11 个 Module）：每个业务模块实现 `IModule` 接口，通过 XML 配置声明式注册，支持启动加载和按需加载
3. **服务接口层**（Hjmos.Service）：定义 30+ 业务接口（IBasicDataService、IVideoService、IEmergencyService 等），模块只依赖接口，不依赖实现
4. **服务实现层**：TempService（真实 REST/WebSocket/MQ 实现）和 OfflineService（装饰器模式的离线模拟），通过 DI 容器注入

核心设计思路是**模块间零耦合**——模块之间不直接引用，通过 EventBus 事件总线、Region 导航传参、共享服务接口三种方式通信。

---

### Q2：为什么选择 Prism 框架？它解决了什么问题？

**A：** 选择 Prism 主要解决三个问题：

1. **模块化**：11 个业务模块按需加载，避免所有代码打包在一起导致启动慢。通过 `ConfigurationModuleCatalog` 从 XML 配置读取模块清单，`startupLoaded=false` 的模块在首次导航时才加载
2. **区域导航**：`IRegionManager` 提供内容区域管理，我们在此基础上封装了 `RegionService`，实现了视图缓存和复用
3. **事件聚合**：`EventAggregator` 实现跨模块通信，我们进一步封装为 `EventBus`，用枚举键管理事件

同时 Prism 提供了成熟的 MVVM 基础设施（BindableBase、DelegateCommand、ViewModelLocator），减少重复造轮子。

---

### Q3：项目中使用了哪些设计模式？请举例说明

**A：** 项目中至少使用了以下设计模式：

| 模式 | 应用场景 |
|------|---------|
| **观察者/发布-订阅** | EventBus 事件总线，模块间解耦通信 |
| **策略模式** | IVideoService 三种实现（ISCS代理/RTSP直连/ONVIF），通过配置切换 |
| **装饰器模式** | OfflineService 包装 TempService，选择性覆盖方法返回本地数据 |
| **外观模式** | RestApi 静态类封装 RestSharp + OAuth + 重试 + 降级 |
| **工厂模式** | ConnectionFactory 创建 RocketMQ 消费者（编程式或配置驱动） |
| **模板方法** | ViewModelBase 抽象 GetData()，子类实现具体数据获取逻辑 |
| **适配器模式** | RegionService 适配 Prism IRegionManager 为简化 API |
| **单例模式** | CefBrowserTool 初始化（双重检查锁定）、CacheHelper |
| **命令模式** | DelegateCommand 封装所有用户交互 |
| **中介者模式** | EventBus + RegionService，模块不直接引用彼此 |

---

### Q4：模块之间如何通信？为什么不直接引用？

**A：** 模块间通过三种方式通信，不直接引用是为了**解耦和独立部署**：

1. **EventBus 事件总线**：最常用的方式。模块 A 发布事件，模块 B 订阅处理，双方不感知对方的存在
   ```csharp
   // 应急模块发布
   PrismService.EventBus.GetEvent(CommonEvents.EmergencyEvent).Publish();
   // 全息模块订阅并联动
   PrismService.EventBus.GetEvent(CommonEvents.EmergencyEvent)
       .Subscribe(handler, ThreadOption.BackgroundThread, false);
   ```

2. **Region 导航传参**：通过 `RegionParameters` 在视图切换时传递数据
   ```csharp
   var p = new RegionParameters { {"data", jsonData} };
   PrismService.RegionService.ShowView("MainRegion", typeof(TargetView), p);
   ```

3. **共享服务接口**：模块通过 `PrismService.Resolve<IVideoService>()` 调用其他模块注册的服务，依赖接口而非实现

---

### Q5：项目的权限体系是如何设计的？

**A：** 采用**声明式标注 + 运行时过滤**的权限体系：

1. **声明层**：在菜单枚举和功能类上通过 `[Resource]` 和 `[Menu]` Attribute 声明元数据（ID、名称、DLL路径、RegionType 作用域）
2. **运行时过滤**：`IAuthorityService` 从服务端获取用户权限树，缓存到内存。根据用户 `RoleType`（线网=1/线路=2/车站=4）与菜单 `RegionType` 做位运算，过滤可见菜单
3. **UI 绑定层**：`AuthorityToEnableConverter` 转换器在 XAML 中绑定，控制按钮/功能的可见性

```csharp
[Flags] enum RegionType { 线网=1, 线路=2, 车站=4, All=7 }

// 位运算过滤示例：
// 用户 RoleType=2（线路角色），菜单 RegionType=2（线路）
// 2 & 2 = 2 ≠ 0 → 显示该菜单
// 菜单 RegionType=4（车站）
// 4 & 2 = 0 → 隐藏该菜单
```

好处是**权限声明在代码中**，不需要额外的权限配置文件，新增功能只需加 Attribute 即可。

---

## 二、Prism/MVVM 框架类

### Q6：ViewModel 基类的继承体系是怎么设计的？

**A：** 设计了多层次的 ViewModel 基类链，每一层增加一种能力：

```
BindableBase                    ← INotifyPropertyChanged
  └── ModelBase                 ← +IDialogAware（可作对话框）+ BeginInvoke
       ├── ViewModelBase        ← +抽象 GetData()，全局刷新订阅
       ├── ActiveModelBase      ← +IUserControlStatus，IsActive 自动管理事件订阅
       └── ModelBase1           ← 简化版
            └── DialogModelBase ← +IDialogAware + ButtonResult 关闭
```

关键设计是 `ActiveModelBase`：当视图 `Loaded` 时 `IsActive=true`，自动订阅 `GlobalUpdate` 事件；`Unloaded` 时 `IsActive=false`，自动取消订阅。这样**不活跃的视图不会消耗资源**处理全局刷新事件。

另外 `ModelBase` 同时实现了 `IDialogAware`，意味着任何继承它的 ViewModel 都可以直接作为对话框使用，不需要额外适配。

---

### Q7：RegionService 和 Prism 原生 RequestNavigate 有什么区别？

**A：** 主要区别在于**视图生命周期管理**策略不同：

| 特性 | Prism RequestNavigate | 自研 RegionService |
|------|----------------------|-------------------|
| 视图创建 | 每次导航可能创建新实例 | 按 FullName 缓存，复用已有实例 |
| 通知接口 | `INavigationAware` | 自定义 `IRegionAware`（更简洁） |
| 子视图广播 | 无 | `SendMsgToChildren` 遍历可视化树 |
| 回调支持 | 无 | `IRegionCallBackAware` 父子通信 |
| 强制刷新 | 不支持 | `isReload` 参数强制重建 |

核心算法：
```csharp
var obj = region.GetView(viewType.FullName)  // 1. 缓存查找
         ?? Container.Resolve(viewType);       // 2. 首次创建
region.Activate(obj);                           // 3. 激活
(vm as IRegionAware)?.OnViewActived(params);   // 4. 通知
```

**设计考量**：地铁管控平台的视图创建开销大（包含大量控件、CefSharp 浏览器、图表），复用视图实例能显著提升切换速度。

---

### Q8：EventBus 的线程安全是如何保证的？

**A：** EventBus 内部用 `lock(events)` 保护字典的 get-or-create 操作：

```csharp
public PubSubEvent<T> GetEvent<T>(Enum key) {
    lock (events) {
        if (!events.TryGetValue(key, out var existing)) {
            var evt = new PubSubEvent<T> { SynchronizationContext = syncContext };
            events[key] = evt;
            return evt;
        }
        return (PubSubEvent<T>)existing;
    }
}
```

- **锁粒度**：只锁字典访问，不锁 `Subscribe/Unsubscribe/Publish`（Prism 的 `PubSubEvent` 内部本身线程安全）
- **SynchronizationContext 捕获**：构造时捕获 UI 线程的 `SynchronizationContext`，确保 `ThreadOption.UIThread` 的订阅者能正确回到 UI 线程
- **弱引用**：订阅时传 `keepSubscriberReferenceAlive: false`，使用弱引用防止内存泄漏

---

### Q9：为什么用 PrismService 静态服务定位器而不是构造函数注入？

**A：** 这是一个**权衡选择**，不是最佳实践，但在项目规模下有实际原因：

1. **ViewModel 数量多**：项目有上百个 ViewModel，如果全部构造函数注入，构造函数参数列表会很长
2. **按需获取**：ViewModel 中可能只在特定方法中用到某个服务，构造注入会导致不必要的依赖
3. **历史原因**：项目早期使用服务定位器模式，后续保持了一致性

```csharp
// 服务定位器模式（项目采用）
var station = await PrismService.Resolve<IBasicDataService>().GetStationByStationCode(code);

// 构造函数注入模式
public MyViewModel(IBasicDataService basicDataService) { ... }
```

**如果重新设计**，我会优先使用构造函数注入，服务定位器作为备选。好处是依赖关系更透明、单元测试更容易 mock。

---

## 三、数据通信类

### Q10：项目中有几种数据通信方式？分别用于什么场景？

**A：** 四种通信通道，各司其职：

| 通道 | 技术方案 | 数据特征 | 场景 |
|------|---------|---------|------|
| REST API | RestSharp 封装 | 请求-响应，低频 | 业务数据查询、设备控制指令下发 |
| WebSocket | WebSocketSharp | 服务端推送，高频 | 设备状态实时变更、列车位置更新 |
| RocketMQ | 阿里云 ONS C++ SDK | 广播消息，跨工作站 | 告警广播、联动指令同步 |
| ZeroC ICE | TCP RPC + 回调 | 双向，订阅-推送 | 与综合监控系统（ISCS）集成 |

**选型逻辑**：
- REST 适合一问一答的查询操作
- WebSocket 适合服务端主动推送实时数据
- RocketMQ 广播模式确保每个工作站都能收到告警
- ICE 是 ISCS 系统提供的标准接口，用于设备点位数据订阅

---

### Q11：REST API 的 Token 自动刷新是怎么实现的？如何防止并发刷新？

**A：** 实现了**401 拦截 + 自动刷新 + 双重检查锁定**机制：

```
请求 → 服务端返回 401
  → 进入 lock(_Locker)
  → 双重检查：requestToken != OAuthResult.AccessToken ?
     ├── 是 → 说明其他线程已刷新完毕，直接返回成功
     └── 否 → 执行 RefreshToken 接口
              → 更新 OAuthResult.AccessToken
              → 释放锁
  → 使用新 Token 重试原请求（仅重试一次）
```

**并发场景（惊群效应防护）**：
1. 线程 A 收到 401 → 进入锁 → 开始刷新
2. 线程 B 收到 401 → 阻塞在锁外
3. 线程 A 完成 → 更新 AccessToken → 释放锁
4. 线程 B 进入锁 → **双重检查**发现 Token 已更新 → 跳过刷新
5. 两个线程都用新 Token 重试

这样即使 10 个请求同时收到 401，也只触发**一次** RefreshToken 调用。

---

### Q12：WebSocket 断线重连策略是怎样的？

**A：** 采用**心跳检测 + 线性重连**策略：

```
后台心跳线程（LongRunning Task）
  ├── 连接存活（IsAlive）→ 每 295 秒发送心跳（匹配服务端 5 分钟超时）
  └── 连接断开（!IsAlive）→ ReConnect(times=3, interval=5000ms)
       ├── 第 1 次：等待 5 秒 → 尝试重连
       ├── 第 2 次：等待 5 秒 → 尝试重连
       ├── 第 3 次：等待 5 秒 → 尝试重连
       └── 全部失败 → _isConnectionClosed = true → 终止心跳循环
```

**心跳间隔选择 295 秒**的原因：服务端设置 5 分钟无活动断开连接，295 秒略小于 300 秒留出网络传输余量。

**如果让我优化**，会考虑指数退避重连（5s → 10s → 20s → 40s），避免在服务端长时间不可用时频繁重试浪费资源。

---

### Q13：RocketMQ 为什么用广播模式而不是集群模式？

**A：** 这是业务需求决定的。

- **集群模式**：同一 ConsumerGroup 下，每条消息只被一个消费者消费。适合后端微服务分工处理。
- **广播模式**：同一 ConsumerGroup 下，每条消息被所有消费者消费。适合需要每个终端都收到通知的场景。

地铁管控平台是**多工作站部署**，每个调度员的工作站都需要收到告警广播、联动指令。如果用集群模式，一条告警只会被一台工作站收到，其他工作站无感知，这在运营场景下是不可接受的。

---

## 四、性能优化类

### Q14：项目中做了哪些性能优化？

**A：** 多个层面的优化：

**1. 视图复用（Region 缓存）**
视图首次创建后缓存在 Region 中，后续切换直接激活，避免重复 XAML 解析和控件创建。

**2. 视图生命周期管理（ActiveModelBase）**
不活跃的视图自动取消 `GlobalUpdate` 事件订阅，避免后台视图无意义地刷新数据。

**3. 视频播放节流（Reactive Extensions）**
```csharp
_subject
    .Throttle(TimeSpan.FromMilliseconds(1000))  // 快速切换摄像头时去抖
    .DistinctUntilChanged()                       // 过滤重复请求
    .Switch()                                     // 取消前一个未完成的请求
    .Subscribe(PlayVideo);
```
用户快速切换摄像头时，只在停止操作 1 秒后才真正播放，中间过程全部丢弃。

**4. 视频保活批量处理**
```csharp
_sessionSubject
    .Buffer(TimeSpan.FromSeconds(10))  // 每 10 秒收集一批
    .Select(x => x.Distinct())          // 去重
    .Subscribe(KeepAliveBatch);
```
多个播放器的保活请求合并为一次批量调用，减少 HTTP 请求数。

**5. 数据缓存（CacheHelper）**
线路、车站等基础数据首次获取后缓存在内存中，后续直接读取，不重复请求 REST API。

**6. CefSharp 优化**
磁盘缓存（`CachePath`）、禁用代理检测（加速页面加载）、GPU 加速。

---

### Q15：如何排查 WPF 应用的内存泄漏？项目中遇到过吗？

**A：** WPF 内存泄漏常见原因和排查方法：

**排查工具**：
- **Visual Studio 诊断工具**：快照对比，查看对象增长
- **dotMemory / ANTS Memory Profiler**：查看 GC Root 引用链
- **Snoop**：可视化树检查

**常见泄漏源（本项目中的对应措施）**：

| 泄漏源 | 项目中的应对 |
|--------|------------|
| 事件订阅未取消 | ActiveModelBase 在 IsActive=false 时自动 Unsubscribe |
| 静态集合持续增长 | CacheHelper 使用 MemoryCache（支持容量限制） |
| 定时器未停止 | 视频播放器 Dispose 时停止 keepAliveTimer |
| 窗口关闭未释放 | Closing 事件中显式清理播放器、取消事件注册 |
| 对话框反复创建 | NewDialogService1 防重复打开，关闭时清理字典 |

**实际案例**：视频播放器控件使用 `DispatcherTimer` 做心跳保活，如果窗口关闭时不停止定时器，定时器持有 ViewModel 引用导致整条引用链无法被 GC 回收。解决方案是在 `Dispose()` 中显式 `_keepAliveTimer.Dispose()`。

---

## 五、Web 渲染与 H5 通信类

### Q16：WPF 和 H5 页面之间是如何通信的？

**A：** 设计了三种通信模式：

**1. WPF → H5（推送数据）**
```csharp
// ViewModel 改变 SendH5Msg 属性 → DependencyProperty 变化回调
// → 构造 JS 函数调用 → ExecuteScriptAsync 执行
CefSharp.ExecuteScriptAsync($"receiveData('{jsonString}')");
```

**2. H5 → WPF（方法调用）**
```csharp
// 注册 .NET 对象为 JS 全局变量
JavascriptObjectRepository.Register("wpfContentWindow",
    new DeviceInterop(this), BindingOptions.DefaultBinder);
// H5 端: window.wpfContentWindow.call(jsonData)
```

**3. 双向回调桥**
```csharp
// H5 传递 JS 函数引用给 .NET
public void SetCallBack(IJavascriptCallback callback, string json) {
    // .NET 后续异步回调 H5
    await callback.ExecuteAsync(JsonConvert.SerializeObject(data));
}
```

**认证透传**：通过 Cookie 注入 `mosToken` 和请求头注入 `hjmos-authorization`，H5 页面无需重新登录。

---

### Q17：CefSharp 初始化为什么要用双重检查锁定？

**A：** CefSharp（Chromium）是**进程级单例**——一个进程只能初始化一次 CEF 运行时。项目中可能有多个 HJBrowser 控件同时创建（比如全息感知页面嵌入多个 H5 面板），如果不用锁保护，多个线程同时检测到 `!Cef.IsInitialized` 会尝试重复初始化，导致崩溃。

```csharp
public static void Init() {
    if (Cef.IsInitialized) return;        // 快速路径：已初始化直接返回
    lock (_lock) {
        if (Cef.IsInitialized) return;    // 双重检查：等待锁期间可能被其他线程初始化
        Cef.Initialize(settings, ...);    // 实际初始化
    }
}
```

第一次检查避免加锁开销（`IsInitialized` 是原子读），第二次检查防止在锁等待期间被其他线程抢先初始化。

---

## 六、视频播放与 3D 类

### Q18：视频播放系统的架构是怎样的？为什么用 System.Reactive？

**A：** 视频播放基于 LibVLCSharp + System.Reactive 构建：

```
全局共享 LibVLC 实例（静态构造器初始化一次）
  └── 每个 VideoPlayerView 一个 MediaPlayer
       └── Reactive 管道：
            Subject → Throttle(1s) → DistinctUntilChanged → Switch → Play
```

**为什么用 Rx 而不是传统事件/回调：**

1. **Throttle（去抖）**：用户快速切换摄像头时，不需要每个切换都真正播放，只需播放最后停留的那个。传统方式需要手写定时器，Rx 一行代码解决
2. **Switch（取消前序）**：如果上一个摄像头的 URL 解析还没完成，用户已经切换到下一个，`Switch` 自动取消前一个异步操作，避免乱序
3. **Buffer（批量）**：多个播放器的保活请求合并处理
4. **统一生命周期**：所有 Rx 订阅通过 `Dispose()` 统一清理，不需要手动逐个取消

```csharp
// 传统方式（伪代码）
private Timer _debounceTimer;
void OnCameraChanged(string url) {
    _debounceTimer?.Stop();
    _debounceTimer = new Timer(() => PlayVideo(url), 1000);
}

// Rx 方式（实际代码）
_subject.Throttle(1s).DistinctUntilChanged().Switch().Subscribe(PlayVideo);
```

---

### Q19：Unity 3D 是如何嵌入 WPF 的？焦点问题怎么解决的？

**A：** Unity 以**独立进程**运行，通过 Win32 API 嵌入 WPF 窗口：

**嵌入过程**：
```
1. Process.Start("3DViewer.exe", "-parentHWND {wpfHandle} {wsUrl} {token}")
2. EnumChildWindows 找到 Unity 子窗口句柄
3. SetParent(unityHWND, wpfPanelHandle)  // Win32 嵌入
```

**焦点问题是核心难点**：Unity 进程嵌入后，用户点击 Unity 区域会导致键盘焦点从 WPF 主窗口丢失，WPF 的快捷键失效。

**解决方案**：300ms 定时器轮询焦点归属：
```csharp
// 每 300ms 检查光标位置
var cursorHwnd = WindowFromPoint(GetCursorPos());
if (cursorHwnd == unityHWND)
    SendMessage(unityHWND, WM_ACTIVATE, WA_ACTIVE, 0);    // 激活 Unity
else
    SendMessage(unityHWND, WM_ACTIVATE, WA_INACTIVE, 0);  // 归还焦点给 WPF
```

**IPC 通信**：通过 PESocket TCP（`127.0.0.1:9968`）发送控制指令（全屏、设备详情、Token 刷新等）。

---

## 七、多线程与并发类

### Q20：项目中的多线程场景有哪些？如何保证线程安全？

**A：** 主要有以下多线程场景：

**1. UI 线程调度**
ViewModel 中的异步操作完成后需要更新 UI 属性，通过 `BeginInvoke` 回到 UI 线程：
```csharp
Application.Current?.Dispatcher?.BeginInvoke(action);
```

**2. 独立 UI 线程（UIDispatcher）**
某些对话框需要在独立 UI 线程运行，避免阻塞主界面：
```csharp
var dispatcher = await UIDispatcher.RunNewAsync("CCTVWindow");
// STA 线程 + Dispatcher.Run() 消息泵
```
关键点：WPF UI 线程必须是 **STA**（单线程公寓），通过 `thread.SetApartmentState(ApartmentState.STA)` 设置。

**3. Token 并发刷新**
`lock(_Locker)` + 双重检查，防止多线程同时刷新 Token。

**4. EventBus 事件字典**
`lock(events)` 保护 get-or-create 操作。

**5. TaskScheduler 任务调度**
`Interlocked.Exchange` 作为轻量级锁，防止同一任务并发执行。

---

### Q21：async/await 在项目中是怎么使用的？有什么注意事项？

**A：** 项目中大量使用 async/await，主要模式：

```csharp
// 典型模式：异步获取数据，UI 更新
public async void ExecuteSaveCommand() {
    var result = await PrismService.Resolve<IDeviceService>().SaveDevice(device);
    if (result.Success) {
        // 已在 UI 线程（SynchronizationContext 自动回到调用线程）
        Message = "保存成功";
    }
}
```

**注意事项**：

1. **ConfigureAwait(false)**：服务层代码使用 `ConfigureAwait(false)` 避免回到 UI 线程，减少死锁风险。只有 ViewModel 层需要回到 UI 线程更新属性
2. **async void 的风险**：`DelegateCommand` 的回调是 `Action`（非 `Task`），所以执行方法签名是 `async void`，异常无法被 await 捕获。项目通过三层全局异常处理兜底
3. **Task.Run 包装**：`RestApi.RequestAsync` 内部用 `Task.Run` 包装同步的 RestSharp 调用，这不是真正的异步 I/O，而是将阻塞操作移到线程池

---

## 八、安全与认证类

### Q22：项目的安全体系是怎样的？

**A：** 多层安全设计：

**1. 认证**
- OAuth2 Password Grant 获取 Token
- 每个 REST 请求附加 `ID-Token` 请求头
- 401/403 自动刷新 Token + 重试

**2. 传输加密**
- 登录密码：RSA PKCS#1 加密传输（服务端提供公钥）
- 密码哈希：SHA256

**3. 本地存储加密**
- 登录凭证持久化到 Windows 注册表，使用 AES-CBC 加密
- 密钥为主板序列号（截取/填充至 16 字节），实现**机器绑定**——换机器凭证失效

**4. 权限控制**
- 声明式 `[Resource]` Attribute 标注功能权限
- 运行时 `IAuthorityService` 权限树过滤
- UI 层转换器绑定控制元素可见性

**5. 嵌入页面认证**
- Cookie 注入 + 请求头注入，实现 WPF → H5 的 SSO

---

## 九、工程实践类

### Q23：项目如何处理全局异常？为什么不让应用崩溃？

**A：** 三层异常捕获，覆盖所有未处理异常：

| 层级 | 处理器 | 覆盖范围 | 行为 |
|------|--------|---------|------|
| UI 线程 | `DispatcherUnhandledException` | WPF 主线程未捕获异常 | 日志 + `Handled=true`（继续运行） |
| 任务调度 | `TaskScheduler.UnobservedTaskException` | 未 await 的 Task 异常 | 日志 |
| 进程域 | `AppDomain.UnhandledException` | 致命级异常 | 日志 |

**为什么不让应用崩溃**：这是地铁运营管控系统，7×24 小时运行。一次未捕获异常导致应用退出可能影响调度员监控，后果严重。所以即使出现异常也要保持运行，同时记录日志供排查。

当然 `AppDomain.UnhandledException` 级别的异常通常意味着进程状态已不可恢复，实际中可能仍然会退出，但至少保证了日志记录。

---

### Q24：自动升级系统是怎么实现的？如何保证升级安全？

**A：** 基于 FTP 的增量升级：

```
1. 版本检查
   → FTP 下载远程 version.json
   → 本地递归扫描文件 + 计算 MD5
   → 逐文件比对 → 生成 diff.version.json

2. 增量下载
   → 只下载有差异的文件到 upgradeFiles/ 临时目录

3. 备份与替换
   → 原文件备份到 backFiles/
   → 新文件复制到原位
   → 失败时自动从备份回滚

4. 重启
   → 启动新版程序 → 关闭升级工具
```

**安全保证**：
- **MD5 完整性校验**：确保文件未被篡改
- **备份回滚**：替换前备份原文件，失败自动恢复
- **FTP 凭证加密**：AES-192 加密存储，解密失败降级为明文

---

### Q25：项目的构建和部署流程是怎样的？

**A：**

- **双目标框架**：大部分模块同时支持 `net472` 和 `net6.0-windows`，通过 MSBuild 多目标构建
- **模块化编译**：每个模块独立 `.csproj`，通过解决方案统一管理
- **构建脚本**：`Compile.bat`（清理+构建）和 `build(Release).bat`（完整发布）
- **自动升级**：发布新版本时，将文件上传到 FTP 服务器，客户端启动时自动检测并增量更新
- **分支策略**：`master`（稳定基线）→ `release`（发布集成）→ `CS6_2`（开发主线）→ `feature/*`（特性分支）

---

## 十、业务场景与问题解决类

### Q26：开关站流程是怎么实现的？遇到什么问题？

**A：** 开关站是车站每日运营前后的标准操作流程（开站前设备检查、关站后设备关闭），分为**自动项**和**手动项**逐步执行。

**技术实现**：
- 流程引擎按步骤顺序执行，每一步调用设备控制 REST API
- 自动项支持设备执行失败**自动重试**
- 仿真模式可以在非运营时段**预演**流程，验证设备响应

**遇到的问题**：
1. 自动项重试时不应该传定时执行时间（DeviceTriggerTime），否则重试时使用过期时间导致执行失败——通过区分首次执行和重试逻辑解决
2. 设备数据实体缺少 DeviceType 字段，导致无法区分设备类型进行差异化处理——通过实体扩展和枚举补充解决

---

### Q27：防汛作战系统是如何设计的？

**A：** 防汛是地铁在暴雨等极端天气下的应急响应系统：

**功能组成**：
- 防汛消息弹窗：实时接收水情预警，弹窗提醒调度员
- 全屏作战页面：嵌入 H5 地图页面，展示实时水位、设备状态、应急资源分布
- 单实例协调：多个防汛事件触发时，确保只有一个全屏窗口，新事件更新已有窗口内容

**技术要点**：
- **弹窗单实例机制**：检查是否已存在防汛窗口，存在则更新数据并激活，不存在则创建
- **H5 通信**：通过 `ExecuteScriptAsync` 向防汛地图推送水情数据、摄像头位置、Token 等参数
- **窗口置顶**：使用 `Topmost` 确保防汛窗口不被其他窗口遮挡
- **F5 快捷键**：简化为刷新当前防汛 H5 页面

---

### Q28：切换车站时数据是如何刷新的？

**A：** 切换车站是一个典型的**级联刷新**流程：

```
用户在顶栏切换车站
  → 更新全局当前车站上下文
  → Publish(StationChange) 事件

  → 所有活跃视图的 ViewModel 收到事件
    → SetRegionChanged() → RegionChanged = true（延迟标记）

  → 视图下次 Load() 时检查标记
    → if (RegionChanged) { InitParamData(); RegionChanged = false; }
    → GetData()  // 使用新车站参数请求数据
```

**设计亮点**：`RegionChanged` 标记是**延迟求值**——收到切换事件时只设标记，不立即请求数据。等视图真正被激活（用户导航到该页面）时才请求，避免切换一次车站触发几十个 API 请求。

同时 `GlobalUpdate` 事件在每日午夜自动触发，刷新所有活跃视图的日期敏感数据。

---

### Q29：如果让你重新设计这个系统，你会做哪些改进？

**A：**

1. **依赖注入方式**：减少 `PrismService.Resolve<T>()` 服务定位器，优先使用构造函数注入，使依赖关系更透明

2. **异步 I/O 真正异步化**：当前 `RestApi.RequestAsync` 内部是 `Task.Run` 包装同步调用，应改为 `HttpClient.GetAsync` 真正异步 I/O，减少线程池占用

3. **Token 刷新优化**：当前 `lock` 是静态的全进程锁，高并发时所有 API 请求排队。可以用 `SemaphoreSlim(1,1)` 替代，支持 async await 不阻塞线程

4. **弱事件模式**：当前事件订阅都是强引用，忘记 Unsubscribe 就会内存泄漏。可以引入 WeakEventManager 或 WeakReference 订阅

5. **WebSocket 指数退避**：当前线性重试（3 次 × 5 秒），改为指数退避（5s → 10s → 20s → 40s → 上限 60s）更合理

6. **配置管理**：当前 76 个配置项分散在两个 XML 文件中，可以引入配置中心（如 Apollo/Nacos）实现动态配置和版本管理

7. **单元测试**：项目目前缺少自动化测试。服务层接口已经解耦，适合引入 Moq + xUnit 编写单元测试

8. **日志结构化**：当前日志是纯文本，可以引入结构化日志（Serilog + JSON 格式），便于日志检索和分析

---

### Q30：请描述一个你在项目中遇到的技术难题以及解决过程

**A：**（可根据实际经历选择以下之一作答）

**示例 1：Unity 3D 焦点抢占问题**

> **问题**：Unity 进程嵌入 WPF 后，用户点击 3D 区域会导致 WPF 主窗口失去键盘焦点，所有 WPF 快捷键失效。
>
> **排查**：通过 Spy++ 监控窗口消息，发现点击 Unity 区域时 Windows 将焦点切换到了 Unity 子窗口。
>
> **方案**：300ms 定时器轮询光标位置，用 `WindowFromPoint` 判断焦点归属。不在 Unity 区域时发送 `WM_ACTIVATE(WA_INACTIVE)` 给 Unity 窗口，强制释放焦点。
>
> **效果**：焦点在 WPF 和 Unity 之间无缝切换，用户无感知。

**示例 2：视频播放器快速切换导致内存泄漏**

> **问题**：调度员快速切换摄像头时，每个切换都创建新的 Media 和播放会话，来不及释放，内存持续增长。
>
> **排查**：通过 dotMemory 发现 MediaPlayer 对象数量异常增长，引用链指向未完成的 URL 解析异步操作。
>
> **方案**：引入 Rx 的 `Throttle(1s)` + `Switch()` 操作符。Throttle 确保快速切换时只播放最后一个，Switch 自动取消前一个未完成的异步操作。
>
> **效果**：无论切换多快，同时只有一个活跃播放会话，内存稳定。

**示例 3：多工作站告警消息不同步**

> **问题**：调度员 A 处置了某条告警，调度员 B 的界面上仍然显示未处置状态。
>
> **排查**：发现告警状态变更只通过 REST API 写入数据库，其他工作站需要手动刷新才能看到。
>
> **方案**：引入 RocketMQ 广播模式，告警状态变更时发布消息到 MQ，所有工作站订阅并实时更新本地状态。
>
> **效果**：告警处置后所有工作站秒级同步，消除了信息不一致。

---

## 十一、基础知识延伸类

### Q31：WPF 的 Dispatcher 和 SynchronizationContext 有什么关系？

**A：** `Dispatcher` 是 WPF 的消息泵，管理 UI 线程的任务队列。`SynchronizationContext` 是 .NET 的抽象，提供跨线程 Post/Send 的统一接口。

在 WPF 中，`DispatcherSynchronizationContext` 是 `SynchronizationContext` 的 WPF 实现，内部调用 `Dispatcher.BeginInvoke`。当 async/await 在 UI 线程执行时，`await` 会自动捕获当前的 `SynchronizationContext`，`await` 完成后通过它回到 UI 线程。

项目中 EventBus 在构造时捕获 `SynchronizationContext.Current`（即 UI 线程的 DispatcherSynchronizationContext），确保订阅者选择 `ThreadOption.UIThread` 时能正确回到 UI 线程。

---

### Q32：Prism 的 Region 和 WPF 的 ContentControl 有什么关系？

**A：** Region 是 Prism 对 WPF 容器控件的抽象。任何实现了 `IRegion` 接口的容器（ContentControl、ItemsControl、TabControl 等）都可以注册为 Region。

```xml
<!-- XAML 中声明 Region -->
<ContentControl prism:RegionManager.RegionName="MainContentRegion" />
```

`RegionManager` 维护 `RegionName → IRegion` 的映射。`RegionService.ShowView` 通过 `RegionManager.Regions[regionName]` 获取 Region 实例，然后调用 `Add/Activate` 管理子视图。

项目中主窗口有一个 `MainContentRegion`，菜单切换时通过 `ShowView` 将不同模块的视图放入这个区域。

---

### Q33：为什么选择 LiteDB 而不是 SQLite？

**A：** LiteDB 是 .NET 原生的嵌入式 NoSQL 文档数据库，选择它的原因：

1. **零配置**：单文件部署，无需安装运行时
2. **文档模型**：直接存储 C# 对象，无需 ORM 映射（相比 SQLite 需要 Dapper/EF）
3. **API 简洁**：`collection.Insert(entity)` / `collection.FindOne(x => x.Id == id)`
4. **适合场景**：项目中 LiteDB 用于本地缓存和离线数据存储，数据量不大，文档模型足够

如果数据量大或需要复杂关系查询，SQLite 更合适。

---

### Q34：项目中如何处理 WPF 的多分辨率适配？

**A：** 三层适配方案：

1. **DPI 感知层**：`DpiHelper` 通过 Win32 `GetDeviceCaps` 获取 DPI，计算逻辑像素与物理像素的转换矩阵
2. **屏幕识别层**：`WpfScreenHelper` 识别当前显示器，以 1920px 设计稿为基准计算缩放因子 `factor = 1920.0 / screenWidth`
3. **资源字典层**：`Resolution_2K.xaml` 和 `Resolution_4K.xaml` 分别定义字体和尺寸，`ResolutionRatioConverter` 在运行时乘以缩放因子

```xml
<!-- XAML 中使用响应式尺寸 -->
<Grid Width="{extensions:Double 200}"
      Margin="{extensions:Thickness 10,0,10,0}"
      FontSize="{Binding Source, Converter={StaticResource ResolutionRatioConverter}}" />
```

关键原则：**以 1920×1080 为设计基准**，所有尺寸按屏幕实际宽度等比缩放。
