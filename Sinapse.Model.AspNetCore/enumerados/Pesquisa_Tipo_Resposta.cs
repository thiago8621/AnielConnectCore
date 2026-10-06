using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Pesquisa_Tipo_Resposta
    {
        [Display(Name = "UNICA_ESCOLHA", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Unica_Escolha = 0,
        [Display(Name = "MULTIPLA_ESCOLHA", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Multipla_Escolha = 1,
        [Display(Name = "AVALIACAO_ESCALA_LINEAR", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Avaliacao_Escala_Linear = 2,
        [Display(Name = "AVALIACAO_EMOTICONS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Avaliacao_Emoticons = 3,

    }
}
