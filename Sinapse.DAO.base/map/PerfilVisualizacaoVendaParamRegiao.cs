using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoVendaParamRegiaoMap : ClassMapBase<PerfilVisualizacaoVendaParamRegiao>
    {
        public PerfilVisualizacaoVendaParamRegiaoMap() : base()
        {
            Table("TB_PERFIL_VIS_VENDA_REG");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_VENDA_REG");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_VENDA_REG");

            Map(x => x.CodUF, "ID_UF").Index("IDX_PERFIL_VIS_VENDA_REG_UF");

            Map(x => x.CodCidade, "ID_CIDADE").Index("IDX_PERFIL_VIS_VENDA_REG_CID");
        }
    }
}
