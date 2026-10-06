using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.menus
{
    public class Title_IFrame : IMenu_IFrame
    {
        public Int32? id { get; set; }
        public String text { get; set; }
        public String icon { get; set; }
        public Boolean isHeader { get; set; }
        public string cod_area { get; set; }
    }
}
