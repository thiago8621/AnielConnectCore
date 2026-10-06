using Sinapse.DAO.map;
using Sinapse.Model;

namespace Sinapse.T5.DAO.Map
{
    public class ConsultaViabilidadePortaDisponivelMap : ClassMapBase<ConsultaViabilidadePortaDisponivel>
    {
        public ConsultaViabilidadePortaDisponivelMap() : base()
        {
            Table("TB_CON_VIABILIDADE_PORTA");

            Id(p => p.Id).GeneratedBy.Sequence("GN_CON_VIABILIDADE_PORTA");

            References(p => p.Consulta, "ID_CON_VIABILIDADE_DADOS").ForeignKey("FK_CON_VIA_PORTA_DADOS");
            References(p => p.Porta, "ID_PORTA").ForeignKey("FK_CON_VIA_PORTA_PORTA");
        }
    }
}
