using System;

namespace Sinapse.Core.AspNetCore
{
    public interface IObserver
    {
        void update(Object obj);
    }
}
