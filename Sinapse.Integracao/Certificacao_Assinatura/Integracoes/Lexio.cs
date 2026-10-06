
using NHibernate;
using RestSharp;
using Sinapse.DAO;
using Sinapse.Integracao.Certificacao_Assinatura.Interfaces;
using Sinapse.Model;
using Sinapse.Model.enumerados;
using Sinapse.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;


namespace Sinapse.Integracao.Certificacao_Assinatura.Integracoes
{
    public class Lexio : IntegracaoAssinatura
    {
        //ENDPOINT DE ASSINATURA AUTOMÁTICA POR CPF
        //NÃO ESTÁ SENDO USADO, LINK PARA ASSINATURA MANUAL ESTÁ SENDO RETORNADO NO RESPONSE DE ENVIO DO DOCUMENTO
        public Task Assinar_Documento(ISession session, Config_Certificacao_Assinatura_RequestModel config)
        {
            /*{
	            "nome" : "Washington Oliveira",
	            "cpf" : "11111111111",
	            "signer_token" : "55a7797e667d2b3d7b3c4d3e2df17de9ffb3eff3e6e0bea4720bd3a7e1",
	            "data_nascimento" : "05/01/1991"
            }*/
            var body = new
            {
                nome = config.NomeFantasia,
                cpf = config.CpfCnpj.Replace(".", String.Empty).Replace("-", String.Empty),
                signer_token = "token_signatario",
                data_nascimento = config.Data_Nascimento.Value.ToString("dd/MM/yyyy")
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);

            var client = new RestClient("https://app.lexio.legal/api/send_signature_document");
            //client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("lexiotoken", config.Token_Integracao_Assinatura);
            request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                CONSTANTES.log4Net.Error("Lexio API - Assinar_Documento: " + response.ErrorMessage, response.ErrorException);
                throw new Exception("A integração Lexio encontrou um problema e não pôde consultar o documento: " + response.ErrorMessage, response.ErrorException);
            }

            Console.WriteLine(response.Content);

            throw new NotImplementedException();
        }

        public Consultar_Documento_ResponseModel Consultar_Documento(ISession session, Config_Certificacao_Assinatura_RequestModel config)
        {
            Consultar_Documento_ResponseModel retorno = new Consultar_Documento_ResponseModel();

            var client = new RestClient("https://app.lexio.legal/api/view_document_status/"+ config.Token_Documento_Assinatura);
            //client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("lexiotoken", config.Token_Integracao_Assinatura);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                CONSTANTES.log4Net.Error("Lexio API - Consultar_Documento: " + response.ErrorMessage, response.ErrorException);
                throw new Exception("A integração Lexio encontrou um problema e não pôde consultar o documento: " + response.ErrorMessage, response.ErrorException);
            }
            
            Lexio_Consultar_Documento_ResponseModel retornoLexio = JsonService.DeserializeJasonMapper<Lexio_Consultar_Documento_ResponseModel>(response.Content);
            switch (retornoLexio.document.status)
            {
                case Lexio_StatusEnum.Aguardando_Assinatura:
                    retorno.IsAssinado = false;
                    break;
                case Lexio_StatusEnum.Assinado:
                    retorno.IsAssinado = true;
                    retorno.Data_Assinatura = retornoLexio.document.signers.First().signed.date;
                    retorno.Id_Signatario = retornoLexio.document.signers.First().id;
                    retorno.Token_Signatario = retornoLexio.document.signers.First().signer_token;
                    retorno.Link_Download_Documento = retornoLexio.document.download; 
                    Get_Documento_Pdf(config, retornoLexio.document.download);                    
                    break;
                default:
                    break;
            }

            if (config.Id_Contrato.HasValue)
                retorno.Id_Contrato = config.Id_Contrato.Value;
            else 
                retorno.Id_Venda = config.Id_Venda;

            retorno.Id_Cliente = config.Id_Cliente;
            retorno.Tipo_Integracao = TipoIntegracaoAssinaturaEnum.Lexio;
            retorno.Id_Documento_Integracao = retornoLexio.document.document_token;

            return retorno;
        }

        public Enviar_Documento_ResponseModel Enviar_Documento(ISession session, Config_Certificacao_Assinatura_RequestModel config, String termo_adesao_base64)
        {
            Enviar_Documento_ResponseModel retorno = new Enviar_Documento_ResponseModel();

            var client = new RestClient("https://app.lexio.legal/api/receive_document");
            //client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("lexiotoken", config.Token_Integracao_Assinatura);

            /*
              {
                "document": {
                    "filename": "arquivo_123",
                    "base_document": "data:application/pdf;base64,...",
                    "signers": [
                        {
                            "completed_name": "Nome 1",
                            "email": "email@email.com",
                            "function": "parte"
                        }
                    ],
                    "resumo": "resumo here",
                    "send_email": false,
                    "order_by": false
                }
            }
             */
            var signers = new
            {
                completed_name = config.NomeFantasia,
                email = config.Email_Principal ?? config.Email_Auxiliar,
                function = "cliente" //Não alterar
            };
            var body = new 
            {
                document = new
                {
                    filename = String.Format(@"termo_adesao_{0}", config.Id_Venda),
                    base_document = String.Format("data:application/pdf;base64,{0}", termo_adesao_base64),
                    signers = new List<dynamic> { signers },
                    resumo = $"Termo de adesão - Venda Id {config.Id_Venda}",
                    send_email = false, //Não alterar pois o controle do envio de e-mail é nosso
                    //send_email = !String.IsNullOrEmpty(cliente.Email_Principal) || !String.IsNullOrEmpty(cliente.Email_Auxiliar),
                    order_by = true
                }
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);

            request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                CONSTANTES.log4Net.Error("Lexio API - Enviar_Documento: " + response.ErrorMessage, response.ErrorException);
                throw new Exception("A integração Lexio encontrou um problema e não pôde realizar o envio do documento: " + response.ErrorMessage, response.ErrorException);
            }

            Lexio_Enviar_Documento_ResponseModel retornoLexio = JsonService.DeserializeJasonMapper<Lexio_Enviar_Documento_ResponseModel>(response.Content);

            retorno.Id_Venda = config.Id_Venda;
            retorno.Id_Cliente = config.Id_Cliente;
            retorno.Id_Documento_Integracao = retornoLexio.document.document_token;
            retorno.Id_Signatario = retornoLexio.document.signers.First().id;
            retorno.Token_Signatario = retornoLexio.document.signers.First().signer_token;
            retorno.Link_Assinatura = retornoLexio.document.signers.First().signer_link;
            retorno.Tipo_Integracao = TipoIntegracaoAssinaturaEnum.Lexio;
            retorno.Data_Envio_Documento = retornoLexio.document.created_at.date;

            return retorno;
        }

        public String Get_Documento_Pdf(Config_Certificacao_Assinatura_RequestModel config, String url_pdf) //Trocar assinatura do método pra receber URL do PDF
        {
            /*var client = new RestClient("https://app.lexio.legal/api/content/signed_document/");
            //client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("lexiotoken", configEmpresa.Token_Integracao_Assinatura);
            IRestResponse response = client.Execute(request);
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                CONSTANTES.log4Net.Error("Lexio API - Get_Documento_Pdf: " + response.ErrorMessage, response.ErrorException);
                throw new Exception("A integração Lexio encontrou um problema e não pôde obter o PDF do documento: " + response.ErrorMessage, response.ErrorException);
            }

            Console.WriteLine(response.Content);
            */

            string caminhoProjetoWebApi = CONSTANTES.URLPortal;
            string caminhoProjeto = CONSTANTES.CaminhoFisico;

            if (String.IsNullOrEmpty(caminhoProjetoWebApi))
            {
                throw new Exception("Url do Connect não definida na TB_CONFIG_URL.");
            }

            if (String.IsNullOrEmpty(caminhoProjeto))
            {
                throw new Exception("CaminhoProjetoConnect não definido no empresa.config");
            }

            String nome_pdf = $@"termo_adesao_{config.Id_Venda}.pdf";

            string pastaTermoAdesao = String.Format(@"{0}\Arquivos\TermoAdesao", caminhoProjeto);
            if (!Directory.Exists(pastaTermoAdesao)) Directory.CreateDirectory(pastaTermoAdesao);

            string caminho_pdf_local = String.Format(@"{0}\Arquivos\TermoAdesao\{1}", caminhoProjeto, nome_pdf);            

            using (WebClient client = new WebClient())
            {
                string link_baixar_pdf = url_pdf;

                if (!System.IO.Directory.Exists($@"{CONSTANTES.CaminhoFisico}/Arquivos/Tmp"))
                {
                    System.IO.Directory.CreateDirectory($@"{CONSTANTES.CaminhoFisico}/Arquivos/Tmp");
                }

                if (!System.IO.Directory.Exists($@"{CONSTANTES.CaminhoFisico}/Arquivos/TermoAdesao"))
                {
                    System.IO.Directory.CreateDirectory($@"{CONSTANTES.CaminhoFisico}/Arquivos/TermoAdesao");
                }

                client.Headers.Add("lexiotoken", config.Token_Integracao_Assinatura);
                client.DownloadFile(link_baixar_pdf, caminho_pdf_local);

                string caminho_pdf = String.Format(@"{0}/Arquivos/TermoAdesao/{1}", caminhoProjetoWebApi, nome_pdf);
                //var bytes = System.IO.File.ReadAllBytes(caminho_pdf_local);

                return caminho_pdf;
            }

            //throw new NotImplementedException();
        }        
    }
}