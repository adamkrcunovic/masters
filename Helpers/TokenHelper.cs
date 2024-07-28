using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;

namespace FlightSearch.Helpers
{
    public static class TokenHelper
    {
        public static async Task<string> GetUserIdFromHttpContext(this HttpContext httpContext)
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(accessToken) as JwtSecurityToken;
            return token != null ? token.Claims.First(claim => claim.Type == "UserId").Value : "";
        }

        public static async Task<int> GetCountryIdFromHttpContext(this HttpContext httpContext)
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(accessToken) as JwtSecurityToken;
            return token != null ? int.Parse(token.Claims.First(claim => claim.Type == "CountryId").Value) : -1;
        }
    }
}