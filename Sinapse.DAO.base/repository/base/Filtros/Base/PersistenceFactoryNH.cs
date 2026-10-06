using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO
{
    public class PersistenceFactoryNH : IPersistenceFactory
    {

        public IPersistenceManager GetPersistenceManager()
        {
            return new PersistenceManagerNH();
        }


    }
}
