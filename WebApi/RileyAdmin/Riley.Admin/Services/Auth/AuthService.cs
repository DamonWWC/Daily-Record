using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Riley.Admin.Auth.Dto;
using Riley.Admin.Core.Auth;
using Riley.Admin.Core.Configs;
using Riley.Admin.Core.Consts;
using Riley.Admin.Core.Dto;
using Riley.Admin.Domain.User;
using Riley.Admin.Services.Db;
using Riley.Admin.Services.Db.Models;
using Riley.Admin.Services.LoginLog;
using Riley.Admin.Services.LoginLog.Dto;
using Riley.Admin.Tools.Cache;
using Riley.Common.Extensions;
using Riley.Common.Helpers;
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
        private readonly ICacheTools _cache;
        private readonly IPasswordHasher<AdUser> _passwordHasher;
        private readonly ILoginLogService _loginLogService;
        private IMapper _mapper;

        public AuthServiceController(AdminContext adminContext, 
            IOptions<JwtConfig> jwtConfig, 
            IUserToken userToken, 
            ICacheTools cache, 
            IPasswordHasher<AdUser> passwordHasher, 
            IMapper mapper,
            ILoginLogService loginLogService)
        {
            _loginLogService = loginLogService;
            _adminContext = adminContext;
            _jwtConfig = jwtConfig.Value;
            _userToken = userToken;
            _cache = cache;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/GetPasswordEncryptKey")]
        public async Task<AuthGetPasswordEncryptKeyOutput> GetPasswordEncryptKeyAsync()
        {
            var guid = Guid.NewGuid().ToString("N");
            var key = CacheKeys.PassWordEncrypt + guid;
            var encyptKey = StringHelper.GenerateRandom(8);
            await _cache.SetAsync(key, encyptKey, TimeSpan.FromMinutes(5));
            return new AuthGetPasswordEncryptKeyOutput { Key = guid, EncyptKey = encyptKey };
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
        public async Task<dynamic> LoginAsync(AuthLoginInput input)
        {
            var stopwatch = Stopwatch.StartNew();

            #region 密码解密

            if (!string.IsNullOrWhiteSpace(input.PasswordKey))
            {
                var passwordEncryptKey = CacheKeys.PassWordEncrypt + input.PasswordKey;
                var existsPasswordKey = await _cache.ExistsAsync(passwordEncryptKey);
                if (existsPasswordKey)
                {
                    var secretKey = await _cache.GetAsync(passwordEncryptKey);
                    if (string.IsNullOrWhiteSpace(secretKey))
                        throw ResultOutput.Exception("解密失败");
                    input.Password = DesEncrypt.Decrypt(input.Password, secretKey);
                    await _cache.DelAsync(passwordEncryptKey);
                }
                else
                {
                    throw ResultOutput.Exception("解密失败");
                }
            }

            #endregion 密码解密

            #region 登录

            AdUser user = await _adminContext.AdUsers.FirstOrDefaultAsync(a => a.UserName == input.UserName);
            var valid = user?.Id > 0;
            if (valid)
            {
                if (user.PasswordEncryptType == PasswordEncryptType.PasswordHasher)
                {
                    var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, input.Password);
                    valid = passwordVerificationResult == PasswordVerificationResult.Success || passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded;
                }
                else
                {
                    //密码123asd
                    var password = MD5Encrypt.Encrypt32(input.Password);
                    valid = user.Password == password;
                }
            }

            if (!valid)
            {
                throw ResultOutput.Exception("用户名或密码错误");
            }
            if (!user.Enabled)
            {
                throw ResultOutput.Exception("账号已停用，禁止登录");
            }

            #endregion 登录

            #region 获得token

            var authLoginOutput = _mapper.Map<AuthLoginOutput>(user);

            string token = GetToken(authLoginOutput);

            #endregion 获得token

            stopwatch.Stop();

            #region 添加登录日志

            var loginLogAddInput = new LoginLogAddInput
            {
                TenantId = authLoginOutput.TenantId,
                Name = authLoginOutput.Name,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Status = true,
                CreatedUserId = authLoginOutput.Id,
                CreatedUserName = user.UserName,
            };
            await _loginLogService.AddAsync(loginLogAddInput);
            #endregion 添加登录日志

            return new { token};
        }

        [NonAction]
        public Task<dynamic> Refresh([BindRequired] string token)
        {
            throw new NotImplementedException();
        }
    }
}