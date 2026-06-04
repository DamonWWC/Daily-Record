# Riley Server 配置加密指南

## 🔐 概述

Riley Server 现已支持数据库连接字符串的加密存储，使用 AES-256 加密算法保护敏感的配置信息。

## ✨ 功能特性

- ✅ AES-256 对称加密
- ✅ 自动解密运行时配置
- ✅ 命令行加密工具
- ✅ 配置文件批量加密
- ✅ 自动备份原始配置
- ✅ 多环境密钥支持

## 🚀 快速开始

### 1. 生成加密密钥

```bash
dotnet run -- generate-key
```

输出示例：
```
已生成新的加密密钥:
Riley_Encryption_Key_2024_A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0

请将此密钥设置为环境变量或配置文件中的 EncryptionSettings:Key
环境变量设置命令:
set RILEY_ENCRYPTION_KEY=Riley_Encryption_Key_2024_A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8S9T0
```

### 2. 配置加密密钥

#### 方法一：配置文件（推荐用于开发环境）
在 `appsettings.json` 中添加：
```json
{
  "EncryptionSettings": {
    "Key": "你的64位加密密钥"
  }
}
```

#### 方法二：环境变量（推荐用于生产环境）
```bash
# Windows
set RILEY_ENCRYPTION_KEY=你的64位加密密钥

# Linux/Mac
export RILEY_ENCRYPTION_KEY=你的64位加密密钥
```

### 3. 加密数据库连接字符串

#### 批量加密配置文件
```bash
dotnet run -- encrypt-config appsettings.json
```

#### 单独加密字符串
```bash
dotnet run -- encrypt "Server=localhost;Database=test;User=admin;Password=secret;"
```

#### 解密字符串（用于验证）
```bash
dotnet run -- decrypt "ENC:加密的字符串"
```

## 📋 命令参考

### 命令列表
| 命令 | 说明 | 示例 |
|------|------|------|
| `generate-key` | 生成新的加密密钥 | `dotnet run -- generate-key` |
| `encrypt` | 加密指定字符串 | `dotnet run -- encrypt "连接字符串"` |
| `decrypt` | 解密指定字符串 | `dotnet run -- decrypt "ENC:加密字符串"` |
| `encrypt-config` | 加密配置文件 | `dotnet run -- encrypt-config appsettings.json` |

### 命令详解

#### generate-key
生成一个64字符的随机加密密钥。

```bash
dotnet run -- generate-key
```

#### encrypt
加密单个连接字符串或敏感信息。

```bash
dotnet run -- encrypt "Server=localhost;Database=MyDB;User=admin;Password=secret123;"
```

输出：
```
加密成功!
原始字符串: Server=localhost;Database=MyDB;User=admin;Password=secret123;
加密字符串: ENC:wKqYl6dsLEIAot5CqXPDNgJDQE6ndBMyxKyP9XRCzXo...
请将加密字符串复制到配置文件中替换原始连接字符串。
```

#### decrypt
解密已加密的字符串，用于验证或调试。

```bash
dotnet run -- decrypt "ENC:wKqYl6dsLEIAot5CqXPDNgJDQE6ndBMyxKyP9XRCzXo..."
```

#### encrypt-config
批量加密配置文件中的数据库连接字符串。

```bash
dotnet run -- encrypt-config appsettings.json
```

功能特性：
- 自动检测未加密的连接字符串
- 跳过已加密的字符串
- 自动创建备份文件
- 保持JSON格式完整

## 🔧 配置文件格式

### 加密前的配置
```json
{
  "DatabaseSettings": {
    "Provider": "PostgreSQL",
    "ConnectionStrings": {
      "SqlServer": "Server=(localdb)\\mssqllocaldb;Database=RileyServerDb;Trusted_Connection=true;",
      "PostgreSQL": "Host=localhost;Port=5432;Database=rileydb;Username=admin;Password=secret;",
      "MySQL": "Server=localhost;Database=RileyServerDb;Uid=root;Pwd=password;"
    }
  }
}
```

### 加密后的配置
```json
{
  "DatabaseSettings": {
    "Provider": "PostgreSQL",
    "ConnectionStrings": {
      "SqlServer": "ENC:BupTbOmaDT6ZFKCN9MggxifOPprDvvdVSfn5P+7PQ7QX6Hi1jmKT...",
      "PostgreSQL": "ENC:Jk2iqA4eTDNULeMOarXKW3nECXwUN73Vl4L3FnJjacZuGy7tbTDj...",
      "MySQL": "ENC:ilAySAA6idNaZVXtvtvPRRQ5sbrltSoEac6ADKK7dTzg2yOKmRLl..."
    }
  }
}
```

## 🔒 安全最佳实践

### 1. 密钥管理
- **生产环境**：使用环境变量存储加密密钥
- **开发环境**：可以使用配置文件，但不要提交到版本控制
- **密钥强度**：使用至少64个字符的随机密钥
- **密钥轮换**：定期更换加密密钥

### 2. 部署安全
```bash
# 生产环境部署示例
export RILEY_ENCRYPTION_KEY=你的生产环境密钥
dotnet Riley.Server.dll
```

### 3. 备份管理
- 加密工具会自动创建配置文件备份
- 备份文件命名格式：`appsettings.json.backup.yyyyMMddHHmmss`
- 生产环境中应及时删除备份文件

### 4. 权限控制
- 限制对配置文件的访问权限
- 限制对加密密钥的访问
- 使用最小权限原则

## 🛠️ 故障排除

### 常见问题

#### 1. "未配置加密密钥"错误
**错误信息**：
```
未配置加密密钥。请在配置文件中设置 EncryptionSettings:Key 或环境变量 RILEY_ENCRYPTION_KEY
```

**解决方法**：
- 检查 `appsettings.json` 中的 `EncryptionSettings:Key`
- 或设置环境变量 `RILEY_ENCRYPTION_KEY`

#### 2. "加密密钥长度必须至少32个字符"错误
**解决方法**：
- 使用 `dotnet run -- generate-key` 生成新密钥
- 确保密钥长度至少32个字符

#### 3. "解密失败，可能是密钥错误或数据损坏"错误
**解决方法**：
- 检查加密密钥是否正确
- 验证加密字符串是否完整
- 确认使用相同的加密密钥

#### 4. 连接字符串解密失败
**检查步骤**：
1. 验证密钥配置
2. 检查字符串格式（必须以 `ENC:` 开头）
3. 测试解密工具：`dotnet run -- decrypt "ENC:..."`

### 调试工具

#### 验证加密/解密
```bash
# 1. 加密测试字符串
dotnet run -- encrypt "test_connection_string"

# 2. 复制输出的加密字符串，然后解密验证
dotnet run -- decrypt "ENC:输出的加密字符串"

# 3. 检查解密结果是否与原始字符串一致
```

#### 检查配置加载
在应用启动时，查看日志中的解密相关信息：
- "数据库连接字符串已解密"
- "数据库连接字符串未加密，建议加密存储"

## 📈 性能考虑

### 解密性能
- 解密操作在应用启动时执行一次
- 运行时使用解密后的连接字符串
- 对应用性能几乎无影响

### 内存安全
- 解密后的连接字符串仅在内存中存在
- 应用关闭后自动清理
- 不会持久化明文连接字符串

## 🔄 迁移指南

### 从明文配置迁移到加密配置

1. **备份现有配置**
   ```bash
   cp appsettings.json appsettings.json.backup
   ```

2. **生成加密密钥**
   ```bash
   dotnet run -- generate-key
   ```

3. **配置加密密钥**
   ```bash
   # 设置环境变量（推荐）
   export RILEY_ENCRYPTION_KEY=生成的密钥
   ```

4. **加密配置文件**
   ```bash
   dotnet run -- encrypt-config appsettings.json
   ```

5. **测试应用启动**
   ```bash
   dotnet run
   ```

6. **验证数据库连接**
   访问需要数据库的API接口确认连接正常。

### 更换加密密钥

1. **生成新密钥**
2. **解密现有配置**（使用旧密钥）
3. **更新密钥配置**
4. **重新加密配置**（使用新密钥）

## ⚡ 环境配置示例

### 开发环境
```json
// appsettings.Development.json
{
  "EncryptionSettings": {
    "Key": "Riley_Dev_Key_2024_..."
  }
}
```

### 生产环境
```bash
# Docker
ENV RILEY_ENCRYPTION_KEY=Riley_Prod_Key_2024_...

# systemd 服务
Environment=RILEY_ENCRYPTION_KEY=Riley_Prod_Key_2024_...

# Azure App Service
# 在应用设置中添加 RILEY_ENCRYPTION_KEY
```

---

🎉 **恭喜！** 您的 Riley Server 配置现在已经安全加密了！
