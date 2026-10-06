using Aniel.Connect.Integracoes.SerasaExperian.Interfaces;
using Aniel.Connect.Integracoes.SerasaExperian.Models;
using RestSharp;
using Sinapse.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aniel.Connect.Integracoes.SerasaExperian.Services
{  
    public class AuthService : IAuthService
    {
        private string _urlBase;
        private string _clientId;
        private string _clientSecret;

        public AuthService(string clientId, string clientSecret, string urlBase)
        {
            if (string.IsNullOrEmpty(urlBase))
                throw new ArgumentException("Url de Acesso não pode estar vazio ou nulo.");

            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentException("Cliente ID não pode estar vazio ou nulo.");

            if (string.IsNullOrEmpty(clientSecret))
                throw new ArgumentException("Cliente Secret não pode estar vazio ou nulo.");

            _urlBase = urlBase;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task<AuthToken> ObterTokenAuth()
        {
            AuthToken response = new AuthToken();

            string auth = await Base64Encode($"{_clientId}:{_clientSecret}");

            RestClient _client = new RestClient(_urlBase);
            RestRequest _request = new RestRequest("", Method.POST);
            _request.AddHeader("Authorization", $"Basic {auth}");
            _request.AddHeader("Content-Type", "application/json");

            var obterToken = await _client.ExecuteAsync(_request);

            if (obterToken.StatusCode != System.Net.HttpStatusCode.OK && obterToken.StatusCode != System.Net.HttpStatusCode.Created)
            {
                var erros = JsonService.DeserializeObject<List<DescricoesErros>>(obterToken.Content);
                response.Erros = erros?.FirstOrDefault();
            }
            else
                response = JsonService.DeserializeObject<AuthToken>(obterToken.Content);

            return response;
        }

        public static async Task<string> Base64Encode(string textoCodificar)
        {
            var textoCodificado = System.Text.Encoding.UTF8.GetBytes(textoCodificar);
            return Convert.ToBase64String(textoCodificado);
        }
    }
}