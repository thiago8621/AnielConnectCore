using Sinapse.Integracao.SMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.SMS.Interface
{
    public interface IntegracaoSMS
    {
        public EnviarSMS_ResponseModel EnviarSMS(Sinapse.Model.SMS request);
        public EnviarLoteSMS_ResponseModel EnviarLoteSMS(List<Sinapse.Model.SMS> request);
    }
}