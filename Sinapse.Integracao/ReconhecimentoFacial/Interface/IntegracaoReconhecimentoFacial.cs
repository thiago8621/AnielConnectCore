using Sinapse.Integracao.ReconhecimentoFacial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.ReconhecimentoFacial.Interface
{
    public interface IntegracaoReconhecimentoFacial
    {
        public FaceCompareResultResponse FaceCompare(FaceCompareRequest request);
    }
}