using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerado
{
    public enum Natureza_TributacaoEnum
    {
        [Display(Name = "SIMPLES_NACIONAL", ResourceType = typeof(Resources.RsFinanceiro))]
        Simples_Nacional = 1,
        [Display(Name = "FIXO", ResourceType = typeof(Resources.RsFinanceiro))]
        Fixo = 2,
        [Display(Name = "DEPOSITO_JUIZO", ResourceType = typeof(Resources.RsFinanceiro))]
        Deposito_Juizo = 3,
        [Display(Name = "EXIGIBILIDADE_SUSPENSA_DECISAO_JUDICIAL", ResourceType = typeof(Resources.RsFinanceiro))]
        Exigibilidade_Suspensa_Decisao_Judicial = 4,
        [Display(Name = "EXIGIBILIDADE_SUSPENSA_PROCEDIMENTO_ADM", ResourceType = typeof(Resources.RsFinanceiro))]
        Exigibilidade_Suspensa_Procedimento_Adm = 5,
        [Display(Name = "ISENCAO_PARCIAL", ResourceType = typeof(Resources.RsFinanceiro))]
        Isencao_Parcial = 6
    }
}
