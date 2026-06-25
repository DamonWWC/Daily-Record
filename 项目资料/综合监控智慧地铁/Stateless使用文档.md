# Stateless 状态机库 - 零基础入门手册

> 本文档面向从未使用过 Stateless 的初学者，用最直白的方式讲解如何上手。

---

## 第一部分：先搞清楚「状态机」是什么

### 1.1 用生活例子理解「状态」

想象一盏**电灯**：

- **关闭**时：灯不亮
- **开启**时：灯亮着

这盏灯要么在「关」状态，要么在「亮」状态，**不可能同时既关又亮**。  
这种「当前处于哪一种情况」就叫**状态**。

再想想**红绿灯**：

- 红灯 → 绿灯 → 黄灯 → 红灯 → …

某个时刻，灯只会是其中一种颜色，这就是**状态**。

### 1.2 什么是「状态转换」

状态不会自己变，需要有一个**动作**去触发：

- 电灯：按开关 → 从「关」变成「开」
- 红绿灯：定时器到了 → 从「红灯」变成「绿灯」

这种**推动状态变化的动作**，在 Stateless 里叫 **Trigger（触发器）**。

### 1.3 状态机就是「规则表」

状态机的作用，就是把你脑子里想的「在什么情况下可以变成什么状态」**写成一串规则**：

| 当前状态 | 发生了什么事（触发器） | 变成什么状态 |
|----------|------------------------|--------------|
| 关       | 按开关                 | 开           |
| 开       | 按开关                 | 关           |

有了这套规则，程序就不会乱跳状态，也不会出现「灯既开又关」这种逻辑错误。

---

## 第二部分：5 分钟跑通第一个例子

### 2.1 安装 Stateless

在项目中添加 NuGet 包：

```
dotnet add package Stateless
```

或在 Visual Studio 的 NuGet 包管理器中搜索 `Stateless` 并安装。

### 2.2 最简单的例子：一盏电灯

我们的目标：模拟一盏灯，只有「关」和「开」两个状态，按一下开关就切换。

#### 第一步：定义「状态」和「触发器」

用枚举把「关/开」和「按开关」写清楚：

```csharp
using Stateless;

// 灯有两种状态
enum 灯的状态
{
    关,
    开
}

// 我们只设计一个触发器：按开关
enum 灯的触发器
{
    按开关
}
```

> 实际项目里一般用英文命名，这里用中文只是为了更好理解。

#### 第二步：创建状态机

```csharp
// 创建一个状态机，初始状态是「关」
var 灯 = new StateMachine<灯的状态, 灯的触发器>(灯的状态.关);
```

含义：

- `StateMachine<状态类型, 触发器类型>`：这个状态机管理的是「灯的状态」和「灯的触发器」
- 构造函数里的 `灯的状态.关`：一开始灯是关着的

#### 第三步：写规则（配置转换）

```csharp
// 当灯处于「关」状态时：如果发生「按开关」，就变成「开」
灯.Configure(灯的状态.关)
    .Permit(灯的触发器.按开关, 灯的状态.开);

// 当灯处于「开」状态时：如果发生「按开关」，就变成「关」
灯.Configure(灯的状态.开)
    .Permit(灯的触发器.按开关, 灯的状态.关);
```

含义：

- `Configure(状态)`：针对某个状态写规则
- `Permit(触发器, 目标状态)`：**允许**在「发生这个触发器」时，从当前状态转到目标状态

可以理解为：  
**从「关」这个房间，拿着「按开关」这把钥匙，可以走到「开」这个房间。**

#### 第四步：触发一次

```csharp
Console.WriteLine("当前状态: " + 灯.State);  // 输出: 关

灯.Fire(灯的触发器.按开关);  // 模拟「按了一下开关」

Console.WriteLine("当前状态: " + 灯.State);  // 输出: 开

灯.Fire(灯的触发器.按开关);  // 再按一下

Console.WriteLine("当前状态: " + 灯.State);  // 输出: 关
```

`Fire(触发器)` 就是「执行一次这个触发器」。如果规则允许，状态就会变化。

---

## 第三部分：核心流程总结（每次用都要走的 4 步）

用 Stateless 做状态机，每次基本都是这 4 步：

```
1. 定义「状态」和「触发器」（枚举或类）
2. new StateMachine<状态, 触发器>(初始状态)
3. Configure(每个状态).Permit(触发器, 目标状态) 写规则
4. Fire(触发器) 触发转换
```

下面用英文命名再写一遍标准流程，方便你对照官方示例：

```csharp
// 1. 定义
enum MyState { A, B, C }
enum MyTrigger { Go }

// 2. 创建
var sm = new StateMachine<MyState, MyTrigger>(MyState.A);

// 3. 配置
sm.Configure(MyState.A).Permit(MyTrigger.Go, MyState.B);
sm.Configure(MyState.B).Permit(MyTrigger.Go, MyState.C);

// 4. 使用
sm.Fire(MyTrigger.Go);  // 从 A 变成 B
sm.Fire(MyTrigger.Go);  // 从 B 变成 C
```

---

## 第四部分：常用功能详解（按使用频率）

### 4.1 进入 / 离开状态时自动执行代码（OnEntry / OnExit）

**场景**：进入「播放」状态时开始计时，离开时停止计时。

**做法**：用 `OnEntry` 和 `OnExit`：

```csharp
sm.Configure(MyState.播放中)
    .OnEntry(() => {
        // 进入「播放中」时执行
        Console.WriteLine("开始播放，计时器启动");
        开始计时();
    })
    .OnExit(() => {
        // 离开「播放中」时执行
        Console.WriteLine("停止播放，计时器停止");
        停止计时();
    })
    .Permit(MyTrigger.暂停, MyState.已暂停);
```

- `OnEntry`：**每次进入**这个状态时调用
- `OnExit`：**每次离开**这个状态时调用

### 4.2 根据条件走不同分支（PermitIf 守卫）

**场景**：拨号时，如果号码有效就接通，无效就提示错误。

**做法**：用 `PermitIf` 加条件（守卫）：

```csharp
bool 号码有效 = true;  // 实际中这里可能是检查逻辑

sm.Configure(MyState.待机)
    .PermitIf(MyTrigger.拨号, MyState.接通中, () => 号码有效)
    .PermitIf(MyTrigger.拨号, MyState.错误提示, () => !号码有效);
```

含义：

- `PermitIf(触发器, 目标状态, () => 条件)`：**只有条件为 true 时**，才允许这次转换
- 两个 `PermitIf` 的条件要**互斥**（不能同时为 true）

### 4.3 处理事件但不改变状态（InternalTransition）

**场景**：播放中调音量，状态还是「播放中」，只是音量变了。

**做法**：用 `InternalTransition`，表示「响应触发器，但不换状态」：

```csharp
sm.Configure(MyState.播放中)
    .InternalTransition(MyTrigger.调音量, t => {
        // 这里可以调音量，但状态仍然是「播放中」
        调整音量();
    });
```

### 4.4 触发器带参数（SetTriggerParameters）

**场景**：分配任务时，要把「分配给谁」传进去。

**做法**：给触发器设置参数类型，再在 `Fire` 时传入：

```csharp
// 定义带 string 参数的触发器
var 分配任务 = sm.SetTriggerParameters<string>(MyTrigger.分配);

sm.Configure(MyState.已分配)
    .OnEntryFrom(分配任务, 邮箱 => {
        Console.WriteLine("任务已分配给: " + 邮箱);
    });

// 使用时传入参数
sm.Fire(分配任务, "zhangsan@company.com");
```

- `SetTriggerParameters<T>(触发器)`：声明这个触发器带一个 `T` 类型参数
- `OnEntryFrom(带参触发器, 参数 => { ... })`：进入状态时能拿到这个参数
- `Fire(带参触发器, 参数值)`：触发时把参数传进去

### 4.5 运行时决定去哪个状态（PermitDynamic）

**场景**：检查分数，小于 60 去「不及格」，大于等于 60 去「及格」。

**做法**：用 `PermitDynamic`，目标状态由函数返回值决定：

```csharp
int 分数 = 70;

sm.Configure(MyState.等待结果)
    .PermitDynamic(MyTrigger.检查, () => 分数 >= 60 ? MyState.及格 : MyState.不及格);

sm.Fire(MyTrigger.检查);  // 会根据 分数 自动跳到「及格」或「不及格」
```

### 4.6 某些状态下「忽略」某个触发器（Ignore）

**场景**：通话中再按一次拨号，不想做任何事，也不报错。

**做法**：用 `Ignore`：

```csharp
sm.Configure(MyState.通话中)
    .Ignore(MyTrigger.拨号);  // 在通话中拨号被忽略，不转换也不抛异常
```

### 4.7 监听状态变化（OnTransitioned）

**场景**：每次状态变化时打日志或更新 UI。

**做法**：用 `OnTransitioned`：

```csharp
sm.OnTransitioned(转换 => {
    Console.WriteLine($"从 {转换.Source} 变成 {转换.Destination}，触发: {转换.Trigger}");
});
```

`转换` 里有：`Source`（源状态）、`Destination`（目标状态）、`Trigger`（触发器）。

### 4.8 未定义的触发器默认会报错

如果调用了 `Fire(某个触发器)`，但当前状态下**没有**为这个触发器配置 `Permit`，会抛出异常。

不想报错可以自定义处理：

```csharp
sm.OnUnhandledTrigger((当前状态, 触发器) => {
    Console.WriteLine($"忽略: 在 {当前状态} 下触发了 {触发器}，没有对应规则");
});
```

---

## 第五部分：完整示例 - 门的状态（巩固理解）

门有 3 种状态：关闭、打开、上锁。用这个例子把前面的概念串起来。

```csharp
enum 门状态 { 关闭, 打开, 上锁 }
enum 门操作 { 推开, 关上, 上锁, 开锁 }

var 门 = new StateMachine<门状态, 门操作>(门状态.关闭);

// 关闭状态
门.Configure(门状态.关闭)
    .OnEntry(() => Console.WriteLine("门已关上"))
    .Permit(门操作.推开, 门状态.打开)
    .Permit(门操作.上锁, 门状态.上锁);

// 打开状态
门.Configure(门状态.打开)
    .OnEntry(() => Console.WriteLine("门已打开"))
    .Permit(门操作.关上, 门状态.关闭);

// 上锁状态
门.Configure(门状态.上锁)
    .Permit(门操作.开锁, 门状态.关闭)
    .Ignore(门操作.推开);  // 上锁时推不开

// 监听变化
门.OnTransitioned(t => 
    Console.WriteLine($"{t.Source} -> {t.Destination}"));

// 演示
门.Fire(门操作.推开);   // 关闭 -> 打开
门.Fire(门操作.关上);   // 打开 -> 关闭
门.Fire(门操作.上锁);   // 关闭 -> 上锁
门.Fire(门操作.推开);   // 被忽略（上锁中）
门.Fire(门操作.开锁);   // 上锁 -> 关闭
```

---

## 第六部分：小白常见问题

### Q1：状态和触发器可以用 string 吗？

可以。除了枚举，也可以用 `string`、`int` 等：

```csharp
var sm = new StateMachine<string, string>("idle");
sm.Configure("idle").Permit("start", "running");
sm.Fire("start");
```

### Q2：一个触发器可以转到多个状态吗？

可以，用 `PermitIf` 加不同条件，或者用 `PermitDynamic` 动态决定。但同一个触发器 + 同一组条件下，只能有一个目标状态。

### Q3：Fire 之后状态没变？

检查：  
1）当前状态是否正确；  
2）是否配置了 `Permit(这个触发器, 某状态)`；  
3）如果是 `PermitIf`，守卫条件是否满足。

### Q4：能在一个状态里配置多个 Permit 吗？

可以，不同触发器对应不同目标：

```csharp
sm.Configure(MyState.A)
    .Permit(Trigger.X, MyState.B)
    .Permit(Trigger.Y, MyState.C);
```

### Q5：状态要持久化到数据库怎么办？

用「外部状态存储」：状态存在你自己的变量/字段里，让状态机读写它：

```csharp
string 当前状态 = "idle";

var sm = new StateMachine<string, string>(
    () => 当前状态,        // 读
    s => 当前状态 = s     // 写
);
```

---

## 第七部分：常用 API 速查表

| 你想做什么           | 用什么 API                         |
|----------------------|------------------------------------|
| 允许转换             | `Permit(触发器, 目标状态)`         |
| 条件允许             | `PermitIf(触发器, 目标, () => 条件)` |
| 进入时执行           | `OnEntry(() => { })`               |
| 离开时执行           | `OnExit(() => { })`                |
| 响应但不换状态       | `InternalTransition(触发器, t => { })` |
| 触发器带参数         | `SetTriggerParameters<T>(触发器)`  |
| 动态决定目标         | `PermitDynamic(触发器, () => 目标)` |
| 忽略触发器           | `Ignore(触发器)`                   |
| 监听状态变化         | `OnTransitioned(t => { })`         |
| 触发                 | `Fire(触发器)` 或 `Fire(带参触发器, 参数)` |

---

## 第八部分：参考链接

- [GitHub](https://github.com/dotnet-state-machine/stateless)
- [NuGet](https://www.nuget.org/packages/Stateless)

---

*文档最后更新：2025年2月*
