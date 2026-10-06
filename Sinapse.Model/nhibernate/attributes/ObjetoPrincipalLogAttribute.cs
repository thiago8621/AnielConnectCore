using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
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
