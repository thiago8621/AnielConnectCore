using Sinapse.DAO.map;
using Sinapse.Model;

namespace Sinapse.T5.DAO.Map
{
    public class ConsultaViabilidadeDadosMap : ClassMapBase<ConsultaViabilidadeDados>
    {
        public ConsultaViabilidadeDadosMap() : base()
        {
            Table("TB_CON_VIABILIDADE_DADOS");

            Id(p => p.Id).GeneratedBy.Sequence("GN_CON_VIABILIDADE_DADOS");

            References(p => p.Consulta, "ID_CONSULTA_VIABILIDADE").ForeignKey("FK_CON_VIA_DADOS_CONSULTA");
            References(p => p.Etapa, "ID_CON_VIABILIDADE_ETAPA").ForeignKey("FK_CON_VIA_DADOS_ETAPA");
            References(p => p.Caixa, "ID_CAIXA").ForeignKey("FK_CON_VIA_DADOS_CAIXA");
            //Map(p => p.IdMotivoinviabilidade, "MOTIVO_INVIABILIDADE");
            Map(p => p.PortaReservada, "PORTA_RESERVADA");
            Map(p => p.Distancia, "DISTANCIA");

            HasMany(p => p.PortasDisponiveis).KeyColumn("ID_CON_VIABILIDADE_DADOS").Cascade.All();

        }
    }
}
