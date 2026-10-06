using Sinapse.Model.AspNetCore;

namespace Sinapse.DAO.AspNetCore.map
{
    public class Faixa_Comissionamento_ItemMap : ClassMapBase<Faixa_Comissionamento_Item>
    {
        public Faixa_Comissionamento_ItemMap() : base()
        {
            Table("TB_FAIXA_COMISSIONAMENTO_ITEM");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_FAIXA_COMISSIONAMENTO_ITEM");

            Map(x => x.Faixa_Inicial, "FAIXA_INICIAL");

            Map(x => x.Faixa_Final, "FAIXA_FINAL");

            Map(x => x.Coeficiente, "COEFICIENTE");

            Map(x => x.Faixa_Comissionamento, "ID_FAIXA");
        }
    }
}
