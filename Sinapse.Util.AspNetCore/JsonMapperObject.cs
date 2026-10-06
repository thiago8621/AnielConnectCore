using System;

namespace Sinapse.Util.AspNetCore
{
    [Serializable()]
    public class JsonMapperObject
    {
        public String ControlID { get; set; }

        public String PropertyName { get; set; }

        public object Value { get; set; }
    }
}
