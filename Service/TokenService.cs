using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlightSearch.Database.Models;
using FlightSearch.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FlightSearch.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]??""));
        }
        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id??""),
                new Claim("CountryId", (user.CountryId??default(int)).ToString())
            };
            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMonths(3),
                SigningCredentials = credentials,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }
    }
}