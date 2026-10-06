using NHibernate.Engine;
using NHibernate.Id;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
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
