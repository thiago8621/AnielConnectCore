using Sinapse.DAO.map;
using Sinapse.Model;

namespace Aniel.Compras.DAO.map
{
    public class Detalhes_Dash_InicialMap : ClassMapBase<Detalhes_Dash_Inicial>
    {
        public Detalhes_Dash_InicialMap() : base()
        {
            Table("TB_DETALHES_DASH_INICIAL");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_DETALHES_DASH_INICIAL");

            References(x => x.Perfil_DashBoard_Connect, "ID_PERFIL_DASHBOARD_CONNECT").ForeignKey("FK_DETALHEDASH_PERFILDASH");
            Map(x => x.Recurso, "RECURSO");
        }
    }
}
