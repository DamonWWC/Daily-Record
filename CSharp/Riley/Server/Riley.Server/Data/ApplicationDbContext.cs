using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Riley.Server.Configuration;
using Riley.Server.Models;

namespace Riley.Server.Data
{
    /// <summary>
    /// 应用程序数据库上下文
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// 用户表
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// 产品表
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// 订单表
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// 订单项表
        /// </summary>
        public DbSet<OrderItem> OrderItems { get; set; }
  
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
                entity.Property(e => e.Role).HasMaxLength(100).HasDefaultValue("User");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql(GetCurrentTimestampSql());
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // 创建唯一索引
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // 配置Product实体
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasColumnType(GetDecimalColumnType());
                entity.Property(e => e.StockQuantity).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql(GetCurrentTimestampSql());
                entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            });

            // 配置Order实体
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasColumnType(GetDecimalColumnType()).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.ShippingAddress).HasMaxLength(500);
                entity.Property(e => e.ContactPhone).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql(GetCurrentTimestampSql());

                // 创建唯一索引
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            
            });

            // 配置OrderItem实体
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId).IsRequired();
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).HasColumnType(GetDecimalColumnType()).IsRequired();
                entity.Property(e => e.TotalPrice).HasColumnType(GetDecimalColumnType()).IsRequired();

                // 创建索引
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.ProductId);

          

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

        /// <summary>
        /// 获取decimal类型的列定义（兼容不同数据库）
        /// </summary>
        private string GetDecimalColumnType()
        {
            var providerName = Database.ProviderName;
            return providerName switch
            {
                "Microsoft.EntityFrameworkCore.SqlServer" => "decimal(18,2)",
                "Npgsql.EntityFrameworkCore.PostgreSQL" => "numeric(18,2)",
                "Pomelo.EntityFrameworkCore.MySql" => "decimal(18,2)",
                _ => "decimal(18,2)"
            };
        }
    }
}
