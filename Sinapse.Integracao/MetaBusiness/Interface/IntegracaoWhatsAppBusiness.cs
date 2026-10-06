using Aniel.Connect.Model.enumerado;
using Aniel.Connect.Repository.Model;
using Sinapse.Integracao.MetaBusiness.Models;

namespace Sinapse.Integracao.MetaBusiness.Interface
{
    public interface IntegracaoWhatsAppBusiness
    {
        public WhatsAppBusinessResponse GetAccessToken(string token);
        public ModeloMensagemResponse VincularModeloMensagem(string token, string idWhatsAppBusinessAccount, string mensagem, string nome);
        public ModeloMensagemEditarResponse AtualizarModeloMensagem(string idModeloMensagem, string mensagem, string nome);
        public ModeloStatusResponse BuscaStatusModeloMensagem(string token, string nome);
        public ModelosCadastradosResponse AtualizarStatusModeloMensagem(string idWhatsAppBusinessIdentificacao);
        public EnviaModeloMensagemResponse EnviaModeloMensagemWhatsAppBusiness(string numero, string idNumeroTelefone, string modeloMensagem);
        public EnviaMensagemLivreResponse EnviaMensagemLivreWhatsAppBusiness(string numero, string idNumeroTelefone, string mensagem);
        public EnviaMensagemLivreResponse EnviaMensagemMidiaWhatsAppBusiness(string numero, string idNumeroTelefone, string mensagem, string url, Tipo_Bot_Evento_Anexo tipo);
        public MidiaUrlResponse RecuperaURLMidia(string idMidia);
    }
}