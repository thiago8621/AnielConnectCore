using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerado
{
    public enum Mes_Simplicado
    {
        [Display(Name = "JANEIRO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Janeiro = 1,
        [Display(Name = "FEVEREIRO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Fevereiro = 2,
        [Display(Name = "MARCO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Marco = 3,
        [Display(Name = "ABRIL_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Abril = 4,
        [Display(Name = "MAIO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Maio = 5,
        [Display(Name = "JUNHO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Junho = 6,
        [Display(Name = "JULHO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Julho = 7,
        [Display(Name = "AGOSTO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Agosto = 8,
        [Display(Name = "SETEMBRO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Setembro = 9,
        [Display(Name = "OUTUBRO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Outubro = 10,
        [Display(Name = "NOVEMBRO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Novembro = 11,
        [Display(Name = "DEZEMBRO_SIMPLICADO", ResourceType = typeof(Resources.RsBase))]
        Dezembro = 12

    }
}
