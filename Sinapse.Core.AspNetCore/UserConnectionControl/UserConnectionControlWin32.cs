using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Sinapse.Cache.AspNetCore.Caching;

namespace Sinapse.Core.AspNetCore
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

        private readonly HttpContext _httpContext;

        public UserConnectionControlWin32(HttpContext httpContext)
        {
            this._httpContext = httpContext;
        }

        #region IUserConnectionControl Members

        public void SetAcessoUsuario(Object objeto)
        {
            ObjManager.ClearObject(this._httpContext, chavePerfilAcesso, categoria);

            ObjManager.AddObject(this._httpContext, objeto, chavePerfilAcesso, categoria);
        }

        public Object GetAcessoUsuario()
        {
            return ObjManager.GetObject(this._httpContext, chavePerfilAcesso, categoria);
        }


        public Object GetMenu()
        {
            return ObjManager.GetObject(this._httpContext, chaveMENU, categoria);
        }

        public void SetMenu(Object objeto)
        {
            ObjManager.ClearObject(this._httpContext, chaveMENU, categoria);

            ObjManager.AddObject(this._httpContext, objeto, chaveMENU, categoria);
        }

        
        #endregion

    }
}
