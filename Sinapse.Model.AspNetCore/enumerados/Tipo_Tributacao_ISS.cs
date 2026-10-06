using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerado
{
    public enum Tipo_Tributacao_ISS
    {
        [Display(Name = "ISENTO_ISS", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Isento_ISS = 1,
        [Display(Name = "IMUNE", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Imune = 2,
        [Display(Name = "NAO_INCIDENCIA_MUNICIPIO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Nao_Incidencia_Municipio = 3,
        [Display(Name = "NAO_TRIBUTAVEL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Nao_Tributavel = 4,
        [Display(Name = "RETIDO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Retido = 5,
        [Display(Name = "TRIBUTAVEL_DENTRO_MUNICIPIO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Tributavel_Dentro_Do_Municipio = 6,
        [Display(Name = "TRIBUTAVEL_FORA_MUNICIPIO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Tributavel_Fora_Do_Municipio = 7
    }
}
