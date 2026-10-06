using Sinapse.DAO.AspNetCore.map;
using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerados;

namespace Aniel.Compras.DAO.AspNetCore.map
{
    public class Perfil_DashBoard_ConnectMap : ClassMapBase<Perfil_DashBoard_Connect>
    {
        public Perfil_DashBoard_ConnectMap() : base()
        {
            Table("TB_PERFIL_DASHBOARD_CONNECT");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_DASHBOARD_CONNECT");

            Map(x => x.Sistema, "SISTEMA").CustomType<Perfil_DashBoard_Sistema>();
            Map(x => x.Descricao, "DESCRICAO").Length(100);
            Map(x => x.Dashboard_Inicial, "Dashboard_Inicial").CustomType<DashboardInicialConnect>();
            Map(x => x.DashBoard_Gerencial, "DASHBOARD_GERENCIAL").CustomType<DashBoard_Gerencial>();
            HasMany(x => x.Detalhes_Dash_Inicial).KeyColumn("ID_PERFIL_DASHBOARD_CONNECT").Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Detalhes_Dash_Gerencial).KeyColumn("ID_PERFIL_DASHBOARD_CONNECT").Cascade.AllDeleteOrphan().Inverse();
            Map(x => x.Status, "STATUS").CustomType<Status_Default>();

        }
    }
}
