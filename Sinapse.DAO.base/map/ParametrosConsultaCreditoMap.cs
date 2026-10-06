using Sinapse.DAO.map;
using Sinapse.Model;

namespace Aniel.Compras.DAO.map
{
    public class ParametrosConsultaCreditoMap : ClassMapBase<ParametrosConsultaCredito>
    {
        public ParametrosConsultaCreditoMap() : base()
        {
            Table("TB_PARAMETROS_CONSULTA_CREDITO");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PARAMETROS_CONSULTA_CREDITO");

            Map(u => u.PontosPrimeiraFaixaCriterio, "PONTOS_FAIXA_RESTRICAO_1");
            Map(u => u.PontosSegundaFaixaCriterio, "PONTOS_FAIXA_RESTRICAO_2");
            Map(u => u.PontosTerceiraFaixaCriterio, "PONTOS_FAIXA_RESTRICAO_3");
            Map(u => u.TipoAprovacaoFaixa1, "TIPO_APROVACAO_FAIXA1");
            Map(u => u.TipoAprovacaoFaixa2, "TIPO_APROVACAO_FAIXA2");
            Map(u => u.TipoAprovacaoFaixa3, "TIPO_APROVACAO_FAIXA3");
            Map(u => u.UtilizaFaixaCriterio1, "UTILIZA_FAIXA_CRITERIO_1");
            Map(u => u.UtilizaFaixaCriterio2, "UTILIZA_FAIXA_CRITERIO_2");
            Map(u => u.UtilizaFaixaCriterio3, "UTILIZA_FAIXA_CRITERIO_3");
            HasMany(x => x.listaCriterios).KeyColumn("ID_CRITERIOS_RESTRICAO").Cascade.All();
        }
    }
}
