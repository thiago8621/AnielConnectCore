using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    /// <summary>
    ///     Registra um consulta de endereço realizada para determinado cliente
    /// </summary>
    /// <summary>
    ///     Dados associados a uma consulta de pendência do endereço
    /// </summary>
    public class PendenciaEnderecoDados : BaseModel
    {
        public virtual PendenciaEndereco Consulta { get; set; }
        public virtual string CodCliente { get; set; }
        public virtual string CodContrato { get; set; }
        public virtual string CodConexao { get; set; }
        public virtual string Cliente { get; set; }
        public virtual string Cpf { get; set; }
        public virtual string Cnpj { get; set; }
        public virtual string ContratoCancelado { get; set; }
        public virtual string ConexaoBloqueada { get; set; }
        public virtual string Logradouro { get; set; }
        public virtual string Numero { get; set; }
        public virtual string Bairro { get; set; }
        public virtual string Cidade { get; set; }
        public virtual string CodFatura { get; set; }
        public virtual string ValorTotal { get; set; }
        public virtual string Liquidado { get; set; }
        public virtual string Descricao { get; set; }
        public virtual string FaturaSuspensa { get; set; }
        public virtual string DataVencimento { get; set; }
        public virtual string MotivoSuspensao { get; set; }
        public virtual bool FlagCEP { get; set; }
    }

}
