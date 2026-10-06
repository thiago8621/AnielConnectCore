namespace Sinapse.DAO.AspNetCore
{
    public interface IPersistenceFactory
    {
        IPersistenceManager GetPersistenceManager();
    }
}
