using System;

namespace Sinapse.Model.AspNetCore.nhibernate
{
    public class ObjetoPrincipalLogAttribute : Attribute
    {
        public string OBSMessageResourceName { get; set; }
        public Type OBSMessageResourceType { get; set; }
        public Type TypeLog { get; set; }

        public ObjetoPrincipalLogAttribute()
        {

        }



    }
}
