namespace Sinapse.DAO.AspNetCore
{
    public class PersistenceFactoryNH : IPersistenceFactory
    {

        public IPersistenceManager GetPersistenceManager()
        {
            return new PersistenceManagerNH();
        }


    }
}
