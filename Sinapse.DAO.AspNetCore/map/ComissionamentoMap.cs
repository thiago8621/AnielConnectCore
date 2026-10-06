using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerados;

namespace Sinapse.DAO.AspNetCore.map
{
    public class ComissionamentoMap : ClassMapBase<Comissionamento>
    {
        public ComissionamentoMap() : base()
        {
            Table("TB_COMISSIONAMENTO");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_COMISSIONAMENTO");

            Map(x => x.Codigo, "CODIGO").Length(20);

            Map(x => x.Descricao, "DESCRICAO").Length(100);

            Map(x => x.Id_Filial, "ID_FILIAL");

            Map(x => x.Status, "STATUS").CustomType<Status_Default>();


        }
    }
}
