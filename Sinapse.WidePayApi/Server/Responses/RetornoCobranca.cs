using System.Collections.Generic;

namespace Sinapse.WidePayApi.Server.Responses
{
    public class RetornoCobranca: RetornoWideBase
    {
        public bool sucesso { get; set; }
        public int status { get; set; }
        public dynamic id { get; set; }
        public string link { get; set; }
        public string pdf { get; set; }
        public RetornoCobrancaBoleto boleto { get; set; }
        public RetornoCobrancaPix pix { get; set; }
        public string nossoNumero { get; set; }
        public string referencia { get; set; }
        public string RetornoMensagem { get; set; }
        public dynamic IdRecorrencia { get; set; }
        public dynamic IdPagamento { get; set; }
        public dynamic IdTransacao { get; set; }
        public string codigoBusca { get; set; }
        public string RetornoPagamentoSerial { get; set; }
    }

    public class teste
    {
        public List<long> ids_conta_receber { get; set; }
    }
}
