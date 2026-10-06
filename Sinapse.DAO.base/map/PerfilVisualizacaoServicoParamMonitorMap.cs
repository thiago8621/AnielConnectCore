using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoServicoParamMonitorMap : ClassMapBase<PerfilVisualizacaoServicoParamMonitor>
    {
        public PerfilVisualizacaoServicoParamMonitorMap() : base()
        {
            Table("TB_PERFIL_VIS_SERV_MONITOR");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_SERV_MONITOR");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_SERV_MONITOR");

            Map(x => x.UsuarioMonitor, "USUARIO_MONITOR").Index("IDX_PERFIL_VIS_SERV_MONITOR");
        }
    }
}
