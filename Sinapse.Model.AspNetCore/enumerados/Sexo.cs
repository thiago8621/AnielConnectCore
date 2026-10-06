using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Sexo
    {
        [Display(Name = "MASCULINO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Masculino = 0,
        [Display(Name = "FEMININO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Feminino = 1
    }
}
