using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum TipoIntegracaoAssinaturaEnum
    {
        [Display(Name = "LEXIO", ResourceType = typeof(Resources.RsCadastros))]
        Lexio = 0
    }
}
