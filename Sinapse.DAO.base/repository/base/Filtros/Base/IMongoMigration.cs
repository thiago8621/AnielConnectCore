using Sinapse.DAO.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO
{
    public interface IMongoMigration
    {
        String Codigo { get; set; }
        String Descricao { get; set; }
        TipoMongoMigration Tipo { get; set; }
        IConexaoMongo Conexao { get; set; }
        void Execute();
    }

}
