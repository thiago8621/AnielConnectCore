using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
{
    public class BindModelViewAttribute : Attribute
    {
        public String Property { get; set; }

        public BindModelViewAttribute()
        {

        }

        public BindModelViewAttribute(String pProperty)
        {
            Property = pProperty;
        }
    }
}
