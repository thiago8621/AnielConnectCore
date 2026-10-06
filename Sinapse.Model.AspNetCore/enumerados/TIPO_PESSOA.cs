using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Tipo_Pessoa
    {
        [Display(Name = "FISICA", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Fisica = 0,
        [Display(Name = "JURIDICA", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Juridica = 1 ,
        //[Display(Name = "FISICA_RURAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Fisica_Rural = 2,
        //[Display(Name = "JURIDICA_RURAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Juridica_Rural = 3

    }
}
