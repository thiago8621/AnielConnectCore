using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoServicoParamContratoProjetoMap : ClassMapBase<PerfilVisualizacaoServicoParamContratoProjeto>
    {
        public PerfilVisualizacaoServicoParamContratoProjetoMap() : base()
        {
            Table("TB_PERFIL_VIS_SERV_CONTRATO");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_SERV_CONTRATO");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_SERV_CONTRATO");

            Map(x => x.CodCt, "CODCT").Length(4).Index("IDX_PERFIL_VIS_SERV_CODCT");
            Map(x => x.Projeto, "PROJETO").Length(20).Index("IDX_PERFIL_VIS_SERV_PROJ");
        }
    }
}
