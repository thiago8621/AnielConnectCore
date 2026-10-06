using System;

namespace Sinapse.Model.AspNetCore.menus
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
