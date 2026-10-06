using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum SecaoConnectPerfilEnum
    {
        [Display(Name = "ATENDIMENTO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        ATENDIMENTO = 0,
        [Display(Name = "COMERCIAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        COMERCIAL = 1,
        [Display(Name = "SERVICOS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        SERVICOS = 2,
        [Display(Name = "")]
        NENHUM = 3,
    }
    
    public enum TipoUsuarioEnum
    {
        [Display(Name = "ATENDENTE", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        ATENDENTE = 0,
        [Display(Name = "SUPERVISOR_ATENDENTE", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        SUPERVISOR_DE_ATENDENTE = 1,
        [Display(Name = "SUPERVISOR_COMERCIAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        SUPERVISOR_COMERCIAL = 2,
        [Display(Name = "VENDEDOR", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        VENDEDOR = 3,
        [Display(Name = "MONITOR_EQUIPE", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        MONITOR_EQUIPE = 4,
        [Display(Name = "SUPERVISOR_EQUIPE", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        SUPERVISOR_EQUIPE = 5,
        [Display(Name = "")]
        NENHUM = 6,
    }
}
