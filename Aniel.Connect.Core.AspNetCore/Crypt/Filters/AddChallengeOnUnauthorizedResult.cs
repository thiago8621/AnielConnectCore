using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.AspNetCore.Crypt.Filters
{
    public class AddChallengeOnUnauthorizedResult : IActionResult
    {

        private AuthenticationHeaderValue _challenge;
        private ActionResult<HttpResponseMessage> _innerResult;

        public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, ActionResult<HttpResponseMessage> innerResult)
        {
            this._challenge = challenge;
            this._innerResult = innerResult;
        }

        // O ActionResult funciona de forma diferente no ASP.NET Core,
        // o ExecuteResultAsync da interface IActionResult não retorna o código de status HTTP da requisição.
        
        // Para adaptar a classe, chamei o ExecuteResultAsync do innerResult assumindo que ele preencherá o retorno
        // no campo Value que existe dentro do innerResult. 
        public Task ExecuteResultAsync(ActionContext context)
        {
            // Objetos do tipo ActionResult<TValue> possuem dois campos: ActionResult<TValue>.Result, que retorna
            // o ActionResult associoado, e ActionResult<TValue>.Value, que contém o valor retornado pela requisição.
            
            // Somente um desses campos estará preenchido na instância que receberemos pelo construtor. Caso o Value esteja vazio,
            // executamos o ActionResult do campo Result para preencher o Value.

            // PS.: Esse código não foi testado.
            // Referência: https://stackoverflow.com/a/63217643

            if (this._innerResult.Value == null)
            {
                this._innerResult.Result.ExecuteResultAsync(context).Wait();
            }

            HttpResponseMessage response = this._innerResult.Value;

            if (response != null)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Adicione apenas um Challenge por esquema de autenticação.
                    if (response.Headers.WwwAuthenticate.All(h => h.Scheme != this._challenge.Scheme))
                    {
                        response.Headers.WwwAuthenticate.Add(this._challenge);
                    }
                }
            }

            return Task.FromResult(response);
        }
    }
}