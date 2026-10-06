using System;
using System.Collections.Generic;

namespace Sinapse.Model.AspNetCore.menus
{
    public class MenuPai_IFrame : IMenu_IFrame
    {
        public Int32? id { get; set; }
        public String text { get; set; }
        public String icon { get; set; }
        public List<Menu_IFrame> children { get; set; }
        public string cod_area { get; set; }
    }
}
