using FluentNHibernate.Mapping;
using Sinapse.ModelLog.AspNetCore;

namespace Sinapse.DAOLog.AspNetCore
{
    public class ClassMapLogBase<T> : ClassMap<T> where T : ILog
    {
        public ClassMapLogBase()
        {
            
            Map(p => p.JSON, "JSON").Length(10000);

        }

    }
}
