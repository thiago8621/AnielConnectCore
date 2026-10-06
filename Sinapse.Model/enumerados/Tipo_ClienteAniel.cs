using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_ClienteAniel
    {
        [Display(Name = "CLIENTE", ResourceType = typeof(Resources.RsCadastros))]
        Cliente = 0,
        [Display(Name = "PROSPECTO", ResourceType = typeof(Resources.RsCadastros))]
        Prospecto = 1
    }
}
