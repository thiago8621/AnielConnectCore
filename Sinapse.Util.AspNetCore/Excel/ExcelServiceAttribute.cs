using System;

namespace Sinapse.Util.AspNetCore
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
