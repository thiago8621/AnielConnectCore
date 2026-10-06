using Sinapse.Integracao.MostqContenctExtractionModels.Models;

namespace Sinapse.Integracao.ValidacaoDocumento.Interface
{
    public interface IntegracaoValidacaoDocumentoContent
    {
        public ContentExtractionResultResponse Extract(ContentExtractionRequest request);
    }
}