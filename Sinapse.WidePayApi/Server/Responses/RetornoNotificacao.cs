using Sinapse.WidePayApi.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Server.Responses
{
    public class RetornoNotificacao : RetornoWideBase
    {
        public Cobranca cobranca { get; set; }
    }
}
