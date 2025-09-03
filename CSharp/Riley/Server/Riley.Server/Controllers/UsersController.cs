using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riley.Server.Data;
using Riley.Server.Models;

namespace Riley.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有用户
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.Name)
                    .ToListAsync();
                
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户列表时发生错误");
                return StatusCode(500, "获取用户列表时发生内部错误");
            }
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                
                if (user == null)
                {
                    return NotFound($"未找到ID为{id}的用户");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户ID {UserId} 时发生错误", id);
                return StatusCode(500, "获取用户时发生内部错误");
            }
        }

        /// <summary>
        /// 创建新用户
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            try
            {
                // 检查邮箱是否已存在
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == user.Email);
                
                if (existingUser != null)
                {
                    return BadRequest("该邮箱已被注册");
                }

                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("创建新用户: {UserName} ({UserEmail})", user.Name, user.Email);

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户时发生错误");
                return StatusCode(500, "创建用户时发生内部错误");
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            try
            {
                if (id != user.Id)
                {
                    return BadRequest("ID不匹配");
                }

                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound($"未找到ID为{id}的用户");
                }

                // 检查邮箱是否被其他用户使用
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == user.Email && u.Id != id);
                
                if (emailExists)
                {
                    return BadRequest("该邮箱已被其他用户使用");
                }

                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.UpdatedAt = DateTime.UtcNow;
                existingUser.IsActive = user.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("更新用户: {UserId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户ID {UserId} 时发生错误", id);
                return StatusCode(500, "更新用户时发生内部错误");
            }
        }

        /// <summary>
        /// 删除用户（软删除）
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound($"未找到ID为{id}的用户");
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("删除用户: {UserId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户ID {UserId} 时发生错误", id);
                return StatusCode(500, "删除用户时发生内部错误");
            }
        }
    }
}
