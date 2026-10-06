using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Porte
    {
        [Display(Name = "PEQUENO", ResourceType = typeof(Resources.RsBase))]
        Pequeno = 1,
        [Display(Name = "MEDIO", ResourceType = typeof(Resources.RsBase))]
        Medio = 2,
        [Display(Name = "GRANDE", ResourceType = typeof(Resources.RsBase))]
        Grande = 3
    }
}
