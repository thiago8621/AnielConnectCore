using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum TipoOperacao
    {
        [Display(Name = "INSERCAO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Insercao = 1,
        [Display(Name = "ALTERACAO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Alteracao = 2,
        [Display(Name = "EXCLUSAO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Exclusao = 3,
        [Display(Name = "BAIXA", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Baixa = 4,
        [Display(Name = "IMPORTACAO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Importacao = 5
    }
}