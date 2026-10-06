using RestSharp;
using Sinapse.Integracao.SMS.Interface;
using Sinapse.Integracao.SMS.Models;
using Sinapse.Util;
using System;
using System.Collections.Generic;

namespace Sinapse.Integracao.SMS.Integracoes
{
    public class PontalTech : IntegracaoSMS
    {
        private readonly string _usuario;
        private readonly string _senha;

        private readonly RestClient _restClient;

        public PontalTech(string usuario, string senha)
        {
            if (String.IsNullOrEmpty(usuario))
            {
                throw new ArgumentNullException("usuario");
            }
            
            if (String.IsNullOrEmpty(senha))
            {
                throw new ArgumentNullException("senha");
            }

            this._usuario = usuario;
            this._senha = senha;

            this._restClient = new RestClient("http://sms-api-pointer-mm2.pontaltech.com.br:3102");
        }

        private IRestResponse ExecutarRequestPontalTech(Sinapse.Model.SMS sms)
        {
            var request = new RestRequest("envio");

            string celular = sms.Num_Celular;

            // Caso o número do celular não contenha o número do país ele será adicionado manualmente
            if (celular.Length <= 11)
            {
                celular = "55" + celular;
            }

            request.AddParameter("operacao", "ENVIO");
            request.AddParameter("usuario", this._usuario);
            request.AddParameter("senha", this._senha);
            request.AddParameter("destino", celular);
            request.AddParameter("mensagem", sms.Mensagem);

            return this._restClient.Execute(request);
        }

        public EnviarLoteSMS_ResponseModel EnviarLoteSMS(List<Sinapse.Model.SMS> listaSms)
        {
            var retorno = new EnviarLoteSMS_ResponseModel();

            string message = null;
            bool hasRequestError = false;

            foreach (Sinapse.Model.SMS sms in listaSms)
            {
                IRestResponse response = this.ExecutarRequestPontalTech(sms);

                PontalTechEnvioResponse responseContent = JsonService.DeserializeObject<PontalTechEnvioResponse>(response.Content);

                bool hasErrorMessage = 
                    !response.IsSuccessful || 
                    (String.IsNullOrEmpty(responseContent.code) && String.IsNullOrEmpty(responseContent.message));

                if (hasErrorMessage && !hasRequestError)
                {
                    hasRequestError = true;
                    message = responseContent?.message;
                }
            }

            retorno.Sucesso = !hasRequestError;
            retorno.MensagemErro = message;

            return retorno;
        }

        public EnviarSMS_ResponseModel EnviarSMS(Sinapse.Model.SMS sms)
        {
            var retorno = new EnviarSMS_ResponseModel();

            retorno.Id_Integracao = sms.Id;
            retorno.Mensagem = sms.Mensagem;

            IRestResponse response = this.ExecutarRequestPontalTech(sms);
            PontalTechEnvioResponse responseContent = JsonService.DeserializeObject<PontalTechEnvioResponse>(response.Content);

            bool hasErrorMessage = !String.IsNullOrEmpty(responseContent.code) && !String.IsNullOrEmpty(responseContent.message);

            if (response.IsSuccessful && !hasErrorMessage)
            {
                retorno.Sucesso = true;
            }
            else
            {
                retorno.Sucesso = false;
                retorno.MensagemErro = responseContent?.message;
            }

            return retorno;
        }
    }
}