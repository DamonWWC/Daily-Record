using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riley.Server.Data;
using Riley.Server.Models;

namespace Riley.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有订单
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            try
            {
                var orders = await _context.Orders

                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表时发生错误");
                return StatusCode(500, "获取订单列表时发生内部错误");
            }
        }

        /// <summary>
        /// 根据ID获取订单
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            try
            {
                var order = await _context.Orders

                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound($"未找到ID为{id}的订单");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单ID {OrderId} 时发生错误", id);
                return StatusCode(500, "获取订单时发生内部错误");
            }
        }

        /// <summary>
        /// 根据用户ID获取订单
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByUser(int userId)
        {
            try
            {
                var orders = await _context.Orders

                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户 {UserId} 的订单时发生错误", userId);
                return StatusCode(500, "获取用户订单时发生内部错误");
            }
        }

        /// <summary>
        /// 根据状态获取订单
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _context.Orders

                    .Where(o => o.Status == status)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取状态为 {Status} 的订单时发生错误", status);
                return StatusCode(500, "获取订单时发生内部错误");
            }
        }

        /// <summary>
        /// 创建新订单
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            try
            {
                // 生成订单号
                order.OrderNumber = GenerateOrderNumber();
                order.CreatedAt = DateTime.UtcNow;
                order.Status = "Pending";

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("创建新订单: {OrderNumber}", order.OrderNumber);

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建订单时发生错误");
                return StatusCode(500, "创建订单时发生内部错误");
            }
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound($"未找到ID为{id}的订单");
                }

                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                // 根据状态设置相应的时间戳
                if (status == "Shipped" && order.ShippedAt == null)
                {
                    order.ShippedAt = DateTime.UtcNow;
                }
                else if (status == "Delivered" && order.DeliveredAt == null)
                {
                    order.DeliveredAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("更新订单 {OrderId} 状态为: {Status}", id, status);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单 {OrderId} 状态时发生错误", id);
                return StatusCode(500, "更新订单状态时发生内部错误");
            }
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound($"未找到ID为{id}的订单");
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("删除订单: {OrderId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除订单ID {OrderId} 时发生错误", id);
                return StatusCode(500, "删除订单时发生内部错误");
            }
        }

        /// <summary>
        /// 生成订单号
        /// </summary>
        private string GenerateOrderNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random();
            var randomPart = random.Next(1000, 9999);
            return $"ORD-{timestamp}-{randomPart}";
        }
    }
}