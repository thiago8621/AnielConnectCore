using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum IntegracaoExternaConsultaCredEnum
    {
        [Display(Name = "SERASA", ResourceType = typeof(Resources.RsCadastros))]
        SERASA = 0,
        [Display(Name = "PREQUAL", ResourceType = typeof(Resources.RsCadastros))]
        PREQUAL = 1,
        [Display(Name = "SERASA_EXPERIAN", ResourceType = typeof(Resources.RsCadastros))]
        SERASA_EXPERIAN = 2
    }
}
