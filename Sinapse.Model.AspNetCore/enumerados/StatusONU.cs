using ResourcesCore.AspNetCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum StatusONU
    {
        [Display(Name = "ONLINE", ResourceType = typeof(RsCadastros))]
        Online,
        [Display(Name = "OFFLINE", ResourceType = typeof(RsCadastros))]
        Offline,
        [Display(Name = "NAO_DISPONIVEL", ResourceType = typeof(RsCadastros))]
        Indisponivel,
        //[Display(Name = "ERRO_STATUS_ONU", ResourceType = typeof(RsCadastros))]
        Erro,
    }
}
