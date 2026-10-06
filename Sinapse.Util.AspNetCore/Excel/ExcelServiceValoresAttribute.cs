using System;

namespace Sinapse.Util.AspNetCore
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
