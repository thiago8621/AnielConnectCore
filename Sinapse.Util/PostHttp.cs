using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    public class PostHttp : IDisposable
    {
        private ILog Log = null;
        private string AuthorizationToken = null;

        public PostHttp(String pLog4Net)
        {
            if (!String.IsNullOrEmpty(pLog4Net))
                Log = LogManager.GetLogger(pLog4Net);
        }

        public PostHttp(ILog pLog4Net)
        {
            Log = pLog4Net;
        }
        public PostHttp(ILog pLog4Net,string AuthorizationToken)
        {
            Log = pLog4Net;
            this.AuthorizationToken = AuthorizationToken;
        }

        public T Execute<T>(String pUrl, String pJsonParam) where T : new()
        {
            var client = new RestClient(pUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(pJsonParam);

            var response = client.Execute<T>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Erro PostByUrl Status: {response.StatusCode} Url: {pUrl} Obj: {JsonService.SerializeObject(pJsonParam)} Erro: {response.ErrorMessage} {response.ErrorException}");
            }

            return JsonService.DeserializeObject<T>(response.Content);
        }

        public String ExecuteString<T>(String pUrl, String pJsonParam) where T : new()
        {
            var client = new RestClient(pUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");

            if(AuthorizationToken != null)
                request.AddHeader("Authorization", AuthorizationToken);

            request.AddJsonBody(pJsonParam);

            var response = client.Execute<T>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Erro PostByUrl Status: {response.StatusCode} Url: {pUrl} Obj: {JsonService.SerializeObject(pJsonParam)} Erro: {response.ErrorMessage} {response.ErrorException}");
            }

            return response.Content;
        }

        public String ExecuteStringPut<T>(String pUrl, String pJsonParam) where T : new()
        {
            var client = new RestClient(pUrl);
            var request = new RestRequest(Method.PUT);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");

            if (AuthorizationToken != null)
                request.AddHeader("Authorization", AuthorizationToken);

            request.AddJsonBody(pJsonParam);

            var response = client.Execute<T>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Erro PostByUrl Status: {response.StatusCode} Url: {pUrl} Obj: {JsonService.SerializeObject(pJsonParam)} Erro: {response.ErrorMessage} {response.ErrorException}");
            }

            return response.Content;
        }

        public String Execute(String pUrl, Int32 pTimeOut = 0)
        {
            String retorno = String.Empty;

            try
            {
                if (Log != null)
                    Log.Info("Iniciando PostHttp.");

                // Create a request using a URL that can receive a post. 
                WebRequest request = WebRequest.Create(pUrl);

                request.Method = "GET";

                if (pTimeOut != 0)
                    request.Timeout = pTimeOut;

                WebResponse myWebResponse = request.GetResponse();

                Stream ReceiveStream = myWebResponse.GetResponseStream();

                Encoding encode = Encoding.GetEncoding("utf-8");

                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";

                // Pipe the stream to a higher level stream reader with the required encoding format. 
                StreamReader reader = new StreamReader(ReceiveStream, encode);

                try
                {
                    // Read the content.
                    retorno = reader.ReadToEnd();

                    if (Log != null)
                        Log.Info("Finalizado PostHttp.");
                }
                finally
                {
                    reader.Close();
                }


            }
            catch (Exception ex)
            {
                if (Log != null)
                    Log.Error("Erro no serviço :" + pUrl, ex);
                throw new PostHttpException(ex);
            }

            return retorno;
        }

        public String ExecuteAsync(String pUrl, Int32 pTimeOut = 0)
        {
            var client = new RestClient(pUrl);
            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.Timeout = pTimeOut;

            if (AuthorizationToken != null)
                request.AddHeader("Authorization", AuthorizationToken);

            var response = client.Execute(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Erro PostByUrl Status: {response.StatusCode} Url: {pUrl} Erro: {response.ErrorMessage} {response.ErrorException}");
            }

            return response.Content;
        }

        public T GeoMaps<T>(String addrees) where T : class
        {
            T retorno = default(T);

            var request = (HttpWebRequest)HttpWebRequest.Create(addrees);
            request.Method = "GET";

            //request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            Encoding encode = Encoding.GetEncoding("utf-8");
            request.ContentType = "application/json";

            var response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream(), encode);//, encode

            try
            {
                // Read the content.
                String responseFromServer = reader.ReadToEnd();

                // Retorna Objeto
                try
                {
                    retorno = JsonService.DeserializeJasonMapper<T>(responseFromServer);
                }
                catch (Exception ex)
                {
                    if (Log != null)
                        Log.Error("Erro no objeto de retorno:" + responseFromServer, ex);
                    throw new Exception("Erro no serviço de retorno", ex);
                }

                if (Log != null)
                    Log.Info("Finalizado PostHttp.");
            }
            finally
            {
                reader.Close();
            }

            return retorno;
        }

        public T Execute<T>(String pUrl, Int32 pTimeOut = 6000000) where T : class
        {
            T retorno = default(T);

            try
            {
                if (Log != null)
                    Log.Info("Iniciando PostHttp.");

                // Create a request using a URL that can receive a post. 
                WebRequest request = WebRequest.Create(pUrl);

                request.Method = "GET";

                if (pTimeOut != 0)
                    request.Timeout = pTimeOut;

                WebResponse myWebResponse = request.GetResponse();

                Stream ReceiveStream = myWebResponse.GetResponseStream();

                Encoding encode = Encoding.GetEncoding("utf-8");

                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";

                // Pipe the stream to a higher level stream reader with the required encoding format. 
                StreamReader reader = new StreamReader(ReceiveStream, encode);

                try
                {
                    // Read the content.
                    String responseFromServer = reader.ReadToEnd();

                    // Retorna Objeto
                    try
                    {
                        retorno = JsonService.DeserializeJasonMapper<T>(responseFromServer);
                    }
                    catch (Exception ex)
                    {
                        if (Log != null)
                            Log.Error("Erro no objeto de retorno:" + responseFromServer, ex);
                        throw new Exception("Erro no serviço de retorno", ex);
                    }

                    if (Log != null)
                        Log.Info("Finalizado PostHttp.");
                }
                finally
                {
                    reader.Close();
                }


            }
            catch (Exception ex)
            {
                if (Log != null)
                    Log.Error("Erro no serviço :" + pUrl, ex);
                throw new PostHttpException(ex);
            }

            return retorno;
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
