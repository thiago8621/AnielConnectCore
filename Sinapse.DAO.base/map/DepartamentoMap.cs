using Sinapse.DAO.map;
using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class DepartamentoMap : ClassMapBase<Departamento>
    {
        public DepartamentoMap() : base()
        {
            Table("TB_DEPARTAMENTO");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_DEPARTAMENTO");

            Map(x => x.Cod_Depto, "COD_DEPTO").Length(10);
            Map(x => x.Num_Depto, "NUM_DEPTO").Length(10);
            Map(x => x.Descricao, "DESCRICAO").Length(30);
        }
    }
}
