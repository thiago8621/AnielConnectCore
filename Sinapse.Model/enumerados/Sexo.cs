using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Sexo
    {
        [Display(Name = "MASCULINO", ResourceType = typeof(Resources.RsBase))]
        Masculino = 0,
        [Display(Name = "FEMININO", ResourceType = typeof(Resources.RsBase))]
        Feminino = 1
    }
}
