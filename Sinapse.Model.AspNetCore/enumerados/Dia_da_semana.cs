using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Enum_Dia_SemanaBaseModel
    {
        [Display(Name = "DOMINGO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Domingo = 0,
        [Display(Name = "SEGUNDA", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Segunda = 1,
        [Display(Name = "TERCA", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Terca = 2,
        [Display(Name = "QUARTA", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Quarta = 3,
        [Display(Name = "QUINTA", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Quinta = 4,
        [Display(Name = "SEXTA", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Sexta = 5,
        [Display(Name = "SABADO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        Sabado = 6
    }
}
