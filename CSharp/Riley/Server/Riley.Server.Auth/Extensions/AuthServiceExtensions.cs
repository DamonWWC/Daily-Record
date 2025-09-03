using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Riley.Server.Auth.Data;
using Riley.Server.Auth.Services;
using System;
using System.Text;

namespace Riley.Server.Auth.Extensions
{
    /// <summary>
    /// 认证服务扩展
    /// </summary>
    public static class AuthServiceExtensions
    {
        /// <summary>
        /// 添加Riley认证模块服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddRileyAuthModule(this IServiceCollection services, IConfiguration configuration)
        {
            // 注册认证相关服务
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
            
            // 注册用户管理服务
            services.AddScoped<IUserManagementService, UserManagementService>();
            
            // 注册默认的用户仓储实现
            services.AddScoped<IUserRepository, UserRepository>();

            // 配置JWT认证
            services.AddJwtAuthentication(configuration);

            return services;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IServiceCollection AddAuthDbContext(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> action )
        {
            services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
            {
                action.Invoke(serviceProvider,options);
            });
            return services;
        }
        /// <summary>
        /// 添加JWT认证配置
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey未配置");
            var issuer = jwtSettings["Issuer"] ?? "Riley.Server";
            var audience = jwtSettings["Audience"] ?? "Riley.Client";

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // 不允许时间偏差
                    RequireExpirationTime = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("JWT");
                        logger.LogWarning("JWT认证失败: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("JWT");
                        logger.LogWarning("JWT认证挑战: {Error}", context.Error);
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }

        /// <summary>
        /// 注册用户仓储实现
        /// </summary>
        /// <typeparam name="TUserRepository">用户仓储实现类型</typeparam>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddUserRepository<TUserRepository>(this IServiceCollection services)
            where TUserRepository : class, IUserRepository
        {
            services.AddScoped<IUserRepository, TUserRepository>();
            return services;
        }
    }
}
