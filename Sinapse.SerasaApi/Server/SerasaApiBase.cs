using RestSharp;
using RestSharp.Authenticators;
using Sinapse.SerasaApi.Server.Responses;
using System;
using System.Net;
using Sinapse.JSON;
using System.Collections.Generic;

namespace Sinapse.SerasaApi.Server
{
    public class SerasaApiBase
    {
        public T PostHTTP<T>(List<Parameter> parametros) where T : RetornoSerasaBase
        {
            T retorno = default;

            var client = new RestClient("https://mqlinuxext-2.serasa.com.br/Homologa/consultahttps?p=");

            // url producao: https://sitenet43-2.serasa.com.br/Prod/consultahttps?p=

            // client.Authenticator = new HttpBasicAuthenticator(SerasaUserLogin, SerasaUserSenha);

            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;

            request.Parameters.AddRange(parametros);


            client.AddDefaultHeader("Content-Type", "application/x-www-form-urlencoded;");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            IRestResponse response = client.Post(request);

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new Exception("Erro autenticação: " + response.ErrorMessage, response.ErrorException);
            }

            retorno = JsonService.DeserializeJasonMapper<T>(response.Content);

            //retorno = await client.GetAsync(request);
            return retorno;
        }
    }
}
