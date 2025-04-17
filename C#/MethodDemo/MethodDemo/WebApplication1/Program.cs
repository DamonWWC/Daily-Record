
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;
using WebApplication1.Service;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1
{
    public class Program
    {
        public class DateTimeOffsetJsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateTime.ParseExact(reader.GetString()!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            }
        }
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton< Transient>();
            builder.Services.AddScoped<Singleton>();
            builder.Services.AddScoped<Scope>();
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new DateTimeOffsetJsonConverter());
            });
            CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
            // builder.Services.AddSingleton<MyBackgroundService>();
            builder.Services.AddHostedService<MyBackgroundService>();
            var app = builder.Build();

            


            app.MapWhen(context => context.Request.Query.ContainsKey("branch"), HandleBranch);




            // Approach 1: Terminal Middleware.
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    await context.Response.WriteAsync("Terminal Middleware.");
                    return;
                }

                await next(context);
            });

            app.UseRouting();
           
            // Approach 2: Routing.
            app.MapGet("/Routing", async () =>
            {
               await _cancellationTokenSource.CancelAsync();
               
               //await aa.StopAsync(_cancellationTokenSource.Token);
            });
            app.MapGet("/Routing1", async ([FromServices] MyBackgroundService aa) =>
            {

              await  aa.StopAsync(CancellationToken.None);
            });


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
        static void HandleBranch(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var branchVer = context.Request.Query["branch"];
                await context.Response.WriteAsync($"Branch used = {branchVer}");
            });
        }

    }
    public class MyBackgroundService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 执行后台任务（例如：定时清理、数据处理）
                await ProcessDataAsync(stoppingToken);

                // 控制任务频率（例如：每5秒执行一次）
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ProcessDataAsync(CancellationToken token)
        {
            // 业务逻辑
            await Task.Delay(1000, token); // 模拟耗时操作
        }

        public override void Dispose()
        {
            // 释放资源（如数据库连接、文件句柄）
            base.Dispose();
        }
    }

}
