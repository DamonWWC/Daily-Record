using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Riley.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrderInfosToOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 重命名表
            migrationBuilder.Sql("ALTER TABLE \"OrderInfos\" RENAME TO \"OrderItems\";");
            
            // 重命名主键约束
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" RENAME CONSTRAINT \"PK_OrderInfos\" TO \"PK_OrderItems\";");
            
            // 重命名外键约束
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" RENAME CONSTRAINT \"FK_OrderInfos_Orders_OrderId\" TO \"FK_OrderItems_Orders_OrderId\";");
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" RENAME CONSTRAINT \"FK_OrderInfos_Products_ProductId\" TO \"FK_OrderItems_Products_ProductId\";");
            
            // 重命名索引
            migrationBuilder.Sql("ALTER INDEX \"IX_OrderInfos_OrderId\" RENAME TO \"IX_OrderItems_OrderId\";");
            migrationBuilder.Sql("ALTER INDEX \"IX_OrderInfos_ProductId\" RENAME TO \"IX_OrderItems_ProductId\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 重命名索引（回滚）
            migrationBuilder.Sql("ALTER INDEX \"IX_OrderItems_OrderId\" RENAME TO \"IX_OrderInfos_OrderId\";");
            migrationBuilder.Sql("ALTER INDEX \"IX_OrderItems_ProductId\" RENAME TO \"IX_OrderInfos_ProductId\";");
            
            // 重命名外键约束（回滚）
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" RENAME CONSTRAINT \"FK_OrderItems_Orders_OrderId\" TO \"FK_OrderInfos_Orders_OrderId\";");
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" RENAME CONSTRAINT \"FK_OrderItems_Products_ProductId\" TO \"FK_OrderInfos_Products_ProductId\";");
            
            // 重命名主键约束（回滚）
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" RENAME CONSTRAINT \"PK_OrderItems\" TO \"PK_OrderInfos\";");
            
            // 重命名表（回滚）
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" RENAME TO \"OrderInfos\";");
        }
    }
}
