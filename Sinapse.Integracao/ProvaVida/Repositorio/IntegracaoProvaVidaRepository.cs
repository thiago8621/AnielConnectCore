using Sinapse.Integracao.ProvaVida.Interface;
using Sinapse.Integracao.ProvaVida.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Sinapse.Integracao.ProvaVida.Repositorio
{
    public class IntegracaoProvaVidaRepository
    {
        public async Task<DetectResultResponse> Detect(IntegracaoProvaVida integracao, DetectRequest value)
        {
            return integracao.Detect(value);
        }
    }
}