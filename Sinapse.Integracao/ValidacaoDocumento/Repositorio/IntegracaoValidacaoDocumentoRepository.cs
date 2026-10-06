using Sinapse.Integracao.MostqVioExtractionModels.Models;
using Sinapse.Integracao.MostqContenctExtractionModels.Models;
using Sinapse.Integracao.ValidacaoDocumento.Interface;
using System.Threading.Tasks;

namespace Sinapse.Integracao.ValidacaoDocumento.Repositorio
{
    public class IntegracaoValidacaoDocumentoRepository
    {
        public async Task<VioExtractionResultResponse> Extract(IntegracaoValidacaoDocumentoVio integracao, VioExtractionRequest value)
        {
            return integracao.Extract(value);
        }
        public async Task<ContentExtractionResultResponse> Extract(IntegracaoValidacaoDocumentoContent integracao, ContentExtractionRequest value)
        {
            return integracao.Extract(value);
        }
    }
}