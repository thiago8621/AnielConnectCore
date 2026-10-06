using Resources;
using RestSharp;
using Sinapse.DAO;
using Sinapse.SerasaApi.Server.Request;
using Sinapse.SerasaApi.Server.Responses;
using Sinapse.Util;
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace Sinapse.SerasaApi
{

    public class SerasaDadosPreQual
    {

        public ResponseAuth GetAccessToken(string apiSecret, string apiKey, string usuario, string senha, string urlIntegracao, string domain, string correlationId)
        {
            var url = $"{urlIntegracao}/oauth2/experianone/v1/token";

            var client = new RestClient(url)
            {
                Timeout = -1,
            };

            var body = new RequestAuth()
            {
                client_id = apiKey,
                client_secret = apiSecret,
                username = usuario,
                password = senha
            };

            var request = new RestRequest(url, Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-User-Domain", domain);
            request.AddHeader("X-Correlation-Id", correlationId);
            request.AddJsonBody(body);

            IRestResponse<ResponseAuth> response = client.Execute<ResponseAuth>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com o PreQual. Verifique as permissões necessárias de acesso.");
            }

            ResponseAuth responseBody = JsonService.DeserializeObject<ResponseAuth>(response.Content);

            return responseBody;
        }

        private const string layoutbase = "B49C      0000";
        private const string layoutbasePJ = "B49C      0";



        /// <summary>
        /// 
        /// </summary>
        /// <param name="cpf"></param>
        /// <param name="login"></param>
        /// <param name="senha"></param>        
        /// <param name="url"></param>
        /// <returns></returns>
        /// 

        public async Task<String> RetornoDadosCpf(String cpf, String usuario, String senha, string url, string apisecret, string apiKey, string domain, string correlationId, string paramPreQual)
        {

            string baseURL = $"{url}/decisionanalytics/experianone/{CONSTANTES.KeyUrlPreQual}/services/v1/applications/CreditEligibility";


            try
            {
                var client = new RestClient(baseURL);

                var request = new RestRequest(Method.POST);

                var tokenResult = GetAccessToken(apisecret, apiKey, usuario, senha, url, domain, correlationId).access_token;

                var jsonRequest = new RequestPreQual()
                {
                    DVApplication = new Server.Request.DVApplication
                    {
                        Fonte = "API",
                        Canal = "CE",
                        IDservico = "QualificaçãoCrédito",
                        CDL = new Server.Request.CDL
                        {
                            BlockList = "N",
                            AllowList = "N"
                        },
                        Loja = $"{paramPreQual}"
                    },
                    DVApplicant = new Server.Request.DVApplicant
                    {
                        Applicant = new System.Collections.Generic.List<Server.Request.Applicant>
                        {
                            new Server.Request.Applicant
                            {
                                CPF = cpf
                            }
                        }
                    }
                };

                // Adicionar cabeçalhos
                request.AddHeader("Authorization", $"Bearer {tokenResult}");
                request.AddHeader("Content-Type", "application/json");
                // Adicionar corpo da solicitação
                request.AddParameter("application/json", JsonService.SerializeObject(jsonRequest), ParameterType.RequestBody);
                

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                IRestResponse response = await client.ExecutePostAsync(request);


                if (!String.IsNullOrEmpty(response.ErrorMessage))
                {
                    throw new Exception("Serasa PreQual" + response.ErrorMessage, response.ErrorException);
                }
                String retorno = response.Content;

                return retorno;
            }
            catch (Exception ex)
            {
                throw ex.InnerException ?? ex;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cnpj"></param>
        /// <param name="login"></param>
        /// <param name="senha"></param>        
        /// <param name="url"></param>
        /// <returns></returns>
        /// 

        public async Task<String> RetornoDadosCnpj(String cnpj, String usuario, String senha, string url, string apiSecret, string apiKey, string domain, string correlationId, string paramPreQual)
        {

            string baseURL = $"{url}/decisionanalytics/experianone/{CONSTANTES.KeyUrlPreQual}/services/v1/applications/CreditEligibility";

            try
            {
                var tokenResult = GetAccessToken(apiSecret, apiKey, usuario, senha, url, domain, correlationId).access_token;
                var client = new RestClient(baseURL);

                var request = new RestRequest(Method.POST);
                
                var jsonRequest = new RequestPreQual()
                {
                    DVApplication = new Server.Request.DVApplication
                    {
                        Fonte = "API",
                        Canal = "CE",
                        IDservico = "QualificaçãoCrédito",
                        CDL = new Server.Request.CDL
                        {
                            BlockList = "N",
                            AllowList = "N"
                        },
                        Loja = $"{paramPreQual}"
                    },
                    DVApplicant = new Server.Request.DVApplicant
                    {
                        Applicant = new System.Collections.Generic.List<Server.Request.Applicant>
                        {
                            new Server.Request.Applicant
                            {
                                CNPJ = cnpj
                            }
                        }
                    }
                };

                // Adicionar cabeçalhos
                request.AddHeader("Authorization", $"Bearer {tokenResult}");
                request.AddHeader("Content-Type", "application/json");
                // Adicionar corpo da solicitação
                request.AddParameter("application/json", JsonService.SerializeObject(jsonRequest), ParameterType.RequestBody);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                IRestResponse response = await client.ExecutePostAsync(request);


                if (!String.IsNullOrEmpty(response.ErrorMessage))
                {
                    throw new Exception("Serasa PreQual" + response.ErrorMessage, response.ErrorException);
                }
                String retorno = response.Content;

                return retorno;
            }
            catch (Exception ex)
            {
                throw ex.InnerException ?? ex;
            }
        }
    }
}
