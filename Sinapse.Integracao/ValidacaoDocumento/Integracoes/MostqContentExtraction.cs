using Aniel.Connect.Core;
using Sinapse.Integracao.ValidacaoDocumento.Interface;
using Sinapse.Integracao.MostqContenctExtractionModels.Models;
using System;
using System.Collections.Generic;
using System.Net;
using Sinapse.Integracao.ProvaVida.Models;
using RestSharp;
using Sinapse.Util;
using Aniel.Connect.Repository.MostQI;

namespace Sinapse.Integracao.ValidacaoDocumento.Integracoes
{
    public class MostqContentExtraction : BaseApiController, IntegracaoValidacaoDocumentoContent
    {
        private string tokenApi { get; set; }

        public MostqContentExtraction(String tokenApi)
        {
            this.tokenApi = tokenApi;
        }

        public ContentExtractionResultResponse Extract(ContentExtractionRequest contentRequest)
        {
            var client = new RestClient("https://mostqiapi.com/process-image/content-extraction");
            var request = new RestRequest(Method.POST);

            var tokenResult = new MostQIApiRepository().GetAccessToken(tokenApi).token;

            // Adicionar cabeçalhos
            request.AddHeader("Authorization", $"Bearer {tokenResult}");
            request.AddHeader("Content-Type", "application/json");
            // Adicionar corpo da solicitação
            request.AddJsonBody(contentRequest);

            IRestResponse<ContentExtractionResultResponse> response = client.Execute<ContentExtractionResultResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a MostQI. Verifique as permissões necessárias de acesso.");
            }

            ContentExtractionResultResponse responseBody = JsonService.DeserializeObject<ContentExtractionResultResponse>(response.Content);

            return responseBody;
        }
    }

}