using Sinapse.DAO.map;
using Sinapse.Model;

namespace Aniel.Compras.DAO.map
{
    public class PendenciaFinanceiraMap : ClassMapBase<PendenciaFinanceira>
    {
        public PendenciaFinanceiraMap() : base()
        {
            Table("TB_PENDENCIA_FINANCEIRA");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PENDENCIA_FINANCEIRA");

            Map(u => u.Cpf, "CPF").Index("IDX_PEND_FIN_CPF");
            Map(u => u.Cnpj, "CNPJ").Index("IDX_PEND_FIN_CNPJ");

            HasMany(u => u.DadosConsulta).KeyColumn("ID_CONSULTA").Cascade.All();
        }
    }
}
