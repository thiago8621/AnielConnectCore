using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Grau_Instrucao
    {
        [Display(Name = "ANALFABETO", ResourceType = typeof(Resources.RsBase))]
        ANALFABETO = 0,
        [Display(Name = "ENSINO_FUNDAMENTAL", ResourceType = typeof(Resources.RsBase))]
        ENSINO_FUNDAMENTAL = 1,
        [Display(Name = "ENSINO_MEDIO", ResourceType = typeof(Resources.RsBase))]
        ENSINO_MEDIO = 2,
        [Display(Name = "EDUCACAO_SUPERIOR", ResourceType = typeof(Resources.RsBase))]
        EDUCACAO_SUPERIOR = 3,
        [Display(Name = "MESTRADO", ResourceType = typeof(Resources.RsBase))]
        MESTRADO = 4,
        [Display(Name = "DOUTORADO", ResourceType = typeof(Resources.RsBase))]
        DOUTORADO = 5

    }
}
