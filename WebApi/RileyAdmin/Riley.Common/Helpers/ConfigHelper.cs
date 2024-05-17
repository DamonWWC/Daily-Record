using Microsoft.Extensions.Configuration;

namespace Riley.Common.Helpers
{
    /// <summary>
    /// 配置帮助类
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="enviromentName"></param>
        /// <param name="optional"></param>
        /// <param name="reloadOnChange"></param>
        /// <returns></returns>
        public static IConfiguration Load(string fileName, string enviromentName = "", bool optional = true, bool reloadOnChange = false)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Configs");

            if (!Directory.Exists(filePath))
                return null;


            var builder = new ConfigurationBuilder()
                .SetBasePath(filePath)
                .AddJsonFile($"{fileName}.json", optional, reloadOnChange);

            if (!string.IsNullOrWhiteSpace(enviromentName))
            {
                builder.AddJsonFile(fileName + "." + enviromentName + ".json", optional: optional, reloadOnChange: reloadOnChange);
            }
            return builder.Build();

        }
    }
}
