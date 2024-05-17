using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Riley.Hosts.Program;

namespace Riley.Hosts.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ApplicationDbContext applicationDb)
        {
            _logger = logger;
            //applicationDb.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
            //applicationDb.SaveChanges();
            //var aa= applicationDb.demo.Add(new Api { Id=122344556 , Label="1111"});
            //applicationDb.SaveChanges();
            var bb = applicationDb.Apis.ToList();

        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
