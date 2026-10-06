using Sinapse.DAO;
using Sinapse.DAO.repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Aniel.Connect.Core.Attributes
{

    public class OlaAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            try
            {
                if (actionContext.Request.Headers.Contains("OLA-TOKEN") == false)
                {
                    HandleUnauthorizedRequest(actionContext);
                }
                else
                {
                    IEnumerable<string> headerValues = actionContext.Request.Headers.GetValues("OLA-TOKEN");
                    var token = headerValues.FirstOrDefault();
                    if (String.IsNullOrEmpty(token) == false)
                    {
                        var tokenOla = GetTokenOla();
                        if (tokenOla != token)
                        {
                            HandleUnauthorizedRequest(actionContext);
                        }
                    }
                    else
                    {
                        HandleUnauthorizedRequest(actionContext);
                    }
                }
            }
            catch
            {
                HandleUnauthorizedRequest(actionContext);
            }
        }

        protected override void HandleUnauthorizedRequest(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        private string GetTokenOla()
        {
            using (var session = HibernateUtil.GetSession())
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<String, Object> parametros = new Dictionary<string, object>();
                sql.AppendLine("SELECT TOKEN_OLA FROM TB_CONFIGURACOES_GERAIS");
                using (var reader = CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null).Result)
                {
                    if (reader.Read())
                    {
                        return Convert.ToString(reader["TOKEN_OLA"]);
                    }
                }
            }

            return null;
        }


    }



}
