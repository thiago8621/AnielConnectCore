using Sinapse.DAO.map;
using Sinapse.Model;

namespace Aniel.Compras.DAO.map
{
    public class PendenciaEnderecoMap : ClassMapBase<PendenciaEndereco>
    {
        public PendenciaEnderecoMap() : base()
        {
            Table("TB_PENDENCIA_ENDERECO");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PENDENCIA_ENDERECO");

            Map(u => u.Logradouro, "LOGRADOURO").Index("IDX_PEND_ENDER_LOGRADOURO");
            Map(u => u.Numero, "NUMERO").Index("IDX_PEND_ENDER_NUMERO");

            HasMany(u => u.DadosConsulta).KeyColumn("ID_CONSULTA").Cascade.All();
        }
    }
}
