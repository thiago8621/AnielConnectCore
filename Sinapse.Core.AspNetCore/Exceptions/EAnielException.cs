using System;

namespace Sinapse.Core.AspNetCore
{
    public class EAnielException : Exception
    {
        public EAnielException()
        {

        }
        
        public EAnielException(string message): base(message)
        {

        }

        public EAnielException(string message, System.Exception inner): base(message, inner)
        {

        }
    }
}
