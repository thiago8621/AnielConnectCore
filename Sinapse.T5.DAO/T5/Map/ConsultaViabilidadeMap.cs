using Sinapse.DAO.map;
using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.T5.DAO.Map
{
    public class ConsultaViabilidadeMap : ClassMapBase<ConsultaViabilidade>
    {
        public ConsultaViabilidadeMap() : base()
        {
            Table("TB_CONSULTA_VIABILIDADE");

            Id(p => p.Id).GeneratedBy.Sequence("GN_CONSULTA_VIABILIDADE");

            Map(p => p.Latitude, "LATITUDE").Index("IDX_CON_VIA_LATITUDE");
            Map(p => p.Longitude, "LONGITUDE").Index("IDX_CON_VIA_LONGITUDE");
            Map(p => p.Status, "STATUS").CustomType<StatusConsultaViabilidade>();

            HasMany(p => p.DadosConsulta).KeyColumn("ID_CONSULTA_VIABILIDADE").Cascade.All();
            HasMany(p => p.Motivos).KeyColumn("ID_CONSULTA_VIABILIDADE").Cascade.All();
            HasMany(p => p.Etapas).KeyColumn("ID_CONSULTA_VIABILIDADE").Cascade.All();
        }
    }
}
