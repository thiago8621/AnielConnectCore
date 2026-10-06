using RestSharp;
using RestSharp.Authenticators;
using Sinapse.JSON;
using Sinapse.SerasaApi.Server.Responses;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Sinapse.SerasaApi
{

    public class SerasaDados
    {
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
        public async Task<String> RetornoDadosCpf(String cpf, String login, String senha, string url)
        {

            string user = login;
            string pass = senha;
            
            //Oito espaços em branco para nova senha
            string nova_senha = "        ";
            //var layout = token;
            var layout = "FC     FI0001000000000000000N99SINIAN                                            N                       S                                                                                                                                                                                                                                                                             P002RE02                     REHMHSPN                                                                              N00100PPBPCBN0                                                                                                     N00300                     SPRXPS                                                                                  T999";
           
            string baseURL = url + user + pass + nova_senha + layoutbase + cpf + layout;

            try
            {
                var client = new RestClient(baseURL);

                var request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.RequestFormat = DataFormat.None;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                IRestResponse response = await client.ExecutePostAsync(request);

                
                if (!String.IsNullOrEmpty(response.ErrorMessage))
                {
                    throw new Exception("Serasa " + response.ErrorMessage, response.ErrorException);
                }
                String  retorno = response.Content;            

                return retorno;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }


        public async Task<String> RetornoDadosCnpj(String cnpj, String login, String senha, string url)
        {

            string user = login;
            string pass = senha;
            string nova_senha = "        ";
            //var layout = token;
            var layout = "JC     CH0000000             00SINIA                                  00000000000 00      000000000000000 C83892A S100        000000000000000 000000000               00                                                                                                0000000000000000                0                                   00                                         P002RE02                     REHJ                                                                                  N00100PPBPCBN0N                                   S                                                                N00300000000000000000000000  NRC5                                                                                  T999";
            
            //url homologacao
            //string baseURL = "https://mqlinuxext-2.serasa.com.br/Homologa/consultahttps?p=" + user + pass + nova_senha + layoutbase + cnpj + layout;
           
            //url producao
            //string baseURL = "https://sitenet43-2.serasa.com.br/Prod/consultahttps?p=" + user + pass + nova_senha + layoutbase + cnpj + layout;
            string baseURL = url + user + pass + nova_senha + layoutbasePJ + cnpj + layout;

            try
            {
                var client = new RestClient(baseURL);

                var request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.RequestFormat = DataFormat.None;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                IRestResponse response = await client.ExecutePostAsync(request);


                if (!String.IsNullOrEmpty(response.ErrorMessage))
                {
                    throw new Exception("Serasa " + response.ErrorMessage, response.ErrorException);
                }
                String retorno = response.Content;

                return retorno;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
