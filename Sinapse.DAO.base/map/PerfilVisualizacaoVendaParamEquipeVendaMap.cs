using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoVendaParamEquipeVendaMap : ClassMapBase<PerfilVisualizacaoVendaParamEquipeVenda>
    {
        public PerfilVisualizacaoVendaParamEquipeVendaMap() : base()
        {
            Table("TB_PERFIL_VIS_VENDA_EQUI_VEN");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_VENDA_EQUI_VEN");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_VENDA_EQUI_VEN");

            Map(x => x.CodEquipeVenda, "ID_EQUIPE_VENDA").Index("IDX_PERFIL_VIS_VENDA_EQUI");

        }
    }
}
