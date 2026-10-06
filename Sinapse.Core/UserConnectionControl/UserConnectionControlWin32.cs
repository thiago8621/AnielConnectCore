using System;
using System.Collections.Generic;
using System.Linq;
using Sinapse.Cache.Caching;

namespace Sinapse.Core
{
    public class UserConnectionControlWin32: IUserConnectionControl
    {
        private const string chaveUsuario = "USUARIO";
        private const string chavePerfilAcesso = "PERFILUSUARIO";
        private const string chaveMENU = "MENU";
        private const string categoria = "ACESSO";

        private const string chavePais = "PAIS";
        private const string categoriaSistema = "SISTEMA";

        private const string chaveEmpresa = "EMPRESA";
        private const string categoriaEmpresa = "EMPRESA";


        #region IUserConnectionControl Members

        public void SetAcessoUsuario(Object objeto)
        {
            ObjManager.ClearObject(chavePerfilAcesso, categoria);

            ObjManager.AddObject(objeto, chavePerfilAcesso, categoria);
        }

        public Object GetAcessoUsuario()
        {
            return ObjManager.GetObject(chavePerfilAcesso, categoria);
        }


        public Object GetMenu()
        {
            return ObjManager.GetObject(chaveMENU, categoria);
        }

        public void SetMenu(Object objeto)
        {
            ObjManager.ClearObject(chaveMENU, categoria);

            ObjManager.AddObject(objeto, chaveMENU, categoria);
        }

        
        #endregion

    }
}
