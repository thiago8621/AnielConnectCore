using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_Feriado
    {
        [Display(Name = "FIXO", ResourceType = typeof(Resources.RsCadastros))]
        Fixo = 0,
        [Display(Name = "VARIAVEL", ResourceType = typeof(Resources.RsCadastros))]
        Variavel = 1

    }
}
