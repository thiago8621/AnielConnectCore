using Aniel.Connect.Core;
using Sinapse.Integracao.ProvaVida.Interface;
using Sinapse.Integracao.Rastreamento.Integracoes;
using Sinapse.Integracao.ProvaVida.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using Aniel.Connect.Repository.MostQI;
using Aniel.Connect.Repository.Model;
using Sinapse.Util;
using System.Net;

namespace Sinapse.Integracao.ProvaVida.Integracoes
{
    public class MostqLivenessDetect : BaseApiController, IntegracaoProvaVida
    {
        private string tokenApi { get; set; }

        public MostqLivenessDetect(String tokenApi)
        {
            this.tokenApi = tokenApi;
        }

        public DetectResultResponse Detect(DetectRequest detectRequest)
        {
            var client = new RestClient("https://mostqiapi.com/liveness/detect");
            var request = new RestRequest(Method.POST);

            var tokenResult = new MostQIApiRepository().GetAccessToken(tokenApi).token;

            // Adicionar cabeçalhos
            request.AddHeader("Authorization", $"Bearer {tokenResult}");
            request.AddHeader("Content-Type", "application/json");
            // Adicionar corpo da solicitação
            request.AddJsonBody(detectRequest);

            IRestResponse<DetectResultResponse> response = client.Execute<DetectResultResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a MostQI. Verifique as permissões necessárias de acesso.");
            }

            DetectResultResponse responseBody = JsonService.DeserializeObject<DetectResultResponse>(response.Content);

            return responseBody;
        }
    }

}