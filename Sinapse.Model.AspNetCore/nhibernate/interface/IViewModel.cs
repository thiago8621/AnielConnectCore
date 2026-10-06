namespace Sinapse.Model.AspNetCore.nhibernate
{
    public interface IViewModel<K>
    {
        K Id { get; set; }
    }
}
