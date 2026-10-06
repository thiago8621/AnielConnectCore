using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Enum_Dia_SemanaBaseModel
    {
        [Display(Name = "DOMINGO", ResourceType = typeof(Resources.RsBase))]
        Domingo = 0,
        [Display(Name = "SEGUNDA", ResourceType = typeof(Resources.RsBase))]
        Segunda = 1,
        [Display(Name = "TERCA", ResourceType = typeof(Resources.RsBase))]
        Terca = 2,
        [Display(Name = "QUARTA", ResourceType = typeof(Resources.RsBase))]
        Quarta = 3,
        [Display(Name = "QUINTA", ResourceType = typeof(Resources.RsBase))]
        Quinta = 4,
        [Display(Name = "SEXTA", ResourceType = typeof(Resources.RsBase))]
        Sexta = 5,
        [Display(Name = "SABADO", ResourceType = typeof(Resources.RsBase))]
        Sabado = 6
    }
}
