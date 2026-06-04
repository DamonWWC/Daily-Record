using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Riley.Admin.Domain.LogAbstract.Dto;
using Riley.Admin.Services.Db;
using Riley.Admin.Services.Db.Models;
using Riley.Admin.Services.LoginLog.Dto;
using Riley.Common.Helpers;

namespace Riley.Admin.Services.LoginLog
{
    [Route("[controller]/[Action]")]
    [ApiController]
    public class LoginLogService : ControllerBase, ILoginLogService
    {
        private readonly AdminContext _adminContext;
        private readonly IHttpContextAccessor _context;
        private readonly IMapper _mapper;

        public LoginLogService(
            AdminContext adminContext,
            IHttpContextAccessor context,
            IMapper mapper
        )
        {
            _mapper = mapper;
            _context = context;
            _adminContext = adminContext;
        }

        [HttpPost]
        public async Task<long> AddAsync(LoginLogAddInput input)
        {
            input.IP = IPHelper.GetIP(_context.HttpContext?.Request);
            string ua = _context.HttpContext.Request.Headers["User-Agent"];
            if (!string.IsNullOrWhiteSpace(ua)) { }

            var entity = _mapper.Map<AdLoginLog>(input);
            await _adminContext.AdLoginLogs.AddAsync(entity);
            await _adminContext.SaveChangesAsync();
            return entity.Id;
        }

        [NonAction]
        public Task<List<LoginLogListOutput>> GetPageAsync(LogGetPageDto input)
        {
            throw new NotImplementedException();
        }
    }
}
