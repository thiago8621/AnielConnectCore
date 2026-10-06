using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    public class ExcelServiceValoresAttribute : Attribute
    {
        public String[] Valores { get; set; }
        private ExcelServiceValoresAttribute() : base()
        {

        }

        public ExcelServiceValoresAttribute(params String[] pValores)
        {
            this.Valores = pValores;
        }
    }
}
