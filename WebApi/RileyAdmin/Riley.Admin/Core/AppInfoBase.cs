using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Riley.Admin.Core
{
    internal static class AppInfoBase
    {
        internal static IServiceCollection Services;
        internal static IServiceProvider ServiceProvider;
        internal static IWebHostEnvironment WebHostEnvironment;
        internal static IConfiguration Configuration;
        internal static HostInfo HostInfo;

        internal static void ConfigureApplication(this WebApplicationBuilder webApplicationBuilder,Assembly assembly)
        {
            WebHostEnvironment = webApplicationBuilder.Environment;
            Services = webApplicationBuilder.Services;
            Configuration = webApplicationBuilder.Configuration;
            HostInfo = HostInfo.CreateInstance(assembly);
        }

        internal static void ConfigureApplication(this WebApplication app)
        {
            ServiceProvider = app.Services;
        }

    }
}
