using Aniel.Connect.Core.Cripty;
using Aniel.Connect.Core.Crypt.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;

namespace Aniel.Connect.Core.Crypt
{
    public class JwtAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        public string Realm { get; set; }
        public bool AllowMultiple => false;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var authorization = request.Headers.Authorization;
            try
            {

                if (authorization == null || authorization.Scheme != "Bearer")
                {
                    context.ErrorResult = new AuthenticationFailureResult("Autorização negada", request);
                    return;
                }

                if (string.IsNullOrEmpty(authorization.Parameter))
                {
                    context.ErrorResult = new AuthenticationFailureResult("Falta o Token de Verificação", request);
                    return;
                }

                var token = CryptModel.Decrypt(authorization.Parameter);
                var principal = await AuthenticateJwtToken(token);

                if (principal == null)
                {
                    context.ErrorResult = new AuthenticationFailureResult("Token invalido", request);
                }
                else
                {
                    context.Principal = principal;
                }
            }
            catch
            {
                context.ErrorResult = new AuthenticationFailureResult("Token invalido", request);
            }

        }


        protected Task<IPrincipal> AuthenticateJwtToken(string token)
        {
            string username;
            if (ValidateToken(token, out username))
            {
                var claims = new List<Claim> { 
                    //com base no nome de usuário para obter mais informações do banco de dados para criar identidade local
                    new Claim(ClaimTypes.Name, username)
                    // Adicione mais reivindicações, se necessário: funções, ...

                };

                var identity = new ClaimsIdentity(claims, "Jwt");
                IPrincipal user = new ClaimsPrincipal(identity);

                return Task.FromResult(user);

            }

            return Task.FromResult<IPrincipal>(null);
        }


        private static bool ValidateToken(string token, out string username)
        {
            username = null;
            var simplePrinciple = JwtManager.GetPrincipal(token);
            var identity = simplePrinciple?.Identity as ClaimsIdentity;


            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var usernameClaim = identity.FindFirst(ClaimTypes.Name);
            username = usernameClaim?.Value;

            if (string.IsNullOrEmpty(username))
                return false;

            // Verificação para validar se o usuario existe no sistema
            // Consultar no Banco de dados

            return true;

        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            Challenge(context);
            return Task.FromResult(0);
        }

        private void Challenge(HttpAuthenticationChallengeContext context)
        {
            string parameter = null;

            if (!string.IsNullOrEmpty(Realm))
                parameter = "realm=\"" + Realm + "\"";

            context.ChallengeWith("Bearer", parameter);
        }
    }
}