using Sinapse.Integracao.Rastreamento.Cobli.Models;
using Sinapse.Integracao.SMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.SMS.Interface
{
    public interface IntegracaoRastreamento
    {
        public string Nome { get; set; }
        public RastreamentoGetPathResponseModel GetPath(DateTime inicio,DateTime fim);
        public RastreamentoLastLocationResponseModel LastLocation();
        public TrajetoRealizadoResponseModel GetTrajeto(DateTime inicio, DateTime fim,string tecnico);
        public OdometroResponseModel GetOdometro(string placa);
    }
}