using Aniel.Connect.Core;
using Aniel.Connect.Core.Models;
using Aniel.Connect.Core.Swagger;
using Aniel.Connect.Models.T5;
using NHibernate;
using Sinapse.DAO;
using Sinapse.DAO.repository;
using Sinapse.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Aniel.Connect.ControllersWebApi
{
    [HideInDocs]
    public class AcessoApiController : ApiController
    {


        [HttpGet()]
        [Route("api/AcessoApi/GetArea_ModuloUsuario")]
        public async Task<GetApiRetorno<List<AreaModuloModel>>> GetArea_ModuloUsuario(String id_usuario)
        {
            GetApiRetorno<List<AreaModuloModel>> retorno = new GetApiRetorno<List<AreaModuloModel>>();

            try
            {
                using (ISession session = HibernateUtil.GetSessionT5())
                {
                    StringBuilder sql = new StringBuilder()

                    .AppendLine("SELECT X.SIGLA_MODULO, X.COD_AREA, X.DESCRICAO, X.SUB_ACESSO ")
                    .AppendLine("FROM( ")

                    .AppendLine("SELECT B.SIGLA_MODULO, B.COD_AREA, B.DESCRICAO, B.SUB_ACESSO ")
                    .AppendLine("  FROM TB_PERFIL_DE_USUARIO_ACESSO A ")
                    .AppendLine(" INNER JOIN TB_AREA_MODULO B ")
                    .AppendLine("    ON(B.SIGLA_MODULO = A.SIGLA_MODULO AND B.COD_AREA = A.COD_AREA) ")
                    .AppendLine(" INNER JOIN TB_USUARIO_PERFIL C ")
                    .AppendLine("    ON(C.COD_PERFIL_USUARIO = A.COD_PERFIL_USUARIO) ")
                    .AppendLine(" WHERE A.SIGLA_MODULO = :PSIGLA_MODULO AND C.USUARIO = :PUSUARIO ")
                    .AppendLine(" GROUP BY B.SIGLA_MODULO, B.COD_AREA, B.DESCRICAO, B.SUB_ACESSO ")

                    .AppendLine(" UNION ")

                    .AppendLine("SELECT B.SIGLA_MODULO, B.COD_AREA, B.DESCRICAO, B.SUB_ACESSO ")
                    .AppendLine("  FROM TB_ACESSO A ")
                    .AppendLine(" INNER JOIN TB_AREA_MODULO B ")
                    .AppendLine("    ON(B.SIGLA_MODULO = A.SIGLA_MODULO AND B.COD_AREA = A.COD_AREA) ")
                    .AppendLine(" WHERE A.SIGLA_MODULO =:PSIGLA_MODULO AND A.USUARIO =:PUSUARIO ")
                    .AppendLine(" GROUP BY B.SIGLA_MODULO, B.COD_AREA, B.DESCRICAO, B.SUB_ACESSO) X ")

                    .AppendLine(" GROUP BY X.SIGLA_MODULO, X.COD_AREA, X.DESCRICAO, X.SUB_ACESSO");

                    List<Hashtable> retonoBD = null;
                    using (Query query = new Query(session))
                    {
                        query.setParametro("PSIGLA_MODULO", CONSTANTES.SIGLA_MODULO);
                        query.setParametro("PUSUARIO", id_usuario);
                        retonoBD = await query.Values(sql.ToString());
                    }

                    retorno.Objeto = retonoBD.Select(p => new AreaModuloModel
                    {
                        Cod_Area = Convert.ToString(p["COD_AREA"]),
                        Descricao = Convert.ToString(p["DESCRICAO"]),
                        Sigla_Modulo = Convert.ToString(p["SIGLA_MODULO"]),
                        Sub_Acesso = Convert.ToString(p["SUB_ACESSO"])
                    }).ToList();

                    retorno.Ok = true;
                }
            }
            catch (Exception ex)
            {
                retorno.Objeto = new List<AreaModuloModel>();
                retorno.Mensagem = ex.Message;
                if (ex.InnerException != null) retorno.InnerException = ex.InnerException.Message;
                CONSTANTES.log4Net.Error(ex);
            }

            return retorno;

        }

        [HttpGet()]
        [Route("api/AcessoApi/GetModulosUsuario")]
        public async Task<GetApiRetorno<List<String>>> GetModulosUsuario(String id_usuario)
        {
            GetApiRetorno<List<String>> retorno = new GetApiRetorno<List<String>>();

            try
            {
                using (ISession session = HibernateUtil.GetSession())
                {
                    StringBuilder sql = new StringBuilder()

                    .AppendLine("SELECT X.SIGLA_MODULO ")
                    .AppendLine("FROM( ")

                    .AppendLine("SELECT B.SIGLA_MODULO ")
                    .AppendLine("  FROM TB_PERFIL_DE_USUARIO_ACESSO A ")
                    .AppendLine(" INNER JOIN TB_AREA_MODULO B ")
                    .AppendLine("    ON(B.SIGLA_MODULO = A.SIGLA_MODULO AND B.COD_AREA = A.COD_AREA) ")
                    .AppendLine(" INNER JOIN TB_USUARIO_PERFIL C ")
                    .AppendLine("    ON(C.COD_PERFIL_USUARIO = A.COD_PERFIL_USUARIO) ")
                    .AppendLine(" WHERE C.USUARIO = :PUSUARIO ")
                    .AppendLine(" GROUP BY B.SIGLA_MODULO ")

                    .AppendLine(" UNION ")

                    .AppendLine("SELECT B.SIGLA_MODULO ")
                    .AppendLine("  FROM TB_ACESSO A ")
                    .AppendLine(" INNER JOIN TB_AREA_MODULO B ")
                    .AppendLine("    ON(B.SIGLA_MODULO = A.SIGLA_MODULO AND B.COD_AREA = A.COD_AREA) ")
                    .AppendLine(" WHERE A.USUARIO =:PUSUARIO ")
                    .AppendLine(" GROUP BY B.SIGLA_MODULO) X ")

                    .AppendLine(" GROUP BY X.SIGLA_MODULO");

                    List<Hashtable> retonoBD = null;
                    using (Query query = new Query(session))
                    {
                        query.setParametro("PUSUARIO", id_usuario);
                        retonoBD = await query.Values(sql.ToString());
                    }

                    retorno.Objeto = retonoBD.Select(p => Convert.ToString(p["SIGLA_MODULO"])).ToList();

                    retorno.Ok = true;
                }
            }
            catch (Exception ex)
            {
                retorno.Objeto = new List<String>();
                retorno.Mensagem = ex.Message;
                if (ex.InnerException != null) retorno.InnerException = ex.InnerException.Message;
                CONSTANTES.log4Net.Error(ex);
            }

            return retorno;

        }

        [HttpPost()]
        [Route("api/AcessoApi/GetFilialUsuario")]
        public async Task<FilialUsuarioModel> GetFilialUsuario(String id_usuario)
        {
            FilialUsuarioModel retorno = new FilialUsuarioModel();

            using (ISession session = HibernateUtil.GetSession())
            {
                try
                {
                    //Filtros 
                    Dictionary<String, Object> parametros = new Dictionary<String, Object>();
                    parametros.Add("@ID", id_usuario);

                    //Select
                    using (var com = session.Connection.CreateCommand())
                    {
                        StringBuilder sql = new StringBuilder("SELECT C.ID AS ID_FILIAL, C.NOME AS NOME_FILIAL, D.ID AS ID_EMPRESA, D.NOME AS NOME_EMPRESA")
                                    .AppendLine(" FROM ASPNETUSERS A")
                                    .AppendLine(" INNER JOIN TB_FUNCIONARIO B")
                                    .AppendLine(" ON(A.ID_FUNCIONARIO = B.ID)")
                                    .AppendLine(" INNER JOIN TB_FILIAL C")
                                    .AppendLine(" ON(B.ID_FILIAL = C.ID)")
                                    .AppendLine(" INNER JOIN TB_EMPRESA D")
                                    .AppendLine(" ON(C.ID_EMPRESA = D.ID)")
                                    .AppendLine(" WHERE A.ID = @ID");
                        

                        using (var reader = await CommandHelper.ExecuteReaderPage(com, sql, parametros, session.Connection, null, 1))
                        {
                            if (await reader.ReadAsync())
                            {
                                retorno.Id_Filial = Convert.ToInt64(reader["ID_FILIAL"]);
                                retorno.Nome_Filial = Convert.ToString(reader["NOME_FILIAL"]);
                                retorno.Id_Empresa = Convert.ToInt64(reader["ID_EMPRESA"]);
                                retorno.Nome_Empresa = Convert.ToString(reader["NOME_EMPRESA"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return retorno;

        }

        [HttpPost()]
        [Route("api/AcessoApi/GetComboModulo")]
        public async Task<ObjCombo<String>> GetComboModulo(RequestObjCombo<String> request)
        {
            ObjCombo<String> retorno = new ObjCombo<String>
            {
                results = new List<IObjComboResult<String>>()
            };

            using (ISession session = HibernateUtil.GetSessionT5())
            {
                try
                {
                    //Filtros 
                    Dictionary<String, Object> parametros = new Dictionary<string, object>();

                    StringBuilder sqlFrom = new StringBuilder(" FROM TB_MODULO A ");
                    List<String> condicoes = new List<String>();



                    if (!String.IsNullOrEmpty(request.id))
                    {
                        condicoes.Add($"A.SIGLA_MODULO = {request.id}");
                    }

                    if (request != null && !String.IsNullOrEmpty(request.q))
                    {
                        condicoes.Add($"( UPPER(A.SIGLA_MODULO) LIKE '%{StringService.Truncate(request.q.ToUpperInvariant(), 60)}%')");
                    }

                    if (condicoes.Count() > 0)
                    {
                        sqlFrom.AppendLine($"WHERE ( {String.Join(" AND ", condicoes) } ) ");
                    }

                    using (var com = session.Connection.CreateCommand())
                    {
                        StringBuilder sql = new StringBuilder("SELECT COUNT(A.SIGLA_MODULO) AS CO ");
                        sql.AppendLine(sqlFrom.ToString());

                        using (var reader = await CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                        {
                            if (await reader.ReadAsync())
                                retorno.total_count = Convert.ToInt32(reader["CO"]);
                        }
                    }

                    //Select
                    using (var com = session.Connection.CreateCommand())
                    {
                        StringBuilder sql = new StringBuilder("SELECT A.SIGLA_MODULO");
                        sql.AppendLine(sqlFrom.ToString());
                        sql.AppendLine("ORDER BY UPPER(A.SIGLA_MODULO)");

                        using (var reader = await CommandHelper.ExecuteReaderPage(com, sql, parametros, session.Connection, null, request.page))
                        {
                            while (await reader.ReadAsync())
                            {
                                var obj = new ObjComboResult<String>
                                {
                                    Id = Convert.ToString(reader["SIGLA_MODULO"]),
                                    Text = Convert.ToString(reader["SIGLA_MODULO"]),

                                };
                                retorno.results.Add(obj);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return retorno;

        }
    }
}
