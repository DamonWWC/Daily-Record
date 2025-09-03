using Microsoft.EntityFrameworkCore;
using Riley.Server.Data;
using Riley.Server.Models;

namespace Riley.Server.Services
{
    /// <summary>
    /// 数据库初始化服务
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(ApplicationDbContext context, ILogger<DatabaseInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("开始初始化数据库...");

                // 确保数据库已创建
                await _context.Database.EnsureCreatedAsync();

                // 检查是否需要种子数据
                if (!await _context.Users.AnyAsync())
                {
                    await SeedDataAsync();
                }
                else
                {
                    _logger.LogInformation("数据库中已存在数据，跳过种子数据初始化");
                }
                _logger.LogInformation("数据库初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据库初始化失败");
                throw;
            }
        }

        /// <summary>
        /// 添加种子数据
        /// </summary>
        private async Task SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("添加种子数据...");

                // 添加示例用户（如果不存在）
                var existingUsers = await _context.Users.ToListAsync();
                var usersToAdd = new List<User>();

                var userEmails = new[] { "zhangsan@example.com", "lisi@example.com", "wangwu@example.com" };
                var userNames = new[] { "张三", "李四", "王五" };
                var userPhones = new[] { "13800138001", "13800138002", "13800138003" };

                for (int i = 0; i < userEmails.Length; i++)
                {
                    if (!existingUsers.Any(u => u.Email == userEmails[i]))
                    {
                        usersToAdd.Add(new User
                        {
                            Name = userNames[i],
                            Email = userEmails[i],
                            Phone = userPhones[i],
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                if (usersToAdd.Any())
                {
                    await _context.Users.AddRangeAsync(usersToAdd);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"添加了 {usersToAdd.Count} 个用户");
                }

                // 重新获取所有用户（包括新添加的）
                var allUsers = await _context.Users.ToListAsync();

                // 添加示例产品（如果不存在）
                var existingProducts = await _context.Products.ToListAsync();
                var productsToAdd = new List<Product>();

                var productNames = new[] { "笔记本电脑", "智能手机", "无线耳机" };
                var productDescriptions = new[] 
                { 
                    "高性能笔记本电脑，适合办公和游戏", 
                    "最新款智能手机，拍照功能强大", 
                    "高品质无线蓝牙耳机，音质出色" 
                };
                var productPrices = new[] { 5999.99m, 3999.99m, 299.99m };
                var productStocks = new[] { 50, 100, 200 };

                for (int i = 0; i < productNames.Length; i++)
                {
                    if (!existingProducts.Any(p => p.Name == productNames[i]))
                    {
                        productsToAdd.Add(new Product
                        {
                            Name = productNames[i],
                            Description = productDescriptions[i],
                            Price = productPrices[i],
                            StockQuantity = productStocks[i],
                            CreatedAt = DateTime.UtcNow,
                            IsAvailable = true
                        });
                    }
                }

                if (productsToAdd.Any())
                {
                    await _context.Products.AddRangeAsync(productsToAdd);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"添加了 {productsToAdd.Count} 个产品");
                }

                // 重新获取所有产品（包括新添加的）
                var allProducts = await _context.Products.ToListAsync();

                // 添加示例订单（如果不存在）
                var existingOrders = await _context.Orders.ToListAsync();
                var ordersToAdd = new List<Order>();

                var orderNumbers = new[] { "ORD-2024-001", "ORD-2024-002", "ORD-2024-003" };
                var orderAmounts = new[] { 6299.98m, 3999.99m, 299.99m };
                var orderStatuses = new[] { "Delivered", "Processing", "Pending" };
                var orderAddresses = new[] 
                { 
                    "北京市朝阳区某某街道123号", 
                    "上海市浦东新区某某路456号", 
                    "广州市天河区某某大道789号" 
                };
                var orderPhones = new[] { "13800138001", "13800138002", "13800138003" };
                var orderNotes = new[] { "请尽快发货", null, null };

                for (int i = 0; i < orderNumbers.Length; i++)
                {
                    if (!existingOrders.Any(o => o.OrderNumber == orderNumbers[i]))
                    {
                        var user = allUsers.FirstOrDefault(u => u.Phone == orderPhones[i]);
                        if (user != null)
                        {
                            var order = new Order
                            {
                                UserId = user.Id,
                                OrderNumber = orderNumbers[i],
                                TotalAmount = orderAmounts[i],
                                Status = orderStatuses[i],
                                ShippingAddress = orderAddresses[i],
                                ContactPhone = orderPhones[i],
                                Notes = orderNotes[i],
                                CreatedAt = DateTime.UtcNow.AddDays(-30 + i * 25)
                            };

                            // 为已发货和已送达的订单设置相应时间
                            if (orderStatuses[i] == "Delivered")
                            {
                                order.ShippedAt = order.CreatedAt.AddDays(2);
                                order.DeliveredAt = order.CreatedAt.AddDays(5);
                            }

                            ordersToAdd.Add(order);
                        }
                    }
                }

                if (ordersToAdd.Any())
                {
                    await _context.Orders.AddRangeAsync(ordersToAdd);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"添加了 {ordersToAdd.Count} 个订单");
                }

                // 重新获取所有订单（包括新添加的）
                var allOrders = await _context.Orders.ToListAsync();

                // 添加示例订单项（如果不存在）
                var existingOrderItems = await _context.OrderItems.ToListAsync();
                var orderItemsToAdd = new List<OrderItem>();

                // 订单项配置：订单索引，产品索引，数量
                var orderItemConfigs = new[]
                {
                    (0, 0, 1), // 订单1-笔记本电脑
                    (0, 2, 1), // 订单1-无线耳机
                    (1, 1, 1), // 订单2-智能手机
                    (2, 2, 1)  // 订单3-无线耳机
                };

                foreach (var (orderIndex, productIndex, quantity) in orderItemConfigs)
                {
                    if (orderIndex < allOrders.Count && productIndex < allProducts.Count)
                    {
                        var order = allOrders[orderIndex];
                        var product = allProducts[productIndex];
                        
                        // 检查是否已存在相同的订单项
                        var existingOrderItem = existingOrderItems.FirstOrDefault(oi => 
                            oi.OrderId == order.Id && oi.ProductId == product.Id);
                        
                        if (existingOrderItem == null)
                        {
                            orderItemsToAdd.Add(new OrderItem
                            {
                                OrderId = order.Id,
                                ProductId = product.Id,
                                Quantity = quantity,
                                UnitPrice = product.Price,
                                TotalPrice = product.Price * quantity
                            });
                        }
                    }
                }

                if (orderItemsToAdd.Any())
                {
                    await _context.OrderItems.AddRangeAsync(orderItemsToAdd);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"添加了 {orderItemsToAdd.Count} 个订单项");
                }

                _logger.LogInformation("种子数据添加完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加种子数据时发生错误");
                throw;
            }
        }
    }
}
