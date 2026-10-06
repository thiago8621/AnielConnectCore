using Aniel.Connect.Core;
using Sinapse.Integracao.ValidacaoDocumento.Interface;
using Sinapse.Integracao.MostqVioExtractionModels.Models;
using System;
using System.Collections.Generic;
using Aniel.Connect.Repository.MostQI;
using RestSharp;
using Sinapse.Util;
using System.Net;

namespace Sinapse.Integracao.ValidacaoDocumento.Integracoes
{
    public class MostqVioExtraction : BaseApiController, IntegracaoValidacaoDocumentoVio
    {
        private string tokenApi { get; set; }

        public MostqVioExtraction(String tokenApi)
        {
            this.tokenApi = tokenApi;
        }


        public VioExtractionResultResponse Extract(VioExtractionRequest vioRequest)
        {
            var client = new RestClient("https://mostqiapi.com/process-image/vio-extraction");
            var request = new RestRequest(Method.POST);

            var tokenResult = new MostQIApiRepository().GetAccessToken(tokenApi).token;

            // Adicionar cabeçalhos
            request.AddHeader("Authorization", $"Bearer {tokenResult}");
            request.AddHeader("Content-Type", "application/json");
            // Adicionar corpo da solicitação
            request.AddJsonBody(vioRequest);

            IRestResponse<VioExtractionResultResponse> response = client.Execute<VioExtractionResultResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a MostQI. Verifique as permissões necessárias de acesso.");
            }

            VioExtractionResultResponse responseBody = JsonService.DeserializeObject<VioExtractionResultResponse>(response.Content);

            return responseBody;
        }
    }

}