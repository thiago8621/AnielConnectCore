using Sinapse.Core;
using Sinapse.Model.core;
using Sinapse.Model.Identity;
using Sinapse.Model.nhibernate;
using Sinapse.Service;
using Sinapse.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.MGTEC.Core
{
    public class AcessoUsuarioService
    {
        public static List<AcessosPerfil> RegistraAcessoUsuario(String UserName)
        {
            //return Task.Run(() =>
            //{
            IUserConnectionControl usercc = UserConnectionFactory.GetConnectionControl(UserConnectionControlConfig.tipoConnection);

            PerfilAcessoUsuarioService repoAcessoUsuario = new PerfilAcessoUsuarioService();
            AcessosPerfilService repoAcessoPerfil = new AcessosPerfilService();

            ICollection<PerfilAcessoUsuario> listaPerfilUsuario = repoAcessoUsuario.FindAllByWhere(p => p.Usuario == new IdentityUserService().FindByWhere(u => u.UserName == UserName));

            List<AcessosPerfil> retorno = new List<AcessosPerfil>();

            foreach (PerfilAcessoUsuario perfilUsuario in listaPerfilUsuario)
            {
                ICollection<AcessosPerfil> acessos = repoAcessoPerfil.FindAllByWhere(p => p.PerfilAcesso.Id == perfilUsuario.PerfilAcesso.Id);

                foreach (AcessosPerfil acesso in acessos)
                {
                    if (retorno.Find(a => a.IdPagina == acesso.IdPagina && a.Controle == acesso.Controle) == null)
                    {
                        acesso.PaginaSistema = PaginaSistemaService.GetPaginaSistema(UserConnectionControlConfig.sistema, acesso.Controle, acesso.IdPagina);
                        retorno.Add(acesso);
                    }
                }
            }

            usercc.SetAcessoUsuario(retorno);

            RegistraMenuUsuario(retorno, usercc);

            return retorno;

            //});
        }

        public static void RegistraMenuUsuario(List<AcessosPerfil> acessos, IUserConnectionControl pUsercc)
        {
            IList<MenuSistema> menus = MenuSistemaService.GetMenusSistema().FindAll(m => m.MenuPai == null);

            foreach (MenuSistema menu in menus)
            {
                foreach (MenuSistema subMenu in menu.MenusFilhos)
                {
                    if (subMenu.PaginaSistema != null)
                    {
                        if (acessos.Find(a => a.IdPagina == subMenu.PaginaSistema.Id) == null)
                        {
                            subMenu.Visible = false;
                        }
                        else
                        {
                            subMenu.Visible = true;
                        }
                    }
                    foreach (MenuSistema menuInterno in subMenu.MenusFilhos)
                    {
                        if (!menuInterno.CriticaAcesso)
                            continue;

                        if (menuInterno.PaginaSistema == null)
                            continue;
                        
                        if (acessos.Find(a => a.IdPagina == menuInterno.PaginaSistema.Id && a.Controle == menuInterno.Controle) == null)
                        {
                            menuInterno.Visible = false;
                        }
                        else
                        {
                            menuInterno.Visible = true;
                        }
                    }
                    if (subMenu.MenusFilhos.FindAll(p => p.Visible == true).Count == 0)
                    {
                        subMenu.Visible = false;
                    }
                    else
                    {
                        subMenu.Visible = true;
                    }
                }

            }
            pUsercc.SetMenu(menus);
        }

        public static Boolean TemAcesso(String pControle, List<PaginaSistema> pPaginas, Recurso_Pagina pRecurso, String pDescricao = "DescOutros")
        {
            IUserConnectionControl usercc = UserConnectionFactory.GetConnectionControl(UserConnectionControlConfig.tipoConnection);

            PaginaSistema pagina;
            
            if (pRecurso == Recurso_Pagina.Outro)
                pagina = pPaginas.Find(p => p.Recursos.Find(r => r.Recurso_Pagina == pRecurso && r.Descricao == pDescricao) != null);
            else
                pagina = pPaginas.Find(p => p.Recursos.Find(r => r.Recurso_Pagina == pRecurso) != null);

            List<AcessosPerfil> acessosUsuario = ((List<AcessosPerfil>)usercc.GetAcessoUsuario()).FindAll(p => p.Controle == pControle && p.IdPagina == pagina.Id);

            foreach (AcessosPerfil acesso in acessosUsuario)
            {
                if (acesso.TodosRecursos)
                    return true;
                else
                    return acesso.Recursos.Find(p => p.Recurso_Pagina == pRecurso || p.Descricao == pDescricao) != null;
            }

            return false;
        }

        public static List<String> GetCamposBloqueados(String pControle, List<PaginaSistema> pPaginas, Recurso_Pagina pRecurso, String pDescricao = "DescOutros")
        {
            IUserConnectionControl usercc = UserConnectionFactory.GetConnectionControl(UserConnectionControlConfig.tipoConnection);

            PaginaSistema pagina;

            if (pRecurso == Recurso_Pagina.Outro)
                pagina = pPaginas.Find(p => p.Recursos.Find(r => r.Recurso_Pagina == pRecurso && r.Descricao == pDescricao) != null);
            else
                pagina = pPaginas.Find(p => p.Recursos.Find(r => r.Recurso_Pagina == pRecurso) != null);

            List<AcessosPerfil> acessosUsuario = ((List<AcessosPerfil>)usercc.GetAcessoUsuario()).FindAll(p => p.Controle == pControle && p.IdPagina == pagina.Id);

            foreach (AcessosPerfil acesso in acessosUsuario)
            {
                if (String.IsNullOrEmpty(acesso.CamposBloqueadosJSON))
                    return new List<String>();
                else
                    return JsonService.DeserializeJasonMapper<List<String>>(acesso.CamposBloqueadosJSON);
            }

            return new List<String>();
        }

        public static Boolean CampoBloqueado(List<String> pCamposBloqueados, String pCampo)
        {
            if (pCamposBloqueados.Count == 0)
                return false;

            return (pCamposBloqueados.Find(p => p == pCampo) != null);
        }
    }
}
