using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    public class ExcelServiceKeyAttribute : Attribute
    {
        public String Coluna { get; set; }
        private ExcelServiceKeyAttribute() : base()
        {

        }

        public ExcelServiceKeyAttribute(String pColuna)
        {
            this.Coluna = pColuna;
        }
    }
}
