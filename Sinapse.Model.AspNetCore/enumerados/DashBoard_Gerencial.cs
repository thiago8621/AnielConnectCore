using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum DashBoard_Gerencial
    {
        [Display(Name = "GESTAO_ATENDIMENTO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Gestao_Atendimento = 1,
        [Display(Name = "GESTAO_CLIENTES", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Gestao_Clientes = 2,
        [Display(Name = "GESTAO_SERVICOS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Gestao_Serviços = 3,
        [Display(Name = "GESTAO_MATERIAIS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Gestao_Materiais = 4,
        [Display(Name = "GESTAO_COMERCIAL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Gestao_Comercial = 5,
    }
}
