using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Tipo_Feriado
    {
        [Display(Name = "FIXO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Fixo = 0,
        [Display(Name = "VARIAVEL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Variavel = 1

    }
}
