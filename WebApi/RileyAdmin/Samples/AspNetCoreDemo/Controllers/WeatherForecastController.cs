using AspNetCoreDemo.DemoContext;
using AspNetCoreDemo.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly AdminContext _adminContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, AdminContext adminContext)
        {
            _logger = logger;
            _adminContext = adminContext;
        }

        /// <summary>
        /// 获取天气数据
        /// </summary>
        /// <param name="isOnly"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///        "id": 1,
        ///        "name": "Item #1",
        ///        "isComplete": true
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        [HttpGet(Name = "GetWeatherForecast")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async IAsyncEnumerable<AdApi> Get([FromQuery] bool isOnly = false)
        {
            var apis = _adminContext.AdApis.OrderBy(p => p.CreatedTime).AsAsyncEnumerable();

            await foreach (var api in apis)
            {
                yield return api;
            }
        }

        [HttpGet("{username}")]
        public IActionResult GetByName(string username)
        {
            
            AdUser? userInfo = _adminContext.AdUsers.FirstOrDefault(p => p.UserName == username);

            return userInfo == null ? NotFound() : Ok(userInfo);
        }

        [HttpGet("Results/{username}")]
        public Results<NotFound,Ok<AdUser>> GetByName1(string username)
        {
            AdUser? userInfo = _adminContext.AdUsers.FirstOrDefault(p => p.UserName == username);

            return userInfo == null ? TypedResults.NotFound() : TypedResults.Ok(userInfo);
        }

        [HttpPost("UpdateDemo")]
        public async Task<IActionResult> UpDateDemo(Demo demo)
        {
            using(var transaction=_adminContext.Database.BeginTransaction())
            {
              
                demo.CreatedTime = DateTime.Now;
                _adminContext.Demos.Add(demo);
                await _adminContext.SaveChangesAsync();

                return CreatedAtAction(nameof(UpDateDemo), new { id = demo.Id }, demo);
            }
          
        }
        [HttpPost("UpdateDemo1")]
        public async Task<IResult> UpDateDemo1(Demo demo)
        {

            
            demo.CreatedTime = DateTime.Now;
            _adminContext.Demos.Add(demo);
            await _adminContext.SaveChangesAsync();
            var location = Url.Action(nameof(UpDateDemo1), new { id = demo.Id }) ?? $"/{demo.Id}";
            return Results.Created(location, demo); ;
        }


        [HttpPost("PostJson")]
        [Consumes("application/json")]
        public IActionResult PostJson(IEnumerable<int> values) =>
              Ok(new { Consumes = "application/json", Values = values });

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        [Route("PostForm")]
        public IActionResult PostForm([FromForm] IEnumerable<int> values)
        {
            return Ok(new { Consumes = "application/x-www-form-urlencoded", Values = values });
        }

        [HttpPost]
        public IActionResult Post([FromQuery] WeatherForecast weatherForecast)
        {
            return Ok();
        }
        [HttpGet("Version")]
        public ContentResult GetVersion() => Content("v1.0.0");
    }
}