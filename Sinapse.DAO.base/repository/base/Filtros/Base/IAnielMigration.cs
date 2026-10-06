using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO
{
    public interface IAnielMigration
    {
        String GetSql();

        String Codigo { get; set; }

        String Descricao { get; set; }

        TipoAnielMigration Tipo { get; set; }
    }
}
