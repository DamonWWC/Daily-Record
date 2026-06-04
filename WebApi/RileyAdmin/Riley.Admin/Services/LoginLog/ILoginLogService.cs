using Riley.Admin.Domain.LogAbstract.Dto;
using Riley.Admin.Services.LoginLog.Dto;

namespace Riley.Admin.Services.LoginLog
{
    public interface ILoginLogService
    {
        Task<List<LoginLogListOutput>> GetPageAsync(LogGetPageDto input);
        Task<long> AddAsync(LoginLogAddInput input);
    }
}
