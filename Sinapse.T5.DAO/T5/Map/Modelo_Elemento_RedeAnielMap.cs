using FluentNHibernate.Mapping;
using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.T5.DAO.Map
{
    public class Modelo_Elemento_RedeAnielMap : ClassMap<Modelo_Elemento_Rede>
    {
        public Modelo_Elemento_RedeAnielMap()
        {
            Table("TB_MODELO_ELEMENTO_REDE");

            Id(t => t.Id, "ID").GeneratedBy.Sequence("GN_MOD_ELEMENTO_REDE");

            Map(t => t.Descricao, "DESCRICAO").Length(50);
            Map(t => t.Status, "STATUS").CustomType<Status_Default>();

        }
    }
}
