using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerado
{
    public enum Regime_TributarioEnum
    {
        [Display(Name = "NENHUM", ResourceType = typeof(Resources.RsFinanceiro))]
        Nenhum = 0,
        [Display(Name = "SIMPLES_NACIONAL", ResourceType = typeof(Resources.RsFinanceiro))]
        Simples_Nacional = 1,
        [Display(Name = "SIMPLES_NACIONAL_EXCESSO", ResourceType = typeof(Resources.RsFinanceiro))]
        Simples_Nacional_Excesso = 2,
        [Display(Name = "NORMAL_LUCRO_PRESUMIDO", ResourceType = typeof(Resources.RsFinanceiro))]
        Normal_Lucro_Presumido = 3,
        [Display(Name = "NORMAL_LUCRO_REAL", ResourceType = typeof(Resources.RsFinanceiro))]
        Normal_Lucro_Real = 4
    }
}
