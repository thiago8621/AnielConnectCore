using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerado
{
    public enum Exigibilidade_ISS
    {
        [Display(Name = "EXIGIVEL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Exigivel = 1,
        [Display(Name = "NAO_INCIDENCIA", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Nao_Incidencia = 2,
        [Display(Name = "ISENCAO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Isencao = 3,
        [Display(Name = "EXPORTACAO", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Exportacao = 4,
        [Display(Name = "IMUNIDADE", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Imunidade = 5,
        [Display(Name = "SUSPENSO_ACAO_JUDICIAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Suspenso_Acao_Judicial = 6,
        [Display(Name = "SUSPENSO_ACAO_ADM", ResourceType = typeof(ResourcesCore.AspNetCore.RsFinanceiro))]
        Suspenso_Acao_Administrativa = 7
    }
}
