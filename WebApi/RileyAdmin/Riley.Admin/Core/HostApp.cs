using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Riley.Admin.Core.Configs;
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

        public void Run(string[] args, Assembly assembly=null)
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

                services.Configure<DbConfig>(ConfigHelper.Load("dbconfig", env.EnvironmentName));
                
                

                //var appConfig=AppInfo


                var app = builder.Build();
         


            }
            catch(Exception ex)
            {

            }
        }

      
    }
}
