using Sinapse.DAO.MongoDB;
using System;
using System.Threading.Tasks;

namespace Sinapse.DAO
{
    public class MongoMigration : IMongoMigration
    {
        public MongoMigration()
        {

        }

        public virtual void Execute()
        {
           
        }

        public virtual String Codigo { get; set; }
        public virtual String Descricao { get; set; }
        public virtual TipoMongoMigration Tipo { get; set; }
        public virtual IConexaoMongo Conexao { get; set; }
    }
}
