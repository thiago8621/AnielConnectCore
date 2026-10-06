using System;

namespace Sinapse.Model.AspNetCore.nhibernate
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
