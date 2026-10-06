using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Pesquisa_Tipo_Resposta
    {
        [Display(Name = "UNICA_ESCOLHA", ResourceType = typeof(Resources.RsCadastros))]
        Unica_Escolha = 0,
        [Display(Name = "MULTIPLA_ESCOLHA", ResourceType = typeof(Resources.RsCadastros))]
        Multipla_Escolha = 1,
        [Display(Name = "AVALIACAO_ESCALA_LINEAR", ResourceType = typeof(Resources.RsCadastros))]
        Avaliacao_Escala_Linear = 2,
        [Display(Name = "AVALIACAO_EMOTICONS", ResourceType = typeof(Resources.RsCadastros))]
        Avaliacao_Emoticons = 3,
        [Display(Name = "AVALIACAO_TEXTO", ResourceType = typeof(Resources.RsCadastros))]
        Texto = 4
    }
}
