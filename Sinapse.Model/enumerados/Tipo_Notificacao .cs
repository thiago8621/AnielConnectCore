using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_Notificacao
    {
        [Display(Name = "INICIO", ResourceType = typeof(Resources.RsCadastros))]
        Inicio = 0,
        [Display(Name = "TERMINO", ResourceType = typeof(Resources.RsCadastros))]
        Termino = 1,
        [Display(Name = "INICIO_INTERVALO", ResourceType = typeof(Resources.RsCadastros))]
        Inicio_Intervalo = 2,
        [Display(Name = "TERMINO_INTERVALO", ResourceType = typeof(Resources.RsCadastros))]
        Termino_Intervalo = 3,
        [Display(Name = "DURACAO", ResourceType = typeof(Resources.RsCadastros))]
        Duracao = 4
    }
}
