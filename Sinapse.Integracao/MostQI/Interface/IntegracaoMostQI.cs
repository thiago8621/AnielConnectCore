using Aniel.Connect.Repository.Model;

namespace Sinapse.Integracao.MostQI.Interface
{
    public interface IntegracaoMostQI
    {
        public MostQIResponse GetAccessToken(string token);
    }
}