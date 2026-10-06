using System;

namespace Sinapse.Model.AspNetCore.menus
{
    public class Footer_IFrame : IMenu_IFrame
    {
        public Int32? id { get; set; }
        public String text { get; set; }
        public String icon { get; set; }
        public Boolean isFooter { get; set; }
        public string cod_area { get; set; }
    }
}
