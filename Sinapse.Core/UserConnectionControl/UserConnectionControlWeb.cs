using System;
using System.Collections.Generic;
using System.Linq;
using Sinapse.Cache.Caching;

namespace Sinapse.Core
{
    public class UserConnectionControlWeb : IUserConnectionControl
    {
        private const string chavePerfilAcesso = "PERFILUSUARIO";
        private const string chaveMENU = "MENU";
        private const string categoria = "ACESSO";


        #region IUserConnectionControl Members

        public void SetAcessoUsuario(Object objeto)
        {
            ObjManager.ClearObject(categoria, chavePerfilAcesso);

            ObjManager.AddObject(objeto, categoria, chavePerfilAcesso);
        }

        public Object GetAcessoUsuario()
        {
            return ObjManager.GetObject(categoria, chavePerfilAcesso);
        }

        public Object GetMenu()
        {
            return ObjManager.GetObject(categoria, chaveMENU);
        }

        public void SetMenu(Object objeto)
        {
            ObjManager.ClearObject(categoria, chaveMENU);

            ObjManager.AddObject(objeto, categoria, chaveMENU);
        }

        

        #endregion


        
    }
}
