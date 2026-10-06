using log4net;
using NHibernate;
using Sinapse.DAO;
using Sinapse.Model.enumerados;
using Sinapse.Util;
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
    public static class MobileService
    {
        /// <summary>
        /// Push de notificação de nova Jornada para o Aniel Point.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="label"></param>
        /// <param name="titulo"></param>
        /// <param name="texto"></param>
        /// <param name="type"></param>
        /// <param name="numFre"></param>
        public static void NotificarJornadaNova(String deviceId, String label, String titulo, String texto, String type, String numFre)
        {
            string applicationID = CONSTANTES.APLICATION_ID_POINTO_NOVO;

            var data = new
            {
                message = new
                {
                    token = deviceId,
                    data = new
                    {
                        tag = label,
                        title = titulo,
                        body = texto,
                        typeNotification = type,

                    }
                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            var retorno = new FirebaseCloudMessagingService(CONSTANTES.JsonCredencialGoogle_Point, CONSTANTES.ProjectIdGoogle_Point).PushNotificacao(json, "NotificarJornadaNova").Result;
        }

        /// <summary>
        /// Push de notificação de lembrete da Jornada para o Aniel Point.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="label"></param>
        /// <param name="titulo"></param>
        /// <param name="texto"></param>
        /// <param name="type"></param>
        /// <param name="numFre"></param>
        /// <returns></returns>
        public static Boolean NotificarLembretesJornada(String deviceId, String label, String titulo, String texto, String type, String numFre)
        {
            string applicationID = CONSTANTES.APLICATION_ID_POINTO_NOVO;

            var data = new
            {
                message = new
                {
                    token = deviceId,
                    data = new
                    {
                        tag = label,
                        title = titulo,
                        body = texto,
                        typeNotification = type,

                    }
                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);


            var retorno = new FirebaseCloudMessagingService(CONSTANTES.JsonCredencialGoogle_Point, CONSTANTES.ProjectIdGoogle_Point).PushNotificacao(json, "NotificarLembretesJornada").Result;
            if (retorno != null && retorno.result == false)
            {
                CONSTANTES.log4Net.Error(String.Format("Recurso: {0} NumFre: {1} Erro: {2} Device ID: {3} ", type, numFre, retorno.message, deviceId));
                ImprimeLogNotificacaoPoint(retorno.message);
            }


            return retorno?.result ?? false;
        }

        /// <summary>
        /// Push de notificações para o Aniel Touch.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="label"></param>
        /// <param name="titulo"></param>
        /// <param name="texto"></param>
        /// <param name="intent"></param>
        /// <param name="codCt"></param>
        /// <param name="codprojeto"></param>
        /// <param name="codcontrato"></param>
        /// <param name="codobra"></param>
        /// <param name="numFre"></param>
        public static void NotificarSmartphone(String deviceId, String label, String titulo, String texto, String intent, String codCt, String codprojeto, String codcontrato, String codobra, String numFre = null)
        {
            string nomeEquipe = "";

            var data = new
            {
                message = new
                {
                    token = deviceId,
                    data = new
                    {
                        tag = label,
                        title = titulo,
                        body = texto,
                        click_action = intent,
                        nome_equpe = nomeEquipe,
                        equipe = numFre,
                        codigoContrato = codCt,
                        projeto = codprojeto,
                        contrato = codcontrato,
                        obra = codobra

                    }
                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            if (String.IsNullOrEmpty(numFre))
                numFre = "";
            //var retorno = new FirebaseCloudMessagingService(CONSTANTES.JsonCredencialGoogle_Touch, CONSTANTES.ProjectIdGoogle_Touch).PushNotificacao(json).Result;

            Task.Run(async () => {
                var retorno = new FirebaseCloudMessagingService(CONSTANTES.JsonCredencialGoogle_Touch, CONSTANTES.ProjectIdGoogle_Touch).PushNotificacao(json, "NotificarSmartphone").ContinueWith(task => {
                    if (task.IsCompleted && task.Result.result == false)
                    {
                        CONSTANTES.log4Net.Error(String.Format("Recurso: {0} NumFre: {1} Erro: {2} Device ID: {3} ", intent, numFre, task.Result.message, deviceId));
                    }
                });

            });
        }

        /// <summary>
        /// Push de notificação de nova mensagem do Chat BackOffice para o Aniel Touch.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="label"></param>
        /// <param name="titulo"></param>
        /// <param name="codCt"></param>
        /// <param name="codprojeto"></param>
        /// <param name="codcontrato"></param>
        /// <param name="codobra"></param>
        /// <param name="numFre"></param>
        /// <param name="jsonMensagem"></param>
        public static void NotificarChatBackOffice(String deviceId, String label, String titulo, String codCt, String codprojeto, String codcontrato, String codobra, String numFre = null, String jsonMensagem = null)
        {
            string applicationID = "AIzaSyCMokLbdc7ifx7O2wv-5YOdJKenfrNIW-o";
            string nomeEquipe = "";

            var data = new
            {
                message = new
                {
                    token = deviceId,
                    data = new
                    {
                        tag = label,
                        title = titulo,
                        body = jsonMensagem,
                        nome_equipe = nomeEquipe,
                        equipe = numFre,
                        click_action = "chatBackOffice",
                        codigoContrato = codCt,
                        projeto = codprojeto,
                        contrato = codcontrato,
                        obra = codobra

                    }
                }
            };

            var json = new JavaScriptSerializer().Serialize(data);
            if (String.IsNullOrEmpty(numFre))
                numFre = "";
            Task.Run(async () => { var retorno = new FirebaseCloudMessagingService(CONSTANTES.JsonCredencialGoogle_Touch, CONSTANTES.ProjectIdGoogle_Touch).PushNotificacao(json, "NotificarChatBackOffice").Result; });

        }

        /// <summary>
        /// Push de notificações para o Aniel Point.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="nomeRecurso"></param>
        /// <param name="descricaoRecurso"></param>
        /// <param name="tag"></param>
        /// <param name="titulo"></param>
        /// <param name="texto"></param>
        /// <param name="type"></param>
        /// <param name="numFre"></param>
        /// <param name="jsonData"></param>
        public static void Notificar(String deviceId, String nomeRecurso, String descricaoRecurso, String tag, String titulo, String texto, String type, String numFre, String jsonData = null)
        {
            string applicationID = CONSTANTES.APLICATION_ID_POINTO_NOVO;

            var data = new
            {
                message = new
                {
                    token = deviceId,
                    data = new
                    {
                        tag = tag,
                        title = titulo,
                        body = texto,
                        typeNotification = type,

                        nomeRecurso = nomeRecurso,
                        descricaoRecurso = descricaoRecurso,
                        jsonData = jsonData

                    }
                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            var retorno = new FirebaseCloudMessagingService(CONSTANTES.JsonCredencialGoogle_Point, CONSTANTES.ProjectIdGoogle_Point).PushNotificacao(json, "Notificar").Result;
        }

        /// <summary>
        /// Push de notificações para o Aniel Point.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="nomeRecurso"></param>
        /// <param name="descricaoRecurso"></param>
        /// <param name="tag"></param>
        /// <param name="titulo"></param>
        /// <param name="texto"></param>
        /// <param name="type"></param>
        /// <param name="numFre"></param>
        /// <param name="jsonData"></param>
        public static void Notificar(String deviceId, String nomeRecurso, String descricaoRecurso, String tag, String titulo, String texto, Recurso_Mobile type, String numFre, String jsonData = null)
        {            
            string applicationID = CONSTANTES.APLICATION_ID_POINTO_NOVO;                        

            var data = new
            {
                message = new
                {
                    token = deviceId,
                    data = new
                    {
                        tag = tag,
                        title = titulo,
                        body = texto,
                        typeNotification = type,

                        nomeRecurso = nomeRecurso,
                        descricaoRecurso = descricaoRecurso,
                        jsonData = jsonData

                    }
                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            var retorno = new FirebaseCloudMessagingService(CONSTANTES.JsonCredencialGoogle_Point, CONSTANTES.ProjectIdGoogle_Point).PushNotificacao(json, "Notificar2").Result;
        }

        /// <summary>
        /// Push de notificação sobre o vencimento da fatura para o Olá Cliente.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="titulo"></param>
        /// <param name="emoji"></param>
        /// <param name="mensagem"></param>
        /// <param name="numFre"></param>
        public static void NotificarUsuarioVencimentoFatura(String deviceId, String titulo, String emoji, String mensagem, String numFre = null)
        {
            string applicationID = CONSTANTES.APLICATION_ID_OLA;
            
            var data = new
            {
                message = new
                {
                    token = deviceId,
                    data = new
                    {
                        title = titulo,
                        body = mensagem,
                        emoji = emoji,
                        type = 2

                    }
                }
            };

            if (String.IsNullOrEmpty(numFre))
                numFre = "";

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            var retorno = new FirebaseCloudMessagingService(CONSTANTES.JsonCredencialGoogle_Ola, CONSTANTES.ProjectIdGoogle_Ola).PushNotificacao(json, "VencimentoFaturaOlá").Result;            
        }

        [Obsolete("Essa API é legada e não realiza mais o push das notificações.")]
        private static Boolean EnviarNotificacao(String applicationID, String json, String deviceId, String numFre, String recurso, Action<String> LogExecucao = null)
        {
            try
            {
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";

                Byte[] byteArray = Encoding.UTF8.GetBytes(json);

                tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));
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

                                Dictionary<string, object> retorno = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(sResponseFromServer);

                                if (retorno["failure"].Equals(1))
                                {
                                    var results = ((object[])retorno["results"])[0];
                                    var msg = ((Dictionary<string, object>)results)["error"];

                                    LogExecucao?.Invoke(String.Format("Recurso: {0} NumFre: {1} Erro: {2} Device ID: {3} ", recurso, numFre, msg, deviceId));
                                }

                                if (retorno["success"].Equals(1))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string str = ex.Message;
                CONSTANTES.log4Net.Error(String.Format("Recurso: {0} NumFre: {1} Erro: {2} Device ID: {3} ", recurso, numFre, str, deviceId));
                LogExecucao?.Invoke(String.Format("Recurso: {0} NumFre: {1} Erro: {2} Device ID: {3} ", recurso, numFre, str, deviceId));
                //LogManager.GetLogger("Aniel.PontoEletronico").Error(String.Format("Device ID: {0} Erro: {1}", deviceId, str));
            }

            return false;
        }

        private static void ImprimeLogNotificacaoPoint(String log)
        {
            String caminho = CONSTANTES.CaminhoLogServico;

            if (caminho != null)
            {
                using (LogExecucao logExecucao = new LogExecucao("LOG_NOTIFICACAO", caminho))
                {
                    logExecucao.RegistraLog(log);
                }
            }
        }
    }
}
