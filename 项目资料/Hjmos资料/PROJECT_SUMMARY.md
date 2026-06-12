# 智慧地铁管控平台（HJMos NCC Client）项目总结

## 一、项目概述

**智慧地铁管控平台**（HJMos NCC Client）是由**佳都科技（PCI Technology Group）**开发的 WPF 桌面应用程序，定位为城市轨道交通的统一运营管控中心。平台覆盖线网、线路、车站三个层级的监控与指挥功能，集成了实时监控、应急指挥、视频监控、设备管理、数据分析等核心业务能力。

| 属性 | 内容 |
|------|------|
| 产品名称 | 智慧地铁管控平台 |
| 当前版本 | 1.7.0.0 |
| 开发公司 | 佳都科技（PCI Technology Group Co., Ltd.） |
| 项目周期 | 2020年11月 ~ 至今（持续迭代中） |
| 目标框架 | .NET Framework 4.7.2 / .NET 6.0-windows |
| 架构模式 | Prism MVVM（DryIoc 容器） |
| Git 总提交数 | 4018 次 |
| 参与开发者 | 12 人 |

---

## 二、团队贡献

| 开发者 | 提交次数 |
|--------|---------|
| wangwenchao | 1,387 |
| wengzepeng | 847 |
| zouwanmao | 555 |
| wutangyuan | 452 |
| chenzijiong | 361 |
| 王文超 | 284 |
| linxiaobin | 90 |
| Damon | 32 |
| 其他 | 10 |

> 核心开发团队以 wangwenchao、wengzepeng、zouwanmao、wutangyuan、chenzijiong 为主，前五位贡献了超过 90% 的代码提交。

---

## 三、版本演进历程

### 3.1 项目阶段划分

| 阶段 | 时间 | 关键内容 |
|------|------|---------|
| **基础框架搭建** | 2020.11 ~ 2021 | 初始框架、样式控件体系、线网图、CefSharp集成、各模块项目创建 |
| **核心功能开发** | 2021 ~ 2022 | 应急指挥、视频监控、设备监控、全息感知等主要模块上线 |
| **功能完善迭代** | 2022 ~ 2024 | 开关站流程、模式控制、数据看板、智能联动等业务功能深化 |
| **持续优化** | 2025 ~ 2026 | 防汛作战系统、数据看板扩展、车站能耗、菜单状态管理重构等 |

### 3.2 主要版本节点

| 版本 | 核心变更 |
|------|---------|
| 1.6.12.x | 数据看板（客运管理/乘客画像/线网数据/RDP报表）、水淹评估、车站模式控制联调 |
| 1.6.14.x | 开关站仿真模式、综合监控会话管理、3D显示隐藏优化、车站能耗修复 |
| 1.7.0.0 | 防汛作战系统（消息弹窗/全屏页面/单实例协调）、菜单状态独立存储重构 |

---

## 四、系统架构

### 4.1 整体架构

```
┌─────────────────────────────────────────────────────────────┐
│                   Hjmos.Ncc.WS (主入口)                      │
│              App.xaml → HjmosApplication                     │
│         ┌──────────┼──────────┐                             │
│    DI容器(DryIoc)  模块加载    服务注册                       │
└─────────┬──────────┬──────────┬─────────────────────────────┘
          │          │          │
┌─────────▼──┐ ┌────▼────┐ ┌──▼──────────────┐
│ Hjmos.Shell│ │ Modules │ │ Hjmos.Service    │
│ 主窗口/菜单 │ │ 功能模块 │ │ 服务接口层        │
│ 导航/主题   │ │ (11个)  │ │ (30+业务接口)     │
└────────────┘ └─────────┘ └──┬───────┬──────┘
                              │       │
                    ┌─────────▼┐ ┌───▼──────────┐
                    │TempService│ │OfflineService │
                    │ 真实服务   │ │ 离线/模拟服务  │
                    └──────────┘ └──────────────┘
```

### 4.2 解决方案结构

```
HJMos_NCC_Client_V1.0.sln
├── src/                          # 核心基础项目
│   ├── Hjmos.Ncc.WS/            # 主入口 (WinExe)，引用全部模块
│   ├── Hjmos.Shell/              # 应用外壳（主窗口、菜单、主题）
│   ├── Hjmos.Ncc.Common/         # 共享枚举、事件、实体、工具类
│   ├── Hjmos.Ncc.Resource/       # 资源（样式/主题/转换器/图片）
│   ├── Hjmos.Service/            # 服务接口定义（30+ 业务接口）
│   ├── Hjmos.Ncc.TempService/    # 服务真实实现（REST/WebSocket/MQ）
│   ├── Hjmos.Ncc.OfflineService/ # 离线/模拟服务（装饰器模式）
│   └── Hjmos.TempEntity/         # 实体/DTO 类（35个子目录）
│
├── Modules/                      # 功能模块（Prism IModule 插件）
│   ├── Hjmos.Ncc.Holoception.All/       # 全息感知 - 全网
│   ├── Hjmos.Ncc.Holoception.Line/      # 全息感知 - 线路
│   ├── Hjmos.Ncc.Holoception.Station/   # 全息感知 - 车站
│   ├── Hjmos.Ncc.EmergencyCommandV1/    # 应急指挥 - 线路级
│   ├── Hjmos.Ncc.StationEmergencyCommand/# 应急处置 - 车站级
│   ├── Hjmos.Ncc.DeviceMonitoring/      # 设备监控
│   ├── Hjmos.Ncc.Cctv/                  # 视频监控 - 线路级
│   ├── Hjmos.Ncc.CctvMonitoring/        # 视频监控 - 车站级
│   ├── Hjmos.Ncc.DataBoard/             # 数据看板
│   ├── Hjmos.Ncc.IntelligentApp/        # 智能应用
│   └── Hjmos.Ncc.IntelligentLinkage/    # 智能联动
│
├── Nuget/Nuget/                  # 内部共享库
│   ├── Hjmos/                    # 核心框架（权限/配置/DI）
│   ├── Hjmos.Browser/            # CefSharp 浏览器封装
│   ├── Hjmos.WebView2/           # WebView2 集成
│   ├── Hjmos.SharedStyle/        # 共享样式与控件
│   ├── Hjmos.Restful/            # REST API 客户端
│   ├── Hjmos.MQProxy/            # RocketMQ 消息代理
│   ├── Hjmos.WebSocket/          # WebSocket 实时推送
│   ├── Hjmos.Ice/                # ZeroC ICE RPC
│   ├── Hjmos.Log/                # NLog 日志
│   ├── Hjmos.Security/           # 安全/加密
│   └── ... (更多基础库)
│
└── Work/                         # 构建输出目录
```

### 4.3 启动流程

```
App.xaml (HjmosApplication : PrismApplication)
    │
    ├── CreateShell()          → 创建 LoginWindow / MainWindow
    ├── CreateModuleCatalog()  → ConfigurationModuleCatalog（读取 App.config）
    ├── RegisterTypes()        → 40+ 单例服务注册
    │   ├── 基础数据服务 (IBasicDataService)
    │   ├── 设备服务 (IDeviceService)
    │   ├── 应急服务 (IEmergencyService)
    │   ├── 视频服务 (IVideoService, 3种实现按配置切换)
    │   ├── 能源服务 (IEnergyService)
    │   ├── MQ服务 (IMQService)
    │   ├── WebSocket服务 (IWsService)
    │   ├── 客流服务 (IPassengerFlowService)
    │   ├── 防汛服务 (IFloodPreventionService)
    │   └── ... 更多
    └── OnInitialized()        → 菜单构建、模块按需加载
```

---

## 五、核心业务模块

### 5.1 模块总览

| 模块 | 功能名称 | 加载方式 | 作用域 |
|------|---------|---------|--------|
| Holoception.All | 全息感知（全网） | 按需 | 线网+线路+车站 |
| Holoception.Line | 全息感知（线路） | 按需 | 线路 |
| Holoception.Station | 全息感知（车站） | 启动加载 | 车站 |
| EmergencyCommandV1 | 应急指挥（线路级） | 启动加载 | 线路 |
| StationEmergencyCommand | 应急处置（车站级） | 启动加载 | 车站 |
| DeviceMonitoring | 设备监控 | 按需 | 线路+车站 |
| Cctv | 视频监控（线路级） | 启动加载 | 线路 |
| CctvMonitoring | 视频监控（车站级） | 按需 | 车站 |
| DataBoard | 数据看板 | 按需 | 线路+车站 |
| IntelligentApp | 智能应用 | 按需 | 线路+车站 |
| IntelligentLinkage | 智能联动 | 启动加载 | 全部 |

### 5.2 各模块核心能力

#### 全息感知（Holoception）
- **全网层级**：顶层容器，汇总展示线网整体态势
- **线路层级**：列车追踪、客流监测、防汛预警、能源监控、屏蔽门状态、广播/PIS、设备态势
- **车站层级**：开关站流程、视频巡逻、清客管理、卷帘门/扶梯/垂梯/站台门控制、广播/PIS、节能照明、客流监测

#### 应急指挥（EmergencyCommand）
- **线路级**：告警监控、事件发布/处置、应急预案、防汛指挥、火灾响应（区间/车站）、疏散指挥、视频联动
- **车站级**：告警处理、应急联动、AFC/闸机/门控、防汛作战、排水处置、火灾响应、通讯录

#### 视频监控（Cctv）
- 视频资源分类管理、视频巡检、全屏/轮巡弹窗、视频分析事件

#### 数据看板（DataBoard）
- 公共看板与个人看板、数据分析、能耗统计（线路/牵引/车站）、Web报表、LiveCharts图表
- 客运管理、乘客画像、线网数据、RDP报表集成

#### 智能联动（IntelligentLinkage）
- 应用消息推送、事件处理与发布

---

## 六、技术栈总览

| 分类 | 技术 | 版本/说明 |
|------|------|----------|
| **UI 框架** | WPF | .NET Framework 4.7.2（主） / .NET 6.0-windows（辅） |
| **MVVM 框架** | Prism | 8.1.97 + DryIoc 容器 |
| **DI 容器** | DryIoc + MS DI | Microsoft.Extensions.DependencyInjection |
| **Web 渲染** | CefSharp | Chromium 89，主方案；WebView2 备选 |
| **图表库** | LiveCharts | 本地 Fork 版本，自定义扩展 |
| **消息队列** | RocketMQ | 阿里云 ONS C++ SDK (SWIG P/Invoke) |
| **实时推送** | WebSocket | WebSocketSharp 封装 |
| **REST 客户端** | RestSharp | 封装为 RestApi 静态门面 |
| **RPC** | ZeroC ICE | v3.7.4，TCP 协议 + Slice 接口定义 |
| **嵌入式数据库** | LiteDB | 5.0.16，NoSQL 文档存储 |
| **日志** | NLog | 结构化日志 + 异步缓冲 + 文件归档 |
| **对象映射** | AutoMapper | 10/11 |
| **FTP** | FluentFTP | 37.1.0，用于自动升级 |
| **加密** | BouncyCastle | AES-192/256、RSA PKCS#1、SHA256 |
| **JSON** | Newtonsoft.Json | 13.0.1，自研序列化器 + 属性过滤 |
| **SVG** | SharpVectors | 1.7.6 |
| **视频播放** | LibVLCSharp | VideoLAN.LibVLC 3.0.16 + System.Reactive |
| **3D 渲染** | Unity | 独立进程嵌入 + PESocket TCP IPC |
| **IPC** | Named Pipes | 自研泛型管道框架 |
| **语音** | System.Speech | TTS 语音播报 |
| **响应式** | System.Reactive | 4.4.1，视频播放/事件节流 |
| **自动更新** | 自研 FTP 升级 | MD5 差异比对 + 备份回滚 |
| **DPI 适配** | WpfScreenHelper | 多分辨率 2K/4K 自适应 |

---

## 七、MVVM 框架详细设计

### 7.1 ViewModel 继承体系

项目构建了丰富的 ViewModel 基类继承链，统一了属性通知、对话框感知、生命周期管理等横切关注点：

```
Prism.Mvvm.BindableBase                ← INotifyPropertyChanged (SetProperty<T>)
  └── ModelBase                        ← +IDialogAware（对话框支持）
       │                                  +Title, Description 属性
       │                                  +CloseWindow(IDialogResult)
       │                                  +BeginInvoke(Action) 线程调度
       │
       ├── ViewModelBase               ← +抽象 GetData() / InitBaseData()
       │                                  +GlobalUpdate 事件订阅
       │
       ├── ActiveModelBase             ← +IUserControlStatus（视图生命周期）
       │    │                             +IsActive 自动订阅/取消订阅
       │    └── ResetViewModelBase     ← +Reset 事件响应
       │
       └── ModelBase1                  ← 简化版本（无 IDialogAware）
            └── DialogModelBase        ← +IDialogAware + CloseWindow(ButtonResult)
```

### 7.2 视图生命周期管理

通过 `IUserControlStatus` 接口 + `UserControlAttach` 附加属性实现自动生命周期：

```
UserControl.Loaded   → IsActive = true  → 自动订阅 GlobalUpdate 事件
UserControl.Unloaded → IsActive = false → 自动取消订阅，释放资源
```

```xml
<!-- XAML 中通过附加属性启用 -->
<UserControl local:UserControlAttach.ApplyDataReceptionStyle="True">
```

`ActiveModelBase` 在 `IsActive` 状态变化时自动管理事件订阅，防止后台视图占用资源：

```csharp
public bool IsActive {
    set {
        if (value)
            PrismService.EventBus.GetEvent(CommonEvents.GlobalUpdate).Subscribe(GlobalUpdateHandler);
        else
            PrismService.EventBus.GetEvent(CommonEvents.GlobalUpdate).Unsubscribe(GlobalUpdateHandler);
    }
}
```

### 7.3 命令模式

项目统一使用 `DelegateCommand` 的懒加载模式：

```csharp
// 无参命令
private DelegateCommand _saveCommand;
public DelegateCommand SaveCommand =>
    _saveCommand ?? (_saveCommand = new DelegateCommand(ExecuteSave));

// 带参命令 + CanExecute
private DelegateCommand<object> _checkCommand;
public DelegateCommand<object> CheckCommand =>
    _checkCommand ?? (_checkCommand = new DelegateCommand<object>(
        ExecuteCheck, obj => !IsReadOnly));
```

部分命令通过 XAML `Interaction.Triggers` 绑定 `Loaded` 事件，实现视图加载时自动执行初始化逻辑。

### 7.4 依赖注入与服务定位

采用**混合注入模式**：

- **构造函数注入**：Service 层实现类的依赖
- **静态服务定位**：ViewModel 层通过 `PrismService.Resolve<T>()` 获取服务

```csharp
public static class PrismService {
    public static T Resolve<T>() => ContainerLocator.Container.Resolve<T>();

    // 高频服务快捷属性
    public static IEventBus EventBus => Resolve<IEventBus>();
    public static IRegionService RegionService => Resolve<IRegionService>();
    public static INewDialogService NewDialogService => Resolve<INewDialogService>();
    public static IAuthorityService AuthorityService => Resolve<IAuthorityService>();
    // ...
}
```

实际使用：
```csharp
var station = await PrismService.Resolve<IBasicDataService>().GetStationByStationCode(code);
var authority = await PrismService.Resolve<IAuthorityService>().GetAuthority();
```

---

## 八、Region 导航与模块管理

### 8.1 Region 导航机制

项目自定义了 `IRegionService` 替代 Prism 原生 `RequestNavigate`，实现了**视图缓存 + 手动激活**策略：

```csharp
public void ShowView(string regionName, Type viewType, IRegionParameters parameters = null) {
    var region = PrismService.RegionManager.Regions[regionName];

    // 1. 缓存查找：按 FullName 查找已添加的视图实例
    object obj = region.GetView(viewType.FullName);

    // 2. 容器解析：未命中则从 DryIoc 容器创建新实例并添加到 Region
    if (obj == null) {
        obj = ContainerLocator.Container.Resolve(viewType);
        region.Add(obj, viewType.FullName);
    }

    // 3. 激活视图
    region.Activate(obj);

    // 4. 通知 ViewModel（自定义 IRegionAware 接口）
    var vm = ((FrameworkElement)obj).DataContext;
    if (vm is IRegionAware aware)
        aware.OnViewActived(parameters);
}
```

**核心设计决策：**
- **视图单例缓存**：同一类型视图只创建一次，后续切换复用已有实例
- **`isReload` 参数**：可强制删除并重建视图实例
- **`SendMsgToChildren`**：遍历可视化树，向所有子 UserControl 广播 `OnViewActived`
- **回调变体**：`IRegionCallBackAware` 支持父子视图间的双向通信

### 8.2 模块加载策略

使用 `ConfigurationModuleCatalog`（XML 配置驱动，非目录扫描）：

```xml
<!-- App.config 中声明模块 -->
<modules>
  <module assemblyFile="Hjmos.Ncc.Holoception.Station.dll"
          moduleType="...HoloceptionStationModule"
          moduleName="HoloceptionStation"
          startupLoaded="true" />
  <module assemblyFile="Hjmos.Ncc.DataBoard.dll"
          moduleType="...DataBoardModule"
          moduleName="DataBoard"
          startupLoaded="false" />  <!-- 按需加载 -->
</modules>
```

每个模块实现 `IModule` 接口，在 `RegisterTypes()` 中注册视图和对话框：

```csharp
public class StationEmergencyCommandModule : IModule {
    public void RegisterTypes(IContainerRegistry registry) {
        registry.RegisterDialog<AlarmDetailView>("StationAlarmDetailView");
        registry.RegisterDialog<EmergencyEventDetailView>("StationEmergencyEventDetailView");
        // ... 30+ 对话框注册
    }
}
```

---

## 九、事件总线与跨模块通信

### 9.1 EventBus 实现

对 Prism `EventAggregator` 的封装，使用**枚举键字典**管理事件：

```csharp
public class EventBus : IEventBus {
    private readonly Dictionary<Enum, EventBase> events = new();
    private readonly SynchronizationContext syncContext = SynchronizationContext.Current;

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
}
```

### 9.2 事件键定义

```csharp
public enum CommonEvents {
    GlobalUpdate,      // 全局数据刷新（切换车站/线路时触发）
    MQMessage,         // 消息队列通知
    ReflashToken,      // OAuth Token 刷新（通知 3D 等外部进程）
    EmergencyEvent,    // 跨模块应急通信
    FloodEvent         // 防汛事件
}
```

### 9.3 使用模式

```csharp
// 订阅（通常在 ViewModel 构造或 IsActive=true 时）
PrismService.EventBus.GetEvent<dynamic>(CommonEvents.GlobalUpdate)
    .Subscribe(() => { InitBaseData(); GetData(); },
               ThreadOption.BackgroundThread,
               keepSubscriberReferenceAlive: false);  // 弱引用防内存泄漏

// 发布（切换上下文时）
PrismService.EventBus.GetEvent<dynamic>(CommonEvents.GlobalUpdate).Publish();

// 取消订阅（IsActive=false 时自动执行）
PrismService.EventBus.GetEvent<dynamic>(CommonEvents.GlobalUpdate).Unsubscribe(handler);
```

---

## 十、对话框服务

### 10.1 标准对话框（INewDialogService）

```csharp
public interface INewDialogService {
    void Show(string name, IDialogParameters parameters,
              Action<IDialogResult> callback, string windowName);
}
```

- 按注册名称从容器解析对话框内容
- 自动通过 `ViewModelLocator` 关联 ViewModel
- 支持命名窗口样式：`"FullWindow"`（全屏）、`"CustomBlurWindow"`（毛玻璃）

### 10.2 多线程对话框（NewDialogService1）

- 每个对话框在**独立 UI 线程**运行（`UIDispatcher.RunNewAsync()`）
- 维护 `Dictionary<string, (Dispatcher, Window)>` 管理已打开窗口
- **防重复打开**：已存在的窗口仅激活，不创建新实例
- 关闭时自动清理追踪字典

### 10.3 ViewModel 关闭对话框

```csharp
// ModelBase 实现 IDialogAware，提供便捷方法
CloseWindow(new DialogResult(ButtonResult.OK));
CloseWindow(new DialogResult(ButtonResult.Cancel));
```

---

## 十一、数据通信层详细设计

### 11.1 REST API 客户端

基于 **RestSharp** 封装的静态门面 `RestApi`：

```
RestApi (静态类)
  └── RestSharp.RestClient → HTTP 请求执行
       └── Newtonsoft.Json → JSON 序列化/反序列化
```

**请求/响应模式：**
- **标准信封**：所有响应反序列化为 `RestfulResponse<T>` `{ code, data, message }`，`code == 0` 表示成功
- **认证头**：每个请求附加 `Authorization: Basic {credential}` 和 `ID-Token: {accessToken}`
- **参数过滤**：`ContainParaContractResolver` 支持序列化时的属性包含/排除

**Token 自动刷新（401/403 拦截重试）：**

```
请求 → 服务端返回 401/403
  → lock(_Locker) 防并发刷新
  → 调用 RefreshToken 接口
  → 检查是否已被其他线程刷新（比较 AccessToken）
  → 使用新 Token 重试原请求（仅重试一次）
```

**降级策略**：异常时返回 `Activator.CreateInstance<T>()`（空对象），不抛出异常，通过 `LogHelper.Error()` 记录错误。

**文件操作**：下载使用 `AdvancedResponseWriter` 流式传输 + 进度事件；上传使用 `AlwaysMultipartFormData`。

### 11.2 WebSocket 实时推送

基于 **WebSocketSharp** 封装的 `WebSocketClient`：

**连接与心跳：**

```
构造函数 → 立即连接 → 立即发送订阅参数
  └── 心跳线程（Task.Factory.StartNew, LongRunning）
       ├── 连接存活：每 295 秒（~5分钟）发送心跳
       └── 连接断开：ReConnect(3次重试, 5秒间隔)
            └── 全部失败 → 标记 _isConnectionClosed，终止心跳循环
```

**消息处理：**
- 入站：JSON → `WebSocketResponse { Command, Data }` → 过滤心跳回显 → 触发 `MessageReceived` 事件
- 出站：`IDictionary<string, object>` → `.ToJson()` → `SendAsync()`
- 特殊处理：忽略 `"连接成功"` 字面量响应

### 11.3 RocketMQ 消息队列

通过 **阿里云 ONS C++ SDK 的 SWIG P/Invoke 绑定**集成（非托管 .NET 客户端）：

**消费者配置：**

```csharp
ONSFactoryProperty props = new ONSFactoryProperty();
props.setFactoryProperty(ONSFactoryProperty.AccessKey, AccessKey);
props.setFactoryProperty(ONSFactoryProperty.SecretKey, SecretKey);
props.setFactoryProperty(ONSFactoryProperty.NAMESRV_ADDR, NameServerAddress);
props.setFactoryProperty(ONSFactoryProperty.ConsumerId, ConsumerGroupID);
props.setFactoryProperty(ONSFactoryProperty.MessageModel, BROADCASTING);

PushConsumer consumer = ONSFactory.getInstance().createPushConsumer(props);
consumer.subscribe(Topic, Tag, listener);
consumer.start();
```

**广播模式**：默认 `BROADCASTING`（每个工作站收到所有消息），也支持 `CLUSTERING`。

**消息处理**：`MyMsgListener.consume()` 自动提交，按消息体格式转换为：
- 字符串 → `TextRocketMessage`
- JSON 对象 → `ObjectRocketMessage`

**工厂模式**：支持编程式创建 `ConnectionFactory.Consumer(new RocketMQPara{...})` 和配置驱动 `ConnectionFactory.Consumer("instanceName")`（读取 App.config XML）。

### 11.4 ZeroC ICE RPC

基于 ICE v3.7.4 的 RPC 通信，用于与综合监控系统（ISCS）集成：

**连接管理**：`IceInterfaceBase` 管理 `Ice.Communicator` 生命周期：

```csharp
// 连接属性
Ice.Trace.Network = 3        // 网络追踪级别
Ice.Trace.Protocol = 1       // 协议追踪
Ice.ACM.Heartbeat = 3        // 始终发送心跳

// 代理创建（带 20 秒超时）
var p = Communicator.stringToProxy("{serviceName}:tcp -h {host} -p {port}");
Prx = IDataPointAccessorPrxHelper.checkedCast(p.ice_timeout(20000));
```

**回调注册（实时数据推送）：**

```csharp
// 创建 ObjectAdapter，自动检测物理网卡 IP + 扫描空闲端口
Identity identity = Util.stringToIdentity("UpdateDataReceiver");
Adapter.add(callback, identity);

// 注册数据监控回调
IDataMonitorUpdatorPrx updatorPrx = IDataMonitorUpdatorPrxHelper.uncheckedCast(
    Adapter.createProxy(identity));
Prx.RegisterDataMonitor(lineId, locationId, updatorPrx, pointNameArray);
```

**Slice 生成的接口：**
- `IDataPointAccessor`：注册/取消注册数据回调、批量查询点位状态
- `IDataChangedUpdator`：UpdateDataPoint、UpdateDataNode 回调
- `IDataMonitorUpdator`：UpdateMonitorPointStatus 回调
- `IDataTrainTravelInfoUpdator`：UpdateTrainTravelInfo 回调

### 11.5 数据缓存策略

服务层使用 `CacheHelper` 实现内存缓存：

```csharp
// BasicDataService 缓存示例
public async Task<List<Line>> GetLineAndStation() {
    var cached = CacheHelper.Get<List<Line>>(CACHE_NAME_LINESTATIONS);
    if (cached != null) return cached;

    var result = await RestApi.RequestAsync<List<Line>>(url, params);
    CacheHelper.Set(CACHE_NAME_LINESTATIONS, result);  // 无 TTL，应用生命周期内有效
    return result;
}
```

- 缓存键为字符串常量
- 无 TTL/过期机制，缓存与应用同生命周期
- 通过 `GlobalUpdate` 事件触发数据刷新

### 11.6 离线服务模式（装饰器模式）

`OfflineService` 包装真实服务实现，选择性覆盖方法以从本地 JSON 文件读取数据：

```csharp
public class BasicDataService : IBasicDataService {
    private readonly IBasicDataService _realService;  // 被装饰的真实服务

    public async Task<List<Line>> GetLineAndStation() {
        // 从本地 JSON 读取（离线演示模式）
        return await Task.Run(() =>
            MQService.ReadJson<List<Line>>(RestfulServiceNames.GetLineAndStation));
    }

    // 未覆盖的方法直接委托给真实服务
    public Task<Station> GetStation(string id) => _realService.GetStation(id);
}
```

---

## 十二、Web 渲染与 H5 通信

### 12.1 CefSharp 集成（HJBrowser）

`HJBrowser` 是封装 `ChromiumWebBrowser` 的 WPF UserControl：

**CefSharp 全局配置：**

```
Locale = "zh-CN"
--disable-web-security     （禁用跨域限制）
--ignore-certificate-errors
enable-gpu                 （GPU 加速）
disable-gpu-vsync
enable-media-stream        （麦克风访问）
CachePath = ./cache
SubprocessExitIfParentProcessClosed = true
```

**生命周期管理：**
- `Loaded` → `InitCefSharp()` 创建浏览器实例并设置处理器
- `Unloaded` → 销毁（除非 `KeepAlive=true`）
- 配置处理器：
  - `KeyBoardHandler`：F5=刷新、F12=DevTools
  - `MenuHandler`：完全清除右键菜单
  - `CustomRequestHandler`：注入 `hjmos-authorization` 请求头
  - `DownloadHandler`：文件下载提示

### 12.2 WPF ↔ H5 双向通信

**WPF → H5（推送数据）：**

```csharp
// 通过 DependencyProperty 绑定 H5SendMsgModel
// 变化时自动执行 JavaScript
var script = $"{msg.JavaScripMethodName}('{msg.SendJsonMsg}')";
CefSharp.ExecuteScriptAsync(script);
```

**H5 → WPF（回调注册）：**

```csharp
// .NET 对象注册为 JS 全局变量
JavascriptObjectRepository.Register("wpfContentWindow",
    new DeviceInterop(this), BindingOptions.DefaultBinder);

// H5 端调用: window.wpfContentWindow.callBack(data)
```

**双向回调桥（IJavascriptCallback）：**

```csharp
// H5 传递 JS 函数引用给 .NET
public void SetCallBack(IJavascriptCallback callback, string json) {
    // .NET 端异步回调 H5
    await callback.ExecuteAsync(JsonConvert.SerializeObject(data));
}
```

**Cookie 认证传递：**
- `mosToken` Cookie：注入 OAuth Token，实现 WPF → H5 单点登录
- `isClient=true` Cookie：标识客户端环境，H5 据此调整行为

### 12.3 DeviceBrowser（设备配置浏览器）

独立的 `ChromiumWebBrowser` 子类，用于 HMI/SCADA 画面展示：

```csharp
// 注册 JS 互操作对象
JavascriptObjectRepository.Register("wpfContentWindow",
    new DeviceInterop(this), BindingOptions.DefaultBinder);

// DeviceInterop 功能：
// - SetCallBack: H5 注册回调，WPF 异步推送 WebSocket URL、屏幕配置
// - Call(json): H5 发送命令 10004 导航到 HMI 页面
// - 背景色设置为暗色主题 (6, 23, 39)
```

---

## 十三、视频播放系统

### 13.1 LibVLCSharp 集成（主方案）

```
静态 LibVLC 实例（全局共享）
  └── VideoPlayerView (每个控件一个 MediaPlayer)
       ├── System.Reactive 响应式管道
       │    ├── Subject<Tuple<string,long>> → 播放/停止命令
       │    ├── Throttle(1000ms) → DistinctUntilChanged
       │    └── ObserveOnDispatcher → 执行
       ├── 5 秒心跳定时器 → KeepAlive(sessionId)
       └── PTZ 控制：上下左右、变焦
```

**视频播放流程：**

```csharp
// 1. 获取 RTSP 流地址
var result = await PrismService.Resolve<IVideoService>().PlayVideo(cameraId);
// 返回: { RtspUrl, SessionId }

// 2. 创建 Media 并播放
mediaPlayer.Play(new Media(_libVlc, result.RtspUrl, FromType.FromLocation));

// 3. 心跳保活（5 秒定时器）
_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
_timer.Tick += (s, e) => videoService.KeepAlive(sessionId);

// 4. PTZ 控制
videoService.ToLeft(sessionId);   // 左移
videoService.ToRight(sessionId);  // 右移
videoService.ZoomIn(sessionId);   // 变焦放大
```

### 13.2 三种视频服务实现

| 实现 | 配置值 | 对接方案 |
|------|--------|---------|
| `VideoService2`（默认） | `VideoMode=0` | 直接 RTSP 流，通过 REST API 获取流地址 |
| `VideoService` | `VideoMode=1` | ISCS 媒体代理集成 |
| `VideoService3` | `VideoMode=2` | ONVIF 协议直连摄像头 |

通过 `AddressConfigEnum.VideoMode` 配置切换，在 `App.Service.cs` 注册到 DI 容器。

---

## 十四、3D 渲染与进程嵌入

### 14.1 Unity 进程级嵌入

Unity 3D 视图以**独立进程**方式运行，嵌入到 WPF 窗口中：

```
WPF 应用                              Unity 进程
┌────────────────────┐                ┌────────────────────┐
│ WinFormsHost       │  SetParent     │ Unity Player       │
│  └── Panel         │◄──────────────│  (-parentHWND)     │
│     └── HWND 宿主  │  Win32 API     │                    │
└────────────────────┘                └─────────┬──────────┘
                                                │
                              PESocket TCP (127.0.0.1:9968)
                              ┌─────────────────┴──────────┐
                              │ PEMessage { CmdCode, Data } │
                              └────────────────────────────┘
```

**启动流程：**

```csharp
// 1. 启动 Unity 进程
Process.Start("3DViewer.exe",
    $"-parentHWND {wpfPanelHandle} {width} {height} {wsUrl} {token}");

// 2. 查找 Unity 子窗口
WinApiHelper.EnumChildWindows(pid, (hwnd, lParam) => {
    unityHWND = hwnd;  // 存储 Unity 窗口句柄
});

// 3. 嵌入到 WPF Panel
SetParent(unityHWND, wpfPanelHandle);
```

**焦点管理**（防止 Unity 抢夺键盘焦点）：
- `DispatcherTimer` 每 300ms 轮询光标位置
- `GetCursorPos` + `WindowFromPoint` 判断焦点归属
- 向 Unity 发送 `WM_ACTIVATE` 消息：光标在 Unity 区域时 `WA_ACTIVE`，否则 `WA_INACTIVE`

### 14.2 Unity IPC 通信（PESocket）

通过 PESocket TCP 框架与 Unity 进程通信：

```csharp
// 消息格式
PEMessage { CmdCode, Data(JSON), ErrCode, Message }

// 支持的命令
CmdCode.CloseProcess    // 关闭 Unity 进程
CmdCode.FullScreen      // 全屏显示
CmdCode.NomalScreen     // 恢复正常窗口
CmdCode.DeviceDetails   // 设备详情展示
CmdCode.Panorama        // 全景视角
CmdCode.DataReply       // 数据响应

// Token 刷新推送
PrismService.EventBus.GetEvent(CommonEvents.ReflashToken)
    .Subscribe(() => socket.Send(new PEMessage { CmdCode = ReflashToken, Data = newToken }));
```

---

## 十五、自定义控件与主题系统

### 15.1 控件库（Hjmos.SharedStyle，100+ 自定义控件）

| 类别 | 控件列表 |
|------|---------|
| **附加行为** (28) | `Attacher`, `DataGridAttach`, `ScrollViewerAttach`, `PasswordBoxHelper`, `TreeViewAttach`, `WindowAttach`, `VisualElement`, `IconElement`, `TitleElement`, `TipElement` 等 |
| **输入控件** | `TextBox` (带验证), `PasswordBox`, `ComboBox`, `DatePicker`, `DateTimePicker`, `TimePicker`, `NumericUpDown`, `SearchBar`, `MemoBox` |
| **按钮** | `ButtonGroup`, `EditButton`, `ImageButton` |
| **展示** | `ImageViewer`, `ImageViewerPlus`, `SvgBox`, `Badge`, `Card`, `Divider`, `Tag`, `StepBar`, `RunningBlock` |
| **布局** | `Carousel` (轮播), `Drawer` (抽屉), `SimplePanel`, `DesignSurface` (设计画布), `ZoomBox` (缩略导航) |
| **导航** | `Menu`, `CustomMenuItem`, `SemicircleMenu` (扇形菜单), `TabControl` |
| **对话框/浮层** | `HJDialog` (Adorner 弹窗), `Growl` (Toast 通知), `Notification`, `WaitDialog`, `MessageBox` |
| **数据** | `DataGrid` (扩展), `ListBox`, `TreeView`, `CheckComboBox` |
| **窗口** | `ExWindow` (自定义边框), `FullScreenWindow`, `MaskWindow`, `CustomBlurWindow` |
| **加载** | `LoadingCircle`, `LoadingLine` |
| **嵌入** | `EmbedView`, `EmbedHwndHost`, `ForegroundWindow` (Win32 HWND 宿主) |

**控件设计模式：**
- **TemplatePart 约定**：所有控件使用 `[TemplatePart]` 标注 + `GetTemplateChild()` 在 `OnApplyTemplate()` 中获取模板部件
- **附加属性作为行为**：28 个 Attach 类扩展现有 WPF 控件，无需子类化
- **Adorner 弹窗**：`HJDialog` 通过 `AdornerLayer` 渲染遮罩弹窗，而非创建独立窗口

### 15.2 主题系统

**多分辨率资源字典：**
- `Resolution_2K.xaml`：2K 分辨率字体与尺寸
- `Resolution_4K.xaml`：4K 分辨率字体与尺寸

**配色方案（地铁深蓝主题）：**

```
Primary:      #1A4868    主色调（深蓝）
Secondary:    #0FBBAA    辅助色（青色）
Selected:     #13FFF5    选中高亮
Warn:         #F8E71C    警告色（黄色）
```

**响应式 Markup Extensions：**
- `{extensions:Double}`：编译时按分辨率计算尺寸
- `{extensions:Thickness}`：响应式边距
- `{extensions:ResponsiveSize}`：响应式控件尺寸
- `ResolutionRatioConverter`：MultiValueConverter，运行时乘以缩放因子

### 15.3 转换器库（43 个）

核心转换器：`BooleanToVisibilityConverter`、`ResolutionRatioConverter`（DPI 感知）、`BackgroundMutiValueConverter`（菜单选中状态）、`BorderCircularClipConverter`（圆角裁剪）、`Number2PercentageConverter`、`String2GeometryConverter`、`DataGridRowNumberConverter`、`EnumDescriptionTypeConverter`、`Text2ForegroundConverter` 等。

---

## 十六、安全与加密

### 16.1 AES 加密

```
算法: AES-CBC
密钥: 16/24/32 字节（UTF-8 编码字符串）
IV:   随机生成，前置于密文
输出: Base64 编码

应用场景:
├── 登录凭证持久化（注册表 HKLM\SOFTWARE\PCI\HjmosClient）
│    └── 密钥 = 主板序列号（截取/填充至 16 字节）
└── FTP 升级凭证加密（硬编码 24 字节密钥 = AES-192）
```

### 16.2 RSA 加密（登录密码）

```
流程: 服务端提供公钥 → RSAHelper 加密密码 → 密文传输
算法: RSACryptoServiceProvider + PKCS#1 v1.5
密钥转换: RSAKeyConverter.ToXmlPublicKey()
```

### 16.3 OAuth2 Token 管理

```
认证流程: OAuth2 Password Grant
请求头:   Authorization: Basic {Base64(credential)}
Token:    静态存储于 RestApi.OAuthResult（单例）
发送方式: 每个 REST 请求附加 ID-Token 头
自动刷新: 401/403 → lock → RefreshToken → 检查并发刷新 → 重试原请求
```

### 16.4 SHA256 密码升级

近期将登录密码加密方式从原始方案升级为 SHA256（`efdbdac9` 提交）。

---

## 十七、权限与菜单系统

### 17.1 权限服务

```csharp
public interface IAuthorityService {
    Task<AuthorityInfo> GetAuthority();                    // 完整权限树
    Task<AuthorityResource> GetAuthorityById(string id);   // 特定资源权限
    Task<(Line, Station, UserType)> GetUserOrgInfo();      // 组织上下文
    bool GetFuncEnableAsync(string funcName);              // 功能开关检查
}
```

**实现要点：**
- 从 REST API 获取权限树（`getAuthorityInfo?isShowResourceTree=1`）
- 通过 `CacheHelper` 缓存（键：`CACHE_AUTHORITY_RESOURCE`）
- 过滤仅保留 `hjmos_client` 代码条目
- 递归搜索资源树

### 17.2 声明式权限标注

```csharp
// 菜单枚举标注
public enum ClientMenu {
    [Resource(Id="holoception", Name="全息感知", Url="Hjmos.Ncc.Holoception.All.dll",
              Region=RegionType.All)]
    [Menu(Region=RegionType.All, FontIcon="icon-view")]
    Holoception,

    [Resource(Id="emergency_v1", Name="应急指挥", Url="Hjmos.Ncc.EmergencyCommandV1.dll",
              Region=RegionType.Line)]
    [Menu(Region=RegionType.Line)]
    EmergencyCommandV1,
}

// 功能资源标注
[Resource(Id="flood_prevention", Name="防汛指挥", ResourceType=ResourceType.Page)]
public class FloodPreventionView { }
```

### 17.3 RegionType 标志枚举

```csharp
[Flags]
public enum RegionType {
    Network = 1,    // 线网
    Line    = 2,    // 线路
    Station = 4,    // 车站
    All     = 7     // 全部
}
```

**菜单过滤逻辑：**
```
用户 RoleType=2（线路角色）
  → 遍历 ClientMenu 枚举
  → 按位与: menu.Region & RoleType
  → 结果非零则显示该菜单项
```

### 17.4 UI 权限绑定

```xml
<!-- XAML 中通过转换器控制元素可见性 -->
<Button Visibility="{Binding FuncName,
    Converter={StaticResource AuthorityToEnableConverter}}" />

<!-- 多值转换器：权限 + 额外条件组合 -->
<Button Visibility="{Binding MultiBinding,
    Converter={StaticResource AuthorityToEnableMultiConverter}}" />
```

---

## 十八、DPI 适配与多屏支持

### 18.1 三层 DPI 适配方案

```
┌─────────────────────────────────────────────────────┐
│ 第一层: DpiHelper                                    │
│   Win32 GetDeviceCaps → DPI 矩阵计算                  │
│   LogicalToDeviceUnits() / DeviceToLogicalUnits()    │
├─────────────────────────────────────────────────────┤
│ 第二层: ScreenHelper + WpfScreenHelper               │
│   识别当前显示器 → 计算缩放因子                        │
│   GetFactor() = 1920.0 / screen.Bounds.Width         │
│   （以 1920px 设计稿为基准）                           │
├─────────────────────────────────────────────────────┤
│ 第三层: 响应式资源字典                                 │
│   Resolution_2K.xaml / Resolution_4K.xaml             │
│   {extensions:Double} / {extensions:Thickness}        │
│   ResolutionRatioConverter (运行时动态计算)            │
└─────────────────────────────────────────────────────┘
```

### 18.2 多屏通知定位

使用 `WpfScreenHelper.Screen.FromHandle()` 获取窗口所在显示器信息，正确定位 Toast 通知在对应屏幕的位置。

---

## 十九、IPC 进程间通信

### 19.1 Named Pipe 框架

自研泛型命名管道框架 `PipeServer<T>` / `PipeClient<T>`：

**协议格式：**
```
┌──────────┬─────────────┐
│ 4字节长度 │   消息负载   │
│ (大端序)  │  (JSON UTF-8)│
└──────────┴─────────────┘
```

**连接流程：**
```
服务端创建管道 ({name}_{GUID})
  → 客户端连接初始管道
  → 服务端发送专属管道名
  → 客户端重连到专属管道
  → 双向通信建立
```

**服务端特性：**
- 支持多客户端并发连接
- 广播（全部/子集/指定）
- 事件：`ClientConnected`、`Disconnected`、`MessageReceived`

**客户端特性：**
- 自动重连（默认 1000ms 间隔）
- `CancellationToken` 支持
- 事件：`Connected`、`Disconnected`、`MessageReceived`

### 19.2 PESocket TCP 框架

用于 Unity 3D 进程通信（详见第十四章）。

---

## 二十、日志系统

### 20.1 NLog 配置

```xml
<targets async="true">
  <target name="file" xsi:type="BufferingWrapper" bufferSize="1">
    <target xsi:type="File"
            fileName="../Log/${shortdate}/${processname}/${level}.log"
            layout="${longdate} THREAD[${threadid}] GUID[${pciguid}]
                    &lt;${methodname}&gt; ${message}"
            archiveAboveSize="1024000"
            maxArchiveFiles="10" />
  </target>
</targets>
```

**特性：**
- 异步缓冲写入（bufferSize=1）
- 按日期/进程名/级别组织文件
- 自动归档（>1MB 时归档，最多保留 10 个文件）
- 自定义渲染器：`${pciguid}`（会话 GUID）、`${methodname}`（调用方法名）

### 20.2 LogHelper 封装

```csharp
// 通过 StackTrace 自动获取调用类名作为 Logger 名称
LogHelper.Info("操作成功");
LogHelper.Error("异常信息", exception);
LogHelper.Error("结构化数据: {@value}", order);  // JSON 序列化
LogHelper.Error("结构化数据: {value}", order);   // ToString()
```

---

## 二十一、任务调度框架

### 21.1 TaskScheduler 设计

基于 `System.Timers.Timer` + `ConcurrentDictionary<string, TaskRecord>` 注册表：

| API | 说明 |
|-----|------|
| `Do(interval, action)` | 周期性任务 |
| `DoOnce(interval, action)` | 一次性延迟任务 |
| `DoIt(action, interval)` | 简化周期任务（同步/异步） |
| `Start(name)` / `Stop(name)` | 生命周期控制 |

**执行模式：**
- **并行模式**（默认）：定时器触发不受上次执行影响
- **串行模式**：`IsCompleteOfPrev` 标志位，上次未完成则跳过

**线程安全**：`Interlocked.Exchange` 防止同一任务并发执行

**容错机制**：连续 10 次异常后自动停止任务

---

## 二十二、自动升级系统

### 22.1 升级流程

```
┌────────────────────────────────────────────────────────┐
│ 1. 版本检查                                             │
│    FTP 下载远程 version.json                            │
│    本地递归扫描文件 → 计算 MD5 哈希                       │
│    逐文件比对 → 生成 diff.version.json                  │
├────────────────────────────────────────────────────────┤
│ 2. 增量下载                                             │
│    创建 upgradeFiles/ 临时目录                           │
│    FluentFTP 异步下载差异文件（Retry=5, UTF-8）           │
│    进度回调 0% → 90%                                   │
├────────────────────────────────────────────────────────┤
│ 3. 备份与替换                                           │
│    原文件备份到 backFiles/ 目录                           │
│    新文件复制到原位（90% → 100%）                         │
│    失败时自动回滚（从备份恢复）                            │
├────────────────────────────────────────────────────────┤
│ 4. 重启                                                 │
│    启动新版 ExeFileName → 关闭升级程序                    │
└────────────────────────────────────────────────────────┘
```

### 22.2 配置加密

FTP 凭证使用 AES-192 加密存储于配置文件，属性设置时自动解密，解密失败则降级为明文。

---

## 二十三、全局异常处理

### 23.1 三层异常捕获

| 层级 | 处理器 | 覆盖范围 | 行为 |
|------|--------|---------|------|
| UI 线程 | `DispatcherUnhandledException` | WPF 主线程 | 日志 + `Handled=true`（应用继续） |
| 任务调度 | `TaskScheduler.UnobservedTaskException` | 未 await 的 Task | 日志 + `[子线程级别]` 标记 |
| 进程域 | `AppDomain.UnhandledException` | AppDomain 致命错误 | 日志 + `[程序集底层级别]` 标记 |

**Release 模式**：异常静默记录，不弹窗。
**Debug 模式**：显示 MessageBox 供调试。

### 23.2 单实例保护

启动时通过 `Process.GetProcessesByName` 检测重复进程，自动 kill 旧实例。同时清理残留的 `IOMS3DViewer` 进程。

---

## 二十四、表单验证

项目使用**控件级自定义验证**（非 `IDataErrorInfo`/`INotifyDataErrorInfo`）：

```csharp
public class TextBox : System.Windows.Controls.TextBox, IDataInput {
    // 自定义验证函数（DependencyProperty，支持 XAML 绑定）
    public Func<string, OperationResult<bool>> VerifyFunc { get; set; }

    // 文本类型验证（手机号、邮箱、数字等）
    public TextType TextType { get; set; }

    public virtual bool VerifyData() {
        if (VerifyFunc != null)
            result = VerifyFunc.Invoke(Text);
        else if (TextType != TextType.Common)
            result = Text.IsKindOf(TextType) ? Success() : Failed("格式错误");
        // 自动设置 IsError、ErrorStr 状态
    }
}
```

- 触发时机：`LostFocus` + `TextChanged`
- ViewModel 保存时通过 `VisualHelper.GetChildren<TextBox>(uc)` 收集所有输入控件并调用 `VerifyData()`

---

## 二十五、Git 仓库管理

### 25.1 分支策略

| 分支 | 用途 |
|------|------|
| `master` | 主分支，稳定版本基线 |
| `release` | 发布分支，版本发布前的集成测试 |
| `CS6_2` | 当前主要开发分支 |
| `CS6` | 功能开发分支（CS6系列） |
| `dev` | 开发分支 |
| `hotfix` | 紧急修复分支 |
| `feature/*` | 功能特性分支（如 IntelligentLighting、LineFloodPrevention） |

### 25.2 提交规范

项目近期采用了**约定式提交（Conventional Commits）**规范：

```
feat(menu): 重构菜单状态管理为按一级菜单独立存储二级菜单
feat(flood): 实现防汛作战弹窗单实例协调机制
perf(browser): 浏览器控件修改
chore(release): 升级版本号至 1.7.0.0
```

早期提交以中文描述为主（如"开关站修改"、"视频联动"），后期逐步规范化。

### 25.3 代码托管

- 托管地址：内部 GitLab（`gjgit.pcitech.com`）
- 协议：SSH（端口 2222 / 29418）
- 仓库路径：`HJMos/cs6/frontend/hjmos_ncc_client`

---

## 二十六、近期重点工作（2025-2026）

基于最近 100 条提交记录分析：

### 26.1 防汛作战系统（新功能）
- 实现防汛消息提示弹窗和全屏作战页面
- 防汛作战弹窗单实例协调机制
- 增强防汛 H5 通信参数并优化全屏窗口交互
- 弹窗置顶功能与 F5 快捷键简化

### 26.2 数据看板扩展
- 新增客运管理、乘客画像模块
- 线网数据与 RDP 报表集成
- 车站能耗显示问题修复
- 指标表格与统计时段优化

### 26.3 开关站与模式控制
- 开关站仿真模式开发与优化
- 车站模式控制联调
- 开关站设备数据实体增加 DeviceType
- 自动项重试逻辑优化

### 26.4 菜单系统重构
- 重构菜单状态管理为按一级菜单独立存储二级菜单

### 26.5 综合监控集成
- 综合监控会话 ID 获取方法修改
- 接口重试方法优化
- 登录密码加密方式升级为 SHA256

---

## 二十七、构建与部署

### 27.1 构建命令

```bash
# 还原 NuGet 包
msbuild HJMos_NCC_Client_V1.0.sln /t:restore

# Release 模式构建
msbuild HJMos_NCC_Client_V1.0.sln /t:Build /m:1 /p:Configuration=Release

# 清理并构建（批处理）
.\Compile.bat

# 完整 Release 版本构建（含所有输出）
.\build(Release).bat
```

### 27.2 输出目录

构建产物输出到 `Work\Hjmos_Client` 目录。

### 27.3 自动更新

内置基于 FTP 的自动升级系统（`Hjmos.UpgradeApp`），支持应用内版本检测与增量更新。

---

## 二十八、项目特点总结

1. **大型业务系统**：4018 次提交、11 个功能模块、30+ 业务服务接口，覆盖地铁运营管控的全业务流程
2. **模块化架构**：基于 Prism 的模块化设计，支持按需加载，启动性能可控
3. **多层级管控**：线网 → 线路 → 车站三级视图，权限与功能按角色动态适配
4. **技术栈丰富**：融合 WPF、CefSharp、Unity 3D、WebSocket、RocketMQ 等多种技术
5. **持续演进**：项目运行 5 年以上，团队稳定，持续迭代新功能
6. **双框架支持**：同时兼容 .NET Framework 4.7.2 和 .NET 6.0-windows
