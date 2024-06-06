using Microsoft.EntityFrameworkCore;

namespace RileyAdmin.自定义配置提供程序
{
    public class EFConfigurationSource : IConfigurationSource
    {
        private readonly Action<DbContextOptionsBuilder> _optionAction;
        public EFConfigurationSource(Action<DbContextOptionsBuilder> optionAction) => _optionAction = optionAction;
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EFConfigurationProvider(_optionAction);
        }
    }
}
