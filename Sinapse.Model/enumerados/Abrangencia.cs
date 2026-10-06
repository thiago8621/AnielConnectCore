using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sinapse.Model.enumerados
{

    public enum Abrangencia
    {
        [Display(Name = "INTERNACIONAL", ResourceType = typeof(Resources.RsCadastros))]
        Internacional = 0,
        [Display(Name = "NACIONAL", ResourceType = typeof(Resources.RsCadastros))]
        Nacional = 1,
        [Display(Name = "MUNICIPAL", ResourceType = typeof(Resources.RsCadastros))]
        Municipal = 2,
        [Display(Name = "ESTADUAL", ResourceType = typeof(Resources.RsCadastros))]
        Estadual = 3        
    }
}