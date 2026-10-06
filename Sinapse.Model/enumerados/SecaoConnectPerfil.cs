using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum SecaoConnectPerfilEnum
    {
        [Display(Name = "ATENDIMENTO", ResourceType = typeof(Resources.RsCadastros))]
        ATENDIMENTO = 0,
        [Display(Name = "COMERCIAL", ResourceType = typeof(Resources.RsCadastros))]
        COMERCIAL = 1,
        [Display(Name = "SERVICOS", ResourceType = typeof(Resources.RsCadastros))]
        SERVICOS = 2,
        [Display(Name = "")]
        NENHUM = 3,
    }
    
    public enum TipoUsuarioEnum
    {
        [Display(Name = "ATENDENTE", ResourceType = typeof(Resources.RsCadastros))]
        ATENDENTE = 0,
        [Display(Name = "SUPERVISOR_ATENDENTE", ResourceType = typeof(Resources.RsCadastros))]
        SUPERVISOR_DE_ATENDENTE = 1,
        [Display(Name = "SUPERVISOR_COMERCIAL", ResourceType = typeof(Resources.RsCadastros))]
        SUPERVISOR_COMERCIAL = 2,
        [Display(Name = "VENDEDOR", ResourceType = typeof(Resources.RsCadastros))]
        VENDEDOR = 3,
        [Display(Name = "MONITOR_EQUIPE", ResourceType = typeof(Resources.RsCadastros))]
        MONITOR_EQUIPE = 4,
        [Display(Name = "SUPERVISOR_EQUIPE", ResourceType = typeof(Resources.RsCadastros))]
        SUPERVISOR_EQUIPE = 5,
        [Display(Name = "")]
        NENHUM = 6,
    }
}
