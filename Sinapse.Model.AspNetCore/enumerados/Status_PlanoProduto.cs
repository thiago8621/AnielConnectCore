using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Status_PlanoProduto
    {


        [Display(Name = "AGUARDANDO_CRIACAO_OS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Aguardando_Criacao_OS = 0,
        [Display(Name = "AGUARDANDO_INSTALACAO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Aguardando_Instalacao = 1,
        [Display(Name = "HABILITADO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Habilitado = 2,
        [Display(Name = "SUSPENSO_DEBITO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Suspenso_Debito = 3,
        [Display(Name = "SUSPENSO_EMPRESA", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Suspenso_Empresa = 4,
        [Display(Name = "CANCELADO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Cancelado = 5,
        [Display(Name = "AGUARDANDO_APROVACAO_VENDA", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Aguardando_Aprovacao_Venda = 6,
        [Display(Name = "VENDA_CANCELADA", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Venda_Cancelada = 7,

    }
}
