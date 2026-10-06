using Sinapse.Integracao.ReconhecimentoFacial.Interface;
using Sinapse.Integracao.ReconhecimentoFacial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Sinapse.Integracao.ReconhecimentoFacial.Repositorio
{
    public class IntegracaoReconhecimentoFacialRepository
    {
        public async Task<FaceCompareResultResponse> FaceCompare(IntegracaoReconhecimentoFacial integracao, FaceCompareRequest value)
        {
            return integracao.FaceCompare(value);
        }
    }
}