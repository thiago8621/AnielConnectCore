using System;

namespace Sinapse.Core.AspNetCore
{
    public interface IUserConnectionControl
    {
        Object GetAcessoUsuario();

        void SetAcessoUsuario(Object objeto);

        Object GetMenu();

        void SetMenu(Object objeto);

    }
}
