using FluentNHibernate.Mapping;
using Sinapse.DAO.map;
using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.T5.DAO.Map
{
    public class TipoCaboAnielMap : ClassMap<Tipo_Cabo>
    {
        public TipoCaboAnielMap()
        {
            Table("TB_TIPO_CABO");

            Id(x => x.Id, "ID").GeneratedBy.Sequence("GN_TIPO_CABO");

            Map(x => x.Descricao, "DESCRICAO").Length(50);

        }
    }
}
