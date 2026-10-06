using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Server.Responses
{
    public class RetornoWideBase
    {
        public bool sucesso { get; set; }
        public string erro { get; set; }
        public List<ValidExecWideBase> validacao { get; set; }
        public List<ValidExecWideBase> execucao { get; set; }
    }
}
