using Sinapse.Model.AspNetCore;

namespace Sinapse.DAO.AspNetCore.map
{
    public class ChangeLogMap : ClassMapBase<ChangeLog>
    {
        public ChangeLogMap() : base()
        {
            Table("TB_CHANGELOG");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_CHANGELOG");

            Map(x => x.Codigo, "CODIGO").Length(50).Index("IDX_CHANGELOG");
            Map(x => x.Descricao, "DESCRICAO").Length(100);
            Map(x => x.Executado, "EXECUTADO");
            Map(x => x.Log, "LOG").Length(5000);
        }
    }
}
