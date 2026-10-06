using FluentNHibernate.Mapping;
using Sinapse.ModelLog;
using System;

namespace Sinapse.DAOLog
{
    public class ClassMapLogBase<T> : ClassMap<T> where T : ILog
    {
        public ClassMapLogBase()
        {
            
            Map(p => p.JSON, "JSON").Length(10000);

        }

    }
}
