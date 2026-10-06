using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO
{
    public static class PersistenceAbstractFactory
    {
        private static IPersistenceFactory persistenceFactory = null;

        public static IPersistenceFactory GetPersistenceFactory()
        {
            if (persistenceFactory == null)
            {
                persistenceFactory = new PersistenceFactoryNH();
            }
            return persistenceFactory;
        }
    }
}
