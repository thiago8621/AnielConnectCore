using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Porte
    {
        [Display(Name = "PEQUENO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Pequeno = 1,
        [Display(Name = "MEDIO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Medio = 2,
        [Display(Name = "GRANDE", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Grande = 3
    }
}
