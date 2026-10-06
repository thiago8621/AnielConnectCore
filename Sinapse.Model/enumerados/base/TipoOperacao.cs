using System.ComponentModel.DataAnnotations;
using Resources;

namespace Sinapse.Model.enumerados
{
    public enum TipoOperacao
    {
        [Display(Name = "INSERCAO", ResourceType = typeof(RsBase))]
        Insercao = 1,
        [Display(Name = "ALTERACAO", ResourceType = typeof(RsBase))]
        Alteracao = 2,
        [Display(Name = "EXCLUSAO", ResourceType = typeof(RsBase))]
        Exclusao = 3,
        [Display(Name = "BAIXA", ResourceType = typeof(RsBase))]
        Baixa = 4,
        [Display(Name = "IMPORTACAO", ResourceType = typeof(RsBase))]
        Importacao = 5,
        [Display(Name = "INTEGRACAO", ResourceType = typeof(RsBase))]
        Integracao = 6,
    }
}