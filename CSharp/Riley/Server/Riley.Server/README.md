# Riley.Server - 多数据库支持的EntityFramework

## 概述

本项目已集成 EntityFramework Core 8.0，支持多种数据库（SQL Server、PostgreSQL、MySQL），可通过配置轻松切换不同的数据库连接。

## 支持的数据库

- ✅ **SQL Server** - 使用 `Microsoft.EntityFrameworkCore.SqlServer`
- ✅ **PostgreSQL** - 使用 `Npgsql.EntityFrameworkCore.PostgreSQL`
- ✅ **MySQL** - 使用 `Pomelo.EntityFrameworkCore.MySql`

## 项目结构

```
Riley.Server/
├── Configuration/         # 配置模型
│   └── DatabaseSettings.cs    # 数据库配置类
├── Controllers/           # API控制器
│   ├── UsersController.cs        # 用户管理API
│   ├── ProductsController.cs     # 产品管理API
│   └── DatabaseController.cs      # 数据库信息API
├── Data/                  # 数据访问层
│   └── ApplicationDbContext.cs    # EF数据库上下文
├── Extensions/            # 扩展方法
│   └── DatabaseServiceExtensions.cs # 数据库服务扩展
├── Models/               # 实体模型
│   └── User.cs           # 用户和产品实体
├── Services/             # 业务服务
│   └── DatabaseInitializer.cs # 数据库初始化服务
├── appsettings.json      # 默认配置
├── appsettings.PostgreSQL.json # PostgreSQL配置
├── appsettings.MySQL.json      # MySQL配置
└── Program.cs           # 应用程序入口
```

## 数据库配置

### 配置文件结构

在 `appsettings.json` 中配置数据库连接：

```json
{
  "DatabaseSettings": {
    "Provider": "SqlServer",
    "ConnectionStrings": {
      "SqlServer": "Server=(localdb)\\mssqllocaldb;Database=RileyServerDb;Trusted_Connection=true;MultipleActiveResultSets=true",
      "PostgreSQL": "Host=localhost;Database=RileyServerDb;Username=postgres;Password=your_password",
      "MySQL": "Server=localhost;Database=RileyServerDb;Uid=root;Pwd=your_password;"
    }
  }
}
```

### 支持的数据库提供程序

- `SqlServer` - SQL Server数据库
- `PostgreSQL` - PostgreSQL数据库
- `MySQL` - MySQL数据库

### 连接字符串格式

#### SQL Server
```
Server=server_name;Database=database_name;Trusted_Connection=true;MultipleActiveResultSets=true
```
或
```
Server=server_name;Database=database_name;User Id=username;Password=password;MultipleActiveResultSets=true
```

#### PostgreSQL
```
Host=hostname;Database=database_name;Username=username;Password=password
```

#### MySQL
```
Server=hostname;Database=database_name;Uid=username;Pwd=password;
```

## 环境配置

### 使用不同环境的配置

1. **SQL Server环境**：
   ```bash
   dotnet run --environment SqlServer
   ```

2. **PostgreSQL环境**：
   ```bash
   dotnet run --environment PostgreSQL
   ```

3. **MySQL环境**：
   ```bash
   dotnet run --environment MySQL
   ```

### 环境变量配置

也可以通过环境变量设置数据库提供程序：

```bash
# Windows
set DatabaseSettings__Provider=PostgreSQL

# Linux/macOS
export DatabaseSettings__Provider=PostgreSQL
```

## API 端点

### 数据库管理 API

- `GET /api/database/info` - 获取数据库信息
- `GET /api/database/test-connection` - 测试数据库连接
- `GET /api/database/statistics` - 获取数据库统计信息

### 用户管理 API

- `GET /api/users` - 获取所有用户
- `GET /api/users/{id}` - 根据ID获取用户
- `POST /api/users` - 创建新用户
- `PUT /api/users/{id}` - 更新用户信息
- `DELETE /api/users/{id}` - 删除用户(软删除)

### 产品管理 API

- `GET /api/products` - 获取所有产品
- `GET /api/products/{id}` - 根据ID获取产品
- `POST /api/products` - 创建新产品
- `PUT /api/products/{id}` - 更新产品信息
- `DELETE /api/products/{id}` - 删除产品(软删除)
- `GET /api/products/search?name={name}&minPrice={min}&maxPrice={max}` - 搜索产品

## 数据库迁移

### 创建迁移
```bash
dotnet ef migrations add InitialCreate
```

### 更新数据库
```bash
dotnet ef database update
```

### 生成SQL脚本
```bash
dotnet ef migrations script
```

## 运行项目

### 前置条件

1. 确保已安装 .NET 8.0 SDK
2. 根据选择的数据库，确保相应的数据库服务已安装并运行

### 数据库安装指南

#### SQL Server
- 安装 SQL Server LocalDB 或 SQL Server Express
- 或使用 Azure SQL Database

#### PostgreSQL
- 安装 PostgreSQL 数据库服务器
- 创建数据库和用户
- 更新连接字符串中的用户名和密码

#### MySQL
- 安装 MySQL 数据库服务器
- 创建数据库和用户
- 更新连接字符串中的用户名和密码

### 运行步骤

1. 修改 `appsettings.json` 中的连接字符串
2. 选择数据库提供程序（修改 `Provider` 字段）
3. 运行项目：
   ```bash
   cd Server/Riley.Server
   dotnet run
   ```
4. 访问 Swagger UI：`https://localhost:7001/swagger`

## 特性

- ✅ **多数据库支持** - SQL Server、PostgreSQL、MySQL
- ✅ **配置驱动** - 通过配置文件切换数据库
- ✅ **自动重试** - 数据库连接失败时自动重试
- ✅ **数据库兼容性** - 自动适配不同数据库的SQL语法
- ✅ **连接测试** - 提供数据库连接测试API
- ✅ **统计信息** - 提供数据库统计信息API
- ✅ **EntityFramework Core 8.0**
- ✅ **自动数据库初始化**
- ✅ **种子数据**
- ✅ **完整的CRUD操作**
- ✅ **软删除支持**
- ✅ **数据验证**
- ✅ **异常处理和日志记录**
- ✅ **Swagger API文档**
- ✅ **异步操作**

## 数据库特定功能

### SQL Server
- 使用 `GETUTCDATE()` 作为默认时间戳
- 支持 `decimal(18,2)` 数据类型

### PostgreSQL
- 使用 `CURRENT_TIMESTAMP` 作为默认时间戳
- 支持 `numeric(18,2)` 数据类型
- 支持 JSON 数据类型（可扩展）

### MySQL
- 使用 `CURRENT_TIMESTAMP` 作为默认时间戳
- 支持 `decimal(18,2)` 数据类型
- 自动检测服务器版本

## 扩展建议

1. **添加数据库连接池配置** - 针对不同数据库优化连接池设置
2. **添加数据库性能监控** - 集成性能监控工具
3. **添加数据库备份API** - 提供数据库备份和恢复功能
4. **添加数据库迁移API** - 通过API执行数据库迁移
5. **添加多租户支持** - 支持多数据库多租户架构
6. **添加数据库版本管理** - 管理不同数据库版本
7. **添加数据库健康检查** - 定期检查数据库健康状态

## 注意事项

- 所有删除操作都是软删除，通过 `IsActive` 或 `IsAvailable` 字段控制
- 邮箱字段有唯一索引，确保数据一致性
- 使用 UTC 时间存储时间戳
- 所有数据库操作都是异步的，提高性能
- 连接字符串中的密码会被API自动掩码显示
- 不同数据库的SQL语法差异已自动处理
- 建议在生产环境中使用环境变量存储敏感信息
