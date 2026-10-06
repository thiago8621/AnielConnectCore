using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    [Serializable()]
    public class JsonMapperObject
    {
        public String ControlID { get; set; }

        public String PropertyName { get; set; }

        public object Value { get; set; }
    }
}
