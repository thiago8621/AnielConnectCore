using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
{
    public class ObjetoAuxiliarLogAttribute : Attribute
    {
        public String ForeingKey { get; set; }
        public Type TipoPai { get; set; }
        public string OBSMessageResourceName { get; set; }
        public Type OBSMessageResourceType { get; set; }
        public Type TipoLog { get; set; }


        public ObjetoAuxiliarLogAttribute()
        {
            
        }

    }
}
