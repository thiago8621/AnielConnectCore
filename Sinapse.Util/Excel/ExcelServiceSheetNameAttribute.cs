using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util.Excel
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
