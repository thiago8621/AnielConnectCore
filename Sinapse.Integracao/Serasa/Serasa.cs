using Aniel.Connect.Core;
using RestSharp;
using Sinapse.Integracao.Serasa.Interface;
using Sinapse.Integracao.Serasa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.Serasa
{
    public class Serasa : BaseApiController, IntegracaoSerasa
    {   
   
        public string tokenLayoutFixo{ get; set; }

        public RetornoCpfSerasaModel GetConsultaSerasaCpf(string cpf)
        {
            RetornoCpfSerasaModel retorno = new RetornoCpfSerasaModel();

            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();

                headers.Add("usuario", retorno.Usuario);
                headers.Add("senha", retorno.Senha);
                headers.Add("B49C      ", tokenLayoutFixo);
                headers.Add("cpf", retorno.Cpf);
                headers.Add("layout", retorno.LayoutString);

                var retornoApi = GetIntegracao<ResponseConsulta>("https://mqlinuxext-2.serasa.com.br/Homologa/consultahttps?p=", headers);

                if (retornoApi != null)
                {
                    retorno.Sucesso = true;
                    retorno.RetornoString = retornoApi.ToString();
                }
                else
                {
                    retorno.Sucesso = false;
                }
            }
            catch(Exception ex)
            {
                retorno.ErroMensagem = ex.Message;
                retorno.Sucesso = false;
            }

            return retorno;
        }


        public RetornoCnpjSerasaModel GetConsultaSerasaCnpj(string cnpj)
        {
            RetornoCnpjSerasaModel retorno = new RetornoCnpjSerasaModel();

            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();

                headers.Add("usuario", retorno.Usuario);
                headers.Add("senha", retorno.Senha);
                headers.Add("B49C      ", tokenLayoutFixo);
                headers.Add("cnpj", retorno.Cnpj);
                headers.Add("layout", retorno.LayoutString);

                var retornoApi = GetIntegracao<ResponseConsulta>("https://mqlinuxext-2.serasa.com.br/Homologa/consultahttps?p=", headers);

                if (retornoApi != null)
                {
                    retorno.Sucesso = true;
                    retorno.RetornoString = retornoApi.ToString();
                }
                else
                {
                    retorno.Sucesso = false;
                }
            }
            catch (Exception ex)
            {
                retorno.ErroMensagem = ex.Message;
                retorno.Sucesso = false;
            }

            return retorno;
        }
      
        //public RetornoCnpjModel GetCnpjResult(string cnpj)
        //{
        //    String layout = "B49C      " + cnpj + "FC FI0000000            N99SINIAN                                          00N        000000000000000S         1                                                                                                                                                                                                                                                                   P006SSSSS  05SSSSSSSS SSSSSN                                                                                       T999";


        //    List<Parameter> parametros = new List<Parameter>();

        //    parametros.Add(new Parameter("login", token.SerasaUserLogin, ParameterType.GetOrPost));
        //    parametros.Add(new Parameter("senha", token.SenhaToken, ParameterType.GetOrPost));
        //    parametros.Add(new Parameter("novasenha", token.NovaSenhaToken, ParameterType.GetOrPost));
        //    parametros.Add(new Parameter("layout", layout, ParameterType.GetOrPost));


        //    SerasaApiBase apibase = new SerasaApiBase();

        //    RetornoCnpj rcnpj = apibase.PostHTTP<RetornoCnpj>(parametros);

        //    return rcnpj;
        //}

    }
}