using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoServicoMap : ClassMapBase<PerfilVisualizacaoServico>
    {
        public PerfilVisualizacaoServicoMap() : base()
        {
            Table("TB_PERFIL_VIS_SERV");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_SERV");

            Map(x => x.Descricao, "DESCRICAO");
            Map(x => x.Status, "STATUS").CustomType<Status_Default>();

            HasMany(x => x.ParamsContratoProjeto)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();

            HasMany(x => x.ParamsArea)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();

            HasMany(x => x.ParamsSetorDepto)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();

            HasMany(x => x.ParamsSupervisor)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();

            HasMany(x => x.ParamsMonitor)
                .KeyColumn("ID_PERFIL_VISUALIZACAO")
                .ReadOnly();
        }
    }
}
