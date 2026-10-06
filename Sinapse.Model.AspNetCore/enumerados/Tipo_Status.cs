using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Tipo_Status
    {
        [Display(Name = "INATIVO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Inativo = 0,
        [Display(Name = "ATIVO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Ativo = 1
    }
}
