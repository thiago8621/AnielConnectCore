using FluentNHibernate.Mapping;
using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;

namespace Sinapse.DAO.map
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
