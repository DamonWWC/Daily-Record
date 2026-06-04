using Microsoft.EntityFrameworkCore;
using Riley.Server.Auth.Models;

namespace Riley.Server.Auth.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options):base(options)
        {
                
        }
        /// <summary>
        /// 用户表
        /// </summary>
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 配置User实体
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                
                // 配置默认值
                entity.Property(e => e.Role)
                    .HasMaxLength(100)
                    .HasDefaultValue("User");
                    
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql(GetCurrentTimestampSql());
                    
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // 创建唯一索引
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });
        }
        /// <summary>
        /// 获取当前时间戳的SQL表达式（兼容不同数据库）
        /// </summary>
        private string GetCurrentTimestampSql()
        {
            var providerName = Database.ProviderName;
            return providerName switch
            {
                "Microsoft.EntityFrameworkCore.SqlServer" => "GETUTCDATE()",
                "Npgsql.EntityFrameworkCore.PostgreSQL" => "CURRENT_TIMESTAMP",
                "Pomelo.EntityFrameworkCore.MySql" => "CURRENT_TIMESTAMP",
                _ => "CURRENT_TIMESTAMP"
            };
        }

    }
}
