using MongoDB.Driver;

namespace Sinapse.DAO.MongoDB
{
    public interface IConexaoMongo
    {
        IMongoDatabase GetConnection();
    }
}