# CEF（Chromium）混合渲染方案详细设计文档

> **项目名称**: MICS Client（城市轨道交通综合监控系统客户端）
> **技术栈**: Avalonia UI + Xilium.CefGlue (Chromium 120) + ASP.NET Core Minimal API + YARP + WebSocket
> **运行环境**: .NET 10 / Windows + Linux（麒麟 V10）

---

## 一、架构总览

### 1.1 系统架构拓扑图

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                        MICS Client 桌面应用 (Avalonia)                        │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐  │
│  │                   LocalBasedWindow (4 × CefView)                       │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────────┐  │  │
│  │  │  TopView   │  │  MapView   │  │ ContentView│  │  BottomView    │  │  │
│  │  │  (H5/Web)  │  │ (Native)   │  │  (Hybrid)  │  │  (Native)      │  │  │
│  │  └────────────┘  └────────────┘  └────────────┘  └────────────────┘  │  │
│  └───────────────────────────────┬────────────────────────────────────────┘  │
│                                  │                                           │
│              ┌───────────────────┼───────────────────┐                       │
│              ▼                   ▼                   ▼                       │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │          内嵌 ASP.NET Core (YARP 反向代理 + Minimal API)              │   │
│  │          监听: http://localhost:{ProxyPort}                           │   │
│  │                                                                      │   │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │   │
│  │  │  Minimal API     │  │  WebSocket       │  │  YARP 反向代理   │  │   │
│  │  │  /ClientInternal │  │  /ClientInternal │  │  → MICS Server   │  │   │
│  │  │  /* (本地桥接)    │  │  /notification   │  │  (多站点负载均衡) │  │   │
│  │  └──────────────────┘  └──────────────────┘  └──────────────────┘  │   │
│  └───────────────────────────────┬──────────────────────────────────────┘   │
│                                  │ HTTP / WebSocket                         │
└──────────────────────────────────┼───────────────────────────────────────────┘
                                   │
                                   ▼
              ┌──────────────────────────────────────────┐
              │  MicsClient.Server (ASP.NET Core)         │
              │  多实例部署: XDZ / OCC / TWP / ...         │
              │                                          │
              │  • 静态文件: /pages/scada/HMI/*           │
              │  • 组态页面: /pages/h5/*                  │
              │  • Minimal API + WebSocket               │
              │  • Blazor SSR (运维管理)                   │
              └──────────────────────────────────────────┘
```

### 1.2 核心设计理念

本方案的核心设计理念是**"一个控件，两种渲染"**：

- **CefView** 作为通用渲染容器，通过 `Decorator.Child` 的动态替换，实现原生 Avalonia 视图与嵌入式 Chromium 浏览器页面的**无缝切换**
- **内嵌 ASP.NET Core** 作为本地中间层，同时承担 YARP 反向代理和 Minimal API 本地桥接服务双重角色
- **LocalWebSocket** 通过 `/ClientInternal/notification` 端点构建 Native→H5 的服务端推送通道
- **CEF 请求头注入** 通过 `MicsResourceRequestHandler` 在每个浏览器请求中注入 `mics-winId` 标识，实现 H5→Native 的身份识别

---

## 二、CefView 控件 — 混合渲染核心

### 2.1 XAML 定义

`CefView` 是一个自定义 Avalonia `UserControl`，其 XAML 定义极其简洁——仅包含一个 `Decorator` 作为动态内容容器：

```xml
<!-- MicsClient/Views/BaseView/CefView.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MicsClient.Views.CefView">
    <Decorator x:Name="browserWrapper"/>
</UserControl>
```

> **为什么使用 `Decorator`？** `Decorator` 是 Avalonia 中 `Child` 属性的持有者，允许我们在运行时任意替换其子控件，而无需关心布局容器的约束。这使得原生 `Control` 和 `AvaloniaCefBrowser` 可以被等价地放入和取出。

### 2.2 依赖属性：Url 驱动渲染

`CefView` 暴露 `Url` 和 `WebTitle` 两个直接属性（DirectProperty），外部通过设置 `Url` 即可驱动内容切换：

```csharp
// MicsClient/Views/BaseView/CefView.axaml.cs

private string? _url;
public string? Url
{
    get => _url;
    set
    {
        SetAndRaise(UrlProperty, ref _url, value);
        LoadPage(value);  // ← Url 变化时立即触发页面加载
    }
}

private string? _webTitle;
public string? WebTitle
{
    get => _webTitle;
    set => SetAndRaise(WebTitleProperty, ref _webTitle, value);
}
```

### 2.3 核心逻辑：LoadPage() — 原生/Web 无缝切换

`LoadPage()` 是混合渲染的核心方法。它根据传入的 `url` 参数进行路由匹配，决定渲染原生 Avalonia 控件还是 Chromium 浏览器页面：

```csharp
private void LoadPage(string? url)
{
    // ──── 第一步：原生控件路由匹配 ────
    Control? control = url switch
    {
        // 控制栏
        ManagerControlNames.ControlBar => new ControlBarView(),
        ManagerControlNames.BasControlBar => new BASControlBarView(),

        // 报警管理
        ManagerControlNames.AlarmBanner => new AlarmBarView(),
        ManagerControlNames.AlarmManager => new AlarmManagerView(),
        ManagerControlNames.HistAlarmManager => new HistoryAlarmManagerView(),
        ManagerControlNames.RealTimeAlarmManager => new RealTimeAlarmManagerView(),
        ManagerControlNames.AlarmScopeFilter => new AlarmScopeFilterView(),
        ManagerControlNames.AlarmAudioSettings => new AlertSettingView(),

        // 事件管理
        ManagerControlNames.HistEventManager => new HistoryEventQueryView(),
        ManagerControlNames.RealTimeEventManager => new RealTimeEventQueryView(),

        // 权限/视频/趋势/PA/PIS...
        ManagerControlNames.RightDutyManage => new RightDutyManage(),
        ManagerControlNames.CctvManager => new CctvManagerView(),
        ManagerControlNames.PAManager => new PaManagerView(),
        ManagerControlNames.PISManager => new PisManagerView(),
        ManagerControlNames.TrendManager => new TrendView(),
        ManagerControlNames.SoeManager => new SoeManagerView(),
        // ... 约 30 种原生控件
        _ => null
    };

    if (control is not null)
    {
        // ✅ 原生路径：直接替换为 Avalonia 控件
        browserWrapper.Child = control;
        return;
    }

    // ──── 第二步：Web 页面路径 ────
    if (!string.IsNullOrWhiteSpace(url))
    {
        EnsureBrowser();   // 延迟创建 CEF 浏览器实例
        browser!.Address = UrlResolver.GetProxyUrl(url);  // URL 映射
        browserWrapper.Child = browser;
    }
}
```

**切换流程示意**：

```
Url 值变更
    │
    ├─ 匹配原生控件名？ ──YES──→ browserWrapper.Child = Avalonia Control
    │                              (无 CEF 开销，纯原生渲染)
    │
    └─ 否（Web URL） ──────────→ EnsureBrowser() → 创建 CEF 实例（首次）
                                   │
                                   └→ browser.Address = proxyUrl
                                      browserWrapper.Child = AvaloniaCefBrowser
                                      (Chromium 渲染 Web 组态画面)
```

### 2.4 延迟初始化：EnsureBrowser()

为避免仅为原生控件的 `CefView` 创建不必要的 Chromium 实例，采用**懒加载**策略：

```csharp
private void EnsureBrowser()
{
    if (browser != null) return;  // 已创建则跳过

    browser = new AvaloniaCefBrowser();

    // 注册 CEF 浏览器处理器
    browser.RequestHandler = new MicsRequestHandler(this);       // HTTP 请求拦截
    browser.LifeSpanHandler = new MicsBrowserLiftSpanHandler(this); // 弹出窗口处理
    browser.ContextMenuHandler = new MicsContextMenuHandler();    // 右键菜单禁用
    browser.KeyboardHandler = new MicsKeyBoardHandler();          // F5 刷新

    // 页面标题变化 → 绑定到窗口标题
    browser.TitleChanged += (sender, args) =>
    {
        if (!string.IsNullOrWhiteSpace(args))
            Dispatcher.UIThread.Invoke(() => WebTitle = args);
    };

    browser.LoadEnd += Browser_LoadEnd;
    browserWrapper.Child = browser;
}
```

### 2.5 CEF 浏览器处理器

`CefView` 内定义了 5 个密封内部类作为 CEF 处理器，每个处理器职责明确：

#### (1) MicsResourceRequestHandler — 请求头注入（核心桥接机制）

```csharp
protected sealed class MicsResourceRequestHandler(CefView view) : CefResourceRequestHandler
{
    private CefView _view = view;

    protected override CefReturnValue OnBeforeResourceLoad(
        CefBrowser browser, CefFrame frame, CefRequest request, CefCallback callback)
    {
        // 🔑 核心：在每个浏览器发出的 HTTP 请求中注入窗口标识
        // 服务端通过此标识识别请求来源窗口，实现多窗口隔离
        request.SetHeaderByName(Header.WinId, _view.WinId.ToString(), true);
        return base.OnBeforeResourceLoad(browser, frame, request, callback);
    }
}
```

#### (2) MicsBrowserLiftSpanHandler — 弹出窗口处理

```csharp
protected sealed class MicsBrowserLiftSpanHandler(CefView view) : LifeSpanHandler
{
    protected override bool OnBeforePopup(
        CefBrowser browser, CefFrame frame, string targetUrl, string targetFrameName,
        CefWindowOpenDisposition targetDisposition, bool userGesture,
        CefPopupFeatures popupFeatures, CefWindowInfo windowInfo,
        ref CefClient client, CefBrowserSettings settings,
        ref CefDictionaryValue extraInfo, ref bool noJavascriptAccess)
    {
        // 拦截 window.open() 弹窗，转为原生 PopupWebWindow
        Dispatcher.UIThread.Post(() =>
        {
            WeakReferenceMessenger.Default.Send(
                new PopupWindowMessage(new PopupWinContent()
                {
                    Url = targetUrl,
                    WindowId = _view.WinId,
                }));
        });
        return true; // 阻止 CEF 默认弹窗行为
    }
}
```

#### (3) MicsContextMenuHandler — 右键菜单禁用

```csharp
protected sealed class MicsContextMenuHandler : ContextMenuHandler
{
    protected override void OnBeforeContextMenu(
        CefBrowser browser, CefFrame frame, CefContextMenuParams state, CefMenuModel model)
    {
        model.Clear(); // 清空右键菜单项
    }
}
```

#### (4) MicsKeyBoardHandler — F5 刷新

```csharp
protected sealed class MicsKeyBoardHandler : KeyboardHandler
{
    protected override bool OnKeyEvent(CefBrowser browser, CefKeyEvent keyEvent, nint osEvent)
    {
        if (keyEvent.WindowsKeyCode == 116 && keyEvent.EventType == CefKeyEventType.RawKeyDown)
        {
            browser.ReloadIgnoreCache(); // F5 = 强制刷新（忽略缓存）
        }
        return false;
    }
}
```

#### (5) Browser_LoadEnd — 错误页面处理

```csharp
private void Browser_LoadEnd(object sender, LoadEndEventArgs args)
{
    if (args.HttpStatusCode >= 400 && args.Frame.IsMain)
    {
        var failedUrl = args.Frame.Url ?? string.Empty;
        var configUrl = failedUrl.Split("pages").Last().Split('?').First();
        string errorHtml = $@"
        <html>
        <body style='font-family: Arial; text-align: center; padding-top: 50px;'>
            <h2>页面加载失败</h2>
            <p>页面资源: <strong>{configUrl}</strong></p>
            <p>加载URL: <strong>{failedUrl}</strong></p>
            <p>错误码: {(int)args.HttpStatusCode}</p>
        </body>
        </html>";
        var dataUrl = "data:text/html;charset=utf-8," + Uri.EscapeDataString(errorHtml);
        args.Frame.LoadUrl(dataUrl);
    }
}
```

### 2.6 生命周期管理

```csharp
// 从可视化树移除时，释放 CEF 浏览器资源
protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    if (_isActivatedHandlerSubscribed)
    {
        var pWin = TopLevel.GetTopLevel(this);
        if (pWin is MicsHostWindow w)
        {
            w.Activated -= W_Activated;
            _isActivatedHandlerSubscribed = false;
        }
    }
    if (browser != null)
    {
        browser.Dispose();
        browser = null;
    }
}
```

---

## 三、LocalBasedWindow — 四区域布局

### 3.1 窗口布局结构

`LocalBasedWindow` 是主操作窗口，通过 4 个 `CefView` 构成经典的"顶栏 + 导航栏 + 内容区 + 底栏"布局：

```xml
<!-- MicsClient/Views/BaseView/LocalBasedWindow.axaml -->
<Window x:Class="MicsClient.Views.LocalBasedWindow"
        Title="{Binding #ContentView.WebTitle}">
    <Grid Name="ViewContent">
        <Grid.RowDefinitions>
            <RowDefinition Height="{Binding TopHeight, Converter={StaticResource Double2GridL}}"/>
            <RowDefinition Height="{Binding OptionBarHeight, Converter={StaticResource Double2GridL}}"/>
            <RowDefinition Height="{Binding ContentHeight, Converter={StaticResource Double2GridL}}"/>
            <RowDefinition Height="{Binding BottomHeight, Converter={StaticResource Double2GridL}}"/>
        </Grid.RowDefinitions>

        <views:CefView Name="TopView"     Grid.Row="0" Url="{Binding TopUrl}"/>
        <views:CefView Name="MapView"     Grid.Row="1" Url="{Binding OptionBarUrl}"/>
        <views:CefView Name="ContentView" Grid.Row="2" Url="{Binding ContentUrl}"/>
        <views:CefView Name="BottomView"  Grid.Row="3" Url="{Binding BottomUrl}"/>
    </Grid>
</Window>
```

每个 `CefView` 独立绑定一个 `Url`，ViewModel 通过修改 URL 值即可实现任意区域的原生/Web 切换。例如：

- `TopUrl` → 通常指向 H5 顶栏页面（Web 渲染）
- `OptionBarUrl` → 导航地图/菜单栏（原生控件）
- `ContentUrl` → 核心组态画面（Web 渲染）或报警管理等（原生控件）
- `BottomUrl` → 状态栏（原生控件）

### 3.2 窗口身份标识

每个 `LocalBasedWindow` 实例拥有唯一 `WinId`，通过 `MicsHostWindow` 抽象基类暴露给 `CefView` 使用：

```csharp
public partial class LocalBasedWindow : MicsHostWindow
{
    private Guid WinId = Guid.NewGuid();

    public override Guid GetWinId() => WinId;

    public override void ChangeContent(string url)
    {
        ContentView.Url = url;  // 动态切换内容区页面
    }

    public override void ChangeLocation(int locationId)
    {
        this.locationId = locationId;  // 切换监控站点
    }
}
```

---

## 四、CEF 初始化与配置

### 4.1 CEF 运行时初始化

CEF 在 `App.axaml.cs` 的 `SetupCEF()` 中完成初始化，是整个启动序列的最后一步：

```csharp
// MicsClient/App.axaml.cs

private static async Task InitComplexComponents()
{
    SetupProxy();                // 1. 启动内嵌 ASP.NET Core + YARP
    SetupHttpClient();           // 2. 配置 HTTP 客户端 → localhost:{ProxyPort}
    SetupWebsocketClient();      // 3. 配置 WS 客户端 → ws://localhost:{ProxyPort}
    await SetupMicsWindowDispatcher();  // 4. 窗口管理
    SetupPlanService();          // 5. 预案服务
    SetupNotificationService();  // 6. 通知服务
    SetupAlarmAlert();           // 7. 报警声音
    InjectViewModel();           // 8. ViewModel 注入
    SetupCEF();                  // 9. CEF 初始化（最后执行）
}

private static void SetupCEF()
{
    var cefFlags = new Dictionary<string, string>
    {
        { "disable-web-security", "true" },       // 禁用 CORS 限制
        { "remote-allow-origins", "*" },           // 允许所有源
        { "process-per-site", "true" },            // 同站点共享渲染进程
    };

    var cefsetting = new CefSettings()
    {
        NoSandbox = true,                           // 禁用沙箱（嵌入式场景）
        CommandLineArgsDisabled = false,
        RemoteDebuggingPort = 13229,                // DevTools 远程调试端口
        CachePath = Path.Combine(Environment.CurrentDirectory, "CEFCache"),
        Locale = "zh-CN"
    };

    // Linux（麒麟 V10）特殊配置
    if (OperatingSystem.IsLinux())
    {
        cefsetting.WindowlessRenderingEnabled = true;  // 离屏渲染模式
        cefFlags.Add("disable-seccomp-filter-sandbox", "true");
        cefFlags.Add("disable-gpu", "true");           // 禁用 GPU 加速
    }

    CefRuntimeLoader.Initialize(cefsetting, cefFlags.ToArray());
}
```

### 4.2 NuGet 依赖

```xml
<!-- MicsClient.csproj -->
<PackageReference Include="CefGlue.Avalonia" Version="120.6099.210" />
```

> Linux 部署时使用项目内的自定义构建版本 `MicsClient.Linux/CustomizedLibs/Xilium.CefGlue.Avalonia.dll`。

---

## 五、URL 解析与代理映射

### 5.1 UrlResolver — URL 转换

所有 Web 页面的 URL 在加载前需经过 `UrlResolver` 的映射，将逻辑路径 `MicsServer/...` 替换为本地代理地址：

```csharp
// MicsClient/Common/MicsConfigurations.cs

public static class UrlResolver
{
    private static int proxyPort;
    private static string proxyUrl = string.Empty;

    public static string GetProxyUrl(string url)
    {
        if (proxyPort == 0)
        {
            var config = ServiceManager.Locator.GetRequiredService<IConfiguration>();
            proxyPort = config.GetValue<int>("ProxyPort");
            proxyUrl = $"http://localhost:{proxyPort}";
        }
        // "MicsServer/pages/scada/HMI/XDZ/PSD/Overall.html"
        // → "http://localhost:15024/pages/scada/HMI/XDZ/PSD/Overall.html"
        return url.Replace("MicsServer", proxyUrl);
    }
}
```

### 5.2 统一请求头规范

所有通过 CEF 发出的请求都会自动携带 `mics-winId` 头（由 `MicsResourceRequestHandler` 注入），而 YARP 代理在转发请求时会追加更多身份标识：

```csharp
// MicsClient/App.axaml.cs

public class ClientProxyHeaderTransform : RequestTransform
{
    SessionConfiguration session = ServiceManager.Locator.GetRequiredService<SessionConfiguration>();
    MicsConfigurations micsConfigurations = ServiceManager.Locator.GetRequiredService<MicsConfigurations>();

    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        // 追加会话身份头
        context.ProxyRequest.Headers.Add(Header.Console,
            ClientRequestHelper.GetUrlEncodeString(session.ConsoleName));
        context.ProxyRequest.Headers.Add(Header.OperatorId,
            ClientRequestHelper.GetEncyptContent(session.OperatorId));
        context.ProxyRequest.Headers.Add(Header.OperatorName,
            ClientRequestHelper.GetUrlEncodeString(session.OperatorName));
        context.ProxyRequest.Headers.Add(Header.Session,
            ClientRequestHelper.GetEncyptContent(session.SessionId));
        context.ProxyRequest.Headers.Add(Header.ProfileId,
            ClientRequestHelper.GetEncyptContent(session.ProfileId));

        // 请求体签名追踪
        if (context.ProxyRequest.Content != null)
        {
            var body = await context.ProxyRequest.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(body))
            {
                var pretracking = SHA256.GetSha256(body, session.ConsoleName);
                context.ProxyRequest.Headers.Add(Header.Tracking,
                    ClientRequestHelper.GetEncyptContent(pretracking));
            }
        }
    }
}
```

**完整请求头定义**：

```csharp
// SharedLib/MicsClient.SharedWebModel/Header.cs

public static class Header
{
    public const string ScreenId     = "mics-screenid";
    public const string WinId        = "mics-winId";        // 窗口标识 (CEF注入)
    public const string Session      = "mics-sessionid";    // 会话ID
    public const string OperatorId   = "mics-operatorid";   // 操作员ID
    public const string OperatorName = "mics-operatorName"; // 操作员名
    public const string Console      = "mics-consolename";  // 终端名
    public const string Tracking     = "mics-trackingid";   // 请求追踪签名
    public const string RequestType  = "mics-request";      // 请求类型（路由分流用）
    public const string ProfileId    = "mics-profileid";    // 角色配置ID
    public const string PlaceType    = "mics-place";        // OCC/车站标识
}
```

---

## 六、内嵌 ASP.NET Core + YARP 反向代理

### 6.1 本地服务启动

客户端进程内嵌一个完整的 ASP.NET Core Web 应用，同时承载 Minimal API 本地接口和 YARP 反向代理：

```csharp
// MicsClient/App.axaml.cs → SetupProxy()

private static void SetupProxy()
{
    var builder = WebApplication.CreateSlimBuilder();

    builder.Configuration
        .SetBasePath(Global.GetInstalledPath())
        .AddJsonFile("appsettings.json", true, reloadOnChange: true);

    // 配置监听端口
    var port = builder.Configuration.GetValue<int>("ProxyPort");
    builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));

    // 跨域策略（无限制）
    builder.Services.AddCors(options =>
        options.AddPolicy("MicsPolicy", builder => builder.AllowAnyOrigin()));

    builder.Services.AddSingleton<ClientProxyHeaderTransform>();

    // ──── YARP 反向代理配置 ────
    builder.Services.AddReverseProxy()
        .LoadFromMemory(
            ProxyConfigurations.GetRouteConfigs(),
            ProxyConfigurations.GetClusterConfigs(port))   // 内存路由（站点代理）
        .LoadFromConfig(builder.Configuration.GetSection("ServerProxy")) // 外部配置代理
        .AddTransforms(transforms =>                        // 请求头统一注入
        {
            transforms.AddRequestTransform(async context =>
            {
                var transform = context.HttpContext.RequestServices
                    .GetRequiredService<ClientProxyHeaderTransform>();
                await transform.ApplyAsync(context);
            });
        });

    // ──── 本地 Minimal API ────
    builder.Services.AddLocalService();

    var app = builder.Build();

    app.UseCors();
    app.UseLocalService();     // 注册本地 API 路由
    app.MapReverseProxy();     // 启用反向代理

    app.RunAsync();
}
```

### 6.2 YARP 路由策略

`ProxyConfigurations` 生成三类路由规则，实现多站点智能代理：

```csharp
// MicsClient/Common/ProxyConfigurations.cs

public static IReadOnlyList<RouteConfig> GetRouteConfigs()
{
    var list = new List<RouteConfig>();
    var hosts = HostConfigsReader.ReadHostConfigs();

    // ──── 类型 1: 车站级路由 ────
    // /XDZ/{**remainder} → XDZ 集群
    foreach (var host in hosts)
    {
        if (!list.Any(r => r.RouteId == host.LocationName))
        {
            list.Add(new RouteConfig()
            {
                RouteId = host.LocationName,
                ClusterId = host.LocationName,
                CorsPolicy = "MicsPolicy",
                Match = new RouteMatch()
                {
                    Path = $"/{host.LocationName}/{{**remainder}}",
                    Headers = [new RouteHeader()
                    {
                        Name = Header.RequestType,
                        Mode = HeaderMatchMode.NotExists  // 排除本地请求
                    }]
                }
            }.WithTransformPathRemovePrefix($"/{host.LocationName}"));
        }
    }

    // ──── 类型 2: 服务器级路由 ────
    // /MICS-XDZ-SCADA/{**remainder} → MICS-XDZ-SCADA 集群

    // ──── 类型 3: 本站默认路由（兜底） ────
    // {**catch-all} → LocalStation 集群
    list.Add(new RouteConfig()
    {
        RouteId = "LocalStation",
        ClusterId = GetLocationName(config.LocationId),
        Match = new RouteMatch()
        {
            Path = $"{{**catch-all}}",
            Headers = [new RouteHeader()
            {
                Name = Header.RequestType,
                Mode = HeaderMatchMode.NotExists
            }]
        }
    });

    // ──── 类型 4: 内部服务路由 ────
    // 带 mics-request: local 头的请求 → 本地 Minimal API
    list.Add(new RouteConfig()
    {
        RouteId = "InternalRoute",
        ClusterId = "InternalService",
        Match = new RouteMatch
        {
            Path = "{**catch-all}",
            Headers = [new RouteHeader()
            {
                Name = Header.RequestType,
                Values = ["local"],
                Mode = HeaderMatchMode.ExactHeader
            }]
        }
    }.WithTransformRequestHeader("Mics-Request", "RedirectedLocal"));

    return list;
}
```

**集群配置**：

```csharp
public static IReadOnlyList<ClusterConfig> GetClusterConfigs(int localPort)
{
    var list = new List<ClusterConfig>();

    // 车站集群（带健康检查）
    foreach (var staHost in hosts.GroupBy(h => h.LocationName))
    {
        list.Add(new ClusterConfig()
        {
            ClusterId = staHost.Key,
            Destinations = staDes,
            HealthCheck = new HealthCheckConfig()
            {
                Active = new ActiveHealthCheckConfig()
                {
                    Enabled = true,
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(5),
                    Policy = "ConsecutiveFailures"
                }
            }
        });
    }

    // 本地内部服务集群
    list.Add(new ClusterConfig()
    {
        ClusterId = "InternalService",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            { "InternalServicePath",
              new DestinationConfig()
              { Address = $"http://localhost:{localPort}/ClientInternal" } }
        }
    });

    return list;
}
```

---

## 七、Minimal API — Native↔H5 本地桥接

### 7.1 本地 API 端点注册

`LocalServiceExtension` 定义了一组 `/ClientInternal/*` 前缀的 Minimal API 端点，供 Web 组态页面（H5）调用原生功能：

```csharp
// MicsClient/HttpServices/LocalServiceExtension.cs

private static void MapRoute(WebApplication app)
{
    // ──── 站点管理 ────
    app.MapPost("/ClientInternal/changeLoc",
        (ChangeLocReq req, HttpContext context, [FromServices] LocalService service) =>
        service.ChangeLoc(req, context)).WithName("选择站点");

    app.MapGet("/ClientInternal/currLoc",
        (HttpContext context, [FromServices] LocalService service) =>
        service.GetCurrLoc(context)).WithName("查询当前站点");

    app.MapGet("/ClientInternal/monitorLoc",
        (HttpContext context, [FromServices] LocalService service) =>
        service.GetMonitorLoc(context)).WithName("查询监视站点");

    // ──── 菜单导航 ────
    app.MapPost("/ClientInternal/callmenu",
        (CallMenuReq req, HttpContext context, [FromServices] LocalService service) =>
        service.CallMenu(req, context)).WithName("切换菜单");

    // ──── 弹窗与面板 ────
    app.MapPost("/ClientInternal/popup",
        (PopupReq req, HttpContext context, [FromServices] LocalService service) =>
        service.PopupWindow(req, context)).WithName("弹出窗口");

    app.MapPost("/ClientInternal/inspectorPanel",
        (InspectorPanelReq req, HttpContext context, [FromServices] LocalService service) =>
        service.CallInspectorPanel(req, context)).WithName("打开监控面板");

    // ──── 报警声音 ────
    app.MapPost("/ClientInternal/setAlert", ...).WithName("设置报警声音");
    app.MapGet("/ClientInternal/alertStatus", ...).WithName("查询当前报警声音");
    app.MapPost("/ClientInternal/startAlert", ...).WithName("触发报警");
    app.MapPost("/ClientInternal/stopAlert", ...).WithName("停止报警");

    // ──── 视频监控 ────
    app.MapPost("/ClientInternal/cctvDisplay", ...).WithName("调用视频弹窗");
    app.MapPost("/ClientInternal/tvWallPlay", ...).WithName("视频上墙");

    // ──── WebSocket 通知通道 ────
    app.MapGet("/ClientInternal/notification", async (HttpContext context) =>
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var websocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketFinishedTcs = new TaskCompletionSource();
            var application = context.RequestServices
                .GetRequiredService<LocalWebSocketService>();
            application.AddConnection(websocket,
                $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}",
                socketFinishedTcs);
            await socketFinishedTcs.Task;
        }
    });

    // ──── 服务器信息 ────
    app.MapGet("/ClientInternal/serverList", ...).WithName("获取服务器列表");
}
```

> 每个端点均支持 `/ClientInternal/{loc}/*` 的带站点前缀变体，以兼容多站点 URL 格式。

### 7.2 LocalService — 桥接逻辑实现

每个 Minimal API 端点背后是 `LocalService`，它负责将 H5 的 HTTP 请求转换为原生 MVVM 消息：

```csharp
// MicsClient/HttpServices/LocalService.cs

public class LocalService
{
    /// <summary>
    /// H5 → Native: 切换监控站点
    /// H5 页面调用 POST /ClientInternal/changeLoc
    /// → 发送 ChangeLocationMessage 给原生窗口
    /// </summary>
    public Resp<ChangeLocAck> ChangeLoc(ChangeLocReq changeLocReq, HttpContext httpContext)
    {
        var winId = httpContext.Request.GetRequestWinId();  // 从 CEF 注入的请求头获取窗口ID
        if (Guid.TryParse(winId, out Guid wId))
        {
            // 通过 MVVM 消息总线通知原生窗口切换站点
            WeakReferenceMessenger.Default.Send(
                new ChangeLocationMessage(new CurrentLocationContent()
                {
                    Location = new Location()
                    {
                        Pkey = changeLocReq.locationId,
                        Description = changeLocReq.description,
                        Name = changeLocReq.locationName
                    },
                    WindowId = wId
                }));

            return Resp<ChangeLocAck>.SuccessResult(
                new ChangeLocAck { ... }, winId);
        }
        return Resp<ChangeLocAck>.FailResult(winId, "无对应窗口");
    }

    /// <summary>
    /// H5 → Native: 切换菜单/页面
    /// </summary>
    public Resp<CallMenuAck> CallMenu(CallMenuReq callMenuReq, HttpContext httpContext)
    {
        var winId = httpContext.Request.GetRequestWinId();
        WeakReferenceMessenger.Default.Send(
            new ChangePageMessage(new PageContent
            {
                Info = callMenuReq.info,
                WindowId = Guid.Parse(winId)
            }));
        return Resp<CallMenuAck>.SuccessResult(...);
    }

    /// <summary>
    /// H5 → Native: 弹出原生窗口
    /// </summary>
    public Resp<PopupAck> PopupWindow(PopupReq popupReq, HttpContext httpContext)
    {
        var winId = httpContext.Request.GetRequestWinId();
        WeakReferenceMessenger.Default.Send(
            new PopupWindowMessage(new PopupWinContent
            {
                Info = popupReq.info,
                WindowId = Guid.Parse(winId),
                height = popupReq.height,
                width = popupReq.width,
                title = popupReq.title
            }));
        return Resp<PopupAck>.SuccessResult(...);
    }

    /// <summary>
    /// H5 → Native: 打开监控面板
    /// </summary>
    public Resp<InspectorPanelAck> CallInspectorPanel(
        InspectorPanelReq callPanelReq, HttpContext httpContext)
    {
        var winId = httpContext.Request.GetRequestWinId();
        WeakReferenceMessenger.Default.Send(new CallInspMessage(new CallInspContent()
        {
            deviceName = callPanelReq.deviceName,
            index = callPanelReq.index,
            x = callPanelReq.x,
            y = callPanelReq.y,
            WindowId = Guid.Parse(winId)
        }));
        return Resp<InspectorPanelAck>.SuccessResult(...);
    }
}
```

### 7.3 通信流程（H5 → Native）

```
H5 组态页面 (运行在 CEF 中)
    │
    │  fetch("/ClientInternal/changeLoc", { method: "POST", body: ... })
    │  ↑ 浏览器自动携带 mics-winId 请求头（CEF 注入）
    │
    ▼
YARP 反向代理 (localhost:{ProxyPort})
    │
    │  路由匹配: /ClientInternal/* → InternalService 集群
    │
    ▼
Minimal API: POST /ClientInternal/changeLoc
    │
    ▼
LocalService.ChangeLoc()
    │
    │  1. 从 HttpContext.Request 读取 mics-winId 头
    │  2. 发送 ChangeLocationMessage 到 WeakReferenceMessenger
    │
    ▼
LocalBasedWindow (原生 Avalonia 窗口)
    │
    │  接收消息 → ChangeLocation(locationId) → 更新界面
    │
    ▼
返回 JSON 响应给 H5 页面
```

---

## 八、LocalWebSocket — Native→H5 服务端推送

### 8.1 WebSocket 服务端

`LocalWebSocketService` 是 Native→H5 方向的核心推送通道，监听于 `/ClientInternal/notification`：

```csharp
// MicsClient/HttpServices/LocalWebSocketService.cs

public class LocalWebSocketService
{
    private readonly ConcurrentSet<WebSocketSubscriber> subscribers = [];

    public LocalWebSocketService(ILogger<LocalWebSocketService> logger)
    {
        this.logger = logger;
        InitHeartBeat();

        // 🔑 监听原生 MVVM 消息，转换为 WebSocket 推送
        WeakReferenceMessenger.Default.Register<NotificationMessage>(
            this, async (sender, message) =>
        {
            switch (message.Value.NotifyType)
            {
                case NotificationType.Location:
                    await SendLocationChanged();  // 站点变更 → 推送给 H5
                    break;
                case NotificationType.Menu:
                    await SendMenuChanged();      // 菜单变更 → 推送给 H5
                    break;
            }
        });
    }

    private async Task SendLocationChanged()
    {
        foreach (var subscriber in subscribers.Where(s => s.IsSubcribe(CommandType.LOCATION_CHANGED)))
        {
            await subscriber.SendCommandAsync(new WebSocketCommand
            {
                command = CommandType.LOCATION_CHANGED,
                data = "",
                message = ""
            });
        }
    }

    private async Task SendMenuChanged()
    {
        foreach (var subscriber in subscribers.Where(s => s.IsSubcribe(CommandType.MENU_CHANGED)))
        {
            await subscriber.SendCommandAsync(new WebSocketCommand
            {
                command = CommandType.MENU_CHANGED,
                data = "",
                message = ""
            });
        }
    }
}
```

### 8.2 连接管理与心跳

```csharp
public void AddConnection(WebSocket websocket, string remoteAddr, TaskCompletionSource completionSource)
{
    var subscriber = new WebSocketSubscriber(websocket, remoteAddr, logger);
    subscribers.TryAdd(subscriber);

    Task.Run(async () =>
    {
        var buffer = new byte[4096];
        while (websocket.State == WebSocketState.Open)
        {
            var recived = await websocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), subscriber.CancellationTokenSource.Token);

            if (recived.MessageType == WebSocketMessageType.Text)
            {
                var msg = Encoding.UTF8.GetString(buffer[..recived.Count]).Trim('\0');
                await HandleRecivedMsg(subscriber, msg);
            }
        }
        completionSource.TrySetResult();
        subscribers.TryRemove(subscriber);
    });
}

private async Task HandleRecivedMsg(WebSocketSubscriber subscriber, string msg)
{
    var command = JsonSerializer.Deserialize<WebSocketCommand>(msg);
    switch (command.command)
    {
        case CommandType.HEART_BEAT:
            await ReplyHeatBeat(subscriber);
            break;
        // H5 订阅特定事件主题
        case CommandType.ALERT_SET:
        case CommandType.LOCATION_CHANGED:
        case CommandType.MENU_CHANGED:
            subscriber.AddTopic(command.command);  // 加入订阅列表
            break;
    }
}

// 2 秒间隔心跳检测
private void InitHeartBeat()
{
    Task.Factory.StartNew(() =>
    {
        while (true)
        {
            foreach (var subscriber in subscribers)
            {
                if (subscriber.IsConnected)
                    subscriber.SendCommandAsync(new WebSocketCommand
                    {
                        command = CommandType.HEART_BEAT,
                        data = "",
                        message = "Heart Beat"
                    });
            }
            Task.Delay(2000).Wait();
        }
    }, TaskCreationOptions.LongRunning);
}
```

### 8.3 WebSocket 命令协议

Native↔H5 之间通过 `WebSocketCommand` 对象进行结构化通信：

```csharp
// SharedLib/MicsClient.SharedWebModel/Models/WebSocketCommand.cs

public class WebSocketCommand
{
    public string command { get; set; }   // 命令码
    public object data { get; set; }      // 数据载荷
    public string message { get; set; }   // 描述消息
}

public static class CommandType
{
    public const string HEART_BEAT        = "HEART_BEAT";
    public const string ALARM             = "ALARM";            // 报警推送
    public const string ALARM_TAGCHECK    = "ALARM_TAGCHECK";   // 报警标记检查
    public const string ALARM_SYNC        = "ALARM_SYNC";       // 报警全量同步
    public const string LOCATION_CHANGED  = "LOCATION_CHANGED"; // 站点变更通知
    public const string ALERT_SET         = "ALERT_SET";        // 报警声音设置
    public const string MENU_CHANGED      = "MENU_CHANGED";     // 菜单变更通知
    public const string DEVICE            = "DEVICE";           // 设备数据
    public const string DATA_POINT        = "DATA_POINT";       // 数据点
    public const string ALARM_DISPLAY     = "ALARM_DISPLAY";    // 报警推图
    public const string GUI_ACTION        = "GUI_ACTION";       // GUI 动作指令
    public const string NOTIFICATION_MESSAGE = "NOTIFICATION_MESSAGE"; // 通知消息
    public const string PLAN              = "PLAN";             // 预案
    public const string PLAN_ACTION       = "PLAN_ACTION";      // 预案动作
    // ... 更多命令类型
}
```

### 8.4 通信流程（Native → H5）

```
原生操作 (用户在 Avalonia 界面切换站点)
    │
    ▼
WeakReferenceMessenger.Send(NotificationMessage)
    │
    ▼
LocalWebSocketService (监听 MVVM 消息)
    │
    │  遍历已订阅 LOCATION_CHANGED 的 WebSocket 连接
    │
    ▼
WebSocketSubscriber.SendCommandAsync({
    command: "LOCATION_CHANGED",
    data: "",
    message: ""
})
    │
    │  WebSocket 推送 (ws://localhost:{ProxyPort}/ClientInternal/notification)
    │
    ▼
H5 组态页面 (CEF 中的 JavaScript)
    │
    │  ws.onmessage → 解析 JSON → 刷新页面数据
    │
    ▼
Web 页面更新显示
```

---

## 九、WebSocketSubscriber — 通用订阅者模型

`WebSocketSubscriber` 是 Native 端与 H5 端共享的 WebSocket 连接管理抽象，同时用于客户端本地 WebSocket 和服务端各业务模块：

```csharp
// SharedLib/MicsClient.SharedWebModel/WebSocketSubscriber.cs

public class WebSocketSubscriber
{
    private readonly List<string> topics = [];           // 订阅主题
    private readonly Dictionary<string, string> properties = [];  // 附加属性
    private readonly SemaphoreSlim sendLock = new(1, 1); // 发送并发控制

    public WebSocket Connection { get; private set; }
    public string RemoteAddr { get; private set; }
    public bool IsConnected { get; private set; } = true;
    public CancellationTokenSource CancellationTokenSource { get; } = new();

    // 主题订阅管理
    public void AddTopic(string topic) => topics.Add(topic);
    public void RemoveTopic(string topic) => topics.Remove(topic);
    public bool IsSubcribe(string topic) => topics.Contains(topic);

    // 线程安全的消息发送
    public async Task SendCommandAsync(WebSocketCommand command, CancellationToken token = default)
    {
        await sendLock.WaitAsync(cancellationToken);
        try
        {
            var sendByte = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(command));
            await Connection.SendAsync(
                new ArraySegment<byte>(sendByte, 0, sendByte.Length),
                WebSocketMessageType.Text, true, token);
        }
        catch (Exception ex)
        {
            IsConnected = false;
            CancellationTokenSource.Cancel();
        }
        sendLock.Release();
    }

    // 附加属性（用于存储 OperatorId 等上下文信息）
    public void SetProperty(string name, string value) => properties[name] = value;
    public string GetProperty(string name) =>
        properties.TryGetValue(name, out var value) ? value : string.Empty;
}
```

---

## 十、原生端 WebSocket 客户端

### 10.1 MicsWebsocketClient — 客户端 WS 连接管理

原生 Avalonia 端通过 `MicsWebsocketClient` 连接到本地代理的 WebSocket 端点，实现服务端事件（报警、通知、设备数据等）的实时接收：

```csharp
// SharedLib/MicsClient.SharedWebModel/Helpers/MicsWebsocketClient.cs

public class MicsWebsocketClientImpl : IMicsWebsocketClient
{
    private ClientWebSocket webSocket;
    private DateTime heartBeatTime;
    public Channel<WebSocketCommand> SendChannel { get; }
    public Channel<WebSocketCommand> ReceiveChannel { get; }

    public async Task ConnectAsync(string url)
    {
        // Polly 重试策略：无限重试，线性退避，最大间隔 30s
        var retry = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Linear,
                MaxRetryAttempts = int.MaxValue,
                MaxDelay = TimeSpan.FromSeconds(30),
                Delay = TimeSpan.FromSeconds(1),
            }).Build();

        await retry.ExecuteAsync(async token => await StartConnectAsync(token));
        StartSendMessage();   // 启动发送管道
        StartReciveMessage(); // 启动接收管道（使用 System.IO.Pipelines）
        StartHeartBeat();     // 启动心跳检测（15s 超时重连）
    }
}
```

### 10.2 NotificationService — 消费推送消息

```csharp
// MicsClient/BackgroundServices/NotificationService.cs

public class NotificationService
{
    private IMicsWebsocketClient websocketClient;

    private async Task InitNotification()
    {
        websocketClient = ServiceManager.Locator.GetRequiredService<IMicsWebsocketClient>();

        // 消费 WebSocket 推送消息
        Task.Run(async () =>
        {
            await foreach (var command in websocketClient.ReceiveChannel.Reader.ReadAllAsync())
            {
                if (command.command == CommandType.GUI_ACTION)
                {
                    var act = JsonSerializer.Deserialize<GuiAction>(command.data.ToString());
                    ExcuteAction(act);  // 执行 GUI 动作（切换组态、弹出视频等）
                }
                else if (command.command == CommandType.NOTIFICATION_MESSAGE)
                {
                    var msg = JsonSerializer.Deserialize<NoticeMessage>(command.data.ToString());
                    await ProcessMessage(msg);  // 处理通知消息弹窗
                }
            }
        });

        // 连接到服务端通知 WebSocket
        await websocketClient.ConnectAsync("/notification");
    }

    private void ExcuteAction(GuiAction act)
    {
        switch (act.ActionType)
        {
            case GuiActionType.ShowScada:
                // 切换组态页面
                win.ChangeContent("MicsServer/pages/scada/HMI/" + act.Parameter);
                break;
            case GuiActionType.PopupCCTV:
                // 弹出视频监控窗口
                new CctvDisplayWindow(args).Show(win);
                break;
            case GuiActionType.ShowApplication:
                win.ChangeContent(act.Parameter);
                break;
        }
    }
}
```

---

## 十一、服务端 WebSocket 架构

服务端同样采用 WebSocket 实现实时推送，各业务模块独立管理 WebSocket 连接：

### 11.1 WebSocketServiceBase — 服务端 WS 基类

```csharp
// SharedLib/MicsClient.SharedWebModel/WebSockets/WebSocketServiceBase.cs

public abstract class WebSocketServiceBase
{
    protected readonly ConcurrentSet<WebSocketSubscriber> Subscribers = [];
    protected readonly ConcurrentDictionary<string,
        Func<WebSocketSubscriber, WebSocketCommand, Task>> RecivedCommandDic = new();

    public WebSocketServiceBase(ILogger logger)
    {
        this.InitRecivedCommands();
        this.InitHeartBeat();  // 2s 心跳检测
    }

    public void AddConnection(WebSocket websocket, string remoteAddr,
        TaskCompletionSource completionSource)
    {
        var subscriber = new WebSocketSubscriber(websocket, remoteAddr, Logger);
        Subscribers.TryAdd(subscriber);
        this.NewConnectAction(subscriber);  // 子类自定义新连接行为
        // 启动消息接收循环...
    }

    protected abstract void InitOtherRecivedCommands();  // 子类注册命令处理器
    protected abstract void NewConnectAction(WebSocketSubscriber subscriber);
    protected abstract void RemoveConnectAction(WebSocketSubscriber subscriber);
}
```

### 11.2 服务端 Minimal API WebSocket 端点

以报警模块为例，服务端通过 Minimal API 暴露 WebSocket 端点：

```csharp
// Server/MicsClient.Server.Alarm/AlarmServerExtention.cs

app.MapMethods("/alarm/subscribe", ["CONNECT", "GET"], async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var websocket = await context.WebSockets.AcceptWebSocketAsyncWithHA();
        var socketFinishedTcs = new TaskCompletionSource();
        var application = context.RequestServices.GetRequiredService<IAlarmWebSocket>();
        application.AddConnection(websocket,
            $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}",
            context, socketFinishedTcs);
        await socketFinishedTcs.Task;
    }
});
```

### 11.3 AlarmWebSocket — 报警实时推送

```csharp
// Server/MicsClient.Server.Alarm/AlarmWebSocket.cs

public class AlarmWebSocket : IAlarmWebSocket
{
    private readonly Channel<List<AlarmUnit>> alarmChannel = Channel.CreateUnbounded<List<AlarmUnit>>();
    private readonly ConcurrentDictionary<WebSocketSubscriber, AlarmFilter> alarmSubscriber = [];

    private void InitAlarm()
    {
        alarmService.SetAlarmSendChannel(alarmChannel);

        // 报警推送：从 Channel 读取新报警，推送给所有订阅者
        Task.Run(async () =>
        {
            await foreach (var alarms in alarmChannel.Reader.ReadAllAsync())
            {
                alarmSubscriber.AsParallel().ForAll(async kv =>
                {
                    // 根据订阅过滤条件筛选
                    var preSend = alarms.FindAll(a => kv.Value.IsHit(a));
                    if (preSend.Count == 0) return;

                    await kv.Key.SendCommandAsync(new WebSocketCommand
                    {
                        command = CommandType.ALARM,
                        data = new AlarmQueryAck { type = 1, Alarms = preSend },
                        message = "New Alarm"
                    });
                });
            }
        });
    }
}
```

---

## 十二、完整通信链路图

### 12.1 H5 → Native（HTTP 请求方向）

```
┌─────────────────────────────────────────────────────────────────────┐
│  H5 组态页面 (CEF Chromium)                                         │
│                                                                     │
│  fetch("http://localhost:15024/ClientInternal/callmenu",            │
│        { method:"POST", body: JSON.stringify({info:"menu_id"}) })   │
│                                                                     │
│  请求头: mics-winId: "a1b2c3d4-..." (CEF 自动注入)                  │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│  YARP (localhost:15024)                                             │
│  路由: /ClientInternal/* → InternalService 集群                     │
│  追加请求头: mics-operatorid, mics-sessionid, mics-consolename...  │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Minimal API: POST /ClientInternal/callmenu                         │
│                                                                     │
│  LocalService.CallMenu():                                           │
│    winId = Request.GetRequestWinId()  // "a1b2c3d4-..."            │
│    WeakReferenceMessenger.Send(ChangePageMessage{WindowId, Info})   │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│  LocalBasedWindow (Avalonia 原生窗口)                               │
│    接收 ChangePageMessage → ContentView.Url = "MicsServer/pages/.." │
│    CefView.LoadPage() → 加载新的 Web 组态页面                       │
└─────────────────────────────────────────────────────────────────────┘
```

### 12.2 Native → H5（WebSocket 推送方向）

```
┌─────────────────────────────────────────────────────────────────────┐
│  用户操作: 在原生界面切换监控站点                                    │
│                                                                     │
│  WeakReferenceMessenger.Send(NotificationMessage{Location})         │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│  LocalWebSocketService                                              │
│    遍历已订阅 LOCATION_CHANGED 的 WebSocketSubscriber               │
│    → SendCommandAsync({ command: "LOCATION_CHANGED" })             │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             │  WebSocket (ws://localhost:15024/ClientInternal/notification)
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│  H5 组态页面 (JavaScript)                                           │
│                                                                     │
│  ws.onmessage = (event) => {                                        │
│    const cmd = JSON.parse(event.data);                              │
│    if (cmd.command === "LOCATION_CHANGED") {                        │
│      refreshPageData();  // 刷新页面数据                             │
│    }                                                                │
│  };                                                                 │
└─────────────────────────────────────────────────────────────────────┘
```

### 12.3 Server → Client（远程 WebSocket 推送）

```
┌─────────────────────────────────────────────────────────────────────┐
│  MicsClient.Server (报警服务)                                       │
│                                                                     │
│  新报警产生 → alarmChannel.Writer.Write(newAlarms)                  │
│  AlarmWebSocket 读取 Channel → 推送给所有订阅者                     │
│  SendCommandAsync({ command:"ALARM", data: AlarmQueryAck })        │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             │  WebSocket (/alarm/subscribe)
                             │  经过 YARP 反向代理转发
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│  NotificationService (原生端)                                       │
│                                                                     │
│  websocketClient.ReceiveChannel.Reader.ReadAllAsync()               │
│    → 解析 GUI_ACTION: 切换组态/弹出视频/显示消息                    │
│    → 解析 NOTIFICATION_MESSAGE: 弹出确认对话框                      │
│    → 通过 WeakReferenceMessenger 分发到具体窗口                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 十三、关键设计决策总结

| 设计点 | 方案选择 | 理由 |
|--------|----------|------|
| **浏览器引擎** | Xilium.CefGlue (Chromium 120) | 完整的 Web 标准支持，可渲染复杂 SCADA/HMI 组态画面 |
| **混合渲染** | `Decorator.Child` 动态替换 | 同一控件容器中原生与 Web 视图等价切换，无布局跳变 |
| **CEF 初始化** | 懒加载 (`EnsureBrowser`) | 避免为仅显示原生控件的 CefView 创建昂贵的 Chromium 实例 |
| **本地代理** | ASP.NET Core + YARP | 统一 HTTP/WS 入口，支持多站点负载均衡与健康检查 |
| **H5→Native** | HTTP POST + 请求头 `mics-winId` | 简单可靠，无需 JS 桥接库；请求头由 CEF 自动注入 |
| **Native→H5** | WebSocket `/ClientInternal/notification` | 低延迟推送，支持主题订阅过滤 |
| **消息总线** | `WeakReferenceMessenger` | 解耦原生组件间通信，无强引用泄漏风险 |
| **WS 协议** | JSON `WebSocketCommand` | 统一命令格式，支持扩展（MessagePack 可选） |
| **心跳机制** | 2s 间隔，15s 超时重连 | 及时发现断连，Polly 自动重连 |
| **跨平台** | Linux 离屏渲染 + 禁用 GPU | 兼容麒麟 V10 国产操作系统 |

---

## 十四、mics-winId 窗口标识机制详解

### 14.1 作用概述

`mics-winId` 是 CEF 浏览器窗口实例的身份标识，其核心作用是：**让服务端能够区分"同一个客户端进程上多个窗口各自发出的请求"，从而实现多窗口隔离的精准消息路由。**

MICS 客户端是多窗口应用——一个操作员可以同时打开多个 `LocalBasedWindow`（例如主屏显示 XDZ 站、副屏显示 OCC 总览），每个窗口内又有多个 `CefView` 区域。所有窗口共享同一个进程、同一个本地 YARP 代理。当 H5 组态页面发起 HTTP 请求（如"切换菜单"）时，必须知道**这个请求来自哪个窗口**，否则响应就会发到错误的窗口。

### 14.2 完整生命周期（4 步链路）

```
步骤 1: 生成 ──→ 步骤 2: 注入 ──→ 步骤 3: 提取 ──→ 步骤 4: 路由
```

#### 步骤 1：窗口创建时生成唯一 Guid

每个 `LocalBasedWindow` 实例在构造时生成一个全局唯一的 `Guid`：

```csharp
// MicsClient/Views/BaseView/LocalBasedWindow.axaml.cs

public partial class LocalBasedWindow : MicsHostWindow
{
    private Guid WinId = Guid.NewGuid();  // 每个窗口实例一个唯一 ID

    public override Guid GetWinId() => WinId;
}
```

`CefView` 在 `Loaded` 事件中向上查找父窗口，获取所属窗口的 `WinId`：

```csharp
// MicsClient/Views/BaseView/CefView.axaml.cs

public Guid WinId { get; private set; }

private void CefView_Loaded(object? sender, RoutedEventArgs e)
{
    var pWin = TopLevel.GetTopLevel(this);
    if (pWin != null && pWin is MicsHostWindow w)
    {
        WinId = w.GetWinId();  // 获取最近一层 MicsHostWindow 的 Guid
    }
}
```

#### 步骤 2：CEF 在每个 HTTP 请求中自动注入

`CefView` 内的 `MicsResourceRequestHandler` 拦截 CEF 浏览器的每一个 HTTP 请求，将 `WinId` 注入到请求头中：

```csharp
// MicsClient/Views/BaseView/CefView.axaml.cs

protected sealed class MicsResourceRequestHandler(CefView view) : CefResourceRequestHandler
{
    private CefView _view = view;

    protected override CefReturnValue OnBeforeResourceLoad(
        CefBrowser browser, CefFrame frame, CefRequest request, CefCallback callback)
    {
        // 🔑 关键行：将该 CefView 所属窗口的 WinId 注入到请求头
        request.SetHeaderByName(Header.WinId, _view.WinId.ToString(), true);
        return base.OnBeforeResourceLoad(browser, frame, request, callback);
    }
}
```

> **对 H5 完全透明** —— 无论 JS 代码怎么写 `fetch()` / `XMLHttpRequest`，CEF 都会在底层自动附加这个请求头。H5 前端开发者无需感知 `winId` 的存在。

#### 步骤 3：服务端（本地 Minimal API）从请求头提取

`ControllerBaseExtension` 提供了从 `HttpRequest` 中提取 `winId` 的扩展方法：

```csharp
// SharedLib/MicsClient.SharedWebExtension/Extensions/ControllerBaseExtension.cs

public static class ControllerBaseExtension
{
    public static string GetRequestWinId(this HttpRequest request)
    {
        try
        {
            var WinIdArgs = request.Headers[Header.WinId].ToString();
            return WinIdArgs.Trim();
        }
        catch (Exception)
        {
            return "INVALID";
        }
    }
}
```

`LocalService` 中每个桥接方法的第一步都是提取 `winId`：

```csharp
// MicsClient/HttpServices/LocalService.cs

public Resp<CallMenuAck> CallMenu(CallMenuReq callMenuReq, HttpContext httpContext)
{
    var winId = httpContext.Request.GetRequestWinId();  // ← 提取来源窗口 ID

    WeakReferenceMessenger.Default.Send(new ChangePageMessage(new PageContent
    {
        Info = callMenuReq.info,
        WindowId = Guid.Parse(winId)   // ← 携带目标窗口 ID 发送消息
    }));

    return Resp<CallMenuAck>.SuccessResult(new CallMenuAck { info = callMenuReq.info }, winId);
}
```

#### 步骤 4：消息总线精准投递到对应窗口

`WeakReferenceMessenger` 将消息分发后，各窗口只处理 `WindowId` 匹配自身的消息：

```csharp
// 目标窗口的消息监听
WeakReferenceMessenger.Default.Register<ChangePageMessage>(this, (sender, msg) =>
{
    if (msg.Value.WindowId == this.GetWinId())  // 只处理发给自己窗口的消息
    {
        ContentView.Url = msg.Value.Url;
    }
});
```

### 14.3 实际场景示例

假设操作员同时打开了两个窗口：

| 窗口 | WinId | 监控站点 |
|------|-------|---------|
| 窗口 A | `11111111-...` | XDZ 站 |
| 窗口 B | `22222222-...` | OCC 总览 |

```
操作员在窗口 A 的 H5 组态画面上点击"切换到 PSD 页面"
    │
    │  JS:  fetch("/ClientInternal/callmenu", { body: { info: "PSD" } })
    │  CEF: 自动附加请求头  mics-winId: "11111111-..."
    │
    ▼
LocalService.CallMenu()
    │  提取 winId = "11111111-..."
    │  发送 ChangePageMessage { WindowId = 11111111, Info = "PSD" }
    │
    ▼
窗口 A 收到消息 → ContentView.Url 切换为 PSD 页面 ✅
窗口 B 不受影响 ✅ (WindowId 不匹配，忽略)
```

如果没有 `mics-winId`，这个操作就可能让窗口 B 错误地切换页面，或者两个窗口同时响应。

### 14.4 服务端对 winId 的使用

除了本地桥接，服务端的 WebSocket 连接管理同样依赖 `mics-winId`（及其他请求头）来标识连接归属。以报警 WebSocket 为例：

```csharp
// Server/MicsClient.Server.Alarm/AlarmWebSocket.cs

public void AddConnection(WebSocket websocket, string remoteAddr,
    HttpContext context, TaskCompletionSource completionSource)
{
    var subscriber = new WebSocketSubscriber(websocket, remoteAddr, logger);
    var operatorId = context.Request.GetRequestOperatorId();
    subscriber.SetProperty(Header.OperatorId, operatorId.ToString());  // 关联操作员
    subscribers.TryAdd(subscriber);
    // ...
}
```

### 14.5 多维度总结

| 维度 | 说明 |
|------|------|
| **本质** | 窗口实例级唯一标识（`Guid`） |
| **生成时机** | `LocalBasedWindow` 构造时 `Guid.NewGuid()` |
| **注入方式** | CEF `OnBeforeResourceLoad` 自动注入到每个 HTTP 请求头 |
| **对 H5 透明** | JavaScript 无需感知，所有请求自动携带 |
| **消费方** | `LocalService` 各桥接方法 + 服务端 `AlarmWebSocket.AddConnection()` 等 |
| **解决的问题** | 多窗口共享同一进程/代理时，精准识别请求来源窗口 |
| **路由机制** | 提取 `winId` → 通过 `WeakReferenceMessenger` 投递到匹配窗口的消息处理器 |
| **请求头键名** | `mics-winId`（定义于 `Header.WinId` 常量） |

---

## 十五、文件索引

| 文件路径 | 说明 |
|----------|------|
| `MicsClient/Views/BaseView/CefView.axaml` | CefView XAML 定义 |
| `MicsClient/Views/BaseView/CefView.axaml.cs` | CefView 核心逻辑（LoadPage / EnsureBrowser / CEF 处理器） |
| `MicsClient/Views/BaseView/LocalBasedWindow.axaml` | 主窗口四区域布局 |
| `MicsClient/Views/BaseView/LocalBasedWindow.axaml.cs` | 主窗口逻辑（站点切换、面板调用、对话框） |
| `MicsClient/App.axaml.cs` | 应用启动序列（SetupCEF / SetupProxy / ClientProxyHeaderTransform） |
| `MicsClient/Common/MicsConfigurations.cs` | UrlResolver URL 映射逻辑 |
| `MicsClient/Common/ProxyConfigurations.cs` | YARP 路由与集群配置生成 |
| `MicsClient/HttpServices/LocalServiceExtension.cs` | Minimal API 端点注册 (`/ClientInternal/*`) |
| `MicsClient/HttpServices/LocalService.cs` | 本地桥接服务（H5→Native 消息转换） |
| `MicsClient/HttpServices/LocalWebSocketService.cs` | 本地 WebSocket 推送服务（Native→H5） |
| `MicsClient/BackgroundServices/NotificationService.cs` | 原生端通知消费（WS 消息→GUI 动作） |
| `SharedLib/MicsClient.SharedWebModel/Header.cs` | 请求头常量定义 |
| `SharedLib/MicsClient.SharedWebExtension/Extensions/ControllerBaseExtension.cs` | 请求头提取扩展方法（GetRequestWinId 等） |
| `MicsClient/Common/ClientRequestHelper.cs` | 客户端请求辅助工具（加密/编码） |
| `SharedLib/MicsClient.SharedWebModel/Models/WebSocketCommand.cs` | WebSocket 命令协议与命令类型枚举 |
| `SharedLib/MicsClient.SharedWebModel/WebSocketSubscriber.cs` | WebSocket 订阅者模型（连接管理/主题订阅/线程安全发送） |
| `SharedLib/MicsClient.SharedWebModel/WebSockets/WebSocketServiceBase.cs` | 服务端 WebSocket 基类 |
| `SharedLib/MicsClient.SharedWebModel/Helpers/MicsWebsocketClient.cs` | 客户端 WebSocket 连接管理（Polly 重连/Pipe 读取） |
| `Server/MicsClient.Server.Alarm/AlarmWebSocket.cs` | 报警 WebSocket 推送实现 |
| `Server/MicsClient.Server.Alarm/AlarmServerExtention.cs` | 服务端报警 Minimal API 端点 |
| `Server/MicsClient.Server/Program.cs` | 服务端启动配置 |
