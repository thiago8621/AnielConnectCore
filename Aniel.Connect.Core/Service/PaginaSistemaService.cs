using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sinapse.Cache.Caching;
using Sinapse.Core;

namespace Aniel.MGTEC.Core
{
    public class PaginaSistemaService
    {
        private const string chavePaginas = "PAGINAS";
        private const string categoria = "SISTEMA";

        public static void ClearPaginasSistema()
        {
            ObjManager.ClearObject(chavePaginas, categoria);
        }

        public static void AddPaginasSistema(Assembly pAssembly)
        {
            List<PaginaSistema> lista = GetPaginasSistema().ToList();

            foreach (Type controle in pAssembly.GetTypes().ToList().FindAll(p => p.GetInterface(typeof(ITemPagina).ToString()) != null))
            {
                ITemPagina _controle = (ITemPagina)controle.GetConstructor(Type.EmptyTypes).Invoke(null);

                lista.AddRange(_controle.GetPaginasSistema());
            }

            ObjManager.AddObject(lista, chavePaginas, categoria);
        }

        public static IList<PaginaSistema> GetPaginasSistema()
        {
            IList<PaginaSistema> retorno = (IList<PaginaSistema>)ObjManager.GetObject(chavePaginas, categoria);

            if (retorno == null)
                return new List<PaginaSistema>();
            else
                return retorno.ToList();
        }

        public static List<PaginaSistema> GetPaginasSistemaComAcesso()
        {
            IList<PaginaSistema> retorno = GetPaginasSistema();
            return retorno.ToList().FindAll(p => p.ControlaAcesso == true);
        }

        public static IList<PaginaSistemaRecurso> GetRecursosSistema(SistemaWeb pSistema, Boolean pControlaAcesso)
        {
            IList<PaginaSistema> paginas = (IList<PaginaSistema>)ObjManager.GetObject(chavePaginas, categoria);

            if (paginas == null)
                return new List<PaginaSistemaRecurso>();
            else
            {
                List<PaginaSistemaRecurso> retorno = new List<PaginaSistemaRecurso>();

                foreach (PaginaSistema pagina in paginas)
                {
                    if (pagina.Sistema == pSistema)
                    {
                        if ((pControlaAcesso == true && pagina.ControlaAcesso == true) || pControlaAcesso==false)
                            retorno.AddRange(pagina.Recursos);
                    }
                }
                return retorno;
            }
        }


        //public static PaginaSistema GetPaginaSistema(Int64 pId)
        //{
        //    return ((IList<PaginaSistema>)ObjManager.GetObject(chavePaginas, categoria)).ToList().Find(delegate(PaginaSistema o) { return o.Id == pId; });
        //}

        public static PaginaSistema GetPaginaSistema(SistemaWeb pSistemaWeb, String pControle, Int64 pId)
        {
            IList<PaginaSistema> paginas = ((IList<PaginaSistema>)ObjManager.GetObject(chavePaginas, categoria));

            return paginas.ToList().Find(o => o.Sistema == pSistemaWeb && o.Id == pId && o.Controle == pControle);
        }

        public static IList<PaginaSistema> GetPaginaSistema(SistemaWeb pSistemaWeb, String pControle, String pAction)
        {
            return ((IList<PaginaSistema>)ObjManager.GetObject(chavePaginas, categoria)).ToList().FindAll(o => o.Sistema.Equals(pSistemaWeb) && o.Controle.Equals(pControle) && o.Controle.Equals(pAction) );
        }

    }
}
