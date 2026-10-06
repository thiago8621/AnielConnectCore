using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerados;

namespace Sinapse.DAO.AspNetCore.map
{
    public class BancoMap : ClassMapBase<Banco>
    {
        public BancoMap() : base()
        {
            Table("TB_BANCO");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_BANCO");

            Map(x => x.Descricao, "DESCRICAO").Length(100);
            Map(x => x.Codigo, "CODIGO").Length(5);
            Map(x => x.Status, "STATUS").CustomType<Status_Default>();
            Map(x => x.Carteira, "CARTEIRA");
            Map(x => x.Nosso_Numero_Max, "NOSSO_NUMERO_MAX");
        }
    }
}
