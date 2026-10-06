using System;

namespace Sinapse.DAO.AspNetCore
{
    public interface IAnielMigration
    {
        String GetSql();

        String Codigo { get; set; }

        String Descricao { get; set; }

        TipoAnielMigration Tipo { get; set; }
    }
}
