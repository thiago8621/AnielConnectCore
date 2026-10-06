using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Tipo_Notificacao
    {
        [Display(Name = "INICIO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Inicio = 0,
        [Display(Name = "TERMINO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Termino = 1,
        [Display(Name = "INICIO_INTERVALO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Inicio_Intervalo = 2,
        [Display(Name = "TERMINO_INTERVALO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Termino_Intervalo = 3,
        [Display(Name = "DURACAO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Duracao = 4
    }
}
