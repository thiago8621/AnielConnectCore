using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum StatusONU
    {
        [Display(Name = "ONLINE", ResourceType = typeof(RsCadastros))]
        Online,
        [Display(Name = "OFFLINE", ResourceType = typeof(RsCadastros))]
        Offline,
        [Display(Name = "NAO_DISPONIVEL", ResourceType = typeof(RsCadastros))]
        Indisponivel,
        [Display(Name = "ERRO_STATUS_ONU", ResourceType = typeof(RsCadastros))]
        Erro,
    }
}
