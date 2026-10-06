using System;

namespace Sinapse.Core.AspNetCore
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
