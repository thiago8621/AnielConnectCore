using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerado
{
    public enum Regime_TributarioEnum
    {
        [Display(Name = "NENHUM", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Nenhum = 0,
        [Display(Name = "SIMPLES_NACIONAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Simples_Nacional = 1,
        [Display(Name = "SIMPLES_NACIONAL_EXCESSO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Simples_Nacional_Excesso = 2,
        [Display(Name = "NORMAL_LUCRO_PRESUMIDO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Normal_Lucro_Presumido = 3,
        [Display(Name = "NORMAL_LUCRO_REAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Normal_Lucro_Real = 4
    }
}
