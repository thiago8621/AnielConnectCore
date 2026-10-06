using System;
namespace Sinapse.Model.AspNetCore.nhibernate
{
    public interface ITelefone
    {
        Int64 Id { get; set; }
        string DDD { get; set; }
        string DDI { get; set; }
        string Numero { get; set; }
    }
}
