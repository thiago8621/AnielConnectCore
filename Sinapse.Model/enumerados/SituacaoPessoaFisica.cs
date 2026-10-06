using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum SituacaoSerasaPessoaFisica
    {
        [Display(Name = "MENOR_IDADE_PROTEGIDO_LEGALMENTE", ResourceType = typeof(Resources.RsCadastros))]
        MenorIdadeProtegidoLegalmente = 1,
        Regular = 2,
        [Display(Name = "PENDENTE_REGULARIZACAO", ResourceType = typeof(Resources.RsCadastros))]
        Pendente_Regularizacao = 3,
        Nula = 4,
        Suspensa = 6,
        Falecido = 7,
        Cancelada = 9
    }
    public enum SituacaoSerasaPessoaJuridica
    {
        Regular = 2,
        Pendente_Regularizacao = 3,
        Nula = 4,
        Suspensa = 6,
        Baixada = 7,
        Cancelada = 9
    }
}
