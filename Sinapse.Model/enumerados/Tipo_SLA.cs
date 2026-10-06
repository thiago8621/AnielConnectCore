using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_SLA
    {
        [Display(Name = "DIAS_CORRIDOS", ResourceType = typeof(Resources.RsCadastros))]
        Dias_Corridos = 0,
        [Display(Name = "DIAS_UTEIS", ResourceType = typeof(Resources.RsCadastros))]
        Dias_Uteis = 1
    }
}
