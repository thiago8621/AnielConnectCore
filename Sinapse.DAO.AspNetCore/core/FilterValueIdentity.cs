using System;

namespace Sinapse.Model.AspNetCore.core
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