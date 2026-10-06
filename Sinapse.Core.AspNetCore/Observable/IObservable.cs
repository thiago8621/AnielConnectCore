namespace Sinapse.Core.AspNetCore
{
    public interface IObservable
    {
        void notify();
        void register(IObserver oberver);
        void unregister(IObserver oberver);
    }
}
