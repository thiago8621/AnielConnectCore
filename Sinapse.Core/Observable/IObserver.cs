using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    public interface IObserver
    {
        void update(Object obj);
    }
}
