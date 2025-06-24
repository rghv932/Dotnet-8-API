using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenService(IConfiguration config) : ITokenService
    {
        public string CreateToken(AppUser user)
        {
            //private SymmetricSecurityKey key;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"] ?? throw new Exception("cannot access key from appsettings")));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserName)
            };

            //var claims = new List<Claim>
            //{
            //    new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            //    new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            //};

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
