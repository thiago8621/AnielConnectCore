using Sinapse.WidePayApi.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Entity
{
    public class HistoricoCobranca
    {
        public StatusCobranca status { get; set; }
        public DateTime data { get; set; }

        public HistoricoCobranca(string status)
        {
            switch(status.ToUpper())
            {
                case "AGUARDANDO":
                    this.status = StatusCobranca.Aguardando;
                    break;
                case "CANCELADO":
                    this.status = StatusCobranca.Cancelado;
                    break;

                case "EM ANÁLISE":
                    this.status = StatusCobranca.Analise;
                    break;
                case "EM ANALISE":
                    this.status = StatusCobranca.Analise;
                    break;
                case "ESTORNADO":
                    this.status = StatusCobranca.Estornado;
                    break;
                case "RECEBIDO":
                    this.status = StatusCobranca.Recebido;
                    break;
                case "RECEBIDO MANUALMENTE":
                    this.status = StatusCobranca.RecebidoManual;
                    break;
                case "RECUSADO":
                    this.status = StatusCobranca.Recusado;
                    break;
                case "VENCIDO":
                    this.status = StatusCobranca.Vencido;
                    break;
            }
        }
    }
}
