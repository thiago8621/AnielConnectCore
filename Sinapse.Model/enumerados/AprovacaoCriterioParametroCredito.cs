using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum AprovacaoCriterioParametroCredito
    {
        [Display(Name = "NENHUM", ResourceType = typeof(Resources.RsCadastros))]
        NENHUM = 0,
        [Display(Name = "APROVACAO_COM_INSTALACAO", ResourceType = typeof(Resources.RsCadastros))]
        APROVACAO_COM_INSTALACAO = 1,
        [Display(Name = "APROVACAO_SEM_INSTALACAO", ResourceType = typeof(Resources.RsCadastros))]
        APROVACAO_SEM_INSTALACAO = 2,
        [Display(Name = "AGUARDA_AVALIACAO_GESTOR", ResourceType = typeof(Resources.RsCadastros))]
        AGUARDA_AVALIACAO_GESTOR = 3,
        [Display(Name = "REPROVADO", ResourceType = typeof(Resources.RsCadastros))]
        REPROVADO = 4,
        [Display(Name = "APROVADO", ResourceType = typeof(Resources.RsCadastros))]
        APROVADO = 5
    }
}
