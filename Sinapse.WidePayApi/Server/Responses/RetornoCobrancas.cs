using Sinapse.WidePayApi.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Server.Responses
{
    public class RetornoCobrancas : RetornoWideBase
    {
        public int total { get; set; }
        public List<Cobranca> cobrancas { get; set; }
    }
}
