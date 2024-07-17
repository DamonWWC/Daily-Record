using Microsoft.EntityFrameworkCore;

namespace RileyAdmin.自定义配置提供程序
{
    public class EFConfigurationProvider:ConfigurationProvider
    {
        public EFConfigurationProvider(Action<DbContextOptionsBuilder> optionsAction)
        {
            OptionsAction = optionsAction;
        }
        Action<DbContextOptionsBuilder> OptionsAction { get; }


        public override void Load()
        {
            var builder = new DbContextOptionsBuilder<EFConfigurationContext>();
            OptionsAction(builder);
            using (var dbContext = new EFConfigurationContext(builder.Options))
            {
                if(dbContext ==null || dbContext.Values==null)
                {
                    throw new Exception("Null DB context");
                }
                dbContext.Database.EnsureCreated();

                Data = !dbContext.Values.Any()
                    ? CreateAndSaveDefaultValue(dbContext)
                    : dbContext.Values.ToDictionary(c => c.Id, c => c.Value);
                    
            }
        }

        public static IDictionary<string,string> CreateAndSaveDefaultValue( EFConfigurationContext dbContext)
        {
            var configValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                  { "quote1", "I aim to misbehave." },
                    { "quote2", "I swallowed a bug." },
                    { "quote3", "You can't stop the signal, Mal." }
            };

            if(dbContext==null || dbContext.Values==null)
            {
                throw new Exception("Null DB Context");
            }
            dbContext.Values.AddRange(configValues.Select(kvp => new EFConfigurationValue
            {
                Id = kvp.Key,
                Value = kvp.Value
            }).ToArray());
            dbContext.SaveChanges();
            return configValues;
        }
    }
}
