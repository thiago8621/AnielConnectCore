using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Model.core
{
    [Serializable]
    public enum FilterValueIdentity
    {
        Equal,
        Lower,
        Higher,
        Contains,
        NotEqual,
        BeginWith,
        LowerEqual,
        HigherEqual
    }
}