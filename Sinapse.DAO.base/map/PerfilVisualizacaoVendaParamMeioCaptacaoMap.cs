using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoVendaParamMeioCaptacaoMap : ClassMapBase<PerfilVisualizacaoVendaParamMeioCaptacao>
    {
        public PerfilVisualizacaoVendaParamMeioCaptacaoMap() : base()
        {
            Table("TB_PERFIL_VIS_VENDA_MEIO_CAP");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_VENDA_MEIO_CAP");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_VENDA_MEIO_CAP");

            Map(x => x.CodMeioCaptacao, "ID_MEIO_CAPTACAO").Index("IDX_PERFIL_VIS_VENDA_MEIO_CAP");
        }
    }
}
