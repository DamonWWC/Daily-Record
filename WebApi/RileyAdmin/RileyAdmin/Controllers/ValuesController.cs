using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RileyAdmin.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ValuesController(IConfiguration config)
        {

            _config = config;

        }
        [HttpGet]
        [Route("/GetToken")]
        public ActionResult<string> GetToken()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name,"admin"),
                new Claim(ClaimTypes.Upn,"123456")
            };

            var isyouruser = _config.GetValue<string>("JWT:ISyouuser");
            var isAudience = _config.GetValue<string>("JWT:IsAudience");

            var scKey = _config.GetValue<string>("JWT:SignKey");

            DateTime timeout = DateTime.Now.AddMinutes(30);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(scKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(isyouruser, isAudience, claims, expires: timeout, signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return "Bearer " + token;
        }

    }
}
