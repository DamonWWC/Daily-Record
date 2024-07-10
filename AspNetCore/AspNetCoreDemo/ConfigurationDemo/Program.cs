using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ConfigurationDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //
            var configuration = new ConfigurationBuilder().
                AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    ["SomeKey"] = "SomeValue"
                }).Build();

            Console.WriteLine(configuration["SomeKey"]);
            //

            ///
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
         
            Settings? settings = config.GetRequiredSection("Settings").Get<Settings>();

            Console.WriteLine($"KeyOne = {settings?.KeyOne}");
            Console.WriteLine($"KeyTwo = {settings?.KeyTwo}");
            Console.WriteLine($"KeyThree:Message = {settings?.KeyThree?.Message}");

            ///
            //HostConfig(args);
            XmlConfig(args);
            /////
        



        }


        static async void XmlConfig(string[] args) 
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Configuration.Sources.Clear();

            builder.Configuration
                .AddXmlFile("appsettings.xml", optional: true, reloadOnChange: true);
                //.AddXmlFile("repeating-example.xml", optional: true, reloadOnChange: true);

            builder.Configuration.AddEnvironmentVariables();

            if (args is { Length: > 0 })
            {
                builder.Configuration.AddCommandLine(args);
            }


            IConfigurationRoot configurationRoot = builder.Configuration;

            string key00 = "section:section0:key:key0";
            string key01 = "section:section0:key:key1";
            string key10 = "section:section1:key:key0";
            string key11 = "section:section1:key:key1";

            string? val00 = configurationRoot[key00];
            string? val01 = configurationRoot[key01];
            string? val10 = configurationRoot[key10];
            string? val11 = configurationRoot[key11];

            Console.WriteLine($"{key00} = {val00}");
            Console.WriteLine($"{key01} = {val01}");
            Console.WriteLine($"{key10} = {val10}");
            Console.WriteLine($"{key10} = {val11}");

            using IHost host = builder.Build();

            // Application code should start here.

            await host.RunAsync();
        }
        static async void HostConfig(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.Sources.Clear();

            IHostEnvironment env = builder.Environment;

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

            Settings options = new Settings();

            builder.Configuration.GetSection("Settings").Bind(options);
            Console.WriteLine($"KeyOne = {options?.KeyOne}");
            Console.WriteLine($"KeyTwo = {options?.KeyTwo}");
            Console.WriteLine($"KeyThree:Message = {options?.KeyThree?.Message}");


            using IHost host = builder.Build();
            await host.RunAsync();
        }
    }




    public sealed class Settings
    {
        public  int KeyOne { get; set; }
        public  bool KeyTwo { get; set; }
        public  NestedSettings KeyThree { get; set; } = null!;
    }

    public sealed class NestedSettings
    {
        public required string Message { get; set; } = null!;
    }
}
