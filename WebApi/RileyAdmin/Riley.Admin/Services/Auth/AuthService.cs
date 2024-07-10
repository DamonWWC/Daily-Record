using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Riley.Admin.Auth.Dto;
using Riley.Admin.Core.Auth;
using Riley.Admin.Core.Configs;
using Riley.Admin.Services.Db;
using Riley.Admin.Services.Db.Models;
using Riley.Common.Extensions;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ZhonTai.Admin.Core.Auth;

namespace Riley.Admin.Services.Auth
{
    [Route("[controller]")]
    [ApiController]
    public class AuthServiceController : ControllerBase
    {
        private readonly AdminContext _adminContext;
        private readonly JwtConfig _jwtConfig;
        private readonly IUserToken _userToken;

        public AuthServiceController(AdminContext adminContext, IOptions<JwtConfig> jwtConfig, IUserToken userToken)
        {
            _adminContext = adminContext;
            _jwtConfig = jwtConfig.Value;
            _userToken = userToken;
        }

        [NonAction]
        public Task<AuthGetPasswordEncryptKeyOutput> GetPasswordEncryptKeyAsync()
        {
            throw new NotImplementedException();
        }

        [NonAction]
        public string GetToken(AuthLoginOutput user)
        {
            if (user == null)
                return string.Empty;
            var claim = new List<Claim>()
            {
                  new(ClaimAttributes.UserId, user.Id.ToString(), ClaimValueTypes.Integer64),
                  new(ClaimAttributes.UserName, user.UserName),
                  new (ClaimAttributes.Name, user.Name),
                  new (ClaimAttributes.UserType, user.Type.ToString(), ClaimValueTypes.Integer32),
                  new (JwtRegisteredClaimNames.Iat, DateTime.Now. ToTimestamp().ToString(), ClaimValueTypes.Integer64),
            };
            var token = _userToken.Create([.. claim]);
            return token;
        }

        [NonAction]
        public Task<AuthGetUserInfoOutput> GetUserInfoAsync()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("/Login")]
        public async Task<ActionResult<AdUser>> LoginAsync(AuthLoginInput input)
        {
            var stopwatch = Stopwatch.StartNew();

            #region 密码解密

            if (!string.IsNullOrWhiteSpace(input.PasswordKey))
            {
            }

            #endregion 密码解密

            //var profile = _adminContext.Value.AdUsers.Where(p => p.Id == user.).Select(p => new AuthUserProfileDto
            //{
            //    UserName = p.UserName,
            //    Name = p.Name,
            //    NickName = p.NickName,
            //    Avatar = p.Avatar
            //});
            var infos = _adminContext.AdUsers.ToList();
            var userinfo = await _adminContext.AdUsers.FirstOrDefaultAsync(p => p.UserName == input.UserName);

            return null;
        }

        [NonAction]
        public Task<dynamic> Refresh([BindRequired] string token)
        {
            throw new NotImplementedException();
        }
    }
}