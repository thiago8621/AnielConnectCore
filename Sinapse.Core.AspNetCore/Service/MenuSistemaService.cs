using Microsoft.AspNetCore.Http;
using Sinapse.Cache.AspNetCore.Caching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core.AspNetCore
{
    public class MenuSistemaService
    {
        private const string chaveMenus = "PAGINAS";
        private const string categoria = "MENU";

        public static void ClearMenusSistema(HttpContext httpContext)
        {
            ObjManager.ClearObject(httpContext, chaveMenus, categoria);
        }

        public static void AddMenusSistema(HttpContext httpContext, List<MenuSistema> lista)
        {
            ObjManager.AddObject(httpContext, lista, chaveMenus, categoria);
        }

        public static List<MenuSistema> GetMenusSistema(HttpContext httpContext)
        {
            IList<MenuSistema> retorno = (IList<MenuSistema>)ObjManager.GetObject(httpContext, chaveMenus, categoria);

            if (retorno == null)
                return new List<MenuSistema>();
            else
                return retorno.OrderBy(p => p.ordem).ToList();
        }

        public static List<MenuSistema> GetMenusSistemaComAcesso(HttpContext httpContext)
        {
            IList<MenuSistema> retorno = GetMenusSistema(httpContext);
            return retorno.ToList().FindAll(p => p.areaModulo != null);
        }

        public static MenuSistema GetMenuSistema(HttpContext httpContext, String pId)
        {
            List<MenuSistema> lista = ((IList<MenuSistema>)ObjManager.GetObject(httpContext, chaveMenus, categoria)).ToList();

            MenuSistema menu = lista.Find(o => o.id == pId);

            return menu;
        }

        public static MenuSistema GetMenuSistema(HttpContext httpContext, Int32 pSistemaWeb, String pId)
        {
            IList<MenuSistema> menus = ((IList<MenuSistema>)ObjManager.GetObject(httpContext, chaveMenus, categoria));

            return menus.ToList().Find(o => o.sistema == ((SistemaWeb)Enum.Parse(typeof(SistemaWeb), pSistemaWeb.ToString())) && o.id == pId);
        }

        //public static IList<MenuSistema> GetMenuSistema(String pUrl)
        //{
        //    return ((IList<MenuSistema>)ObjManager.GetObject(chaveMenus, categoria)).ToList().FindAll(o => o.url.Equals(pUrl));
        //}

    }
}
