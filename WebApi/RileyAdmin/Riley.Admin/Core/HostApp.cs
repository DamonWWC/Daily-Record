using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using Riley.Admin.Core.Configs;
using Riley.Admin.Services.Db;
using Riley.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Riley.Admin.Core
{
    public class HostApp
    {
        public HostApp()
        {
        }

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



                //var filePath = Path.Combine(AppContext.BaseDirectory, "Configs");

                //configuration
                //  .SetBasePath(filePath)
                //      .AddJsonFile($"dbconfig.json", true, false);

                //services.Configure<DBInfo>(configuration.GetSection("DbConfig"));



                //services.AddOptions<DBInfo>().Bind(ConfigHelper.Load("dbconfig", env.EnvironmentName));


                services.Configure<DBInfo>(ConfigHelper.Load("dbconfig", env.EnvironmentName).GetSection("DbConfig"));
           
                var re= AppInfoBase.Services.BuildServiceProvider().GetService<IOptions<DBInfo>>().Value;


                var aaaa1 = configuration.GetSection("DbConfig").Get<DBInfo>();
                var conStr = configuration.GetValue<DBInfo>("DbConfig");
                var aaaa = configuration["DbConfig"];
                var aa = configuration.GetValue<string>("AllowedHosts");
                services.AddDbContext<AdminContext>(context =>
                {
                    context.UseMySql(configuration.GetSection("DbConfig").Get<DbConfig>()?.ConnectionString, ServerVersion.Parse("8.0.36-mysql"));
                });

                var app = builder.Build();
                var a = app.Services.GetService<IConfiguration>();
                var info = a.GetSection("DbConfig").Get<DBInfo>();

                var infos = app.Services.GetRequiredService<IOptions<DBInfo>>().Value;
            }
            catch (Exception ex)
            {
            }
        }
    }

    public class DBInfo
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string ConnectionString { get; set; }
    }
}