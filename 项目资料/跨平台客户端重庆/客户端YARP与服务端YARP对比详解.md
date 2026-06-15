# 客户端 YARP vs 服务端 YARP 对比详解

---

## 一句话总结

> **客户端 YARP** 是桌面应用的"统一出口"，让 CEF 浏览器和原生代码能通过 `localhost` 访问所有后端；**服务端 YARP** 是单个车站服务的"跨站桥梁"，让本站服务能通过 `localhost` 访问其他车站的服务。

---

## 整体拓扑

```
                    ┌─────────────────────────────────────────────────────┐
                    │           桌面应用进程 (MicsClient)                  │
                    │                                                     │
 CEF 浏览器 ────────┤                                                     │
 原生 HttpClient ───┤──→ localhost:15024 ──→ 【客户端 YARP】              │
                    │                        ├─ /XDZ/**     → XDZ 站服务器 │
                    │                        ├─ /OCC/**     → OCC 站服务器 │
                    │                        ├─ /ClientInternal → 本地API  │
                    │                        └─ /** (兜底)   → 本站服务器  │
                    └────────────────────────────────┬────────────────────┘
                                                     │
                          ┌──────────────────────────┼──────────────────────────┐
                          │                          │                          │
                          ▼                          ▼                          ▼
              ┌───────────────────┐    ┌───────────────────┐    ┌───────────────────┐
              │  XDZ 站服务器      │    │  OCC 站服务器      │    │  TWP 站服务器      │
              │  (port 15001)     │    │  (port 15001)     │    │  (port 15001)     │
              │                   │    │                   │    │                   │
              │  localhost:15001  │    │                   │    │                   │
              │  →【服务端 YARP】 │    │                   │    │                   │
              │  ├ /OCC/**→OCC站  │    │                   │    │                   │
              │  └ /TWP/**→TWP站  │    │                   │    │                   │
              └───────────────────┘    └───────────────────┘    └───────────────────┘
```

---

## 详细对比

| 维度 | 客户端 YARP (`MicsClient`) | 服务端 YARP (`MicsClient.Server`) |
|------|--------------------------|----------------------------------|
| **监听端口** | `localhost:15024`（ProxyPort） | `localhost:15001`（ServicePort） |
| **配置类** | `ProxyConfigurations.cs` | `ProxyHelper.cs` |
| **服务对象** | CEF 浏览器 + 原生代码 | 本站服务模块自身 |
| **核心目的** | 桌面应用的统一后端出口 | 本站访问其他车站的桥梁 |

---

## 路由配置差异

| 路由类型 | 客户端 YARP | 服务端 YARP |
|---------|:-----------:|:-----------:|
| 车站级路由 `/{LocationName}/**` | ✅ | ✅ |
| 服务器级路由 `/{ServerName}/**` | ✅ | ✅ |
| **本站兜底路由 `/**`** | ✅ | ❌ |
| **内部 API 路由 `/ClientInternal/**`** | ✅ | ❌ |
| `InternalService` 集群 | ✅ | ❌ |

**客户端比服务端多出两条关键路由**，因为客户端需要处理本地 API 和未匹配请求的兜底转发。

### 客户端路由配置 (`ProxyConfigurations.cs`)

```csharp
public static IReadOnlyList<RouteConfig> GetRouteConfigs()
{
    var list = new List<RouteConfig>();
    var hosts = HostConfigsReader.ReadHostConfigs();

    // ① 车站级路由: /XDZ/{**remainder} → XDZ 集群
    foreach (var host in hosts)
    {
        list.Add(new RouteConfig()
        {
            RouteId = host.LocationName,
            ClusterId = host.LocationName,
            Match = new RouteMatch()
            {
                Path = $"/{host.LocationName}/{{**remainder}}",
                Headers = [new RouteHeader()
                {
                    Name = Header.RequestType,
                    Mode = HeaderMatchMode.NotExists
                }]
            }
        }.WithTransformPathRemovePrefix($"/{host.LocationName}"));

        // ② 服务器级路由: /MICS-XDZ-SCADA/{**remainder}
        list.Add(new RouteConfig() { RouteId = host.ServerName, ... });
    }

    // ③ 本站默认路由（兜底） ← 服务端没有
    list.Add(new RouteConfig()
    {
        RouteId = "LocalStation",
        ClusterId = GetLocationName(config.LocationId),
        Match = new RouteMatch() { Path = "{**catch-all}", ... }
    });

    // ④ 内部服务路由 ← 服务端没有
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
    });

    return list;
}
```

### 服务端路由配置 (`ProxyHelper.cs`)

```csharp
public static IReadOnlyList<RouteConfig> GetRouteConfigs()
{
    var list = new List<RouteConfig>();
    var hosts = HostConfigsReader.ReadHostConfigs();

    // ① 车站级路由: /XDZ/{**remainder} → XDZ 集群
    // ② 服务器级路由: /MICS-XDZ-SCADA/{**remainder}
    // （与客户端相同）

    // 没有兜底路由
    // 没有 InternalService 路由

    return list;
}
```

---

## 集群配置差异

| 集群类型 | 客户端 YARP | 服务端 YARP |
|---------|:-----------:|:-----------:|
| 车站集群（LocationName 分组） | ✅ | ✅ |
| 服务器集群（ServerName 分组） | ✅ | ✅ |
| **InternalService 集群** | ✅ | ❌ |

客户端独有的 `InternalService` 集群指向自身的 Minimal API：

```csharp
// 客户端 ProxyConfigurations.cs

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
```

---

## 请求头注入差异

| 特性 | 客户端 YARP | 服务端 YARP |
|------|:-----------:|:-----------:|
| `ClientProxyHeaderTransform` 加密头注入 | ✅ | ❌ |
| SHA256 请求体签名 | ✅ | ❌ |
| Session / OperatorId / Console 注入 | ✅ | ❌ |

客户端 YARP 在转发时统一注入用户身份信息（因为 CEF 浏览器的请求不包含这些）：

```csharp
// 客户端 App.axaml.cs

public class ClientProxyHeaderTransform : RequestTransform
{
    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        context.ProxyRequest.Headers.Add(Header.Console,
            ClientRequestHelper.GetUrlEncodeString(session.ConsoleName));
        context.ProxyRequest.Headers.Add(Header.OperatorId,
            ClientRequestHelper.GetEncyptContent(session.OperatorId));     // DES 加密
        context.ProxyRequest.Headers.Add(Header.OperatorName,
            ClientRequestHelper.GetUrlEncodeString(session.OperatorName));
        context.ProxyRequest.Headers.Add(Header.Session,
            ClientRequestHelper.GetEncyptContent(session.SessionId));      // DES 加密

        // SHA256 请求体签名
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

服务端 YARP **没有** `ClientProxyHeaderTransform`，因为调用方是本站服务模块，请求头已经由上游携带。

---

## 各自解决的问题

### 客户端 YARP — "CEF 浏览器不知道后端在哪"

CEF 中运行的 H5 组态页面只知道一个地址 `http://localhost:15024`，它不知道后端有 12+ 台服务器分布在不同的车站。客户端 YARP 让 H5 页面只需要：

```javascript
// H5 页面只需要知道逻辑路径
fetch("/XDZ/alarm/query")         // → 自动路由到 XDZ 站的报警服务
fetch("/OCC/pages/scada/...")     // → 自动路由到 OCC 站的静态文件
fetch("/ClientInternal/callmenu") // → 自动路由到本地 Minimal API
```

### 服务端 YARP — "本站服务需要读取其他车站的数据"

以 OCC 为例：OCC 站的 `AlarmService` 需要实时接收所有车站的报警数据。它不需要知道每个车站的 IP 和端口，只需要：

```csharp
// AlarmSyncReciver.cs — OCC 连接各车站的报警同步 WebSocket

foreach (var loc in reciveLoc)  // 遍历所有被监控的车站
{
    var wsc = ServiceLocator.Provider.GetRequiredService<IMicsWebsocketClient>();
    // 连接到 /XDZ/alarm/sync — 服务端 YARP 自动路由到 XDZ 站
    _ = wsc.ConnectAsync($"/{loc.Name}/alarm/sync");
}
```

同样的模式还出现在多个跨站同步模块中：

| 模块 | 跨站调用路径 | 代码文件 |
|------|-------------|---------|
| 报警同步 | `/{loc}/alarm/sync` | `AlarmSyncReciver.cs` |
| 事件同步 | `/{loc}/event/sync` | `EventSyncReciver.cs` |
| 权限同步 | `/{loc}/right/sync` | `SessionSyncReciver.cs` |

---

## 完整请求链路示例

### 场景一：OCC 站操作员在 H5 页面查看 XDZ 站的报警

```
H5 页面 (CEF 中运行)
    │
    │  fetch("http://localhost:15024/XDZ/alarm/query")
    │
    ▼
┌──────────────────────────────────────────────────────┐
│  【客户端 YARP】(localhost:15024)                     │
│                                                      │
│  匹配路由: /XDZ/{**remainder} → Cluster "XDZ"        │
│  注入请求头: mics-winId, mics-operatorid,             │
│             mics-sessionid, mics-trackingid (SHA256) │
│  去掉前缀: /alarm/query                              │
│  负载均衡选择: http://192.168.1.10:15001              │
└──────────────────────┬───────────────────────────────┘
                       │
                       ▼
XDZ 站服务器 (192.168.1.10:15001)
    │
    │  【服务端 YARP】不介入（请求直接到本站，无需二次转发）
    │
    ▼
AlarmController.Query() → 返回报警数据 → 原路返回 H5
```

### 场景二：OCC 站 AlarmService 接收 XDZ 站实时报警

```
XDZ 站报警服务产生新报警
    │
    ▼
XDZ 站 AlarmSyncSender (WebSocket 服务端)
    │  等待其他车站订阅
    │
    │
OCC 站 AlarmSyncReciver
    │
    │  wsc.ConnectAsync("/XDZ/alarm/sync")
    │  → ws://localhost:15001/XDZ/alarm/sync
    │
    ▼
┌──────────────────────────────────────────────────────┐
│  【服务端 YARP】(localhost:15001)                     │
│                                                      │
│  匹配路由: /XDZ/{**remainder} → Cluster "XDZ"        │
│  去掉前缀: /alarm/sync                               │
│  负载均衡选择: http://192.168.1.10:15001              │
└──────────────────────┬───────────────────────────────┘
                       │
                       ▼
XDZ 站服务器 (WebSocket /alarm/sync 端点)
    │  建立 WebSocket 连接
    │  新报警 → 推送给 OCC
    │
    ▼
OCC 站 AlarmSyncReciver 接收
    │  → 写入本地 Channel → AlarmService 处理 → AlarmWebSocket 推给前端
```

---

## 为什么要两层 YARP

```
桌面应用 (MicsClient)
    │
    │  为什么需要客户端 YARP？
    │  ① CEF 浏览器只能访问 http(s)://，不能直连后端
    │  ② 统一注入身份请求头（CEF 无法自动注入）
    │  ③ 本地 Minimal API 桥接 (/ClientInternal)
    │  ④ 屏蔽后端多实例细节（健康检查 + 负载均衡）
    │
    ▼
车站服务器 (MicsClient.Server)
    │
    │  为什么需要服务端 YARP？
    │  ① 本站服务需要跨站访问其他车站（报警/事件/权限同步）
    │  ② 服务代码不硬编码其他车站的 IP:Port
    │  ③ 统一健康检查，自动故障转移
    │  ④ 通过配置文件即可调整跨站拓扑
    │
    ▼
其他车站服务器
```

两层 YARP 的核心价值相同——**让调用方不需要知道后端的真实地址**，但它们服务的对象不同：客户端 YARP 服务于桌面 UI，服务端 YARP 服务于站间服务协作。

---

## 服务端出站请求是否经过自己的 YARP

**会的，而且所有出站请求都必须经过。**

### 证据链

**1. HttpClient 和 WebSocketClient 的 BaseUrl 指向自己的 YARP 端口**

```csharp
// Server/MicsClient.Server/Program.cs

private static void SetupProxy(WebApplicationBuilder builder, int proxyPort)
{
    // YARP 监听此端口
    builder.WebHost.ConfigureKestrel(opt => opt.ListenAnyIP(proxyPort));

    builder.Services.AddReverseProxy()
        .LoadFromMemory(ProxyHelper.GetRouteConfigs(), ProxyHelper.GetClusterConfigs())
        .LoadFromConfig(builder.Configuration.GetSection("ServerProxy"));

    // HttpClient 的 BaseUrl 也是这个端口 ↓
    builder.Services.AddMicsHttpClient(
        options => options.SetBaseUrl("http://localhost:" + proxyPort));

    // WebSocketClient 的 BaseUrl 也是这个端口 ↓
    builder.Services.AddMicsWebsocketClient(
        options => options.SetBaseUrl("ws://localhost:" + proxyPort));
}
```

**2. 所有出站调用的路径都带 `/{loc}/` 或 `/{server}/` 前缀**

搜遍整个 Server 项目，所有出站请求无一例外：

```csharp
// HTTP 调用
httpClient.PostAsync($"/{loc}/alarm/confirm", req)     // AlarmService
httpClient.PostAsync($"/{loc}/alarm/close", req)       // AlarmService
httpClient.PostAsync($"/{loc}/device/control", req)    // DeviceControlService
httpClient.CheckUrl($"/{server}/Health")                // PrimaryCheckService

// WebSocket 连接
wsc.ConnectAsync($"/{loc.Name}/alarm/sync")            // AlarmSyncReciver
wsc.ConnectAsync($"/{loc}/device")                     // DataPointAggregator
wsc.ConnectAsync($"/{loc.Name}/event/sync")            // EventSyncReciver
wsc.ConnectAsync($"/{loc}/right/sync")                 // SessionSyncReciver
```

这些路径全部命中 YARP 的车站级/服务器级路由规则，被转发到对应车站。

### 服务端 YARP 同时承担两个角色

```
                        服务端 Kestrel (localhost:15001)
                        ┌─────────────────────────────────────────────┐
                        │                                             │
  【入站】              │                                             │
  客户端 YARP ─────────→│  YARP 路由匹配                               │
  其他车站 ────────────→│  ├─ /XDZ/** → XDZ 集群 (转发给 XDZ)          │
                        │  ├─ /OCC/** → OCC 集群 (转发给 OCC)          │
                        │  └─ 未匹配  → fallthrough → 本地端点         │
                        │         ├─ /alarm/query   → AlarmController │
                        │         ├─ /alarm/sync    → AlarmWebSocket  │
                        │         └─ /device/**     → DeviceService   │
                        │                                             │
  【出站】              │                                             │
  AlarmService ────────→│  HttpClient → localhost:15001/XDZ/alarm/confirm
  AlarmSyncReciver ────→│  WebSocket  → localhost:15001/XDZ/alarm/sync
  DataPointAggregator ─→│  WebSocket  → localhost:15001/XDZ/device
                        │       │                                     │
                        │       ▼                                     │
                        │  YARP 路由匹配: /XDZ/** → XDZ 集群           │
                        │       │                                     │
                        │       ▼                                     │
                        │  转发到 http://192.168.1.10:15001/alarm/confirm
                        └─────────────────────────────────────────────┘
```

入站和出站共用同一个 YARP，因为**它们走的是同一个端口**。出站请求从本地服务发出 → 回到 `localhost:15001` → YARP 当它是一个新的入站请求来路由 → 转发到远端。

### 这样设计的好处

服务端代码永远不需要知道其他车站的真实 IP：

```csharp
// 不需要这样写（硬编码 IP）
httpClient.PostAsync("http://192.168.1.10:15001/alarm/confirm", req)

// 只需要这样写（逻辑名称）
httpClient.PostAsync($"/{loc}/alarm/confirm", req)
// YARP 自动解析 loc="XDZ" → 查集群配置 → 选健康实例 → 转发
```

换 IP、加节点、故障转移，改配置文件就行，代码零改动。

---

## 请求到达目标服务器后如何找到接口函数

### 问题

YARP 将请求转发到目标服务器，例如 `http://192.168.1.10:15001/alarm/confirm`，目标服务器是如何找到 `/alarm/confirm` 对应的处理函数的？

### 先回忆请求是怎么到达 192.168.1.10 的

```
OCC 站 AlarmService
    │
    │  httpClient.PostAsync("/XDZ/alarm/confirm", req)
    │  BaseUrl = "http://localhost:15001"
    │
    ▼
OCC 站 Kestrel (localhost:15001)
    │
    │  服务端 YARP 匹配: /XDZ/{**remainder}
    │  去掉前缀 /XDZ → remainder = "alarm/confirm"
    │  转发目标: http://192.168.1.10:15001
    │
    ▼
XDZ 站 Kestrel (192.168.1.10:15001)
    收到的请求路径: POST /alarm/confirm    ← 注意：前缀已被去掉
```

### 到达 192.168.1.10 后发生了什么

XDZ 站服务器收到 `POST /alarm/confirm` 后，进入 ASP.NET Core 中间件管道：

```csharp
// Server/MicsClient.Server/Program.cs

var app = builder.Build();

// ① 全局异常处理
app.UseMiddleware<GlobalExceptionMiddleware>();

// ② 授权
app.UseAuthorization();

// ③ 跨域
app.UseCors();

// ④ YARP 反向代理（检查是否匹配路由）
app.MapReverseProxy();

// ⑤ 静态文件（/pages 前缀）
app.UseStaticFiles(new StaticFileOptions { RequestPath = "/pages", ... });

// ⑥ Blazor
app.MapBlazorHub("maintain");

// ⑦ 所有业务 Minimal API 端点 ← 这里注册了 /alarm/confirm
app.UseMicsService();
```

### 关键：第 ④ 步 YARP 匹配不上，放行

XDZ 站的 YARP 路由规则是：

```
/XDZ/{**remainder}   → XDZ 集群
/OCC/{**remainder}   → OCC 集群
```

但现在到达的请求路径是 `/alarm/confirm`，**不带任何车站前缀**，所以 YARP 所有路由都不匹配。

**YARP 不匹配时的行为**：它不拦截请求，而是把请求交给管道中的下一个中间件（即 fallthrough）。

```
请求: POST /alarm/confirm
    │
    ▼
YARP: /XDZ/** 匹配？ ❌  /OCC/** 匹配？ ❌
    │
    │  不匹配 → 不处理 → 放行给下一个中间件
    │
    ▼
EndpointRouting: 查找已注册的端点
    │
    │  /alarm/confirm 有注册吗？ ✅ 有！
    │
    ▼
AlarmServerExtention.MapRoute() 中注册的端点被命中
```

### 第 ⑦ 步：Minimal API 端点匹配

`app.UseMicsService()` 最终调用了 `app.UseAlarmService()`，其中注册了所有报警端点：

```csharp
// Server/MicsClient.Server.Alarm/AlarmServerExtention.cs

public static WebApplication UseAlarmService(this WebApplication app)
{
    MapRoute(app);
    return app;
}

private static void MapRoute(WebApplication app)
{
    // WebSocket 端点
    app.MapMethods("/alarm/subscribe", ["CONNECT", "GET"], async (HttpContext context) => { ... });
    app.MapMethods("/alarm/sync", ["CONNECT", "GET"], async (HttpContext context) => { ... });

    // HTTP 端点 ← 就是这里匹配到了 /alarm/confirm
    app.MapPost("/alarm/confirm", async (AlarmConfirmReq req, HttpContext context) =>
    {
        var controller = context.RequestServices.GetRequiredService<IAlarmController>();
        return await controller.ConfirmAlarms(req, context);
    })
    .WithTags("报警")
    .WithName("确认报警");

    app.MapPost("/alarm/close", ...);
    app.MapPost("/alarm/query", ...);
    app.MapPost("/alarm/queryhist", ...);
    // ... 更多端点
}
```

### 完整流程图

```
┌─────────────────────────────────────────────────────────────────────────┐
│  XDZ 站服务器 (192.168.1.10:15001)  收到 POST /alarm/confirm           │
│                                                                         │
│  ┌─── 中间件管道 ──────────────────────────────────────────────────┐    │
│  │                                                                 │    │
│  │  ① GlobalExceptionMiddleware → 通过                             │    │
│  │  ② Authorization           → 通过                               │    │
│  │  ③ CORS                    → 通过                               │    │
│  │  ④ YARP MapReverseProxy:                                        │    │
│  │     路由表:                                                      │    │
│  │       /XDZ/** → XDZ集群    匹配 /alarm/confirm ? ❌              │    │
│  │       /OCC/** → OCC集群    匹配 /alarm/confirm ? ❌              │    │
│  │     全部不匹配 → 放行 (fallthrough)                              │    │
│  │  ⑤ StaticFiles /pages       匹配 /alarm/confirm ? ❌ → 放行      │    │
│  │  ⑥ Blazor /maintain         匹配 /alarm/confirm ? ❌ → 放行      │    │
│  │  ⑦ EndpointRouting:                                             │    │
│  │     已注册端点:                                                  │    │
│  │       POST /alarm/confirm   匹配 ? ✅ ← 命中！                  │    │
│  │       POST /alarm/close                                          │    │
│  │       POST /alarm/query                                          │    │
│  │       GET  /alarm/sync                                           │    │
│  │       ...                                                        │    │
│  │                                                                  │    │
│  └──────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  执行端点处理函数:                                                       │
│  ┌──────────────────────────────────────────────────────────────┐       │
│  │  AlarmConfirmReq req ← 从请求体反序列化                       │       │
│  │  IAlarmController controller ← 从 DI 容器解析                 │       │
│  │  return await controller.ConfirmAlarms(req, context);         │       │
│  └──────────────────────────────────────────────────────────────┘       │
└─────────────────────────────────────────────────────────────────────────┘
```

### 一句话总结

> 请求到达目标服务器后，**YARP 匹配不上就放行**，然后由 ASP.NET Core 的 **EndpointRouting** 在已注册的 Minimal API 端点中找到 `/alarm/confirm` 并执行对应的处理函数。

之所以能匹配上，根本原因是 **YARP 在转发时已经把车站前缀 `/XDZ` 去掉了**（`WithTransformPathRemovePrefix`），到达目标服务器的路径就是纯净的 `/alarm/confirm`，与目标服务器本地注册的端点路径完全一致。
