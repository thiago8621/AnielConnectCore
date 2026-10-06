using NHibernate.Engine;
using NHibernate.Id;
using System;

namespace Sinapse.Model.AspNetCore
{
    public class CustomGenerator : TableGenerator
    {
        public override object Generate(ISessionImplementor session, object obj)
        {
            var retorno = Convert.ToInt64(base.Generate(session, obj));
            return retorno;
        }

    }
}
