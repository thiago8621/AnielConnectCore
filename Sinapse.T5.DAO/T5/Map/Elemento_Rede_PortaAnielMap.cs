using FluentNHibernate.Mapping;
using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.T5.DAO.Map
{
    public class Elemento_Rede_PortaAnielMap : ClassMap<Elemento_Rede_Porta>
    {
        public Elemento_Rede_PortaAnielMap()
        {
            Table("TB_ELEMENTO_REDE_PORTA");
            Id(x => x.Id, "ID").GeneratedBy.Sequence("GN_ELEMENTO_REDE_PORTA");
            References(x => x.Caixa, "ID_CAIXA").ForeignKey("FK_ELEMENTO_REDE_PORTA_CAIXA");
            //References(x => x.Cliente_Plano_Prod, "CLIENTE_PLANO_PRODUTO").ForeignKey("FK_EL_REDE_PORTA_CLI_PL_PR");
            Map(x => x.Porta, "PORTA");
            Map(x => x.Data_Expiracao_Reserva, "DATA_EXPIRACAO_RESERVA");
            Map(x => x.Login, "LOGIN").Length(100);
            Map(x => x.IdLogin, "ID_LOGIN");
            Map(x => x.Status, "STATUS").CustomType<Situacao_Porta>();
            Map(x => x.Cliente_Plano_Prod, "CLIENTE_PLANO_PRODUTO");
            Map(x => x.CodigoAuxiliar, "COD_AUXILIAR").Length(10);
        }
    }
}
