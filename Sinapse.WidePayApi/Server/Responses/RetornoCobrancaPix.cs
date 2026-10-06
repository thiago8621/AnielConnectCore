using Newtonsoft.Json;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Server.Responses
{
    public class RetornoCobrancaPix : RetornoWideBase
    {
        public string emv { get; set; }
        public string qrCode { get; set; }
        [JsonIgnore]
        public byte[] imagem { get; set; }
    }
}