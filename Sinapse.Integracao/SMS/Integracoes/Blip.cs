using Aniel.Connect.Core;
using NHibernate.Criterion;
using NHibernate.Mapping;
using RestSharp;
using Sinapse.DAO;
using Sinapse.Integracao.Rastreamento.Integracoes;
using Sinapse.Integracao.SMS.Interface;
using Sinapse.Integracao.SMS.Models;
using Sinapse.JSON;
using Sinapse.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.SMS.Integracoes
{
    public class Blip : BaseApiController, IntegracaoSMS
    {
        private Guid unique { get { return Guid.NewGuid(); } }

        private readonly string mask = "@sms.gw.msging.net";
        private readonly string _contractid;
        private readonly string _key;
        private readonly long _client;
        public Blip(string contractid, string key, long client)
        {
            _contractid = contractid;
            _key = key;
            _client = client;
        }

        public EnviarLoteSMS_ResponseModel EnviarLoteSMS(List<Model.SMS> request)
        {
            EnviarLoteSMS_ResponseModel retorno = new EnviarLoteSMS_ResponseModel();

            List<dynamic> lista_msg = new List<dynamic>();

            foreach (var sms in request)
            {
                string destinatario = $"+55 {sms.Num_Celular + this.mask}";
                dynamic item = new
                {
                    Id = this.unique,
                    to = destinatario,
                    type = "text/plain",
                    content = sms.Mensagem,
                };
                lista_msg.Add(item);
            }

            dynamic objeto = new
            {
                bulk = lista_msg
            };

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", $"key {this._key}");
            headers.Add("Content-Type", "application/json");

            EnviarSMSResponseAPI retornoApi = PostIntegracao<EnviarSMSResponseAPI>($"https://{this._contractid}.http.msging.net/", objeto, headers);



            if (retornoApi.message.Contains("success") || retornoApi.message.Contains("sucesso"))
            {
                retorno.Sucesso = true;
            }
            else
            {
                if (retornoApi.errors != null && retornoApi.errors.Count() > 0)
                {
                    foreach (var erro in retornoApi.errors)
                        retorno.MensagemErro = erro.message;
                }
                else retorno.MensagemErro = retornoApi.message;

                retorno.Sucesso = false;
            }

            return retorno;
        }

        public EnviarSMS_ResponseModel EnviarSMS(Model.SMS request)
        {
            var retorno = new EnviarSMS_ResponseModel();
            string destinatario = $"+55 {request.Num_Celular + this.mask}";
            var parametros = new Dictionary<string, string>();
            //headers
            parametros.Add("Authorization", $"key {this._key}");
            parametros.Add("Content-Type", "application/json");
            //body
            var requisicao = new BlipRequestModel
            {
                Id = this.unique,
                to = destinatario,
                type = "text/plain",
                content = $"{request.Mensagem} - {CONSTANTES.URLPortal}/comercial_SMS/formulario?cliente={this._client}&numero={request.Num_Celular}",
            };

            var retornoApi = PostIntegracaoBlip<EnviarSMSResponseAPI>($"https://{this._contractid}.http.msging.net/messages", requisicao, parametros);

            if (retornoApi.Data != null)
            {
                if (retornoApi.Data.message.Contains("success") || retornoApi.Data.message.Contains("sucesso"))
                {
                    retorno.Sucesso = true;
                }
                else
                {
                    if (retornoApi.Data.errors != null && retornoApi.Data.errors.Count() > 0)
                    {
                        foreach (var erro in retornoApi.Data.errors)
                            retorno.MensagemErro = erro.message;
                    }
                    else retorno.MensagemErro = retornoApi.Data.message;

                    retorno.Sucesso = false;
                }
            }
            else if (retornoApi.StatusCode == System.Net.HttpStatusCode.Accepted) 
            {
                foreach (var cookie in retornoApi.Cookies) 
                {
                    var validadeCookie = cookie.Expires;                
                }                    
                retorno.Sucesso = true;                
            }
            
            return retorno;
        }

        public IRestResponse<T> PostIntegracaoBlip<T>(String pUrlIntegracao, object obj, Dictionary<string, string> headers) where T : new ()            
        {

            var client = new RestClient($"{pUrlIntegracao}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");

            if (headers.Count() > 0)
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);

            request.AddJsonBody(obj);

            IRestResponse<T> response = client.Execute<T>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                return response;
            }           
            else
            {
                throw new Exception($"Erro Post Status: {response.StatusCode} Url: {pUrlIntegracao} Obj: {JsonService.SerializeObject(obj)}");
            }
        }
    }
}