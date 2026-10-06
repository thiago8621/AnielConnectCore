using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.AspNetCore.Crypt.Filters
{
    public class AuthenticationFailureResult : IActionResult
    {
        private string _reasonPhrase;
        private HttpRequestMessage _request;

        public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request)
        {
            this._reasonPhrase = reasonPhrase;
            this._request = request;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            return Task.FromResult(this.Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {

                RequestMessage = this._request,
                ReasonPhrase = this._reasonPhrase
            };

            return response;

        }
    }
}