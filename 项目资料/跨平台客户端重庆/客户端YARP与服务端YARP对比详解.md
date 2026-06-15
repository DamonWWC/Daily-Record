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
