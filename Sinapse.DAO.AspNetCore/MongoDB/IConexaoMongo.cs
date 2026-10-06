using MongoDB.Driver;

namespace Sinapse.DAO.AspNetCore.MongoDB
{
    public interface IConexaoMongo
    {
        IMongoDatabase GetConnection();
    }
}