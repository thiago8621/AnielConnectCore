using System;

namespace Sinapse.DAO.AspNetCore
{
    public class EmptyStringConnectionException : Exception
    {
        public EmptyStringConnectionException()
        {
        }

        public EmptyStringConnectionException(string message)
            : base(message)
        {
        }
        public EmptyStringConnectionException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
