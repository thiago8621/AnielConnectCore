using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Versao_OLT
    {
        [Display(Name = "RP700", ResourceType = typeof(Resources.RsCadastros))]
        RP700 = 0,
        [Display(Name = "RP1200", ResourceType = typeof(Resources.RsCadastros))]
        RP1200 = 1,
        [Display(Name = "RP1800", ResourceType = typeof(Resources.RsCadastros))]
        RP1800 = 2
    }
}

