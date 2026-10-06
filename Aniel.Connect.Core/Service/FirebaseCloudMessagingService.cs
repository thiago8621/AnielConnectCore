using Google.Apis.Auth.OAuth2;
using Sinapse.DAO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Aniel.Connect.Core.Service
{
    /// <summary>
    /// Serviço de Push de Notificações do Firebase para dispositivos Mobile.
    /// </summary>
    public class FirebaseCloudMessagingService
    {
        private string credentialJsonPath { get; set; }
        private string projectId { get; set; }

        /// <summary>
        /// Serviço de Push de Notificações do Firebase para dispositivos Mobile.
        /// </summary>
        /// <param name="credentialJsonPath">Caminho físico do Json contendo credenciais do Google.</param>
        /// <param name="projectId">Id do Projeto no Google.</param>
        public FirebaseCloudMessagingService(string credentialJsonPath, string projectId)
        {
            if (string.IsNullOrEmpty(credentialJsonPath))
            {
                throw new Sinapse.Core.EAnielException("Caminho do Json de Credenciais do Google não informado.");
            }

            if (string.IsNullOrEmpty(projectId))
            {
                throw new Sinapse.Core.EAnielException("Project Id do Google não informado.");
            }

            this.credentialJsonPath = credentialJsonPath;
            this.projectId = projectId;
        }

        /// <summary>
        /// Faz o push/envio da notificação para o dispositivo mobile.
        /// </summary>
        /// <param name="json">Notificação a ser enviada.</param>
        /// <param name="nomeRecurso">Nome do recurso que está disparando a notificação.</param>
        /// <returns></returns>
        public async Task<Response> PushNotificacao(string json, string nomeRecurso)
        {
            Response retorno = new Response();
            try
            {
                WebRequest tRequest = WebRequest.Create($"https://fcm.googleapis.com/v1/projects/{this.projectId}/messages:send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";

                Byte[] byteArray = Encoding.UTF8.GetBytes(json);

                tRequest.Headers.Add(string.Format("Authorization: Bearer {0}", this.GetAuthToken(this.credentialJsonPath)));
                tRequest.ContentLength = byteArray.Length;

                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();

                                var retornoApi =  Sinapse.Util.JsonService.DeserializeObject<FirebaseMessageResponse>(sResponseFromServer);

                                if(retornoApi?.error?.code != null && retornoApi?.error?.message != null)                                
                                {                                    
                                    var msg = retornoApi?.error?.message ?? "Erro ao fazer push da notificação pelo Firebase";
                                    // LogExecucao?.Invoke(String.Format("Recurso: {0} NumFre: {1} Erro: {2} Device ID: {3} ", recurso, numFre, msg, deviceId));
                                    throw new Sinapse.Core.EAnielException(Convert.ToString(msg));
                                }

                                if(!string.IsNullOrEmpty(retornoApi?.name) && retornoApi?.name?.Contains(this.projectId) == true)
                                {
                                    retorno.result = true;
                                }

                                /*if (retornoApi.Any(p => p.Key == "name") && retornoApi["name"].ToString().Contains(this.projectId))
                                {
                                    retorno.result = true;

                                }*/
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //CONSTANTES.log4Net.Error(String.Format("Recurso: {0} NumFre: {1} Erro: {2} Device ID: {3} ", recurso, numFre, ex.Message, deviceId));
                CONSTANTES.log4Net.Error(String.Format("Recurso: {0} Erro: {1}", "Firebase Cloud Messaging (FCM)", ex.Message));
                retorno.exception = ex.Message;
                retorno.stacktrace = ex.StackTrace;
                retorno.result = false;
                //LogExecucao?.Invoke(String.Format("Recurso: {0} NumFre: {1} Erro: {2} Device ID: {3} ", recurso, numFre, str, deviceId));
                //LogManager.GetLogger("Aniel.PontoEletronico").Error(String.Format("Device ID: {0} Erro: {1}", deviceId, str));
            }

            return retorno;
        }

        /// <summary>
        /// Gera as credenciais a partir de um Json.
        /// </summary>
        /// <param name="json">Json serializado das credenciais do Google.</param>
        /// <returns></returns>
        private GoogleCredential AuthenticateByJson(string json, List<string> escopos = null)
        {
            if (escopos == null || escopos.Count > 0)
                escopos = new List<string> { "https://www.googleapis.com/auth/firebase.messaging" };

            return GoogleCredential.FromJson(json).CreateScoped(escopos);
        }

        /// <summary>
        /// Gera as credenciais a partir de um arquivo .json em disco.
        /// </summary>
        /// <param name="jsonPath">Caminho físico do Json com as credenciais do Google.</param>
        /// <param name="escopos">Escopos de acesso da API do Google. Se nulo, passa por padrão apenas o escopo para permissão de messageria.</param>
        /// <returns></returns>
        private GoogleCredential AuthenticateByJsonPath(string jsonPath, List<string> escopos = null)
        {
            if (escopos == null || escopos.Count > 0)
                escopos = new List<string> { "https://www.googleapis.com/auth/firebase.messaging" };

            return GoogleCredential.FromFile(jsonPath).CreateScoped(escopos);
        }

        /// <summary>
        /// Gera o Token de autenticação para os request de push da notificação
        /// </summary>
        /// <param name="jsonPath">Caminho físico do Json com as credenciais do Google.</param>
        /// <param name="escopos">Escopos de acesso da API do Google. Se nulo, passa por padrão apenas o escopo para permissão de messageria.</param>
        /// <returns></returns>
        private string GetAuthToken(string jsonPath, List<string> escopos = null)
        {
            var googleCredential = this.AuthenticateByJsonPath(jsonPath, escopos);
            //var googleCredential = this.AuthenticateByJson(jsonPath, escopos);
            return googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync().Result.Replace("key=", string.Empty);
        }

        /// <summary>
        /// Retorna o Json da conta da Sinapse para testes.
        /// </summary>
        /// <returns></returns>
        public static string getJsonDebug()
        {
            return "{\"type\":\"service_account\",\"project_id\":\"anieltouch-440b6\",\"private_key_id\":\"5492907e1c5ddcf214a59e1439388c69cd0555b7\",\"private_key\":\"-----BEGINPRIVATEKEY-----\\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCbN9YF567eXnA0\\nEC9YFxMne0k65DWhs2S7miim3OWQI1vaUu3T/LVCS1M4kx62pTXgHRKSemKNzWI/\\nJ2mqsC+ud+tDDYHFkzh8EMMaltjau73Zj2/6n8Q010BVisNnUKCTj/04ENiRC9Df\\n1xmMsTiyS/GQkU/UsP3BnQaDxWardxA2xfl/lOwNSEJ+nHZZmyueukQ12xZzbu2p\\nQ7n9wGPxWJb/Mqd/SdML56tLhLCEtY2PFRgVs+aEW4uMyng8y+perfB7QFdJhKgc\\np2PUD9MksJKqwctvu4Bac8sVYCu1i7DCe/GYQZSXfmnAWWzgDvCPJSV1CQb0rPxO\\n13rODDY7AgMBAAECggEAB+jg1izdQ2WqmsHsYl/NURNo7ZWtifpF+tgQlELoy988\\nEgnB0YNXip5OClxXkf/PRAbB6Ib7Y2CA+NjoUJNpNUnVH6CePqmRlPjsXabW5tFE\\nYRl8dQK+kTbFh4EVE2cPbRWcVhwS/Sk4pqojxNX5yVQyENI8MDQov5IMlUXHmpst\\nTrXo/HtQWNWGKQE3EdSVbTakELI6tOzWdyM7pOcrGHauEQ/pWCHTInAR5YVvDcoR\\nd/23acosNgPcAIJ2T98z4xcs8WXng5Xv7IFNz2etjoYcFQHYcIRVdfTNCTrDhT2+\\nZnikRpsMvcU1q3IGDWO1idZvY17vSHzlapNbLzxYqQKBgQDQRQlF/zoEtz9qAdIC\\nFFjdTboyUlhVkb6Q7jbljCl3YWZcbf3XZDAkrDkuzMPpQjoURYCknWPOwDIc06V6\\nyxh1QaWqwnOhHLd2ZcRPUTAQ44Qme1u73rlQ4rgod0khCIrkO5251ckwHy5+ZmQ4\\nQV5PURQrBcSDmfcu2A6n5S2F/QKBgQC+ylHx5L/RyOx75kohC8TuSLdn3nrnYDQZ\\n49Pq/saT4Dh0YmXQo5vYaUx/FOTDBs9xDiiXyneLUjJs4Xru58KnTK5UI9itKzve\\nnNuyd5GF8Aw09aHkw/m6xwzWnMbUTUrs9e/eIboIeRUeBVRSLUKCNOU3hHnkHGBd\\nqnD/EXtGlwKBgCn6ECFsjKwQpxZdoY9LvJF2LhYgZK9yfZ9GdAgWFPF7zx7ICIaW\\nKeHqWGgnW0EzWPNnAsOq+UfdkvweaFb0gkx328UsBhQt7ohNgKCf83kj8imzaUzX\\nWjCEUavORw4CY8QWg/bFsRQi5MMTLhXsju6bn93//pirupdVcrbCSUedAoGAbXIs\\nXeIGM7baCh1KKvjQaIzoQTp8MJycZrmuCKFMkunOGN2VYIFqVlPZYRFARaxzdXxU\\nxvaLay2k5ZbXaZMjs8smOfgyROyHNKWw1gkHPtk8d2jtDQlrCb1D3R0kZgdM0vAm\\nQLwCNvgWqBkQjO+PtpfXYeip77Rjh3fNyFRj7lMCgYANSmhNlDDrHF3JBq/ZcD3Z\\n1jujKNhgMFEI4LsW9gIAiielp5imX9qoZzKrMw65lYM14UvoHMEGSvxNH2rmdS+m\\n76c6hyIVYWjSVJdWs1odzgKY8aeNpEznV0ZMV108GUILhGijGtkSIeX8RsnwV/kg\\nZTlwIWzHg0fFrkbpeh3y7g==\\n-----ENDPRIVATEKEY-----\\n\",\"client_email\":\"firebase-adminsdk-0maz9@anieltouch-440b6.iam.gserviceaccount.com\",\"client_id\":\"111256858126065079108\",\"auth_uri\":\"https://accounts.google.com/o/oauth2/auth\",\"token_uri\":\"https://accounts.google.com/o/oauth2/token\",\"auth_provider_x509_cert_url\":\"https://www.googleapis.com/oauth2/v1/certs\",\"client_x509_cert_url\":\"https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-0maz9%40anieltouch-440b6.iam.gserviceaccount.com\"}";
        }

        /// <summary>
        /// Retorna o Project Id da conta da Sinapse para testes.
        /// </summary>
        /// <returns></returns>
        public static string getProjectIdDebug()
        {
            return "anieltouch-440b6";
        }
    }

    public class FirebaseMessageResponse
    {       
        public string name { get; set; }
        public FirebaseMessageError error { get; set; }
        
        public class FirebaseMessageError
        {
            public int? code { get; set; }
            public string message { get; set; }
            public string status { get; set; }
            public FirebaseMessageDetail[] details { get; set; }
        }

        public class FirebaseMessageDetail
        {
            public string type { get; set; }
            public string errorCode { get; set; }
        }
    }
}
