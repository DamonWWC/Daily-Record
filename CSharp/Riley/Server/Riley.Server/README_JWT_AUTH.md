# Riley Server JWT 认证系统

## 概述

Riley Server 已集成完整的 JWT（JSON Web Token）认证系统，支持用户注册、登录、token 验证等功能。

## 功能特性

- ✅ 用户注册与登录
- ✅ JWT Token 生成与验证
- ✅ 密码哈希存储（使用 BCrypt）
- ✅ 用户信息管理
- ✅ Token 刷新功能
- ✅ 请求参数验证
- ✅ Swagger 文档集成
- ✅ 多环境配置支持

## API 接口

### 1. 用户注册
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "用户名",
  "name": "姓名", 
  "email": "邮箱地址",
  "password": "密码",
  "confirmPassword": "确认密码",
  "phone": "手机号（可选）"
}
```

### 2. 用户登录
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "用户名",
  "password": "密码"
}
```

**响应示例：**
```json
{
  "success": true,
  "message": "登录成功",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiresIn": 1704067200,
    "user": {
      "id": 1,
      "username": "testuser",
      "name": "测试用户",
      "email": "test@example.com",
      "phone": "13800138000",
      "role": "User",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### 3. 获取当前用户信息
```
GET /api/auth/me
Authorization: Bearer {token}
```

### 4. 刷新Token
```
POST /api/auth/refresh
Authorization: Bearer {token}
```

### 5. 用户登出
```
POST /api/auth/logout
Authorization: Bearer {token}
```

## 配置说明

### JWT 配置项（appsettings.json）

```json
{
  "JwtSettings": {
    "SecretKey": "你的密钥",
    "Issuer": "Riley.Server",
    "Audience": "Riley.Client", 
    "ExpirationMinutes": 60
  }
}
```

**配置说明：**
- `SecretKey`: JWT 签名密钥，生产环境必须使用强密钥
- `Issuer`: JWT 签发者标识
- `Audience`: JWT 受众标识
- `ExpirationMinutes`: Token 过期时间（分钟）

## 使用方法

### 1. 在控制器中使用认证

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // 需要认证
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // 获取当前用户ID
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdClaim.Value);
        
        // 业务逻辑...
        return Ok();
    }
    
    [HttpGet("public")]
    [AllowAnonymous] // 允许匿名访问
    public IActionResult GetPublicData()
    {
        return Ok("这是公开数据");
    }
}
```

### 2. 客户端使用Token

```javascript
// 登录获取token
const loginResponse = await fetch('/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    username: 'testuser',
    password: '123456'
  })
});

const { data } = await loginResponse.json();
const token = data.accessToken;

// 使用token访问受保护的API
const response = await fetch('/api/auth/me', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

### 3. WPF 客户端使用示例

```csharp
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string? _accessToken;

    public ApiClient()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7000");
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var request = new { username, password };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/auth/login", content);
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseJson);
            
            if (result?.Success == true && result.Data != null)
            {
                _accessToken = result.Data.AccessToken;
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _accessToken);
                return true;
            }
        }
        return false;
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        var response = await _httpClient.GetAsync("/api/auth/me");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<UserInfo>>(json);
            return result?.Data;
        }
        return null;
    }
}
```

## 数据库结构

### Users 表结构

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键，自增 |
| Username | nvarchar(50) | 用户名，唯一索引 |
| Name | nvarchar(100) | 姓名 |
| Email | nvarchar(100) | 邮箱，唯一索引 |
| PasswordHash | nvarchar(255) | 密码哈希 |
| Phone | nvarchar(20) | 手机号，可空 |
| Role | nvarchar(100) | 角色，默认"User" |
| CreatedAt | datetime2 | 创建时间 |
| UpdatedAt | datetime2 | 更新时间，可空 |
| IsActive | bit | 是否激活，默认true |

## 安全注意事项

1. **密钥安全**：生产环境必须使用足够强的JWT密钥
2. **HTTPS**：生产环境必须使用HTTPS传输
3. **Token存储**：客户端应安全存储token，避免XSS攻击
4. **密码策略**：建议实施更强的密码策略
5. **速率限制**：建议添加登录尝试次数限制
6. **Token过期**：合理设置token过期时间

## 测试

使用提供的 `Riley.Server.http` 文件进行API测试，或访问 `/swagger` 查看完整的API文档。

## 常见问题

### 1. JWT验证失败
检查：
- Token是否正确传递
- Token是否已过期
- JWT配置是否正确

### 2. 用户注册失败
检查：
- 用户名/邮箱是否已存在
- 密码是否符合要求
- 请求参数是否完整

### 3. 数据库连接问题
检查：
- 数据库连接字符串
- 数据库迁移是否执行
- 用户权限是否正确
