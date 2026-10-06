using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoServicoParamSupervisorMap : ClassMapBase<PerfilVisualizacaoServicoParamSupervisor>
    {
        public PerfilVisualizacaoServicoParamSupervisorMap() : base()
        {
            Table("TB_PERFIL_VIS_SERV_SUPERVISOR");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_SERV_SUPERVISOR");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_SERV_SUPERVISOR");

            Map(x => x.CodSuperv, "COD_SUPERV").Index("IDX_PERFIL_VIS_SERV_SUPERV");
        }
    }
}
