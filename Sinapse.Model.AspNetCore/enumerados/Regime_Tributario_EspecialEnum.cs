using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerado
{
    public enum Regime_Tributario_EspecialEnum
    {
        [Display(Name = "SEM_REGIME_TRIBUTARIO_ESPECIAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Sem_Regime_Tributario_Especial = 0,
        [Display(Name = "MICRO_EMPRESA_MUNICIPAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Micro_Empresa_Municipal = 1,
        [Display(Name = "ESTIMATIVA", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Estimativa = 2,
        [Display(Name = "SOCIEDADE_PROFISSIONAIS", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Sociedade_Profissionais = 3,
        [Display(Name = "COOPERATIVA", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Cooperativa = 4,
        [Display(Name = "MICROEMPRESARIO_INDIVIDUAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Micro_Empresario_Individual = 5,
        [Display(Name = "MICROEMPRESA_PEQUENO_PORTE", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        MicroEmpresa_Pequeno_Porte = 6
    }
}
