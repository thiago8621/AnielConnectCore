using System;
using System.Collections.Generic;
using System.Linq;


namespace Sinapse.Core
{
    public class InvalidUserConnectionControl : Exception
    {
        public override string Message
        {
            get
            {
                return "Usuário de conexão inválido.";
            }
        }

        
    }
}
