using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;
using Riley.Admin.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace Riley.Hosts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new HostApp().Run(args, typeof(Program).Assembly);
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

       
        public class DbConfig()
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public string ConnectionString { get; set; }
        }

    }
}
