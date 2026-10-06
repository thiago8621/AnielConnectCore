using System;

namespace Sinapse.DAO
{
    public class AnielMigration : IAnielMigration
    {
        public AnielMigration()
        {

        }

        public virtual String GetSql()
        {
            return String.Empty;
        }

        public virtual String Codigo { get; set; }
        public virtual String Descricao { get; set; }
        public virtual TipoAnielMigration Tipo { get; set; }
    }
}
