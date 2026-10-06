using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum DashboardInicialConnect
    {

        [Display(Name = "ATENDIMENTO_CLIENTE", ResourceType = typeof(Resources.RsCadastros))]
        AtendimentoCliente = 1,
        [Display(Name = "COMERCIAL", ResourceType = typeof(Resources.RsCadastros))]
        Comercial = 2,
        [Display(Name = "SERVICOS_MODELO1", ResourceType = typeof(Resources.RsCadastros))]
        Servicos = 3,
        [Display(Name = "SERVICOS_MODELO2", ResourceType = typeof(Resources.RsCadastros))]
        ServicosModelo2 = 4
    }
}
