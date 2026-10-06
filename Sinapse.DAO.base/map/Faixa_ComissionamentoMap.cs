using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class Faixa_ComissionamentoMap : ClassMapBase<Faixa_Comissionamento>
    {
        public Faixa_ComissionamentoMap() : base()
        {
            Table("TB_FAIXA_COMISSIONAMENTO");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_FAIXA_COMISSIONAMENTO");

            Map(x => x.Codigo, "CODIGO").Length(20);

            Map(x => x.Descricao, "DESCRICAO").Length(100);

            Map(x => x.Status, "STATUS").CustomType<Status_Default>();

            HasMany(x => x.Itens).KeyColumn("ID_FAIXA").Cascade.All().Inverse();

            //HasMany(x => x.Itens).KeyColumn("ID_FAIXA").Cascade.All().Inverse();
        }
    }
}
