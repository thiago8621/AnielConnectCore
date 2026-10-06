using System.Net.Mail;
using System.Net;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace Sinapse.Util
{
    public class EmailUtil
    {

        private string de { get; set; }
        private string usuario { get; set; }
        private string senha { get; set; }
        private string smtp { get; set; }
        private int porta { get; set; }
        private string body { get; set; }

        private List<String> ccos { get; set; }
        private List<String> cc { get; set; }
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
            this.usuario = null;

            this.cc = new List<string>();
            this.ccos = new List<string>();
            this.anexos = new List<object>();
            this.anexosCorpo = new Dictionary<string, string>();
        }

        /// <summary>
        /// Construtor da classe utilitária para envio de e-mails.
        /// </summary>
        /// <param name="de">
        /// Endereço e-mail de origem.
        /// </param>
        /// <param name="senha">
        /// Senha para autenticação do provedor SMTP.
        /// </param>
        /// <param name="smtp">
        /// Endereço do provedor SMTP.
        /// </param>
        /// <param name="porta">
        /// Porta utilizada no provedor SMTP.
        /// </param>
        /// <param name="usuario">
        /// Parâmetro opcional. Usuário para autenticação do provedor SMTP, se o provedor não utiliza usuário, o e-mail é utilizado na autenticação.
        /// </param>
        /// 
        public EmailUtil(string de, string senha, string smtp, int porta, string usuario = null)
        {
            this.de = de;
            this.usuario = usuario;
            this.senha = senha;
            this.smtp = smtp;
            this.porta = porta;

            this.cc = new List<string>();
            this.ccos = new List<string>();
            this.anexos = new List<object>();
            this.anexosCorpo = new Dictionary<string, string>();
        }

        public EmailUtil(string de, string senha, string smtp, int porta, List<string> cc, List<string> ccos, string usuario = null)
        {
            this.de = de;
            this.usuario = usuario;
            this.senha = senha;
            this.smtp = smtp;
            this.porta = porta;
            this.cc = new List<String>();
            this.ccos = new List<String>();
            this.anexos = new List<object>();
            this.anexosCorpo = new Dictionary<string, string>();

            if (cc != null && cc.Count > 0)
            {
                foreach (var item in cc)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        this.CC(item);
                    }

                }
            }

            if (ccos != null && ccos.Count > 0)
            {
                foreach (var item in ccos)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        this.CCO(item);
                    }
                        
                }
            }
        }

        public void CC(string value)
        {
            this.cc.Add(value);
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

        public Boolean EnviarMensagem_ComprovanteMarcacao(string para, string assunto, string mensagem, out String mensagemErro, bool ssl)
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
                    host.EnableSsl = ssl;
                    string user_smtp = String.IsNullOrEmpty(this.usuario) ? this.de : this.usuario;
                    host.Credentials = new NetworkCredential(user_smtp, senha);
                    host.Send(messagemEmail);
                }
            }
            mensagemErro = null;
            return true;                       
        }

        public void EnviarMensagem(string para, string assunto, string mensagem, bool ssl, string messageIdPai = null)
        {
            assunto = Regex.Replace(assunto, @"\t|\n|\r", "");
            body = mensagem;
            MailMessage messagemEmail = new MailMessage(de, para);

            foreach (string cco in (ccos ?? new List<string>())) messagemEmail.Bcc.Add(new MailAddress(cco));
            foreach (string contato in (cc ?? new List<string>())) messagemEmail.CC.Add(new MailAddress(contato));

            if (anexosCorpo != null)
                foreach (string anexbody in anexosCorpo.Keys) messagemEmail.Attachments.Add(AnexarBodyE(anexbody, anexosCorpo[anexbody]));

            foreach (object anex in anexos ?? new List<object>())
            {
                Attachment a = anexarE(anex);
                if (a != null) messagemEmail.Attachments.Add(a);
            }

            if (!string.IsNullOrEmpty(messageIdPai)) 
                messagemEmail.Headers.Add("Message-Id", messageIdPai);

            messagemEmail.Subject = assunto;
            messagemEmail.IsBodyHtml = true;
            messagemEmail.Body = body;
            messagemEmail.ReplyToList.Add(de);
            SmtpClient host = new SmtpClient(smtp, porta);
            host.EnableSsl = ssl;

            string user_smtp = String.IsNullOrEmpty(this.usuario) ? this.de : this.usuario;
            host.Credentials = new NetworkCredential(user_smtp, senha);
            host.Send(messagemEmail);
            messagemEmail.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="para">Email para o cliente a ser enviado</param>
        /// <param name="assunto">Assunto do email</param>
        /// <param name="mensagem">Mensagem do corpo do email</param>
        /// <param name="ssl">Verdadeiro ou Falso</param>
        /// <param name="messageIdPai">ID da mensagem de referencia (o primeiro messageId enviado na interacao com o cliente no atendimento connect -> cliente)</param>
        public void EnviarMensagemReplys(string para, string assunto, string mensagem, bool ssl, string messageIdAtual, 
                                        List<string> referenciasAnteriores = null, List<string> mensagensAnteriores = null)
        {
            assunto = $"Re: {assunto}";/*+ ""Regex.Replace(assunto, @"\t|\n|\r", "");*/

            string mensagemCompleta = string.Empty;

            if (mensagensAnteriores != null && mensagensAnteriores.Count > 0)
            {
                mensagemCompleta = mensagem + "<br><br><br><br><br><br><br>";
                
                if (mensagensAnteriores.Any(x => x.Contains(mensagem)))
                {
                    var indexOf = mensagensAnteriores.FindIndex(x => x.Contains(mensagem));
                    mensagensAnteriores.RemoveAt(indexOf);
                }

                foreach (var mss in mensagensAnteriores)
                {
                    mensagemCompleta = string.Concat(mensagemCompleta, mss);
                }
            }
            else
            {
                mensagemCompleta = mensagem;
            }

            body = mensagemCompleta;
            MailMessage messagemEmail = new MailMessage(de, para);

            foreach (string cco in (ccos ?? new List<string>())) messagemEmail.Bcc.Add(new MailAddress(cco));
            foreach (string contato in (cc ?? new List<string>())) messagemEmail.CC.Add(new MailAddress(contato));
            foreach (string anexbody in anexosCorpo.Keys) messagemEmail.Attachments.Add(AnexarBodyE(anexbody, anexosCorpo[anexbody]));

            foreach (object anex in anexos ?? new List<object>())
            {
                Attachment a = anexarE(anex);
                if (a != null) messagemEmail.Attachments.Add(a);
            }

            string referencias = ConcatenarReferencias(referenciasAnteriores);

            messagemEmail.ReplyToList.Add(de);
            messagemEmail.Headers.Add("In-Reply-To", referenciasAnteriores.First());
            messagemEmail.Headers.Add("References", referencias);
            messagemEmail.Headers.Add("Message-Id", messageIdAtual);
            messagemEmail.Subject = assunto;
            messagemEmail.IsBodyHtml = true;
            messagemEmail.Body = body;

            SmtpClient host = new SmtpClient(smtp, porta);
            host.EnableSsl = ssl;
            string user_smtp = String.IsNullOrEmpty(this.usuario) ? this.de : this.usuario;
            host.Credentials = new NetworkCredential(user_smtp, senha);
            host.Send(messagemEmail);

            messagemEmail.Dispose();
        }

        private string ConcatenarReferencias(List<string> referencias)
        {
            string referenciaResponse = "";

            foreach (var refs  in referencias)
            {
                string manuseio = referenciaResponse;

                if (string.IsNullOrEmpty(manuseio))
                    manuseio = refs;
                else
                    manuseio = string.Concat(manuseio, " ", refs);

                referenciaResponse = manuseio;
            }

            return referenciaResponse;
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

                string user_smtp = String.IsNullOrEmpty(this.usuario) ? this.de : this.usuario;
                host.Credentials = new NetworkCredential(user_smtp, senha);
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

