using FluentNHibernate.Mapping;
using Sinapse.Model.AspNetCore.nhibernate;

namespace Sinapse.DAO.AspNetCore.map
{
    public class ClassMapBase<T> : ClassMap<T> where T : IModel
    {
        public ClassMapBase()
        {
            Id(u => u.Id, "ID").GeneratedBy.Identity();

            Map(p => p.UsuarioCriacao).Length(30);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioAlteracao).Length(30);
            Map(p => p.DataHoraAlteracao);

            Map(p => p.Token).Length(50);
        }

    }
}
