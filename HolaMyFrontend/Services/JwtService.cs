using System.IdentityModel.Tokens.Jwt;

namespace HolaMyFrontend.Services
{
    public class JwtService
    {
        public static JwtSecurityToken ParseJwt(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            
            return jwt;
        }
    }
}
