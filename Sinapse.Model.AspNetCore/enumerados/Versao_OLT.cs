using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.Text;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Versao_OLT
    {
        [Display(Name = "RP700", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        RP700 = 0,
        [Display(Name = "RP1200", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        RP1200 = 1,
        [Display(Name = "RP1800", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        RP1800 = 2
    }
}
