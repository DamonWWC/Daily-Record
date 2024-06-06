using Microsoft.EntityFrameworkCore;

namespace RileyAdmin.自定义配置提供程序
{
    public class EFConfigurationContext:DbContext
    {
        public EFConfigurationContext(DbContextOptions<EFConfigurationContext> options):base(options)
        {
            
        }
        public DbSet<EFConfigurationValue> Values => Set<EFConfigurationValue>();
    }
}
