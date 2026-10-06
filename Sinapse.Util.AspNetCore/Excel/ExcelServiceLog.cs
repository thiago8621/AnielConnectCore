using System.Collections.Generic;

namespace Sinapse.Util.AspNetCore
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
