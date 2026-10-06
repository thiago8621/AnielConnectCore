using NHibernate;
using System;

namespace Sinapse.Model.AspNetCore.core
{
    [Serializable]
    public class FilterFetchMode
    {
        public String FieldName { get; set; }
        public SelectMode FetchModeField { get; set; }
    }
}