using Aniel.Connect.Core.AspNetCore.Swagger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sinapse.DAO.AspNetCore;
using Sinapse.Util.AspNetCore;
using System;
using System.Collections.Generic;
using System.Net.Mime;

namespace Aniel.Connect.Core.AspNetCore.ControllersWebApi
{
    [HideInDocs]
    public class EmailApiController : BaseApiController
    {
        [HttpPost]
        [Route("api/EmailApi/postEnvioEmail")]
        public Response postEnvioEmail(ParamWebApi<EmailModels> value) {
            var retorno = new Response();
            try
            {
            //envio de email
                var email = new EmailUtil();
                email.EnviarMensagem(value.Objeto.para, value.Objeto.assunto, value.Objeto.mensagem);
                retorno.result = true;
                retorno.message = "Mensagem Enviada com sucesso";
                
            }
            catch (Exception ex)
            {
                retorno.result = false;
                retorno.message = "Falha ao Enviar a Mensagem";
                retorno.stacktrace = ex.StackTrace;
                retorno.exception = ex.Message; 

            }

            return retorno;
        }


        [HttpPost]
        [Route("api/EmailApi/enviarComprovantePonto")]
        public Response<Boolean> enviarComprovantePonto()
        {
            Response<Boolean> retorno = new Response<Boolean>();

            try
            {
                HttpContext context = this.HttpContext;
                String json = context.Request.Form["json"];

                ParamWebApi<EmailModels> value = JsonService.DeserializeJasonMapper<ParamWebApi<EmailModels>>(json);

                if (String.IsNullOrEmpty(CONSTANTES_CORE.Email_De))
                    throw new Exception("Email_De não informado no empresa.config");
                if (String.IsNullOrEmpty(CONSTANTES_CORE.Email_Senha))
                    throw new Exception("Email_Senha não informado no empresa.config");
                if (String.IsNullOrEmpty(CONSTANTES_CORE.Email_Smtp))
                    throw new Exception("Email_Smtp não informado no empresa.config");
                if (Int32.Equals(CONSTANTES_CORE.Email_Porta, 0))
                    throw new Exception("Email_Porta não informado no empresa.config");

                EmailUtil emailUtil = new EmailUtil(CONSTANTES_CORE.Email_De, CONSTANTES_CORE.Email_Senha, CONSTANTES_CORE.Email_Smtp, CONSTANTES_CORE.Email_Porta);

                var email = value.Objeto;

                foreach (var item in email.anexos ?? new List<string>())
                {
                    if (context.Request.Form.Files[item] == null)
                    {
                        continue;
                    }

                    using (var fileStream = context.Request.Form.Files[item].OpenReadStream())
                    {
                        emailUtil.Anexar(item, fileStream, new ContentType(context.Request.Form.Files[item].ContentType));
                    }
                }

                String mensagemErro = null;
                emailUtil.EnviarMensagem_ComprovanteMarcacao(email.para, email.assunto, email.mensagem, out mensagemErro);

                if (!String.IsNullOrEmpty(mensagemErro))
                {
                    throw new Exception(mensagemErro);
                }

                retorno.result = true;
                retorno.valor = true;
            }
            catch (Exception ex)
            {
                retorno.result = false;
                retorno.message = ex.Message;
                retorno.stacktrace = ex.InnerException != null ? ex.InnerException.Message : String.Empty;
                AddModelError(ex?.Message, ex?.InnerException?.Message, ex?.StackTrace);
            }
            return retorno;
        }
    }

    public class EmailModels
    {
        public String para { get; set; }
        public String assunto { get; set; }
        public String mensagem { get; set; }
        public List<String> anexos { get; set; }
    }
}
