using Sinapse.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Sinapse.WidePayApi.Server.Responses;
using RestSharp;
using RestSharp.Authenticators;
using Sinapse.WidePayApi.Server.Enums;
using System.Net;

namespace Sinapse.WidePayApi.Server
{
    public class WidePayBase
    {
        //https://widepay.github.io/api/#cobranca-notificacao

        public T PostHTTP<T>(Chamada chamada, String idWidepay, String tokenWidePay, List<Parameter> parametros) where T : RetornoWideBase
        {
            T retorno = default(T);

            var client = new RestClient(chamada.Value);
            client.Authenticator = new HttpBasicAuthenticator(idWidepay, tokenWidePay);

            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;

            //request.AddHeader("accept-encoding", "gzip, deflate");
            //request.AddHeader("Accept", "*/*");
            //request.AddHeader("Content-Type", "application/x-www-form-urlencoded;");
            //request.AddHeader("content-type", "multipart/form-data;");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            request.Parameters.AddRange(parametros);

            IRestResponse response = client.Post(request);

            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new Exception("Widepay " + response.ErrorMessage, response.ErrorException);
            }

            retorno = JsonService.DeserializeJasonMapper<T>(response.Content);

            return retorno;
        }

        //GERAR COBRANÇA
        //var client = new RestClient("https://api.widepay.com/v1/recebimentos/cobrancas/adicionar");
        //var request = new RestRequest(Method.POST);
        //request.AddHeader("Postman-Token", "a38b04c1-0dc8-4310-a3e6-c08a727c3c7d");
        //request.AddHeader("cache-control", "no-cache");
        //request.AddHeader("Connection", "keep-alive");
        //request.AddHeader("content-length", "768");
        //request.AddHeader("accept-encoding", "gzip, deflate");
        //request.AddHeader("cookie", "PHPSESSID=1v91pj1p2blurivnq1f3f8vvv5");
        //request.AddHeader("Host", "api.widepay.com");
        //request.AddHeader("Accept", "*/*");
        //request.AddHeader("User-Agent", "PostmanRuntime/7.11.0");
        //request.AddHeader("Authorization", "Basic Mjk1NTE1Ojk4NGZmNzIyZDIzMjkyNjcwZDY5MjZjZTYxZDkwN2Zm");
        //request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        //request.AddHeader("content-type", "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");
        //request.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW", "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"forma\"\r\n\r\nBoleto\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"cliente\"\r\n\r\nTeste\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"pessoa\"\r\n\r\nFísica\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"cpf\"\r\n\r\n463.384.662-02\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"itens\"\r\n\r\n[{\"descricao\":\"Item 1\",\"valor\":\"22.50\"}]\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"vencimento\"\r\n\r\n2019-04-20\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW--", ParameterType.RequestBody);
        //IRestResponse response = client.Execute(request);

        //BOLETO
        //var client = new RestClient("https://api.widepay.com/v1/recebimentos/cobrancas/boleto");
        //var request = new RestRequest(Method.POST);
        //request.AddHeader("Postman-Token", "5644d9c0-51ff-42db-a86c-69a6167b8c70");
        //request.AddHeader("cache-control", "no-cache");
        //request.AddHeader("Connection", "keep-alive");
        //request.AddHeader("content-length", "173");
        //request.AddHeader("accept-encoding", "gzip, deflate");
        //request.AddHeader("cookie", "PHPSESSID=1v91pj1p2blurivnq1f3f8vvv5");
        //request.AddHeader("Host", "api.widepay.com");
        //request.AddHeader("Accept", "*/*");
        //request.AddHeader("User-Agent", "PostmanRuntime/7.11.0");
        //request.AddHeader("Authorization", "Basic Mjk1NTE1Ojk4NGZmNzIyZDIzMjkyNjcwZDY5MjZjZTYxZDkwN2Zm");
        //request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        //request.AddHeader("content-type", "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");
        //request.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW", "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"id\"\r\n\r\n5A69A81FFF4D64C0\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW--", ParameterType.RequestBody);
        //IRestResponse response = client.Execute(request);
    }
}
