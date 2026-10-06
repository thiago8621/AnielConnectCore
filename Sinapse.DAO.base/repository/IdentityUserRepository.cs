using NHibernate;
using Sinapse.Model.core;
using Sinapse.Model.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.repository
{
    public class IdentityUserRepository : GenericRepository<IdentityUser, String>
    {
        public IdentityUserRepository(ISession Session, ISession SessionLog) : base (Session, SessionLog)
        {

        }

        public async Task<IEnumerable<IdentityUser>> GetByLoginContains(String pToken, String pLogin)
        {
            return await base.GetListByFiltroPage(new Filters()
                .AddFilter("UserName", SearchType.Contains, pLogin)
                .AddFilter("Token", SearchType.Equal, pToken)
                , 1, 100);
        }
    }
}
