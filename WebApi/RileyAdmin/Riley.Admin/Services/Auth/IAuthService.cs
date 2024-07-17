using Microsoft.AspNetCore.Mvc.ModelBinding;
using Riley.Admin.Auth.Dto;

namespace Riley.Admin.Services.Auth
{
    public interface IAuthService
    {
        string GetToken(AuthLoginOutput user);
        Task<dynamic> LoginAsync(AuthLoginInput user);
        Task<AuthGetUserInfoOutput> GetUserInfoAsync();
        Task<AuthGetPasswordEncryptKeyOutput> GetPasswordEncryptKeyAsync();

        Task<dynamic> Refresh([BindRequired] string token);
    }
}
