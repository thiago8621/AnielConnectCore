using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;

namespace Sinapse.Model
{
    public class ChangeLog : BaseModel
    {
        public virtual String Codigo { get; set; }
        public virtual String Descricao { get; set; }
        public virtual Boolean Executado { get; set; }
        public virtual String Log { get; set; }
    }
}
