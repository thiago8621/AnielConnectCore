using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    public class ExcelServiceLog<T>
    {
        public ExcelServiceLog()
        {
            Sucessos = new List<string>();
            Erros = new List<string>();
        }

        public List<ExcelServiceLogItem> Log { get; set; }

        public List<T> Items { get; set; }

        public List<string> Erros { get; set; }
        public List<string> Sucessos { get; set; }
    }
}
