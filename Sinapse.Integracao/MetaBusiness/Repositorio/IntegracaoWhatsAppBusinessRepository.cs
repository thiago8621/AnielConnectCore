using Aniel.Connect.Model.enumerado;
using Sinapse.Integracao.MetaBusiness.Interface;
using Sinapse.Integracao.MetaBusiness.Models;
using System.Threading.Tasks;

namespace Sinapse.Integracao.WhatsAppBusiness.Repositorio
{
    public class IntegracaoWhatsAppBusinessRepository
    {
        public async Task<WhatsAppBusinessResponse> GetAccessToken(IntegracaoWhatsAppBusiness integracao, string token)
        {
            return integracao.GetAccessToken(token);
        } 
        public async Task<ModeloMensagemResponse> VincularModeloMensagem(IntegracaoWhatsAppBusiness integracao, string token, string idWhatsAppBusinessAccount, string mensagem, string nome)
        {
            return integracao.VincularModeloMensagem(token, idWhatsAppBusinessAccount, mensagem, nome);
        }
        
        public async Task<ModeloMensagemEditarResponse> AtualizarModeloMensagem(IntegracaoWhatsAppBusiness integracao, string idModeloMensagem, string mensagem, string nome)
        {
            return integracao.AtualizarModeloMensagem(idModeloMensagem, mensagem, nome);
        }
        
        public async Task<ModeloStatusResponse> BuscaStatusModeloMensagem(IntegracaoWhatsAppBusiness integracao, string token, string nome)
        {
            return integracao.BuscaStatusModeloMensagem(token, nome);
        }

        public async Task<ModelosCadastradosResponse> AtualizarStatusModeloMensagem(IntegracaoWhatsAppBusiness integracao, string idWhatsAppBusinessIdentificacao)
        {
            return integracao.AtualizarStatusModeloMensagem(idWhatsAppBusinessIdentificacao);
        }

        public async Task<EnviaModeloMensagemResponse> EnviaModeloMensagemWhatsAppBusiness(IntegracaoWhatsAppBusiness integracao, string numero, string idTelefone, string modeloMensagem)
        {
            return integracao.EnviaModeloMensagemWhatsAppBusiness(numero, idTelefone, modeloMensagem);
        }

        public async Task<MidiaUrlResponse> RecuperaURLMidia(IntegracaoWhatsAppBusiness integracao, string idMidia)
        {
            return integracao.RecuperaURLMidia(idMidia);
        }

        public async Task<EnviaMensagemLivreResponse> EnviaMensagemLivreWhatsAppBusiness(IntegracaoWhatsAppBusiness integracao, string numero, string idNumeroTelefone, string mensagem)
        {
            return integracao.EnviaMensagemLivreWhatsAppBusiness(numero, idNumeroTelefone, mensagem);
        }        

        public async Task<EnviaMensagemLivreResponse> EnviaMensagemMidiaWhatsAppBusiness(IntegracaoWhatsAppBusiness integracao, string numero, string idNumeroTelefone, string mensagem, string url, Tipo_Bot_Evento_Anexo tipo)
        {
            return integracao.EnviaMensagemMidiaWhatsAppBusiness(numero, idNumeroTelefone, mensagem, url, tipo);
        }
    }
}