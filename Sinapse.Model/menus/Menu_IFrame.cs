using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.menus
{
    public class Menu_IFrame : IMenu_IFrame
    {
        public Int32? id { get; set; }
        public String text { get; set; }
        public String icon { get; set; }
        public String url { get; set; }
        public String urlType { get; set; }
        public String targetType { get; set; }
        public String cod_area { get; set; }
    }
}
