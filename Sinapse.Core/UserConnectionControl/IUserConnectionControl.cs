using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    public interface IUserConnectionControl
    {
        Object GetAcessoUsuario();

        void SetAcessoUsuario(Object objeto);

        Object GetMenu();

        void SetMenu(Object objeto);

    }
}
