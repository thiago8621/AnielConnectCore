using System;

namespace Sinapse.Util.AspNetCore
{
    public class PostHttpException : Exception
    {
        private Exception Erro;


        private PostHttpException()
        {
        }

        private PostHttpException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public PostHttpException(Exception inner)
        {
            Erro = inner;
        }


        private PostHttpException(String NomeFuncionario)
        {
        }

        public override string Message
        {
            get
            {
                return String.Format("Erro com a comunicação com o servidor", "\n" + Erro.Message);
            }
        }
    }
}
