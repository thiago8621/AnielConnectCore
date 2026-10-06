using Sinapse.DAO.map;
using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.T5.DAO.Map
{
    public class ConsultaViabilidadeEtapaMap:ClassMapBase<ConsultaViabilidadeEtapa>
    {
        public ConsultaViabilidadeEtapaMap() : base()
        {
            Table("TB_CON_VIABILIDADE_ETAPA");

            Id(x => x.Id).GeneratedBy.Sequence("GN_CON_VIABILIDADE_ETAPA");

            Map(x => x.TipoEtapa, "ETAPA").CustomType<EtapasValidacaoConsultaViabilidadeEnum>();
            Map(x => x.GerouStatus, "GEROU_STATUS");
            HasMany(x => x.DadosConsulta).KeyColumn("ID_CON_VIABILIDADE_ETAPA").Cascade.All();            
            HasMany(x => x.Motivos).KeyColumn("ID_CON_VIABILIDADE_ETAPA").Cascade.All();
            
            References(x => x.Consulta, "ID_CONSULTA_VIABILIDADE").ForeignKey("FK_CONSULTA_VIABILIDADE");            
        }
    }
}
