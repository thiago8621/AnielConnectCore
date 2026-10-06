using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerado
{
    public enum Mes
    {
        [Display(Name = "JANEIRO", ResourceType = typeof(Resources.RsBase))]
        Janeiro = 1,
        [Display(Name = "FEVEREIRO", ResourceType = typeof(Resources.RsBase))]
        Fevereiro = 2,
        [Display(Name = "MARCO", ResourceType = typeof(Resources.RsBase))]
        Marco = 3,
        [Display(Name = "ABRIL", ResourceType = typeof(Resources.RsBase))]
        Abril = 4,
        [Display(Name = "MAIO", ResourceType = typeof(Resources.RsBase))]
        Maio = 5,
        [Display(Name = "JUNHO", ResourceType = typeof(Resources.RsBase))]
        Junho = 6,
        [Display(Name = "JULHO", ResourceType = typeof(Resources.RsBase))]
        Julho = 7,
        [Display(Name = "AGOSTO", ResourceType = typeof(Resources.RsBase))]
        Agosto = 8,
        [Display(Name = "SETEMBRO", ResourceType = typeof(Resources.RsBase))]
        Setembro = 9,
        [Display(Name = "OUTUBRO", ResourceType = typeof(Resources.RsBase))]
        Outubro = 10,
        [Display(Name = "NOVEMBRO", ResourceType = typeof(Resources.RsBase))]
        Novembro = 11,
        [Display(Name = "DEZEMBRO", ResourceType = typeof(Resources.RsBase))]
        Dezembro = 12

    }
}
