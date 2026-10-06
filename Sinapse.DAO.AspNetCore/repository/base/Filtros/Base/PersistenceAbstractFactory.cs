namespace Sinapse.DAO.AspNetCore
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
