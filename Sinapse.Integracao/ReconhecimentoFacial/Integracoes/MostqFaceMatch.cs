using Aniel.Connect.Core;
using Sinapse.Integracao.ReconhecimentoFacial.Models;
using System;
using System.Collections.Generic;
using Sinapse.Integracao.ReconhecimentoFacial.Interface;
using Aniel.Connect.Repository.MostQI;
using RestSharp;
using System.Net;
using Sinapse.Util;

namespace Sinapse.Integracao.ReconhecimentoFacial.Integracoes
{
    public class MostqFaceMatch : BaseApiController, IntegracaoReconhecimentoFacial
    {
        private string tokenApi { get; set; }

        public MostqFaceMatch(String tokenApi)
        {
            this.tokenApi = tokenApi;
        }

        public FaceCompareResultResponse FaceCompare(FaceCompareRequest faceCompareRequest)
        {
            var client = new RestClient("https://mostqiapi.com/process-image/biometrics/face-compare");
            var request = new RestRequest(Method.POST);

            var tokenResult = new MostQIApiRepository().GetAccessToken(tokenApi).token;

            // Adicionar cabeçalhos
            request.AddHeader("Authorization", $"Bearer {tokenResult}");
            request.AddHeader("Content-Type", "application/json");
            // Adicionar corpo da solicitação
            request.AddJsonBody(faceCompareRequest);

            IRestResponse<FaceCompareResultResponse> response = client.Execute<FaceCompareResultResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a MostQI. Verifique as permissões necessárias de acesso.");
            }

            FaceCompareResultResponse responseBody = JsonService.DeserializeObject<FaceCompareResultResponse>(response.Content);

            return responseBody;
        }
    }

}