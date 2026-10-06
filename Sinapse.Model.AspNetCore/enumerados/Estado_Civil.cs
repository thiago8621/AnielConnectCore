using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Estado_Civil
    {
        [Display(Name = "SOLTEIRO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Solteiro = 0,
        [Display(Name = "CASADO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Casado = 1,
        [Display(Name = "VIUVO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Viuvo = 2,
        [Display(Name = "DIVORCIADO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Divorciado = 3,
        [Display(Name = "UNIAOESTAVEL", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        UniaoEstavel = 4
    }
}
