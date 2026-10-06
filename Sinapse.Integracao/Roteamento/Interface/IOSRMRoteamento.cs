using Sinapse.Integracao.Roteamento.Models;
using System;
using System.Collections.Generic;

namespace Sinapse.Integracao.Roteamento.Interface
{
    public interface IOSRMRoteamento
    {
        public OSRMTableResponseModel GetResponseRotasOSRM(double latitude, double longitude, List<(double caixaLatitude, double caixaLongitude)> caixas);
    }
}
