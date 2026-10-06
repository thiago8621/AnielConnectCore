using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.repository
{
    public class BuscaGenericaRepository<T> : GenericRepository<T, Int64> where T : class
    {
        public BuscaGenericaRepository(ISession Session, ISession SessionLog) : base (Session, SessionLog)
        {
            
        }
    }
}
