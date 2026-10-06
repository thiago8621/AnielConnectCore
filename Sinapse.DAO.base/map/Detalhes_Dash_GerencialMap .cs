using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class Detalhes_Dash_GerencialMap : ClassMapBase<Detalhes_Dash_Gerencial>
    {
        public Detalhes_Dash_GerencialMap() : base()
        {
            Table("TB_DETALHES_DASH_GERENCIAL");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_DETALHES_DASH_GERENCIAL");

            References(x => x.Perfil_DashBoard_Connect, "ID_PERFIL_DASHBOARD_CONNECT").ForeignKey("FK_DETDASH_PERFILDASH_GER");
            Map(x => x.Recurso, "RECURSO");
        }
    }
}
