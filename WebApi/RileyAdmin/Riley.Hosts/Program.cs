using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations.Schema;

namespace Riley.Hosts
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

            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();

            builder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy.
                options.FallbackPolicy = options.DefaultPolicy;
            });


            Load(builder.Configuration,"dbconfig");
            builder.Services.Configure<DbConfig>(builder.Configuration.GetSection("DbConfig"));



            var connectionString =  builder.Services.BuildServiceProvider().GetService<IOptions<DbConfig>>();
          
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 29));
            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseMySql(connectionString.Value.ConnectionString, serverVersion));

            

            var app = builder.Build();
        
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


        public class ApplicationDbContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Api> Apis { get; set; }
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
            {
                
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                //modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
            }
        }

        public class BlobEntiyTypeConfiguration : IEntityTypeConfiguration<Blog>
        {
            public void Configure(EntityTypeBuilder<Blog> builder)
            {
                builder.Property(b => b.Url).IsRequired();
                builder.ToTable("blogs");

               
            }
        }

        [EntityTypeConfiguration(typeof(BlobEntiyTypeConfiguration))]
        public class Blog
        {
            public int BlogId { get; set; }
            public string Url { get; set; }

            public List<Post> Posts { get; } = new();
        }

        public class Post
        {
            public int PostId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }

            public int BlogId { get; set; }
            public Blog Blog { get; set; }
        }

        [Table("demo")]
        public class Api
        {
            public int Id { get; set; }
            public string Label { get; set; }
            public string? CreatedTime { get; set; } = null;
        }

        public static void Load(ConfigurationManager configurationManager,string fileName,string enviromentName="",bool optional=true,bool reloadOnChange=false)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Configs");

            if (!Directory.Exists(filePath))
                return ;

            
              configurationManager.SetBasePath(filePath)
                .AddJsonFile(fileName + ".json", optional, reloadOnChange);

            if(!string.IsNullOrWhiteSpace(enviromentName))
            {
                configurationManager.AddJsonFile(fileName + "." + enviromentName + ".json", optional: optional, reloadOnChange: reloadOnChange);
            }
            
        }
        public class DbConfig()
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public string ConnectionString { get; set; }
        }

    }
}
