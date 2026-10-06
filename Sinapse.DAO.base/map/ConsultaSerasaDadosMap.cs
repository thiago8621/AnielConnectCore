using Sinapse.DAO.map;
using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Aniel.Compras.DAO.map
{
    public class ConsultaSerasaDadosMap : ClassMapBase<ConsultaSerasaDados>
    {
        public ConsultaSerasaDadosMap() : base()
        {
            Table("TB_CONSULTA_SERASA");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_CONSULTA_SERASAPF");
            Map(u => u.consulta, "CONSULTA").Length(8000);
            Map(u => u.cpf, "CPF").Length(15);
            Map(u => u.cnpj, "CNPJ").Length(15);
            Map(u => u.ParamLoja, "PARAM_LOJA").Length(15);
            Map(u => u.TipoIntegracao, "TIPO_INTEGRACAO").CustomType<IntegracaoExternaConsultaCredEnum>();
        }
    }
}
