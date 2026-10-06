using System;

namespace Sinapse.Util.AspNetCore.Excel
{
    public class ExcelServiceSheetNameAttribute : Attribute
    {
        public String Sheet { get; set; }
        private ExcelServiceSheetNameAttribute() : base()
        {

        }

        public ExcelServiceSheetNameAttribute(String pSheet)
        {
            this.Sheet = pSheet;
        }
    }
}
