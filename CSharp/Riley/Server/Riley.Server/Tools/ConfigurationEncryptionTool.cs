using Riley.Server.Services;
using System.Text.Json;

namespace Riley.Server.Tools
{
    /// <summary>
    /// 配置加密工具
    /// 用于加密和解密配置文件中的敏感信息
    /// </summary>
    public static class ConfigurationEncryptionTool
    {
        /// <summary>
        /// 运行加密工具
        /// </summary>
        /// <param name="args">命令行参数</param>
        public static async Task RunAsync(string[] args)
        {
            Console.WriteLine("=== Riley Server 配置加密工具 ===");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0].ToLower();
            
            switch (command)
            {
                case "encrypt":
                    await EncryptCommand(args);
                    break;
                case "decrypt":
                    await DecryptCommand(args);
                    break;
                case "encrypt-config":
                    await EncryptConfigFileCommand(args);
                    break;
                case "generate-key":
                    GenerateEncryptionKey();
                    break;
                case "help":
                case "-h":
                case "--help":
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine($"未知命令: {command}");
                    ShowHelp();
                    break;
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("用法:");
            Console.WriteLine("  dotnet run -- encrypt \"要加密的字符串\"");
            Console.WriteLine("  dotnet run -- decrypt \"ENC:加密的字符串\"");
            Console.WriteLine("  dotnet run -- encrypt-config [配置文件路径]");
            Console.WriteLine("  dotnet run -- generate-key");
            Console.WriteLine();
            Console.WriteLine("命令说明:");
            Console.WriteLine("  encrypt        加密指定的字符串");
            Console.WriteLine("  decrypt        解密指定的字符串");
            Console.WriteLine("  encrypt-config 加密配置文件中的数据库连接字符串");
            Console.WriteLine("  generate-key   生成新的加密密钥");
            Console.WriteLine();
            Console.WriteLine("环境变量:");
            Console.WriteLine("  RILEY_ENCRYPTION_KEY  - 加密密钥（必需）");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  dotnet run -- encrypt \"Server=localhost;Database=test;\"");
            Console.WriteLine("  dotnet run -- encrypt-config appsettings.json");
        }

        private static Task EncryptCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("错误: 请提供要加密的字符串");
                Console.WriteLine("用法: dotnet run -- encrypt \"要加密的字符串\"");
                return Task.CompletedTask;
            }

            try
            {
                var encryptionService = CreateEncryptionService();
                var plainText = args[1];
                var encrypted = encryptionService.Encrypt(plainText);
                
                Console.WriteLine("加密成功!");
                Console.WriteLine($"原始字符串: {plainText}");
                Console.WriteLine($"加密字符串: {encrypted}");
                Console.WriteLine();
                Console.WriteLine("请将加密字符串复制到配置文件中替换原始连接字符串。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加密失败: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private static Task DecryptCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("错误: 请提供要解密的字符串");
                Console.WriteLine("用法: dotnet run -- decrypt \"ENC:加密的字符串\"");
                return Task.CompletedTask;
            }

            try
            {
                var encryptionService = CreateEncryptionService();
                var encryptedText = args[1];
                var decrypted = encryptionService.Decrypt(encryptedText);
                
                Console.WriteLine("解密成功!");
                Console.WriteLine($"加密字符串: {encryptedText}");
                Console.WriteLine($"解密字符串: {decrypted}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解密失败: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private static async Task EncryptConfigFileCommand(string[] args)
        {
            var configPath = args.Length > 1 ? args[1] : "appsettings.json";
            
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"错误: 配置文件不存在: {configPath}");
                return;
            }

            try
            {
                Console.WriteLine($"正在处理配置文件: {configPath}");
                
                var jsonContent = await File.ReadAllTextAsync(configPath);
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var options = new JsonSerializerOptions { WriteIndented = true };
                
                var configData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);
                if (configData == null)
                {
                    Console.WriteLine("错误: 无法解析配置文件");
                    return;
                }

                var encryptionService = CreateEncryptionService();
                bool hasChanges = false;

                // 处理数据库设置
                if (configData.TryGetValue("DatabaseSettings", out var dbSettingsObj))
                {
                    var dbSettingsJson = JsonSerializer.Serialize(dbSettingsObj);
                    var dbSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(dbSettingsJson);
                    
                    if (dbSettings?.TryGetValue("ConnectionStrings", out var connStringsObj) == true)
                    {
                        var connStringsJson = JsonSerializer.Serialize(connStringsObj);
                        var connStrings = JsonSerializer.Deserialize<Dictionary<string, string>>(connStringsJson);
                        
                        if (connStrings != null)
                        {
                            foreach (var kvp in connStrings.ToList())
                            {
                                if (!encryptionService.IsEncrypted(kvp.Value))
                                {
                                    var encrypted = encryptionService.Encrypt(kvp.Value);
                                    connStrings[kvp.Key] = encrypted;
                                    hasChanges = true;
                                    Console.WriteLine($"已加密连接字符串: {kvp.Key}");
                                }
                                else
                                {
                                    Console.WriteLine($"连接字符串已加密: {kvp.Key}");
                                }
                            }
                            
                            dbSettings["ConnectionStrings"] = connStrings;
                            configData["DatabaseSettings"] = dbSettings;
                        }
                    }
                }

                if (hasChanges)
                {
                    // 创建备份
                    var backupPath = $"{configPath}.backup.{DateTime.Now:yyyyMMddHHmmss}";
                    File.Copy(configPath, backupPath);
                    Console.WriteLine($"原始配置文件已备份到: {backupPath}");

                    // 保存加密后的配置
                    var encryptedJson = JsonSerializer.Serialize(configData, options);
                    await File.WriteAllTextAsync(configPath, encryptedJson);
                    
                    Console.WriteLine("配置文件加密完成!");
                }
                else
                {
                    Console.WriteLine("没有需要加密的连接字符串。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理配置文件失败: {ex.Message}");
            }
        }

        private static void GenerateEncryptionKey()
        {
            var key = GenerateRandomKey(64); // 生成64字符的密钥
            
            Console.WriteLine("已生成新的加密密钥:");
            Console.WriteLine(key);
            Console.WriteLine();
            Console.WriteLine("请将此密钥设置为环境变量或配置文件中的 EncryptionSettings:Key");
            Console.WriteLine("环境变量设置命令:");
            Console.WriteLine($"set RILEY_ENCRYPTION_KEY={key}");
            Console.WriteLine();
            Console.WriteLine("或在 appsettings.json 中添加:");
            Console.WriteLine("{");
            Console.WriteLine("  \"EncryptionSettings\": {");
            Console.WriteLine($"    \"Key\": \"{key}\"");
            Console.WriteLine("  }");
            Console.WriteLine("}");
        }

        private static string GenerateRandomKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static IConfigurationEncryptionService CreateEncryptionService()
        {
            // 创建临时配置来获取加密密钥
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();
            
            var configuration = configBuilder.Build();
            
            // 创建日志记录器
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<ConfigurationEncryptionService>();
            
            return new ConfigurationEncryptionService(configuration, logger);
        }
    }
}
