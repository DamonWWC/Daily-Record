using Microsoft.EntityFrameworkCore;

namespace RileyAdmin.自定义配置提供程序
{
    public static class EntityFrameworkExtensions
    {
        public static IConfigurationBuilder AddEFConfiguration (this IConfigurationBuilder builder,Action<DbContextOptionsBuilder> optionsAction)
        {
            return builder.Add(new EFConfigurationSource(optionsAction));
        }
    }
}
