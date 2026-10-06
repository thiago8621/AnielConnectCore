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
        public static void NotificarJornadaNova(String deviceId, String label, String titulo, String texto, String type, String numFre)
        {
            //"br.com.sinapse.point_novo.OSDFSDF"
            //AIzaSyD-ScQ53BIQXWtR0jaeNp74ndW4Kpe7H64
            /*try
            {
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";*/

            string applicationID = CONSTANTES.APLICATION_ID_POINTO_NOVO;

            var data = new
            {
                to = deviceId,
                data = new
                {
                    tag = label,
                    title = titulo,
                    body = texto,
                    typeNotification = type,
                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            EnviarNotificacao(applicationID, json, deviceId, numFre, type);
            /*Byte[] byteArray = Encoding.UTF8.GetBytes(json);

            tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));
            //tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
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
                            string str = sResponseFromServer;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            string str = ex.Message;
        }*/


        }

        public static Boolean NotificarLembretesJornada(String deviceId, String label, String titulo, String texto, String type, String numFre)
        {
            string applicationID = CONSTANTES.APLICATION_ID_POINTO_NOVO;

            var data = new
            {
                to = deviceId,
                priority = "high",
                data = new
                {
                    tag = label,
                    title = titulo,
                    body = texto,
                    typeNotification = type,
                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);

            return EnviarNotificacao(
                applicationID,
                json,
                deviceId,
                numFre,
                type,
                (stringLog) => ImprimeLogNotificacaoPoint(stringLog)
            );
        }

        public static void NotificarSmartphone(String deviceId, String label, String titulo, String texto, String intent, String codCt, String codprojeto, String codcontrato, String codobra, String numFre = null)
        {
            string applicationID = "AIzaSyCMokLbdc7ifx7O2wv-5YOdJKenfrNIW-o";
            //string senderId = "115629030288";
            string nomeEquipe = "";

            var data = new
            {
                to = deviceId,
                data = new
                {
                    tag = label,
                    title = titulo,
                    body = texto,
                    click_action = intent,
                    nome_equpe = nomeEquipe,
                    equipe = "",
                    codigoContrato = codCt,
                    projeto = codprojeto,
                    contrato = codcontrato,
                    obra = codobra

                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            if (String.IsNullOrEmpty(numFre))
                numFre = "";
            Task.Run(async () => { EnviarNotificacao(applicationID, json, deviceId, numFre, intent); });



        }

        public static void NotificarChatBackOffice(String deviceId, String label, String titulo, String codCt, String codprojeto, String codcontrato, String codobra, String numFre = null, String jsonMensagem = null)
        {
            string applicationID = "AIzaSyCMokLbdc7ifx7O2wv-5YOdJKenfrNIW-o";
            string nomeEquipe = "";

            var data = new
            {
                to = deviceId,
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
            };

            var json = new JavaScriptSerializer().Serialize(data);
            if (String.IsNullOrEmpty(numFre))
                numFre = "";
            Task.Run(async () => { EnviarNotificacao(applicationID, json, deviceId, numFre, "ChatBackOffice"); });

        }

        public static void Notificar(String deviceId, String nomeRecurso, String descricaoRecurso, String tag, String titulo, String texto, String type, String numFre, String jsonData = null)
        {
            //try
            //{
            string applicationID = CONSTANTES.APLICATION_ID_POINTO_NOVO;
            /*WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = "application/json";*/
            var data = new
            {
                to = deviceId,
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
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            EnviarNotificacao(applicationID, json, deviceId, numFre, type.ToString());


        }

        public static void Notificar(String deviceId, String nomeRecurso, String descricaoRecurso, String tag, String titulo, String texto, Recurso_Mobile type, String numFre, String jsonData = null)
        {
            //try
            //{
            string applicationID = CONSTANTES.APLICATION_ID_POINTO_NOVO;
            /*WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = "application/json";*/
            var data = new
            {
                to = deviceId,
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
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            EnviarNotificacao(applicationID, json, deviceId, numFre, type.ToString());
            /*Byte[] byteArray = Encoding.UTF8.GetBytes(json);

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
                            string str = sResponseFromServer;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            string str = ex.Message;
        }*/


        }

        public static void NotificarUsuarioVencimentoFatura(String deviceId, String titulo, String emoji, String mensagem, String numFre = null)
        {
            string applicationID = CONSTANTES.APLICATION_ID_OLA;
            var data = new
            {
                to = deviceId,
                data = new
                {
                    title = titulo,
                    body = mensagem,
                    emoji = emoji,
                    type = 2
                }
            };
            if (String.IsNullOrEmpty(numFre))
                numFre = "";
            EnviarNotificacao(applicationID, new JavaScriptSerializer().Serialize(data), deviceId, numFre, "VencimentoFaturaOlá");
        }

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
