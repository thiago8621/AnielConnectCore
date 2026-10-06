using System;

namespace Sinapse.Util.AspNetCore.excecoes
{
    public class IntergerInvalidException : Exception
    {
        private String fieldName = String.Empty;

        public IntergerInvalidException(String FieldName)
        {
            this.fieldName = FieldName;
        }

        public override string Message
        {
            get
            {
                return String.Format("Valor Inteiro Invário para o Campo {0}", fieldName);
            }
        }
    }
}
