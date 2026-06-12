# 简历项目介绍 — 智慧地铁管控平台

---

## 项目基本信息

**项目名称：** 智慧地铁管控平台（HJMos NCC Client）

**项目周期：** 2020.11 ~ 至今

**项目规模：** 4000+ 次 Git 提交，11 个功能模块，30+ 业务服务接口，12 人研发团队

**技术栈：** WPF / .NET Framework 4.7.2 & .NET 6 / Prism MVVM / DryIoc / CefSharp / LibVLC / WebSocket / RocketMQ / ZeroC ICE / Unity 3D / LiveCharts

---

## 项目概述

智慧地铁管控平台是面向城市轨道交通运营集团的统一运营管控桌面系统，覆盖**线网—线路—车站**三级管控体系，为地铁调度中心与车站值班人员提供实时监控、应急指挥、视频巡检、设备管理、数据看板等一体化业务能力，支撑多条地铁线路的日常运营管控。

---

## 业务功能

### 全息感知（态势总览）

三级态势感知看板，以不同粒度呈现运营全景：

- **全网视图**：汇总所有线路运营状态、客流热力、告警分布
- **线路视图**：列车实时追踪、客流监测、防汛预警、屏蔽门状态、能源监控、广播/PIS 态势
- **车站视图**：开关站流程可视化、视频巡逻、清客管理、扶梯/卷帘门/站台门状态监控、智能照明、客流统计

### 应急指挥

双级应急体系，覆盖从线路调度到车站现场的全链路处置：

- **线路级**：告警实时监控、突发事件发布与跟踪处置、应急预案自动匹配、防汛指挥调度、区间/车站火灾响应、疏散方案可视化、视频联动（告警自动调取关联摄像头）
- **车站级**：现场告警处置、AFC/闸机/门禁联动控制、防汛作战流程、排水应急处置、火灾响应、通讯录调度

### 视频监控

支持三种后端接入方案（ISCS 媒体代理 / 直连 RTSP / ONVIF 协议），实现视频资源分类管理、多画面轮巡、全屏弹窗监看、视频巡检任务管理、视频 AI 分析事件展示。

### 数据看板

运营数据可视化平台，集成 LiveCharts 图表与 RDP 报表：

- 客运管理分析、乘客画像统计
- 能耗监控（线路级/牵引级/车站级，运营与非运营分拆统计）
- 线网汇总数据、公共看板与个人看板

### 车站运营（开关站 / 模式控制）

- **开关站流程**：自动项+手动项逐步执行，支持仿真模拟模式预演，设备执行失败自动重试，执行结果实时反馈
- **模式控制**：按预设模式批量下发设备控制指令（如节能模式、运营模式），支持联调验证

### 智能联动

跨模块消息推送与事件联动引擎，实现告警触发 → 视频调取 → 预案匹配 → 人员通知的自动化联动链条。

---

## 技术架构与实现

### 模块化插件架构

基于 **Prism 8 + DryIoc** 构建模块化应用框架，11 个业务模块实现 `IModule` 接口，通过 XML 配置声明式注册。采用**启动加载 + 按需加载**双策略，5 个核心模块启动时加载，其余模块在首次导航时动态加载，控制启动耗时。

```
HjmosApplication (PrismApplication)
  ├── ConfigurationModuleCatalog → XML 模块目录
  ├── RegisterTypes() → 40+ 单例服务注册
  └── OnInitialized() → 菜单构建 + 权限过滤
```

### 自研 Region 导航框架

替代 Prism 原生 `RequestNavigate`，设计**视图单例缓存 + 手动激活**机制：视图首次导航时从 DI 容器创建并缓存到 Region，后续导航直接激活已有实例，避免重复创建开销。支持 `isReload` 强制重建、`SendMsgToChildren` 可视化树广播、`IRegionCallBackAware` 父子视图双向通信。

```csharp
public void ShowView(string regionName, Type viewType, IRegionParameters parameters) {
    var obj = region.GetView(viewType.FullName)          // 缓存查找
             ?? ContainerLocator.Container.Resolve(viewType); // 容器创建
    region.Activate(obj);
    (vm as IRegionAware)?.OnViewActived(parameters);     // 通知 ViewModel
}
```

### 多层级权限体系

基于 `[Flags] RegionType { 线网=1, 线路=2, 车站=4 }` 标志枚举实现三级权限过滤。菜单项通过 `[Resource]` 和 `[Menu]` 自定义 Attribute 声明元数据（名称、DLL 路径、作用域），运行时根据用户角色 `RoleType` 位运算动态过滤可见菜单。UI 层通过 `AuthorityToEnableConverter` 转换器绑定实现声明式权限控制。

### 混合数据通信层

针对不同业务场景设计四种通信通道：

| 通道 | 技术方案 | 应用场景 |
|------|---------|---------|
| **REST API** | RestSharp 封装 + OAuth2 Token 自动刷新（401 拦截重试） | 业务数据查询、设备控制指令 |
| **WebSocket** | WebSocketSharp + 295s 心跳 + 线性重连 | 实时数据推送（设备状态、列车位置） |
| **RocketMQ** | 阿里云 ONS C++ SDK（SWIG P/Invoke 绑定），广播模式 | 跨工作站消息同步（告警广播、联动指令） |
| **ZeroC ICE** | TCP RPC + 回调注册 + 心跳保活 | 与综合监控系统（ISCS）集成，数据点位订阅 |

### WPF ↔ H5 混合渲染方案

通过 **CefSharp (Chromium)** 嵌入 H5 页面，设计双向通信桥梁：

- **WPF → H5**：`ExecuteScriptAsync()` 推送数据变更，触发 JS 回调函数
- **H5 → WPF**：`JavascriptObjectRepository.Register()` 注册 .NET 对象为 JS 全局变量，H5 直接调用 WPF 方法
- **双向桥**：`IJavascriptCallback` 实现 H5 传递 JS 函数引用给 .NET，.NET 异步回调 H5
- **认证透传**：Cookie 注入 `mosToken` + `hjmos-authorization` 请求头，实现 SSO 无感切换

### Unity 3D 进程嵌入

以独立进程方式运行 Unity 3D 车站视图，通过 Win32 `SetParent` 嵌入 WPF 窗口。设计焦点轮询机制（300ms 定时器 + `WM_ACTIVATE` 消息）防止 Unity 抢夺键盘焦点。WPF 与 Unity 间通过 **PESocket TCP**（`127.0.0.1:9968`）进行 IPC 通信，支持全屏切换、设备详情展示、Token 刷新推送等指令。

### 视频播放系统

基于 **LibVLCSharp** + **System.Reactive** 构建响应式视频播放管道：

```
播放指令 → Subject<T>.Throttle(1000ms).DistinctUntilChanged()
        → ObserveOnDispatcher → MediaPlayer.Play()
        → 5s DispatcherTimer KeepAlive(sessionId)
```

支持三种后端（ISCS 媒体代理 / RTSP 直连 / ONVIF），通过配置项无代码切换。集成 PTZ 云台控制（上下左右、变焦）。

### 自定义控件库与 DPI 适配

构建 **100+ WPF 自定义控件**（`Hjmos.SharedStyle`），涵盖输入、展示、布局、导航、对话框、浮层、嵌入等类别。设计**三层 DPI 适配方案**：Win32 `GetDeviceCaps` → `WpfScreenHelper` 屏幕识别 → `Resolution_2K/4K.xaml` 资源字典 + 响应式 Markup Extension，支持 2K/4K 多分辨率自适应。

### 安全体系

- **认证**：OAuth2 Password Grant + Token 401 自动刷新重试
- **加密**：AES-CBC（登录凭证持久化，密钥为主板序列号）、RSA PKCS#1（登录密码传输）、SHA256（密码哈希）
- **权限**：声明式 `[Resource]` Attribute + `IAuthorityService` 权限树 + UI 转换器绑定

### 其他技术要点

- **事件总线**：封装 Prism EventAggregator，枚举键字典管理 + 弱引用订阅防内存泄漏 + `IsActive` 生命周期自动订阅/取消
- **对话框服务**：支持标准对话框与多线程独立线程对话框（防重复打开机制）
- **自动升级**：FTP 增量升级，MD5 差异比对 → 下载差异文件 → 备份原文件 → 替换 → 失败自动回滚
- **全局异常**：三层捕获（Dispatcher / TaskScheduler / AppDomain），Release 静默日志 + Debug 弹窗
- **IPC 框架**：自研泛型 Named Pipe 框架，长度前缀协议 + GUID 管道分配 + 自动重连

---

## 项目亮点

1. **大规模模块化 WPF 应用**：11 个 Prism 模块按需加载，40+ 服务接口依赖注入，架构清晰可扩展
2. **异构系统集成**：REST + WebSocket + RocketMQ + ICE RPC 四通道并存，统一封装为服务层接口，业务代码与通信细节解耦
3. **混合渲染体验**：WPF 原生控件 + CefSharp H5 页面 + Unity 3D 视图三种渲染技术共存，通过 IPC 和 JS Bridge 实现无缝交互
4. **生产级可靠性**：Token 自动刷新、WebSocket 心跳重连、MQ 广播容错、三层异常捕获、FTP 升级回滚，多重机制保障系统稳定运行
5. **多分辨率适配**：从控件层到资源层到屏幕层的全链路 DPI 方案，一套代码适配 1080P / 2K / 4K 显示器
