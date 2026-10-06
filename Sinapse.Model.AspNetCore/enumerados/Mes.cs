using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerado
{
    public enum Mes
    {
        [Display(Name = "JANEIRO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Janeiro = 1,
        [Display(Name = "FEVEREIRO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Fevereiro = 2,
        [Display(Name = "MARCO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Marco = 3,
        [Display(Name = "ABRIL", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Abril = 4,
        [Display(Name = "MAIO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Maio = 5,
        [Display(Name = "JUNHO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Junho = 6,
        [Display(Name = "JULHO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Julho = 7,
        [Display(Name = "AGOSTO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Agosto = 8,
        [Display(Name = "SETEMBRO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Setembro = 9,
        [Display(Name = "OUTUBRO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Outubro = 10,
        [Display(Name = "NOVEMBRO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Novembro = 11,
        [Display(Name = "DEZEMBRO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Dezembro = 12

    }
}
