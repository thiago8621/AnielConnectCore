using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Enums
{
    public enum StatusCobranca
    {
        [Display(Name = "AGUARDANDO", ResourceType = typeof(RsFinanceiro))]
        Aguardando,
        [Display(Name = "CANCELADO", ResourceType = typeof(RsFinanceiro))]
        Cancelado,
        [Display(Name = "CONTESTADO", ResourceType = typeof(RsFinanceiro))]
        Contestado,
        [Display(Name = "ANALISE", ResourceType = typeof(RsFinanceiro))]
        Analise,
        [Display(Name = "ESTORNAR", ResourceType = typeof(RsFinanceiro))]
        Estornado,
        [Display(Name = "RECEBIDO", ResourceType = typeof(RsFinanceiro))]
        Recebido,
        [Display(Name = "RECEBIDO_MANUAL", ResourceType = typeof(RsFinanceiro))]
        RecebidoManual,
        [Display(Name = "RECUSADO", ResourceType = typeof(RsFinanceiro))]
        Recusado,
        [Display(Name = "VENCIDO", ResourceType = typeof(RsFinanceiro))]
        Vencido
    }
}
