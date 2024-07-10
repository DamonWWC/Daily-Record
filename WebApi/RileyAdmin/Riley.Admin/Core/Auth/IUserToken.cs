using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Riley.Admin.Core.Auth
{
    public interface IUserToken
    {
        string Create(Claim[] claims);

        JwtSecurityToken Decode(string jwtToken);

    }
}
