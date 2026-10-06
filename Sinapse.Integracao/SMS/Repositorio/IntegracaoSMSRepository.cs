using Sinapse.Integracao.SMS.Interface;
using Sinapse.Integracao.SMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Sinapse.Integracao.SMS.Repositorio
{
    public class IntegracaoSMSRepository
    {
        public async Task<EnviarSMS_ResponseModel> EnviarSMS(IntegracaoSMS integracao, Sinapse.Model.SMS value)
        {
            return integracao.EnviarSMS(value);
        }

        public async Task<EnviarLoteSMS_ResponseModel> EnviarLoteSMS(IntegracaoSMS integracao, List<Sinapse.Model.SMS> value)
        {
            return integracao.EnviarLoteSMS(value);
        }
    }
}