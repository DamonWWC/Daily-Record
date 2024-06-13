using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreDemo.Controllers
{
    //[ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ExceptionController : ControllerBase
    {
        public ExceptionController()
        {
        }

        [Route("/error-development")]
       
        public IActionResult HandleErrorDevelopment([FromServices] IHostEnvironment hostEnvironment)
        {
            if (!hostEnvironment.IsDevelopment())
            {
                return NotFound();
            }
            var exceptionHandleFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
            return Problem(
                detail: exceptionHandleFeature.Error.StackTrace,
                title: exceptionHandleFeature.Error.Message);
        }
        
        [Route("/error")]
        public IActionResult HandleError() =>
            Problem();
    }
}