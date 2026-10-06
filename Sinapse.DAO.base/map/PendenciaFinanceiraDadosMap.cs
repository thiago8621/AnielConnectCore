using Sinapse.DAO.map;
using Sinapse.Model;

namespace Aniel.Compras.DAO.map
{
    public class PendenciaFinanceiraDadosMap : ClassMapBase<PendenciaFinanceiraDados>
    {
        public PendenciaFinanceiraDadosMap() : base()
        {
            Table("TB_PENDENCIA_FINANCEIRA_DADOS");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PENDENCIA_FINANCEIRA_DADOS");

            References(u => u.Consulta, "ID_CONSULTA").ForeignKey("FK_PEND_FIN_DADOS_CONSULTA");

            Map(u => u.CodCliente, "COD_CLIENTE");
            Map(u => u.CodContrato, "COD_CONTRATO");
            Map(u => u.CodConexao, "COD_CONEXAO");
            Map(u => u.Cliente, "CLIENTE");
            Map(u => u.Cpf, "CPF");
            Map(u => u.Cnpj, "CNPJ");
            Map(u => u.ContratoCancelado, "CONTRATO_CANCELADO");
            Map(u => u.ConexaoBloqueada, "CONEXAO_BLOQUEADA");
            Map(u => u.Logradouro, "LOGRADOURO").Index("IDX_PEND_FIN_LOGRADOURO");
            Map(u => u.Numero, "NUMERO").Index("IDX_PEND_FIN_NUMERO");
            Map(u => u.Bairro, "BAIRRO");
            Map(u => u.Cidade, "CIDADE");
            Map(u => u.CodFatura, "COD_FATURA");
            Map(u => u.ValorTotal, "VALOR_TOTAL");
            Map(u => u.Liquidado, "LIQUIDADO");
            Map(u => u.Descricao, "DESCRICAO");
            Map(u => u.FaturaSuspensa, "FATURA_SUSPENSA");
            Map(u => u.DataVencimento, "DATA_VENCIMENTO").Index("IDX_PEND_FIN_DATA_VENC");
            Map(u => u.MotivoSuspensao, "MOTIVO_SUSPENSAO");
        }
    }
}
