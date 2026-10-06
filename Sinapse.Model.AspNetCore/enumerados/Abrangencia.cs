using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{

    public enum Abrangencia
    {
        [Display(Name = "INTERNACIONAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Internacional = 0,
        [Display(Name = "NACIONAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Nacional = 1,
        [Display(Name = "MUNICIPAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Municipal = 2,
        [Display(Name = "ESTADUAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Estadual = 3        
    }
}