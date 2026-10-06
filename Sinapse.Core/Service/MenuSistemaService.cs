using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sinapse.Cache.Caching;

namespace Sinapse.Core
{
    public class MenuSistemaService
    {
        private const string chaveMenus = "PAGINAS";
        private const string categoria = "MENU";

        public static void ClearMenusSistema()
        {
            ObjManager.ClearObject(chaveMenus, categoria);
        }

        public static void AddMenusSistema(List<MenuSistema> lista)
        {
            ObjManager.AddObject(lista, chaveMenus, categoria);
        }

        public static List<MenuSistema> GetMenusSistema()
        {
            IList<MenuSistema> retorno = (IList<MenuSistema>)ObjManager.GetObject(chaveMenus, categoria);

            if (retorno == null)
                return new List<MenuSistema>();
            else
                return retorno.OrderBy(p => p.ordem).ToList();
        }

        public static List<MenuSistema> GetMenusSistemaComAcesso()
        {
            IList<MenuSistema> retorno = GetMenusSistema();
            return retorno.ToList().FindAll(p => p.areaModulo != null);
        }

        public static MenuSistema GetMenuSistema(String pId)
        {
            List<MenuSistema> lista = ((IList<MenuSistema>)ObjManager.GetObject(chaveMenus, categoria)).ToList();

            MenuSistema menu = lista.Find(o => o.id == pId);

            return menu;
        }

        public static MenuSistema GetMenuSistema(Int32 pSistemaWeb, String pId)
        {
            IList<MenuSistema> menus = ((IList<MenuSistema>)ObjManager.GetObject(chaveMenus, categoria));

            return menus.ToList().Find(o => o.sistema == ((SistemaWeb)Enum.Parse(typeof(SistemaWeb), pSistemaWeb.ToString())) && o.id == pId);
        }

        //public static IList<MenuSistema> GetMenuSistema(String pUrl)
        //{
        //    return ((IList<MenuSistema>)ObjManager.GetObject(chaveMenus, categoria)).ToList().FindAll(o => o.url.Equals(pUrl));
        //}

    }
}
