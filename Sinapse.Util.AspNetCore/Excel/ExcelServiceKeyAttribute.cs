using System;

namespace Sinapse.Util.AspNetCore
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
