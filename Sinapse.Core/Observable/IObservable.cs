using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    public interface IObservable
    {
        void notify();
        void register(IObserver oberver);
        void unregister(IObserver oberver);
    }
}
