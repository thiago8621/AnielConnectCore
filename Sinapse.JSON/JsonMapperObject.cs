using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.JSON
{
    [Serializable()]
    public class JsonMapperObject
    {
        public String ControlID { get; set; }

        public String PropertyName { get; set; }

        public object Value { get; set; }
    }
}
