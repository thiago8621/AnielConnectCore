using ResourcesCore.AspNetCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Medida_Velocidade_ConexaoEnum
    {
        [Display(Name = "KBS", ResourceType = typeof(RsCadastros))]
        Kbs = 0,
        [Display(Name = "MBS", ResourceType = typeof(RsCadastros))]
        Mps = 1,
        [Display(Name = "GBS", ResourceType = typeof(RsCadastros))]
        Gbs = 2,
    }
}
