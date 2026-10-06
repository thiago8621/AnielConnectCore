using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.Helpers
{
    public class HttpHelper : IDisposable
    {
        protected String Get(String pUrl, String pToken, String pJsonParam, String operacao)
        {

            ServicePointManager.ServerCertificateValidationCallback +=
            (sender, certificate, chain, sslPolicyErrors) => true;

            var client = new RestClient(pUrl);
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", pToken);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("ixcsoft", operacao);
            request.AddParameter("application/json", pJsonParam, ParameterType.RequestBody);
            request.AddHeader("cache-control", "no-cache");



            try
            {
                return client.Execute(request).Content;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        protected String Post(String pUrl, String pToken, String pJsonParam)
        {

            ServicePointManager.ServerCertificateValidationCallback +=
            (sender, certificate, chain, sslPolicyErrors) => true;

            var client = new RestClient(pUrl);
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", pToken);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", pJsonParam, ParameterType.RequestBody);
            request.AddHeader("cache-control", "no-cache");



            try
            {
                return client.Execute(request).Content;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        protected String Put(String pUrl, String pToken, String pJsonParam, string registro)
        {

            ServicePointManager.ServerCertificateValidationCallback +=
            (sender, certificate, chain, sslPolicyErrors) => true;

            var client = new RestClient(pUrl + "/" + registro);
            client.Timeout = -1;

            var request = new RestRequest(Method.PUT);
            request.AddHeader("Authorization", pToken);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", pJsonParam, ParameterType.RequestBody);
            request.AddHeader("cache-control", "no-cache");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            try
            {
                return client.Execute(request).Content;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #region Disposable
        bool disposed = false;

        // método privado para controle
        // da liberação dos recursos
        private void Dispose(bool disposing)
        {
            // Verifique se Dispose já foi chamado.
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Liberando recursos gerenciados

                }

                // Seta a variável booleana para true,
                // indicando que os recursos já foram liberados
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
