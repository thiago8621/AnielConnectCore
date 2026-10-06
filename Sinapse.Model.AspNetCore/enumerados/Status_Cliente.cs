using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Status_Cliente
    {
        [Display(Name = "INATIVO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Inativo = 0,
        [Display(Name = "ATIVO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Ativo = 1
    }
}
