using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util.excecoes
{
    public class DoubleInvalidException : Exception
    {
        private String fieldName = String.Empty;

        public DoubleInvalidException(String FieldName)
        {
            this.fieldName = FieldName;
        }

        public override string Message
        {
            get
            {
                return String.Format("Valor Decimal inválido para o Campo {0}.", fieldName);
            }
        }
    }
}
