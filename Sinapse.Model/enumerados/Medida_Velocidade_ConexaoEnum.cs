using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Medida_Velocidade_ConexaoEnum
    {
        [Display(Name = "KBS", ResourceType = typeof(Resources.RsCadastros))]
        Kbs = 0,
        [Display(Name = "MBS", ResourceType = typeof(Resources.RsCadastros))]
        Mps = 1,
        [Display(Name = "GBS", ResourceType = typeof(Resources.RsCadastros))]
        Gbs = 2,
    }
}
