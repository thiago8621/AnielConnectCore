using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinapse.Cache.AspNetCore.Caching;

namespace Sinapse.Core.AspNetCore
{
    [Serializable]
    [Caching]
    public class PaginaSistema
    {
        public virtual String id { get; set; }

        public virtual String title { get; set; }

        public virtual Boolean close { get; set; }
        
        public virtual String url { get; set; }

        public virtual String urlType { get; set; }

        public override string ToString()
        {
            return id;
        }
    }
}
