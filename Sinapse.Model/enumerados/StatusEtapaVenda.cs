using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum StatusEtapaVenda
    {
        [Display(Name = "PENDENTE", ResourceType = typeof(Resources.RsCadastros))]
        Pendente =0,
        [Display(Name = "APROVADO", ResourceType = typeof(Resources.RsCadastros))]
        Aprovado =1,
        [Display(Name = "REPROVADO", ResourceType = typeof(Resources.RsCadastros))]
        Reprovado =2,
        [Display(Name = "AGUAR_APRO_GESTOR", ResourceType = typeof(Resources.RsCadastros))]
        AguardandoAprovacaoGestor =3,
        [Display(Name = "APROVADO_MANUALMENTE", ResourceType = typeof(Resources.RsCadastros))]
        AprovadoManualmente =4
    }
}
