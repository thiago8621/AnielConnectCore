using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Tipo_Atividade_Nf
    {
        [Display(Name = "PLANO_PRODUTO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Plano_Produto = 0,
        [Display(Name = "INSTALACAO", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Instalacao = 1
    }
}
