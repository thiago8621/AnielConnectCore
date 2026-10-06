using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Sinapse.Util.AspNetCore
{
    public class EmailUtil
    {

        private string de { get; set; }
        private string senha { get; set; }
        private string smtp { get; set; }
        private int porta { get; set; }
        private string body { get; set; }

        private List<String> ccos { get; set; }
        private List<object> anexos { get; set; }
        private Dictionary<String, String> anexosCorpo;

        public static void EnviarEmail(String Assunto, String CorpoEmail, Exception ex = null)
        {
            try
            {
                MailMessage objEmail = new MailMessage
                {

                    //rementente do email
                    From = new MailAddress("aniel@sinapseinformatica.com.br"),

                    //email para resposta(quando o destinatário receber e clicar em responder, vai para:)
                    //ReplyTo = new MailAddress("email@seusite.com.br")

                    //prioridade do email
                    Priority = MailPriority.Normal,
                    //utilize true pra ativar html no conteúdo do email, ou false, para somente texto
                    IsBodyHtml = true,
                    //Assunto do email
                    Subject = Assunto,
                    //corpo do email a ser enviado
                    Body = CorpoEmail + DadosException(ex),
                    //codificação do assunto do email para que os caracteres acentuados serem reconhecidos.
                    SubjectEncoding = Encoding.GetEncoding("ISO-8859-1"),
                    //codificação do corpo do emailpara que os caracteres acentuados serem reconhecidos.
                    BodyEncoding = Encoding.GetEncoding("ISO-8859-1"),
                };

                //cria o objeto responsável pelo envio do email
                SmtpClient objSmtp = new SmtpClient
                {
                    //endereço do servidor SMTP(para mais detalhes leia abaixo do código)
                    Host = "mail.sinapseinformatica.com.br",
                    Port = 587,
                    //para envio de email autenticado, coloque login e senha de seu servidor de email para detalhes leia abaixo do código
                    Credentials = new NetworkCredential("aniel@sinapseinformatica.com.br", "atl874")
                };

                //destinatário(s) do email(s). Obs. pode ser mais de um, pra isso basta repetir a linha abaixo com outro endereço
                objEmail.To.Add("julio.martins@sinapseinformatica.com.br");
                //se quiser enviar uma cópia oculta pra alguém, utilize a linha abaixo:
                //objEmail.Bcc.Add("oculto@provedor.com.br");

                //envia o email
                objSmtp.Send(objEmail);
            }
            catch
            {

            }
        }

        private static string DadosException(Exception e)
        {
            if (e == null) return String.Empty;

            StringBuilder dados = new StringBuilder();
            dados.Append("Type: ----<b>" + e.GetType().Name + "</b>");
            dados.Append("Message: ----<b>" + e.Message + "</b>");
            dados.Append("Source: ----" + e.Source + "");
            dados.Append("StackTrace: ----" + e.StackTrace + "");
            dados.Append("TargetSite: ----" + e.TargetSite + "");
            return dados.ToString();
        }

        public EmailUtil()
        {
            this.de = "aniel@sinapseinformatica.com.br";
            this.smtp = "mail.sinapseinformatica.com.br";
            this.porta = 587;
            this.senha = "atl874";

            this.ccos = new List<string>();
            this.anexos = new List<object>();
            this.anexosCorpo = new Dictionary<string, string>();
        }


        public EmailUtil(string de, string senha, string smtp, int porta)
        {
            this.de = de;
            this.senha = senha;
            this.smtp = smtp;
            this.porta = porta;

            this.ccos = new List<string>();
            this.anexos = new List<object>();
            this.anexosCorpo = new Dictionary<string, string>();
        }

        public void CCO(string value)
        {
            this.ccos.Add(value);
        }

        public void Anexar(string value)
        {
            anexos.Add(value);
        }

        public void Anexar(string nome, Stream stream, ContentType type)
        {
            anexos.Add(new AnexoStream()
            {
                n = nome,
                s = stream,
                t = type
            });
        }

        public void AnexarBody(string id, string caminho)
        {
            anexosCorpo.Add(id, caminho);
        }

        public Boolean EnviarMensagem_ComprovanteMarcacao(string para, string assunto, string mensagem, out String mensagemErro)
        {
            try
            {
                body = mensagem;
                using (MailMessage messagemEmail = new MailMessage(de, para, assunto, body))
                {
                    foreach (object anex in anexos ?? new List<object>())
                    {
                        Attachment a = anexarE(anex);
                        if (a != null) messagemEmail.Attachments.Add(a);
                    }

                    using (SmtpClient host = new SmtpClient(smtp, porta))
                    {
                        host.Credentials = new NetworkCredential(de, senha);
                        host.Send(messagemEmail);
                    }
                }
                mensagemErro = null;
                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError(String.Format("{0}\n{1}", e.Message, e.StackTrace));
                mensagemErro = e.Message;
                return false;
            }
        }

        public void EnviarMensagem(string para, string assunto, string mensagem)
        {
            try
            {
                body = mensagem;
                MailMessage messagemEmail = new MailMessage(de, para);

                foreach (string cco in (ccos ?? new List<string>())) messagemEmail.Bcc.Add(new MailAddress(cco));

                foreach (string anexbody in anexosCorpo.Keys) messagemEmail.Attachments.Add(AnexarBodyE(anexbody, anexosCorpo[anexbody]));


                foreach (object anex in anexos ?? new List<object>())
                {
                    Attachment a = anexarE(anex);
                    if (a != null) messagemEmail.Attachments.Add(a);
                }

                messagemEmail.Subject = assunto;
                messagemEmail.IsBodyHtml = true;
                messagemEmail.Body = body;


                SmtpClient host = new SmtpClient(smtp, porta);
                host.EnableSsl = false;

                host.Credentials = new NetworkCredential(de, senha);
                host.Send(messagemEmail);

                messagemEmail.Dispose();
            }
            catch (Exception e)
            {
                Trace.TraceError(String.Format("{0}\n{1}", e.Message, e.StackTrace));
            }
        }

        public void EnviarMensagem(string para, string assunto, string mensagem, bool ssl)
        {

            body = mensagem;
            MailMessage messagemEmail = new MailMessage(de, para);

            foreach (string cco in (ccos ?? new List<string>())) messagemEmail.Bcc.Add(new MailAddress(cco));

            foreach (string anexbody in anexosCorpo.Keys) messagemEmail.Attachments.Add(AnexarBodyE(anexbody, anexosCorpo[anexbody]));


            foreach (object anex in anexos ?? new List<object>())
            {
                Attachment a = anexarE(anex);
                if (a != null) messagemEmail.Attachments.Add(a);
            }

            messagemEmail.Subject = assunto;
            messagemEmail.IsBodyHtml = true;
            messagemEmail.Body = body;


            SmtpClient host = new SmtpClient(smtp, porta);
            host.EnableSsl = ssl;

            host.Credentials = new NetworkCredential(de, senha);
            host.Send(messagemEmail);

            messagemEmail.Dispose();
        }


        public void EnviarMensagem_VendaAprovada(string para, string assunto, string mensagem, Boolean? venda_aprovada)
        {
            try
            {
                body = mensagem;
                MailMessage messagemEmail = new MailMessage(de, para);

                foreach (string cco in (ccos ?? new List<string>())) messagemEmail.Bcc.Add(new MailAddress(cco));

                foreach (string anexbody in anexosCorpo.Keys) messagemEmail.Attachments.Add(AnexarBodyE(anexbody, anexosCorpo[anexbody]));


                foreach (object anex in anexos ?? new List<object>())
                {
                    Attachment a = anexarE(anex);
                    if (a != null) messagemEmail.Attachments.Add(a);
                }

                messagemEmail.Subject = assunto;
                messagemEmail.IsBodyHtml = true;
                messagemEmail.Body = "";
                messagemEmail.BodyEncoding = System.Text.Encoding.UTF8;

                // Email Venda Aprovada 
                if (venda_aprovada.HasValue)
                {
                    if (venda_aprovada.Value == true)
                        //messagemEmail.Body = File.ReadAllText(@"C://Checkouts//Connect//Aniel.Connect//Aniel.Connect//Aniel.Connect//Arquivos//email_venda_aprovada.html", System.Text.Encoding.UTF8).Replace("URL_LOGO", "");
                        messagemEmail.Body = mensagem;
                }

                //

                SmtpClient host = new SmtpClient(smtp, porta);
                host.EnableSsl = false;

                host.Credentials = new NetworkCredential(de, senha);
                host.Send(messagemEmail);

                messagemEmail.Dispose();
            }
            catch (Exception e)
            {
                Trace.TraceError(String.Format("{0}\n{1}", e.Message, e.StackTrace));
            }
        }

        private Attachment anexarE(object value)
        {
            Attachment enviar = null;
            if (value is AnexoStream)
            {
                AnexoStream a = (AnexoStream)value;
                enviar = new Attachment(a.s, a.t);
                enviar.Name = a.n;
            }
            else if (value is string) enviar = new Attachment((string)value);

            return enviar;
        }

        private Attachment AnexarBodyE(string key, string value)
        {
            Attachment attachment = new Attachment(value);

            string contentID = Path.GetFileName(value).Replace(".", "") + "@zofm";

            attachment.ContentDisposition.Inline = true;
            attachment.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
            attachment.ContentId = contentID;
            attachment.ContentType.MediaType = "image/png";
            attachment.ContentType.Name = Path.GetFileName(value);

            body = body.Replace(key, "cid:" + contentID);

            return attachment;
        }
    }

    class AnexoStream
    {
        public string n { get; set; }
        public Stream s { get; set; }
        public ContentType t { get; set; }
    }
}

