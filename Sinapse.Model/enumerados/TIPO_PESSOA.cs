using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_Pessoa
    {
        [Display(Name = "FISICA", ResourceType = typeof(Resources.RsBase))]
        Fisica = 0,
        [Display(Name = "JURIDICA", ResourceType = typeof(Resources.RsBase))]
        Juridica = 1
    }
}
