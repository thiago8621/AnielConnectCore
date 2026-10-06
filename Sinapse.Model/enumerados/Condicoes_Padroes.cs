using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Condicoes_Padroes
    {
        [Display(Name = "ABAIXO_DE", ResourceType = typeof(Resources.RsCadastros))]
        Abaixo_De = 0,
        [Display(Name = "ENTRE", ResourceType = typeof(Resources.RsCadastros))]
        Entre = 1, 
        [Display(Name = "ACIMA_DE", ResourceType = typeof(Resources.RsCadastros))]
        Acima_De = 2
    }
}
