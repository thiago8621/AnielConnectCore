using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoServicoParamSetorDeptoMap : ClassMapBase<PerfilVisualizacaoServicoParamSetorDepto>
    {
        public PerfilVisualizacaoServicoParamSetorDeptoMap() : base()
        {
            Table("TB_PERFIL_VIS_SERV_SETOR");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_SERV_SETOR");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_SERV_SETOR");

            Map(x => x.CodDepto, "COD_DEPTO").Index("IDX_PERFIL_VIS_SERV_DEPTO");
        }
    }
}
