using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Sinapse.Cache.AspNetCore.Caching;

namespace Sinapse.Core.AspNetCore
{
    public class UserConnectionControlWeb : IUserConnectionControl
    {
        private const string chavePerfilAcesso = "PERFILUSUARIO";
        private const string chaveMENU = "MENU";
        private const string categoria = "ACESSO";

        private readonly HttpContext _httpContext;

        public UserConnectionControlWeb(HttpContext httpContext)
        {
            this._httpContext = httpContext;
        }

        #region IUserConnectionControl Members

        public void SetAcessoUsuario(Object objeto)
        {
            ObjManager.ClearObject(this._httpContext, categoria, chavePerfilAcesso);

            ObjManager.AddObject(this._httpContext, objeto, categoria, chavePerfilAcesso);
        }

        public Object GetAcessoUsuario()
        {
            return ObjManager.GetObject(this._httpContext, categoria, chavePerfilAcesso);
        }

        public Object GetMenu()
        {
            return ObjManager.GetObject(this._httpContext, categoria, chaveMENU);
        }

        public void SetMenu(Object objeto)
        {
            ObjManager.ClearObject(this._httpContext, categoria, chaveMENU);

            ObjManager.AddObject(this._httpContext, objeto, categoria, chaveMENU);
        }

        

        #endregion


        
    }
}
