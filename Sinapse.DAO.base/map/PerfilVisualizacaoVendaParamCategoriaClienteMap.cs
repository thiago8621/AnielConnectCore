using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoVendaParamCategoriaClienteMap : ClassMapBase<PerfilVisualizacaoVendaParamCategoriaCliente>
    {
        public PerfilVisualizacaoVendaParamCategoriaClienteMap() : base()
        {
            Table("TB_PERFIL_VIS_VENDA_CAT_CLI");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_VENDA_CAT_CLI");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_VENDA_CAT_CLI");

            Map(x => x.CodCategoriaCliente, "ID_CATEGORIA_CLIENTE").Index("IDX_PERFIL_VIS_VENDA_CAT_CLI");
        }
    }
}
