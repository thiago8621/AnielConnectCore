using System;

namespace Sinapse.Util.AspNetCore.excecoes
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
