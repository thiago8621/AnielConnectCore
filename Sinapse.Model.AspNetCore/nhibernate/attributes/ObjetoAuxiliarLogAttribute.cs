using System;

namespace Sinapse.Model.AspNetCore.nhibernate
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
