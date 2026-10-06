using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoVendaParamSetorDeptoMap : ClassMapBase<PerfilVisualizacaoVendaParamSetorDepto>
    {
        public PerfilVisualizacaoVendaParamSetorDeptoMap() : base()
        {
            Table("TB_PERFIL_VIS_VENDA_SETOR");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_VENDA_SETOR");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_VENDA_SETOR");

            Map(x => x.CodDepto, "ID_DEPTO").Index("IDX_PERFIL_VIS_VENDA_DEPTO");
        }
    }
}
