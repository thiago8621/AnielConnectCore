using Aniel.Connect.Integracoes.SerasaExperian.Interfaces;
using Aniel.Connect.Integracoes.SerasaExperian.Models;
using Newtonsoft.Json;
using RestSharp;
using Sinapse.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Aniel.Connect.Integracoes.SerasaExperian.Services
{
    public class ConsultarSerasaService : IConsultarSerasaService
    {
        private string _authToken;
        private string _urlBase;
        private string _reportParametersPJ;

        public ConsultarSerasaService(string tokenAuth, string urlBase, string reportParametersPJ)
        {
            if (string.IsNullOrEmpty(urlBase))
                throw new ArgumentException("Url de Acesso não pode estar vazio ou nulo.");

            if (string.IsNullOrEmpty(tokenAuth))
                throw new ArgumentException("Token de autenticação não pode estar vazio ou nulo.");

            _authToken = tokenAuth;
            _urlBase = urlBase;
            _reportParametersPJ = reportParametersPJ;
        }

        public async Task<ConsultaSerasaResponse> ConsultarSerasaPF(string documento)
        {
            ConsultaSerasaResponse response = new ConsultaSerasaResponse();
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            RestClient _client = new RestClient(_urlBase);
            RestRequest _request = new RestRequest($"person-information-report/v1/creditreport", Method.GET);
            _request.AddHeader("Content-Type", "application/json");
            _request.AddHeader("Accept", "application/json");
            _request.AddHeader("Authorization", $"Bearer {_authToken}");
            _request.AddHeader("X-Document-id", documento);
            _request.AddQueryParameter("reportName", "RELATORIO_BASICO_PF_PME");
            _request.AddQueryParameter("reportParameters", "");

            var consulta = await _client.ExecuteAsync<ConsultaSerasaResponse>(_request);

            if (consulta.StatusCode != HttpStatusCode.OK && consulta.StatusCode != HttpStatusCode.Created) {
                var erros = JsonConvert.DeserializeObject<List<DescricoesErros>>(consulta.Content);
                response.Erros = erros?.FirstOrDefault();

                //response.Erros = JsonService.DeserializeObject<List<DescricoesErros>>(consulta.Content).FirstOrDefault();
            }               
            else
                response = consulta.Data;

            return response;
        }

        public async Task<ConsultaSerasaResponse> ConsultarSerasaPJ(string documento)
        {
            ConsultaSerasaResponse response = new ConsultaSerasaResponse();
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            RestClient _client = new RestClient(_urlBase);
            RestRequest _request = new RestRequest($"business-information-report/v1/reports", Method.GET);
            _request.AddHeader("Content-Type", "application/json");
            _request.AddHeader("Accept", "application/json");
            _request.AddHeader("Authorization", $"Bearer {_authToken}");
            _request.AddHeader("X-Document-id", documento);
            _request.AddQueryParameter("reportName", "RELATORIO_BASICO_PJ_PME");

            if (!string.IsNullOrEmpty(_reportParametersPJ))
                _request.AddQueryParameter("reportParameters", _reportParametersPJ);

            var consulta = await _client.ExecuteAsync<ConsultaSerasaResponse>(_request);

            if (consulta.StatusCode != HttpStatusCode.OK && consulta.StatusCode != HttpStatusCode.Created)
            {
                var erros = JsonConvert.DeserializeObject<List<DescricoesErros>>(consulta.Content);
                response.Erros = erros?.FirstOrDefault();
                //response.Erros = JsonService.DeserializeObject<List<DescricoesErros>>(consulta.Content).FirstOrDefault();
            }
            else
                response = consulta.Data;

            return response;
        }
    }
}