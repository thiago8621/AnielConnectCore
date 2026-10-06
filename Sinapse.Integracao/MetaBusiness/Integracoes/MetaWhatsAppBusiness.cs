using Aniel.Connect.Core;
using System;
using System.Collections.Generic;
using RestSharp;
using Aniel.Connect.Repository.Model;
using Sinapse.Util;
using System.Net;
using Sinapse.Integracao.MetaBusiness.Interface;
using System.Drawing.Imaging;
using System.IO;
using Sinapse.DAO;
using NHibernate.Type;
using System.Linq;
using Aniel.Connect.Model.enumerado;
using NHibernate.Util;
using Sinapse.Integracao.MetaBusiness.Models;

namespace Sinapse.Integracao.MetaBusiness.Integracoes
{
    public class MetaWhatsAppBusiness : BaseApiController, IntegracaoWhatsAppBusiness
    {
        private string tokenApi { get; set; }

        public MetaWhatsAppBusiness(string tokenApi)
        {
            this.tokenApi = tokenApi;
        }

        /// <summary>
        ///     Acessa o endpoint de autenticação da API do WhatsAppBusiness para oter o token de acesso
        /// </summary>
        public WhatsAppBusinessResponse GetAccessToken(string token)
        {
            var urlClient = "https://WhatsAppBusinessapi.com/user/authenticate";
            var client = new RestClient(urlClient)
            {
                Timeout = -1,
            };

            var request = new RestRequest(urlClient, Method.POST)
                .AddHeader("Content-Type", "application/json")
                .AddJsonBody(new { token = $"{token}" });

            IRestResponse<WhatsAppBusinessResponse> response = client.Execute<WhatsAppBusinessResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
            }

            WhatsAppBusinessResponse responseBody = JsonService.DeserializeObject<WhatsAppBusinessResponse>(response.Content);

            return responseBody;
        }


        /// <summary>
        ///     Acessa o endpoint de criar modelo de mensagem da API do WhatsAppBusiness
        /// </summary>
        public ModeloMensagemResponse VincularModeloMensagem(string token, string idWhatsAppBusinessAccount, string mensagem, string nome)
        {
            var modelo = new ModeloMensagem()
            {
                name = nome,
                language = "pt_BR",
                category = "MARKETING",
                allow_category_change = true,
                components = new List<Component>()
            {
                new Component()
                {
                    type = "BODY",
                    text = mensagem,
                },
                }
            };

            //WHATSAPP_BUSINESS_ACCOUNT_ID: 270298542828275
            var url = "https://graph.facebook.com/v16.0/" + idWhatsAppBusinessAccount + "/message_templates";

            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.tokenApi);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(modelo);

            IRestResponse<ModeloMensagemResponse> response = client.Execute<ModeloMensagemResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }
            
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new Exception("Ocorreu um erro no cadastro. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
            }

            ModeloMensagemResponse responseBody = JsonService.DeserializeObject<ModeloMensagemResponse>(response.Content);
            
            if (responseBody.error == null)
            {
                var buscaStatus = BuscaStatusModeloMensagem(idWhatsAppBusinessAccount, nome);

                responseBody.Id = buscaStatus.id;
                responseBody.Status = buscaStatus.status;
            }
            else
            {
                throw new Exception($"Erro ao processar a mensagem: {responseBody.error.message}");
            }

            return responseBody;
        }

        public ModeloStatusResponse BuscaStatusModeloMensagem(string idWhatsAppBusinessIdentificacao, string nome)
        {
            ModeloStatusResponse retorno = new ModeloStatusResponse();
            //WHATSAPP_BUSINESS_ACCOUNT_ID: 270298542828275
            var url = "https://graph.facebook.com/v19.0/" + idWhatsAppBusinessIdentificacao + "/message_templates?fields=name,status&limit=10";
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + this.tokenApi);

            IRestResponse<ModelosCadastradosResponse> response = client.Execute<ModelosCadastradosResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new Exception("Ocorreu um erro no cadastro. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
            }

            ModelosCadastradosResponse responseBody = JsonService.DeserializeObject<ModelosCadastradosResponse>(response.Content);


            if (responseBody.data?.Count > 0)
            {
                var modelo_padrao = responseBody.data
                        .Where(modelo => modelo.name == nome)
                        .FirstOrDefault();

                retorno.id = modelo_padrao.id;
                retorno.status = modelo_padrao.status;
            }

            return retorno;
        }

        public ModelosCadastradosResponse AtualizarStatusModeloMensagem(string idWhatsAppBusinessIdentificacao)
        {
            ModelosCadastradosResponse retorno = new ModelosCadastradosResponse();
            //WHATSAPP_BUSINESS_ACCOUNT_ID: 270298542828275
            var url = "https://graph.facebook.com/v19.0/" + idWhatsAppBusinessIdentificacao + "/message_templates?fields=name,status&limit=10";
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + this.tokenApi);

            IRestResponse<ModelosCadastradosResponse> response = client.Execute<ModelosCadastradosResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new Exception("Ocorreu um erro no cadastro. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
            }

            ModelosCadastradosResponse responseBody = JsonService.DeserializeObject<ModelosCadastradosResponse>(response.Content);

            return responseBody;
        }

        /// <summary>
        ///     Acessa o endpoint de atualizar modelo de mensagem da API do WhatsAppBusiness
        /// </summary>

        public ModeloMensagemEditarResponse AtualizarModeloMensagem(string id, string mensagem, string nome)
        {
            var modelo = new ModeloMensagem()
            {
                name = "modelo_padrao",
                language = "pt_BR",
                category = "MARKETING",
                allow_category_change = true,
                components = new List<Component>()
                    {
                        new Component()
                        {
                            type = "BODY",
                            text = mensagem
                        }
                    }
            };


            //WHATSAPP_BUSINESS_ACCOUNT_ID: 270298542828275
            var url = "https://graph.facebook.com/v19.0/" + id;
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.tokenApi);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(modelo);

            IRestResponse<ModeloMensagemEditarResponse> response = client.Execute<ModeloMensagemEditarResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
            }

            ModeloMensagemEditarResponse responseBody = JsonService.DeserializeObject<ModeloMensagemEditarResponse>(response.Content);

            return responseBody;
        }


        /// <summary>
        ///     Acessa o endpoint de atualizar modelo de mensagem da API do WhatsAppBusiness
        /// </summary>
        public EnviaMensagemLivreResponse EnviaMensagemLivreWhatsAppBusiness(string numero, string idNumeroTelefone, string mensagem)
        {
            var modelo = new
            {
                messaging_product = "whatsapp",
                to = numero,
                type = "text",
                text = new
                {
                    body = mensagem
                }
            };

            //WHATSAPP_BUSINESS_ACCOUNT_ID: 270298542828275
            var url = $"https://graph.facebook.com/v19.0/{idNumeroTelefone}/messages";
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.tokenApi);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(modelo);

            IRestResponse<EnviaMensagemLivreResponse> response = client.Execute<EnviaMensagemLivreResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
            }

            EnviaMensagemLivreResponse responseBody = JsonService.DeserializeObject<EnviaMensagemLivreResponse>(response.Content);

            return responseBody;
        }

        /// <summary>
        ///     Acessa o endpoint de atualizar modelo de mensagem da API do WhatsAppBusiness
        /// </summary>
        public EnviaMensagemLivreResponse EnviaMensagemMidiaWhatsAppBusiness(string numero, string idNumeroTelefone, string mensagem, string urlAnexo, Tipo_Bot_Evento_Anexo tipo)
        {
            var modelo = new object();

            if(tipo == Tipo_Bot_Evento_Anexo.Imagem)
            {
                modelo = new
                {
                    messaging_product = "whatsapp",
                    to = numero,
                    type = "image",
                    image = new
                    {
                        link = urlAnexo
                    }
                };
            }
            
            else if(tipo ==  Tipo_Bot_Evento_Anexo.Video)
            {
                modelo = new
                {
                    messaging_product = "whatsapp",
                    to = numero,
                    type = "video",
                    video = new
                    {
                        link = urlAnexo
                    }
                };
            }
            
            else if(tipo == Tipo_Bot_Evento_Anexo.Audio)
            {
                modelo = new
                {
                    messaging_product = "whatsapp",
                    to = numero,
                    type = "audio",
                    audio = new
                    {
                        link = urlAnexo
                    }
                };
            }

            else if(tipo == Tipo_Bot_Evento_Anexo.Arquivo)
            {
                modelo = new
                {
                    messaging_product = "whatsapp",
                    to = numero,
                    type = "document",
                    document = new
                    {
                        link = urlAnexo
                    }
                };
            }



            //WHATSAPP_BUSINESS_ACCOUNT_ID: 270298542828275
            var url = "https://graph.facebook.com/v19.0/" + idNumeroTelefone + "/messages";
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.tokenApi);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(modelo);

            IRestResponse<EnviaMensagemLivreResponse> response = client.Execute<EnviaMensagemLivreResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                if (!String.IsNullOrEmpty(response.Content))
                {
                    ErrorResponse ErroresponseBody = JsonService.DeserializeObject<ErrorResponse>(response.Content);
                    throw new Exception(ErroresponseBody.Error.Message);
                }
                else
                {
                    throw new Exception("Ocorreu um erro na requisição com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
                }

                    
            }

            EnviaMensagemLivreResponse responseBody = JsonService.DeserializeObject<EnviaMensagemLivreResponse>(response.Content);

            return responseBody;
        }


        /// <summary>
        ///     Acessa o endpoint de atualizar modelo de mensagem da API do WhatsAppBusiness
        /// </summary>
        public EnviaModeloMensagemResponse EnviaModeloMensagemWhatsAppBusiness(string numero, string idNumeroTelefone, string modeloMensagem)
        {

            var modelo = new
            {
                messaging_product = "whatsapp",
                to = numero,
                type = "template",
                template = new
                {
                    name = modeloMensagem,
                    language = new
                    {
                        code = "pt_BR",
                        policy = "deterministic"
                    }
                }
            };

            //WHATSAPP_BUSINESS_ACCOUNT_ID: 270298542828275
            var url = "https://graph.facebook.com/v18.0/" + idNumeroTelefone + "/messages";
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Bearer " + this.tokenApi);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(modelo);

            IRestResponse<EnviaModeloMensagemResponse> response = client.Execute<EnviaModeloMensagemResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
            }

            EnviaModeloMensagemResponse responseBody = JsonService.DeserializeObject<EnviaModeloMensagemResponse>(response.Content);

            return responseBody;
        }


        /// <summary>
        ///     Acessa o endpoint de atualizar modelo de mensagem da API do WhatsAppBusiness
        /// </summary>
        public MidiaUrlResponse RecuperaURLMidia(string idMidia)
        {
            MidiaUrlResponse retorno = new MidiaUrlResponse();

            var url = "https://graph.facebook.com/v19.0/" + idMidia;
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Bearer " + this.tokenApi);
            request.AddHeader("Content-Type", "application/json");

            IRestResponse<RecuperaMidiaUrlResponse> response = client.Execute<RecuperaMidiaUrlResponse>(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
            }

            RecuperaMidiaUrlResponse responseBody = JsonService.DeserializeObject<RecuperaMidiaUrlResponse>(response.Content);

            if (responseBody.file_size > 0)
            {
                var urlMidia = responseBody.url;
                var mime_type = responseBody.mime_type;
                var fileExtension = mime_type.Split('/').Last();

                var clientMidia = new RestClient(urlMidia);
                clientMidia.Timeout = -1;
                var requestMidia = new RestRequest(Method.GET);
                requestMidia.AddHeader("Authorization", "Bearer " + this.tokenApi);
                requestMidia.AddHeader("Content-Type", "application/json");

                IRestResponse responseMidia = clientMidia.Execute(requestMidia);

                if (responseMidia.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new Exception("Ocorreu um erro interno do servidor. Por favor, tente novamente mais tarde.");
                }

                if (responseMidia.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new Exception("Ocorreu um erro na autenticação com a WhatsAppBusiness. Verifique as permissões necessárias de acesso.");
                }

                byte[] responseBodyBytes = responseMidia.RawBytes;

                MemoryStream ms = new MemoryStream(responseBodyBytes);
                var nomeArq = responseBody?.id;

                if (!Directory.Exists($@"{CONSTANTES.CaminhoFisico}\Arquivos\Atendimento\Chat\"))
                {
                    Directory.CreateDirectory($@"{CONSTANTES.CaminhoFisico}\Arquivos\Atendimento\Chat\");
                }

                File.WriteAllBytes($@"{CONSTANTES.CaminhoFisico}\Arquivos\Atendimento\Chat\{nomeArq}.{fileExtension}", responseBodyBytes);

                retorno.url = $@"{CONSTANTES.URLPortal}\Arquivos\Atendimento\Chat\{nomeArq}.{fileExtension}";
                retorno.mime_type = responseBody.mime_type;
                retorno.name = responseBody.sha256;
            }

            return retorno;
        }
    }

}