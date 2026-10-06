using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.core
{
    public enum SearchType
    {
        Equal = 1,
        BeginWith = 2,
        Contains = 3,
        Between = 4,
        NotEqual = 5,
        Lower = 6,
        LowerEqual = 7,
        Higher = 8,
        HigherEqual = 9
    }
}
