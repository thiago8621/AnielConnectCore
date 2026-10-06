using Aniel.Connect.Repository.Model;
using Sinapse.Integracao.MostQI.Interface;
using System.Threading.Tasks;

namespace Sinapse.Integracao.MostQI.Repositorio
{
    public class IntegracaoMostQIRepository
    {
        public async Task<MostQIResponse> GetAccessToken(IntegracaoMostQI integracao, string token)
        {
            return integracao.GetAccessToken(token);
        }
    }
}