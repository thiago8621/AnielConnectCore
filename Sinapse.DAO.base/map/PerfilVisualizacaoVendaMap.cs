using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoVendaMap : ClassMapBase<PerfilVisualizacaoVenda>
    {
        public PerfilVisualizacaoVendaMap() : base()
        {
            Table("TB_PERFIL_VIS_VENDA");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_VENDA");

            Map(x => x.Descricao, "DESCRICAO");

            Map(x => x.PadraoVisualizaLead, "PADRAO_VISUALIZA_LEAD");

            Map(x => x.PadraoVisualizaVenda, "PADRAO_VISUALIZA_VENDA");

            Map(x => x.PerfilSolicitaAutorizacao, "PERFIL_SOLICITA_AUTORIZACAO");

            Map(x => x.Qtd_Dias_Bloqueio_Lead, "QTD_DIAS_BLOQUEIO_LEAD");

            Map(x => x.Status, "STATUS").CustomType<Status_Default>();

            Map(x => x.PadraoVisualizaTodasTemperaturas, "PADRAO_VISUAL_TEMPERATURAS");

            Map(x => x.TemperaturaQuente, "TEMPERATURA_QUENTE");

            Map(x => x.TemperaturaMorno, "TEMPERATURA_MORNO");

            Map(x => x.TemperaturaFrio, "TEMPERATURA_FRIO");

            HasMany(x => x.ParamsTipoPessoa)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();

            HasMany(x => x.ParamsMeioCaptacao)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();

            HasMany(x => x.ParamsCategoriaCliente)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();

            HasMany(x => x.ParamsSetorDepto)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();

            HasMany(x => x.ParamsRegiao)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();

            HasMany(x => x.ParamsEquipeVenda)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();
        }
    }
}
