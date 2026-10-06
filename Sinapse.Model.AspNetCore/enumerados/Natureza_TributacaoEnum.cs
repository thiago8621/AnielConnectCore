using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerado
{
    public enum Natureza_TributacaoEnum
    {
        [Display(Name = "SIMPLES_NACIONAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Simples_Nacional = 1,
        [Display(Name = "FIXO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Fixo = 2,
        [Display(Name = "DEPOSITO_JUIZO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Deposito_Juizo = 3,
        [Display(Name = "EXIGIBILIDADE_SUSPENSA_DECISAO_JUDICIAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Exigibilidade_Suspensa_Decisao_Judicial = 4,
        [Display(Name = "EXIGIBILIDADE_SUSPENSA_PROCEDIMENTO_ADM", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Exigibilidade_Suspensa_Procedimento_Adm = 5,
        [Display(Name = "ISENCAO_PARCIAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Isencao_Parcial = 6
    }
}
