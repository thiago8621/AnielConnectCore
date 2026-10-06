using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Tipo_PlanoProduto
    {
        [Display(Name = "PLANO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Plano = 0,
        [Display(Name = "PRODUTO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Produto = 1
    }
}
