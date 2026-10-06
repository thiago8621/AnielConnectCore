using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Entity
{
    public class Boleto
    {
        public double desconto { get; set; }
        public double multa { get; set; }
        public double juros { get; set; }
        /// <summary>
        /// Informações adicionais para pagamentos que será exibido no boleto.
        /// Ex.: Pagável em qualquer banco até o vencimento
        /// Ex.: Desconto de X até se pagar antes do vencimento
        /// </summary>
        public string instrucoes { get; set; }
    }

     public class Pix
    {
        public string emv { get; set; }
        public string imagem { get; set; }
    }
}
