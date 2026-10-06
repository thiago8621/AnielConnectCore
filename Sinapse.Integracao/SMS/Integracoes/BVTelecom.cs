using Aniel.Connect.Core;
using Sinapse.Integracao.Rastreamento.Integracoes;
using Sinapse.Integracao.SMS.Interface;
using Sinapse.Integracao.SMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Integracao.SMS.Integracoes
{
    public class BVTelecom : BaseApiController, IntegracaoSMS
    {
        private string tokenApi { get; set; }

        public BVTelecom(String tokenApi)
        {
            this.tokenApi = tokenApi;
        }
        /// <summary>
        /// Envio de lote de SMS
        /// <param name="lista_sms"></param>
        /// <example>
        /// <code>
        /// Exemplo em json
        ///{
        ///"bulk": [{"celular": "10969686389", "mensagem": "Teste 1","parceiroId": "msg001"}]
        ///}
        /// </code>
        /// </example>
        /// </summary>
        /// <returns></returns>
        public EnviarLoteSMS_ResponseModel EnviarLoteSMS(List<Sinapse.Model.SMS> lista_sms)
        {
            EnviarLoteSMS_ResponseModel retorno = new EnviarLoteSMS_ResponseModel();

          
            List<dynamic> lista_msg = new List<dynamic>();

            foreach (var sms in lista_sms)
            {
                dynamic item = new
                {
                    celular = sms.Num_Celular,
                    mensagem = sms.Mensagem,
                    parceiroId = Convert.ToString(sms.Id)
                };
                lista_msg.Add(item);
            }

            dynamic objeto = new
            {
                bulk = lista_msg
            };

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("ApiKey", tokenApi);

            EnviarSMSResponseAPI retornoApi = PostIntegracao<EnviarSMSResponseAPI>("https://smartsms.bvtelecom.com.br/webhook/api/delivery/multiple-sms", objeto, headers);

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
        /// <summary>
        /// Envio de Requisições de SMS        
        /// <param name="sms"></param>
        /// <example>
        /// <code>
        ///   {
        ///         "celular": "10969686389",
        ///         "mensagem": "Teste 1",
        ///         "parceiroId": "102030",
        ///         "carteiraId": 32
        ///     }    
        /// </code>    
        /// </example>    
        /// </summary>
        /// <returns></returns>
        public EnviarSMS_ResponseModel EnviarSMS(Sinapse.Model.SMS sms)
        {
            EnviarSMS_ResponseModel retorno = new EnviarSMS_ResponseModel();

          

            dynamic objeto = new
            {
                celular = sms.Num_Celular,
                mensagem = sms.Mensagem,
                parceiroId = Convert.ToString(sms.Id)
            };

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("ApiKey", tokenApi);

            EnviarSMSResponseAPI retornoApi = PostIntegracao<EnviarSMSResponseAPI>("https://smartsms.bvtelecom.com.br/webhook/api/delivery/single-sms", objeto, headers);

            retorno.Id_Integracao = sms.Id;
            retorno.Mensagem = sms.Mensagem;

            if (retornoApi.message.Contains("success"))
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
    }

}