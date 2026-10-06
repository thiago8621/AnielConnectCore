using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.BoletoAPI.Entity
{
    public class Cobranca
    {
        /// <summary>
        /// Instruções para o Caixa
        /// </summary>
        public string instrucoes { get; set; }
        /// <summary>
        /// Valor Total do Desconto
        /// </summary>
        public double? desconto { get; set; }
        /// <summary>
        /// Data máxima que valerá o desconto
        /// </summary>
        public DateTime? dataDesconto { get; set; }
        /// <summary>
        /// Valor diário do juros cobrados ao dia
        /// </summary>
        public double? juros { get; set; }
        /// <summary>
        /// Valor cobrado da multa após o vencimento
        /// </summary>
        public double? multa { get; set; }
        /// <summary>
        /// Valor do boleto
        /// </summary>
        public double valor { get; set; }
        /// <summary>
        /// Data do Vencimento do boleto
        /// </summary>
        public DateTime vencimento { get; set; }
        /// <summary>
        /// Pessoa responsável por pagar o boleto
        /// </summary>
        public Pessoa sacado { get; set; }
        /// <summary>
        /// Instrucoes da Cobranca para Gerar o boleto
        /// </summary>
        public string[] InstrucoesCob { get; set; } = new string[5];
        /// <summary>
        /// SeuNumero utilizado geralmente para NF, não pode se repetir.
        /// </summary>
        public long SeuNumero { get; set; }
    }
}
