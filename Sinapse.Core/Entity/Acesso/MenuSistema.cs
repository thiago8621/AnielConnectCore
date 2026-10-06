using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinapse.Cache.Caching;

namespace Sinapse.Core
{
    [Serializable]
    [Caching]
    public class MenuSistema
    {
        List<MenuSistema> _children;

        public virtual String id { get; set; }

        public virtual String text { get; set; }

        public virtual String icon { get; set; }

        public virtual String url { get; set; }

        public virtual String targetType { get; set; }

        public virtual Int32 ordem { get; set; }

        public virtual String areaModulo { get; set; }

        public virtual Boolean visivel { get; set; }

        public virtual SistemaWeb sistema { get; set; }

        public virtual MenuSistema menuPai { get; set; }

        public virtual List<MenuSistema> children 
        {
            get
            {
                if (_children == null)
                    _children = new List<MenuSistema>();
                return _children;
            }
            set
            {
                _children = value;
            }
        } 
    }
}
