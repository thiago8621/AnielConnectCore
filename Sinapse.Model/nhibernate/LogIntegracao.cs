using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
{
    [Serializable]
    public class LogIntegracao: BaseModel
    {
        public virtual String NomeArquivo { get; set; }
        public virtual String Objeto { get; set; }

        public virtual Int32 QtdeImportado { get; set; }
        public virtual Int32 QtdeNImportado { get; set; }
        public virtual Int32 Total { get; set; }
    }
}
