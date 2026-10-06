using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace Sinapse.JSON
{
    public class PostHttp : IDisposable
    {        
        private ILog Log = null;

        public PostHttp(String pLog4Net)
        {
            if (!String.IsNullOrEmpty(pLog4Net))
                Log = LogManager.GetLogger(pLog4Net);
        }

        public PostHttp(ILog pLog4Net)
        {
            Log = pLog4Net;
        }

        public T Execute<T>(String pUrl, String pJsonParam, Int32 pTimeOut = 6000000) where T : class
        {
            T retorno = default(T);
            try
            {
                if (Log != null)
                    Log.Info("Iniciando PostHttp.");

                // Create a request using a URL that can receive a post. 
                WebRequest request = WebRequest.Create(pUrl);

                // Set the Method property of the request to POST.
                request.Method = "POST";

                // Create POST data and convert it to a byte array.
                string postData = pJsonParam;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";
                
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;

                if (Log != null)
                   Log.Info("Iniciando leitura de buffer.");
                
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();

                try
                {
                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);

                    // Close the Stream object.
                    dataStream.Close();

                    // Get the response.
                    WebResponse response = null;
                    request.Timeout = pTimeOut;
                    try
                    {
                        response = request.GetResponse();
                    }
                    catch (WebException ex)
                    {
                        if (pTimeOut == 6000000)
                            throw ex;
                        else
                            return null;
                    }

                    // Display the status.
                    if (Log != null)
                        Log.Info("Status Requisição:" + ((HttpWebResponse)response).StatusDescription);
                    try
                    {

                        // Get the stream containing content returned by the server.
                        dataStream = response.GetResponseStream();

                        // Open the stream using a StreamReader for easy access.
                        StreamReader reader = new StreamReader(dataStream);
                        try
                        {
                            // Read the content.
                            string responseFromServer = reader.ReadToEnd();

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
                    finally
                    {
                        response.Close();
                        request = null;
                    }
                }
                finally
                {
                    dataStream.Close();
                }

            }
            catch (Exception ex)
            {
                if (Log != null)
                   Log.Error("Erro no post do objeto:" + pJsonParam, ex);
                throw new Exception(String.Format("Erro no post do objeto {0}", ex.Message));
            }
            
            return retorno;
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
                throw new Exception(String.Format("Erro no post do objeto {0}", ex.Message));
            }

            return retorno;
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
                throw new Exception(String.Format("Erro no post do objeto {0}", ex.Message));
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
