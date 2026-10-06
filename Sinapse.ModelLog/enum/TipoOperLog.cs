using Resources;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace Sinapse.ModelLog
{
    public enum TipoOperLog
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
        [Display(Name = "CANCELADO", ResourceType = typeof(RsBase))]
        Cancelado = 7,
        [Display(Name = "TODOS", ResourceType = typeof(RsBase))]
        Todos = 8,
        [Display(Name = "AUTORIZACAO", ResourceType = typeof(RsBase))]
        Autorizacao = 9,
        [Display(Name = "ESTORNO", ResourceType = typeof(RsBase))]
        Estorno = 10
    }
}
