using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum DashboardInicialConnect
    {

        [Display(Name = "ATENDIMENTO_CLIENTE", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        AtendimentoCliente = 1,
        [Display(Name = "COMERCIAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Comercial = 2,
        [Display(Name = "SERVICOS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Servicos = 3
    }
}
