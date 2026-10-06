using Sinapse.Integracao.MostqVioExtractionModels.Models;

namespace Sinapse.Integracao.ValidacaoDocumento.Interface
{
    public interface IntegracaoValidacaoDocumentoVio
    {
        public VioExtractionResultResponse Extract(VioExtractionRequest request);
    }
}