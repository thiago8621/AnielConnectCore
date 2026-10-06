using Sinapse.DAO.map;
using Sinapse.Model;

namespace Sinapse.T5.DAO.Map
{
    public class ConsultaViabilidadeMotivoMap : ClassMapBase<ConsultaViabilidadeMotivo>
    {
        public ConsultaViabilidadeMotivoMap() : base()
        {
            Table("TB_CON_VIABILIDADE_MOT");

            Id(p => p.Id).GeneratedBy.Sequence("GN_CON_VIABILIDADE_MOT");

            Map(p => p.IdMotivo, "ID_MOTIVO");
            Map(p => p.Motivo, "MOTIVO").Length(100);

            References(p => p.Consulta, "ID_CONSULTA_VIABILIDADE").ForeignKey("FK_CONSULTA_VIABILIDADE");
            References(p => p.Etapa, "ID_CON_VIABILIDADE_ETAPA").ForeignKey("FK_CON_VIA_MOT_ETAPA");
        }
    }
}
