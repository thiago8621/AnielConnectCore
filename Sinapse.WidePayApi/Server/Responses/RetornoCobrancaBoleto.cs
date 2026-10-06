using Newtonsoft.Json;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Server.Responses
{
    public class RetornoCobrancaBoleto : RetornoWideBase
    {
        public RetornoBoletoParametro parametros { get; set; }
        public string imagem { get; set; }
        [JsonProperty("linha-digitavel")]
        public string codigo { get; set; }     
        public string html { get; set; }
        public string cobranca { get; set; }
        public RetornoCobrancaPix pix {get;set;}
    }
}
