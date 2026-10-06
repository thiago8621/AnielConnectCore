using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    /// <summary>
    ///     Etapas padrões da venda. 
    ///     Futuramente poderão ser cadastradas novas etapas pelo Connect, 
    ///     que não estarão declaradas aqui nesse enumerado
    /// </summary>
    public enum EnumEtapaVenda
    {
        [Display(Name = "DADOS_GERAIS", ResourceType = typeof(Resources.RsCadastros))]
        DadosGerais = 0,

        [Display(Name = "CONSULTA_VIABILIDADE", ResourceType = typeof(Resources.RsCadastros))]
        ConsultaViabilidade = 1,

        [Display(Name = "PENDENCIA_PEND_FINANCEIRA", ResourceType = typeof(Resources.RsCadastros))]
        ConsultaPendenciaFinanceira = 2,

        [Display(Name = "PENDENCIA_PEND_ENDERECO", ResourceType = typeof(Resources.RsCadastros))]
        ConsultaPendenciaEndereco = 3,

        [Display(Name = "CONSULTA_CREDITO", ResourceType = typeof(Resources.RsCadastros))]
        ConsultaCredito = 4,

        [Display(Name = "PLANO_PRODUTO", ResourceType = typeof(Resources.RsCadastros))]
        PlanoProduto = 5,
      
        [Display(Name = "FINANCEIRO", ResourceType = typeof(Resources.RsCadastros))]
        Financeiro = 6,

        [Display(Name = "CHECKLIST", ResourceType = typeof(Resources.RsCadastros))]
        Checklist = 7,

        [Display(Name = "RESUMO", ResourceType = typeof(Resources.RsCadastros))]
        Resumo = 8,

        [Display(Name = "TERMO_ADESAO", ResourceType = typeof(Resources.RsCadastros))]
        TermoAdesao = 9,

        [Display(Name = "DADOS_CARTAO", ResourceType = typeof(Resources.RsCadastros))]
        CartaoCredito = 10,

        [Display(Name = "ETAPA_ORDEM_SERVICO", ResourceType = typeof(Resources.RsCadastros))]
        OrdemServico = 11,

        [Display(Name = "CONSULTA_PEND_FINAN_NOVO", ResourceType = typeof(Resources.RsCadastros))]
        ConsultaPendenciaFinanceiraNovo = 12,
    }
}
