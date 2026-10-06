using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Model.core
{
    [Serializable]
    public class FilterFetchMode
    {
        public String FieldName { get; set; }
        public SelectMode FetchModeField { get; set; }
    }
}