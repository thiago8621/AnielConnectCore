using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Status_Default
    {
        [Display(Name = "INATIVO", ResourceType = typeof(Resources.RsCadastros))]
        Inativo = 0,
        [Display(Name = "ATIVO", ResourceType = typeof(Resources.RsCadastros))]
        Ativo = 1
    }
}
