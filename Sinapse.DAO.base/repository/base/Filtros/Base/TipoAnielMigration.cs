using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO
{
    public enum TipoAnielMigration
    {
        DDL = 0,
        ProcedureTrigger = 1,
        ScriptUnico = 2,
        ScriptMultiplo = 3,
        Select = 4
    }
}
