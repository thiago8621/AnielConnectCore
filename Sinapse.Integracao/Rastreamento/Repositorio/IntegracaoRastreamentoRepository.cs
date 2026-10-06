using Sinapse.Integracao.Rastreamento.Cobli.Models;
using Sinapse.Integracao.SMS.Interface;
using Sinapse.Integracao.SMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Sinapse.Integracao.Rastreamento.Repositorio
{
    public class IntegracaoRastreamentoRepository
    {
        public RastreamentoGetPathResponseModel GetPath(IntegracaoRastreamento integracao, DateTime inicio, DateTime fim)
        {
            return integracao.GetPath(inicio,fim);
        }

        public RastreamentoLastLocationResponseModel LastLocation(IntegracaoRastreamento integracao)
        {
            return integracao.LastLocation();
        }

        public TrajetoRealizadoResponseModel GetTrajeto(IntegracaoRastreamento integracao, DateTime inicio, DateTime fim,string tecnico)
        {
            return integracao.GetTrajeto(inicio,fim, tecnico);
        }

    }
}