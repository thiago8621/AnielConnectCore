using Aniel.Connect.Repository.Model;
using RestSharp;
using Sinapse.Integracao.MostQI.Repositorio;
using Sinapse.Util;
using System;
using System.Net;
using System.Reflection.Emit;

namespace Aniel.Connect.Repository.MostQI
{
    /// <summary>
    ///     Classe para realizar requisições para a API da Voalle
    /// </summary>
    public class MostQIApiRepository : IntegracaoMostQIRepository
    {
        /// <summary>
        ///     Acessa o endpoint de autenticação da API do Voalle para oter o token de acesso
        /// </summary>
        public MostQIResponse GetAccessToken(string token)
        {
            var urlClient = "https://mostqiapi.com/user/authenticate";
            var client = new RestClient(urlClient)
            {
                Timeout = -1,
            };

            var request = new RestRequest(urlClient, Method.POST)
                .AddHeader("Content-Type", "application/json")
                .AddJsonBody(new { token = $"{token}" });

            IRestResponse<MostQIResponse> response = client.Execute<MostQIResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a MostQI. Verifique as permissões necessárias de acesso.");
            }

            MostQIResponse responseBody = JsonService.DeserializeObject<MostQIResponse>(response.Content);

            return responseBody;
        }
    }
}
