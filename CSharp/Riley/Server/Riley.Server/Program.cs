using Microsoft.EntityFrameworkCore;
using Riley.Server.Data;
using Riley.Server.Extensions;
using Riley.Server.Services;

namespace Riley.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.Services.AddDbContext<ApplicationDbContext>();
            // 添加多数据库支持的EntityFramework服务
            builder.Services.AddMultiDatabaseSupport(builder.Configuration);

            // 注册数据库初始化服务
            builder.Services.AddScoped<DatabaseInitializer>();

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

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
