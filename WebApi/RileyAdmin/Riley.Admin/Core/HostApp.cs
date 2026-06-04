using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using Riley.Admin.Auth.Dto;
using Riley.Admin.Core.Auth;
using Riley.Admin.Core.Configs;
using Riley.Admin.Services.Db;
using Riley.Admin.Services.Db.Models;
using Riley.Admin.Services.LoginLog;
using Riley.Admin.Services.LoginLog.Dto;
using Riley.Admin.Tools.Cache;
using Riley.Common.Helpers;
using System.Reflection;
using System.Text;

namespace Riley.Admin.Core
{
    public class HostApp
    {
        public HostApp() { }

        public void Run(string[] args, Assembly assembly = null)
        {
            var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetLogger("db");
            try
            {
                logger.Info("Applations startup");

                var builder = WebApplication.CreateBuilder(args);
                builder.ConfigureApplication(assembly ?? Assembly.GetCallingAssembly());

                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

                var services = builder.Services;
                var env = builder.Environment;
                var configuration = builder.Configuration;

                configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                ///配置
                services.Configure<DbConfig>(ConfigHelper.Load("dbconfig", env.EnvironmentName));
                services.Configure<JwtConfig>(ConfigHelper.Load("jwtconfig", env.EnvironmentName));
                services.AddDbContext<AdminContext>(context =>
                {
                    var dbConfig = services
                        .BuildServiceProvider()
                        .GetService<IOptions<DbConfig>>()
                        ?.Value;
                    context.UseMySql(
                        dbConfig?.ConnectionString,
                        ServerVersion.Parse("8.0.36-mysql")
                    );
                });
                services.AddHttpClient();
                services.AddControllers();
                services.AddEndpointsApiExplorer();

                services.AddSwaggerGen(s =>
                {
                    s.SwaggerDoc(
                        "v1",
                        new Microsoft.OpenApi.Models.OpenApiInfo
                        {
                            Title = "My API",
                            Version = "v1"
                        }
                    );
                    s.AddSecurityDefinition(
                        "Bearer",
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
                        {
                            Description = "在下框中输入请求头中需要添加Jwt授权Token: Bearer Token",
                            Name = "Authorization",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                            BearerFormat = "JWT",
                            Scheme = "Bearer"
                        }
                    );

                    s.AddSecurityRequirement(
                        new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                Array.Empty<string>()
                            }
                        }
                    );
                });

                ConfigureService(services, env, configuration);
                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                app.MapControllers();
                app.Run();
                logger.Info("Application shutdown");
            }
            catch (Exception ex)
            {
                //应用程序异常
                logger.Error(ex, "Application stopped because of exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public void ConfigureService(
            IServiceCollection services,
            IWebHostEnvironment env,
            IConfiguration configuration
        )
        {
            #region 认证

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = nameof(ResponseAuthenticationHandler); //401
                    options.DefaultForbidScheme = nameof(ResponseAuthenticationHandler); //403
                })
                .AddJwtBearer(options =>
                {
                    var dbConfig = services
                        .BuildServiceProvider()
                        .GetService<IOptions<JwtConfig>>()
                        ?.Value;
                    options.TokenValidationParameters =
                        new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = dbConfig?.Issuer,
                            ValidateAudience = true,
                            ValidAudience = dbConfig?.Audience,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(dbConfig?.SecurityKey ?? string.Empty)
                            ),
                            ClockSkew = TimeSpan.FromMinutes(5)
                        };
                })
                .AddScheme<AuthenticationSchemeOptions, ResponseAuthenticationHandler>(
                    nameof(ResponseAuthenticationHandler),
                    o => { }
                );

            #endregion

            #region 缓存

            services.AddMemoryCache();

            services.AddSingleton<ICacheTools, MemoryCacheTool>();
            services.AddDistributedMemoryCache();

            #endregion


            services.AddSingleton<IUserToken, UserToken>();
            services.AddSingleton(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
            services.AddTransient<ILoginLogService, LoginLogService>();

            #region AutoMapper映射配置
            services.AddAutoMapper(cfp =>
            {
                cfp.CreateMap<AdUser, AuthLoginOutput>();
                cfp.CreateMap<LoginLogAddInput, AdLoginLog>();
            });

            #endregion
        }
    }
}
