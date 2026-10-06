using System.Collections.Generic;
using System;
using Sinapse.Integracao.Rastreamento.Integracoes;


namespace Sinapse.Integracao.MetaBusiness.Models
{

    public class WhatsAppBusinessResponse
    {
        public string token { get; set; }
    }

    #region Mensagem Mídia
    public class Image
    {
        public string link { get; set; }
    }

    public class EnviaMensagemMidia
    {
        public string messaging_product { get; set; }
        public string recipient_type { get; set; }
        public string to { get; set; }
        public string type { get; set; }
        public Image image { get; set; }
    }


    #endregion

    #region Mensagem Livre
    public class EnviaMensagemLivre
    {
        public string messaging_product { get; set; }
        public string to { get; set; }
        public string type { get; set; }
        public Text text { get; set; }
    }

    public class Text
    {
        public string body { get; set; }
    }

    public class EnviaMensagemLivreResponse
    {
        public string messaging_product { get; set; }
        public List<Contact> contacts { get; set; }
        public List<Message> messages { get; set; }
    }

    public class ErrorResponse
    {
        public ErrorDetail Error { get; set; }
    }

    public class ErrorDetail
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public int Code { get; set; }
        public ErrorData Error_Data { get; set; }
        public string Fbtrace_Id { get; set; }
    }

    public class ErrorData
    {
        public string Messaging_Product { get; set; }
        public string Details { get; set; }
    }


    #endregion

    #region Modelo de Mensagem WhatsApp Business
    public class ModeloMensagemResponse
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public Error error { get; set; }
    }

    public class Error
    {
        public string message { get; set; }
        public string type { get; set; }
        public int code { get; set; }
        public int error_subcode { get; set; }
        public bool is_transient { get; set; }
        public string error_user_title { get; set; }
        public string error_user_msg { get; set; }
        public string fbtrace_id { get; set; }
    }

    public class Contact
    {
        public string input { get; set; }
        public string wa_id { get; set; }
    }

    public class Message
    {
        public string id { get; set; }
    }

    public class ModeloMensagemEditarResponse
    {
        public string success { get; set; }
        public Error error { get; set; }
    }

    public class Component
    {
        public string type { get; set; }
        public string text { get; set; }
        public Example example { get; set; }
    }

    public class Example
    {
        public List<List<string>> body_text { get; set; }
    }

    public class ModeloMensagem
    {
        public string name { get; set; }
        public string language { get; set; }
        public string category { get; set; }
        public bool allow_category_change { get; set; }
        public List<Component> components { get; set; }
    }


    public class Cursors
    {
        public string before { get; set; }
        public string after { get; set; }
    }

    public class Datum
    {
        public string name { get; set; }
        public string status { get; set; }
        public string id { get; set; }
    }

    public class Paging
    {
        public Cursors cursors { get; set; }
        public string next { get; set; }
    }

    public class ModelosCadastradosResponse
    {
        public List<Datum> data { get; set; }
        public Paging paging { get; set; }
    }

    public class ModeloStatusResponse
    {
        public string status { get; set; }
        public string id { get; set; }
    }
    public class EnviaModeloMensagemResponse
    {
        public string MessagingProduct { get; set; }
        public List<Contact> Contacts { get; set; }
        public List<Message> Messages { get; set; }
    }


    public class Language
    {
        public string code { get; set; }
        public string policy { get; set; }
    }

    public class EnviaModeloMensagem
    {
        public string messaging_product { get; set; }
        public string to { get; set; }
        public string type { get; set; }
        public Template template { get; set; }
    }

    public class Template
    {
        public string name { get; set; }
        public Language language { get; set; }
    }
    #endregion

    public class RecuperaMidiaUrlResponse
    {
        public string url { get; set; }
        public string mime_type { get; set; }
        public string sha256 { get; set; }
        public int file_size { get; set; }
        public string id { get; set; }
        public string messaging_product { get; set; }
    }

    public class MidiaUrlResponse
    {
        public string url { get; set; }
        public string mime_type { get; set;}
        public string name { get; set; }
    }



}
