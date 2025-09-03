using Microsoft.EntityFrameworkCore;
using Riley.Server.Auth.Extensions;
using Riley.Server.Extensions;
using Riley.Server.Services;
using Riley.Server.Tools;

namespace Riley.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // 检查是否是加密工具命令
            if (args.Length > 0 && IsEncryptionToolCommand(args[0]))
            {
                await ConfigurationEncryptionTool.RunAsync(args);
                return;
            }
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Riley Server API",
                    Version = "v1",
                    Description = "Riley项目服务端API文档"
                });

                // 添加JWT认证配置
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
           
            // 添加多数据库支持的EntityFramework服务
            builder.Services.AddMultiDatabaseSupport(builder.Configuration);

            // 注册数据库初始化服务
            builder.Services.AddScoped<DatabaseInitializer>();

            // 注册Riley认证模块
            builder.Services.AddRileyAuthModule(builder.Configuration);
                     
            var app = builder.Build();

            // 初始化数据库
            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
                initializer.InitializeAsync().Wait();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // 使用认证和授权中间件
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static bool IsEncryptionToolCommand(string command)
        {
            var encryptionCommands = new[] { "encrypt", "decrypt", "encrypt-config", "generate-key" };
            return encryptionCommands.Contains(command.ToLower());
        }
    }
}
