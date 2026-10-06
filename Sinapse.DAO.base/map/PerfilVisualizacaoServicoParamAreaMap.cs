using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoServicoParamAreaMap : ClassMapBase<PerfilVisualizacaoServicoParamArea>
    {
        public PerfilVisualizacaoServicoParamAreaMap() : base()
        {
            Table("TB_PERFIL_VIS_SERV_AREA");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_SERV_AREA");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_SERV_AREA");

            Map(x => x.CodEscritorio, "COD_ESCRITORIO").Length(4).Index("IDX_PERFIL_VIS_SERV_ESC");
            Map(x => x.CodArea, "COD_AREA").Length(4).Index("IDX_PERFIL_VIS_SERV_AREA");
            Map(x => x.CodLocalidade, "COD_LOCALIDADE").Index("IDX_PERFIL_VIS_SERV_LOC");
        }
    }
}
