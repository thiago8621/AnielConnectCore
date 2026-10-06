using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_Status
    {
        [Display(Name = "INATIVO", ResourceType = typeof(Resources.RsBase))]
        Inativo = 0,
        [Display(Name = "ATIVO", ResourceType = typeof(Resources.RsBase))]
        Ativo = 1
    }
}
