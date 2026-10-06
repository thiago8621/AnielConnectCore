using Sinapse.Model;
using Sinapse.Model.nhibernate;
using Sinapse.ModelLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.Connect.ModelLog.nhibernate
{
    public class LogVincularPerfilBase : LogGenerico<Int64>
    {
        public LogVincularPerfilBase()
        {
        }

        public virtual string Tecnico { get; set; }
        public virtual string Monitor { get; set; }


    }

}
