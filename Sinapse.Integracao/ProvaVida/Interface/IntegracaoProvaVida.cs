using Sinapse.Integracao.ProvaVida.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.ProvaVida.Interface
{
    public interface IntegracaoProvaVida
    {
        public DetectResultResponse Detect(DetectRequest request);
    }
}