using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    public class ExcelServiceAttribute : Attribute
    {
        public String Coluna { get; set; }
        private ExcelServiceAttribute() : base()
        {

        }

        public ExcelServiceAttribute(String pColuna)
        {
            this.Coluna = pColuna;
        }
    }
}
