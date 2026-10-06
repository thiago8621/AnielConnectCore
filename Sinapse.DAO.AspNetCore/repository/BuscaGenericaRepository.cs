using NHibernate;
using System;

namespace Sinapse.DAO.AspNetCore.repository
{
    public class BuscaGenericaRepository<T> : GenericRepository<T, Int64> where T : class
    {
        public BuscaGenericaRepository(ISession Session, ISession SessionLog) : base (Session, SessionLog)
        {
            
        }
    }
}
