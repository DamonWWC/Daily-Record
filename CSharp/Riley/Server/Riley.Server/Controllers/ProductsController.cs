using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riley.Server.Data;
using Riley.Server.Models;

namespace Riley.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有产品
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                var products = await _context.Products
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品列表时发生错误");
                return StatusCode(500, "获取产品列表时发生内部错误");
            }
        }

        /// <summary>
        /// 根据ID获取产品
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                
                if (product == null)
                {
                    return NotFound($"未找到ID为{id}的产品");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品ID {ProductId} 时发生错误", id);
                return StatusCode(500, "获取产品时发生内部错误");
            }
        }

        /// <summary>
        /// 创建新产品
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            try
            {
                product.CreatedAt = DateTime.UtcNow;
                product.IsAvailable = true;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("创建新产品: {ProductName}", product.Name);

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建产品时发生错误");
                return StatusCode(500, "创建产品时发生内部错误");
            }
        }

        /// <summary>
        /// 更新产品信息
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest("ID不匹配");
                }

                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound($"未找到ID为{id}的产品");
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.UpdatedAt = DateTime.UtcNow;
                existingProduct.IsAvailable = product.IsAvailable;

                await _context.SaveChangesAsync();

                _logger.LogInformation("更新产品: {ProductId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新产品ID {ProductId} 时发生错误", id);
                return StatusCode(500, "更新产品时发生内部错误");
            }
        }

        /// <summary>
        /// 删除产品（软删除）
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound($"未找到ID为{id}的产品");
                }

                product.IsAvailable = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("删除产品: {ProductId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除产品ID {ProductId} 时发生错误", id);
                return StatusCode(500, "删除产品时发生内部错误");
            }
        }

        /// <summary>
        /// 搜索产品
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string? name, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
        {
            try
            {
                var query = _context.Products.Where(p => p.IsAvailable);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(p => p.Name.Contains(name) || (p.Description != null && p.Description.Contains(name)));
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                var products = await query.OrderBy(p => p.Name).ToListAsync();
                
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索产品时发生错误");
                return StatusCode(500, "搜索产品时发生内部错误");
            }
        }
    }
}
