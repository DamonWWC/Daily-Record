using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RileyAdmin.自定义配置提供程序;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RileyAdmin.选项;
using Microsoft.Extensions.Options;

namespace RileyAdmin
{
    public class Program
    {
       
        public static void Main(string[] args)
        {
         
            var builder = WebApplication.CreateBuilder(args);


            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            // Add services to the container.
            ConfigurationManager configurationManagee = new ConfigurationManager();
            //builder.Services.Configure<PositionOptions>(
            //    builder.Configuration.GetSection(PositionOptions.Position));

            builder.Services.AddOptions<PositionOptions>().Bind(builder.Configuration.GetSection(PositionOptions.Position));


            builder.Services.PostConfigure<PositionOptions>( myoptios =>
            {
                myoptios.Title = "demo";
            });



            var config = builder.Configuration.GetSection(PositionOptions.Position).Get<PositionOptions>();



         
            builder.Configuration.AddEFConfiguration(opt => opt.UseInMemoryDatabase("InMemoryDb"));
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(s=>
            {
                s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" });
                s.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
                {
                    Description="在下框中输入请求头中需要添加Jwt授权Token: Bearer Token",
                    Name="Authorization",
                    In=Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type=Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    BearerFormat="JWT",
                    Scheme="Bearer"
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement
                { 
                    {
                        new OpenApiSecurityScheme{
                            Reference=new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer" }
                            },Array.Empty<string>()
                    }     
                });
            });

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("JWT:SignKey"))),
                    ValidIssuer = builder.Configuration.GetValue<string>("JWT:ISyouuser"),
                    ValidAudience = builder.Configuration.GetValue<string>(key: "JWT:IsAudience"),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew=TimeSpan.Zero
                };
            });
               

            builder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy.
                //options.FallbackPolicy = options.DefaultPolicy;
            });


            builder.Services.Configure<MvcOptions>(o =>
            {

            });



            var app = builder.Build();
            
          
            app.Use(async(context, next) =>
            {
                 if(context.Request.Path=="/")
                {
                    await context.Response.WriteAsync("Terminal Middleware");
                    return;
                }
                await next(context);
            });


            app.MapGet("/Routing/{message:alpha}", (string message) => $"Hello{message}");
            
            var aa = app.Services.GetRequiredService<IOptions<PositionOptions>>().Value;
           
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

           

            app.MapControllers();

            app.Run();
        }
    }
}
