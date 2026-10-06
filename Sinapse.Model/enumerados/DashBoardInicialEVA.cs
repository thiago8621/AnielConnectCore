using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum DashBoardInicialEVA
    {

        [Display(Name = "ATENDIMENTO_CLIENTE", ResourceType = typeof(Resources.RsCadastros))]
        AtendimentoCliente = 1,
        [Display(Name = "COMERCIAL", ResourceType = typeof(Resources.RsCadastros))]
        Comercial = 2,
        [Display(Name = "SERVICOS", ResourceType = typeof(Resources.RsCadastros))]
        Servicos = 3
    }
}
