using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Grau_Instrucao
    {
        [Display(Name = "ANALFABETO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        ANALFABETO = 0,
        [Display(Name = "ENSINO_FUNDAMENTAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        ENSINO_FUNDAMENTAL = 1,
        [Display(Name = "ENSINO_MEDIO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        ENSINO_MEDIO = 2,
        [Display(Name = "EDUCACAO_SUPERIOR", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        EDUCACAO_SUPERIOR = 3,
        [Display(Name = "MESTRADO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        MESTRADO = 4,
        [Display(Name = "DOUTORADO", ResourceType = typeof(ResourcesCore.AspNetCore.RsBase))]
        DOUTORADO = 5

    }
}
