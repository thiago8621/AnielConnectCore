namespace Sinapse.WidePayApi.Server.Responses
{
    public class RetornoBoletoParametro
    {
        public string banco { get; set; }
        public string agencia { get; set; }
        public string conta { get; set; }
        public string convenio { get; set; }
        public string carteira { get; set; }
        public string variacao { get; set; }
        public int numero { get; set; }
        public string cnpj { get; set; }
    }
}
