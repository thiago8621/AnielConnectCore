using Aniel.Connect.Core.Crypt;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;

namespace Aniel.Connect.Core.Cripty
{
    public static class JwtManager
    {
        private const string key  = "c0j1Jq5MjerNm1jFqx0i3j8+JFfLTr2kHQkpKQhOptkBy86LK7l449K+pOtUsCAUfVJ7OA7SBsjznBjPNJWcEg==";

        public static string GenerateToken(string usuario, int tempoExpiracaoEmHora = 24)
        {
           
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, usuario)
                }),
                Expires = now.AddHours(tempoExpiracaoEmHora),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(key)), SecurityAlgorithms.HmacSha256Signature)
            };

            
            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            return CryptModel.Encrypt(tokenHandler.WriteToken(stoken));


        }



        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var symmetricKey = key;

                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(symmetricKey))
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;


            }
            catch
            {
                return null;
            }

        }





        //private static string getKey()
        //{
        //    var hmac = new HMACSHA256();
        //    var symmetricKey = Convert.ToBase64String(hmac.Key);
        //    return symmetricKey;
        //}






    }
}