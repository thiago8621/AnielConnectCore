using FluentNHibernate.Mapping;
using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;

namespace Sinapse.DAO.AspNetCore.map
{
    public class ClassMapBaseLog<T> : ClassMap<T> where T : IModelLog
    {
        public ClassMapBaseLog()
        {
            Id(u => u.Id, "ID").GeneratedBy.Identity();

            Map(p => p.UsuarioCriacao).Length(30);
            Map(p => p.DataHoraCriacao);
            Map(p => p.TipoOperacao).CustomType<TipoOperacao>();

            Map(p => p.Token).Length(50);
        }

    }
}
