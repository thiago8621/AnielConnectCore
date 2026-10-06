using NHibernate;
using Sinapse.DAO.repository;
using Sinapse.Model.enumerados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Aniel.Connect.Core.Service
{
    public static class DashBoardService
    {
        public static async Task<DashboardInicialConnect> GetPaginaInicial(String Usuario)
        {

            using (ISession session = HibernateUtil.GetSession())
            {
                Dictionary<String, Object> parametros = new Dictionary<string, object>();

                //Select
                using (var com = session.Connection.CreateCommand())
                {

                    StringBuilder sql = new StringBuilder()
                    .AppendLine(" SELECT B.Dashboard_Inicial, B.DASHBOARD_GERENCIAL ")
                    .AppendLine(" FROM ASPNETUSERS A ")
                    .AppendLine(" INNER JOIN TB_PERFIL_DASHBOARD_CONNECT B ")
                    .AppendLine(" ON(A.ID_PERFIL_DASH_CONNECT = B.ID AND B.SISTEMA = 1) ")
                    .AppendLine(" WHERE A.ID = @USUARIO ");

                    parametros.Add("@USUARIO", Usuario);
                    

                    //Busca com paginacao
                    using (var reader = await CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                    {
                        if (await reader.ReadAsync())
                        {
                            return (DashboardInicialConnect)Enum.Parse(typeof(DashboardInicialConnect), Convert.ToString(reader["Dashboard_Inicial"]));
                        }
                        else
                        {
                            return DashboardInicialConnect.AtendimentoCliente;
                        }

                    }

                }
            }
        }

        public static async Task<DashBoard_Gerencial> GetPaginaGerencial(String Usuario)
        {

            using (ISession session = HibernateUtil.GetSession())
            {
                Dictionary<String, Object> parametros = new Dictionary<string, object>();

                //Select
                using (var com = session.Connection.CreateCommand())
                {

                    StringBuilder sql = new StringBuilder()
                    .AppendLine(" SELECT B.DASHBOARD_GERENCIAL ")
                    .AppendLine(" FROM ASPNETUSERS A ")
                    .AppendLine(" INNER JOIN TB_PERFIL_DASHBOARD_CONNECT B ")
                    .AppendLine(" ON(A.ID_PERFIL_DASH_CONNECT = B.ID) ")
                    .AppendLine(" WHERE A.ID = @USUARIO ");

                    parametros.Add("@USUARIO", Usuario);

                    //Busca com paginacao
                    using (var reader = await CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                    {
                        if (await reader.ReadAsync())
                        {
                            return (DashBoard_Gerencial)Enum.Parse(typeof(DashBoard_Gerencial), Convert.ToString(reader["DASHBOARD_GERENCIAL"]));
                        }
                        else
                        {
                            return DashBoard_Gerencial.Gestao_Atendimento;
                        }

                    }

                }
            }
        }

        public static async Task<List<Int32>> GetPaginaRecursosInicial(String Usuario)
        {

            using (ISession session = HibernateUtil.GetSession())
            {
                Dictionary<String, Object> parametros = new Dictionary<string, object>();

                //Select
                using (var com = session.Connection.CreateCommand())
                {

                    StringBuilder sql = new StringBuilder()
                    .AppendLine(" SELECT c.RECURSO ")
                    .AppendLine(" FROM ASPNETUSERS A ")
                    .AppendLine(" LEFT JOIN TB_PERFIL_DASHBOARD_CONNECT B ")
                    .AppendLine(" ON(A.ID_PERFIL_DASH_CONNECT = B.ID) ")
                    .AppendLine(" LEFT JOIN TB_DETALHES_DASH_INICIAL C ")
                    .AppendLine(" ON(B.ID = C.ID_PERFIL_DASHBOARD_CONNECT) ")
                    .AppendLine(" WHERE A.ID = @USUARIO ");

                    parametros.Add("@USUARIO", Usuario);

                    //Busca com paginacao
                    using (var reader = await CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                    {
                        List<Int32> retorno = new List<Int32>();
                        while (await reader.ReadAsync())
                        {
                            if (reader["RECURSO"] != DBNull.Value)
                                retorno.Add(Convert.ToInt32(reader["RECURSO"]));
                        }
                        return retorno;
                    }

                }
            }
        }

        public static async Task<List<Int32>> GetPaginaRecursosGerencial(String Usuario)
        {

            using (ISession session = HibernateUtil.GetSession())
            {
                Dictionary<String, Object> parametros = new Dictionary<string, object>();

                //Select
                using (var com = session.Connection.CreateCommand())
                {

                    StringBuilder sql = new StringBuilder()
                    .AppendLine(" SELECT B.RECURSO ")
                    .AppendLine(" FROM ASPNETUSERS A ")
                    .AppendLine(" LEFT JOIN TB_DETALHES_DASH_GERENCIAL B ")
                    .AppendLine(" ON(A.ID_PERFIL_DASH_CONNECT = B.ID) ")
                    .AppendLine(" WHERE A.ID = @USUARIO ");

                    parametros.Add("@USUARIO", Usuario);


                    //Busca com paginacao
                    using (var reader = await CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                    {
                        List<Int32> retorno = new List<Int32>();
                        while (await reader.ReadAsync())
                        {
                            if (reader["RECURSO"] != DBNull.Value)
                                retorno.Add(Convert.ToInt32(reader["RECURSO"]));
                        }
                        return retorno;
                    }

                }
            }
        }
    }
}
