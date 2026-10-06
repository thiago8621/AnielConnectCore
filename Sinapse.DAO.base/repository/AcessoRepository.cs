using NHibernate;
using Sinapse.DAO.repository;
using Sinapse.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.repository
{
    public class AcessoRepository : GenericRepository<Acesso, Int64>
    {
        public AcessoRepository(ISession Session, ISession SessionLog) : base(Session, SessionLog)
        {

        }

        public async Task<Boolean> TemAcesso(String Sigla_Modulo, String CodArea, Int64 Id_Usuario)
        {
            

        }

        public async Task<List<Area_Modulo>> GetArea_ModuloUsuario(String Sigla_Modulo, String Id_Usuario)
        {
            List<Acesso> retorno = new List<Acesso>();

            StringBuilder sql = new StringBuilder();

            sql.AppendLine("SELECT X.ID, X.SIGLA_MODULO, X.COD_AREA, X.DESCRICAO, X.SUB_ACESSO");
            sql.AppendLine("FROM(");
            sql.AppendLine("SELECT B.ID, B.SIGLA_MODULO, B.COD_AREA, B.DESCRICAO, B.SUB_ACESSO");
            sql.AppendLine("FROM TB_PERFIL_USUARIO_ACESSO A");
            sql.AppendLine("INNER JOIN TB_AREA_MODULO B");
            sql.AppendLine("ON(B.ID = A.ID_AREA_MODULO)");
            sql.AppendLine("INNER JOIN TB_USUARIO_PERFIL C");
            sql.AppendLine("ON(C.ID_PERFIL_USUARIO = A.ID_PERFIL_USUARIO)");
            sql.AppendLine("WHERE B.SIGLA_MODULO = :PSIGLA_MODULO AND C.ID_USUARIO = :PID_USUARIO");

            sql.AppendLine("UNION");

            sql.AppendLine("SELECT B.ID, B.SIGLA_MODULO, B.COD_AREA, B.DESCRICAO, B.SUB_ACESSO");
            sql.AppendLine("FROM TB_ACESSO A");
            sql.AppendLine("INNER JOIN TB_AREA_MODULO B");
            sql.AppendLine("ON(B.ID = A.ID_AREA_MODULO)");
            sql.AppendLine("WHERE B.SIGLA_MODULO = :PSIGLA_MODULO AND A.ID_USUARIO = :PID_USUARIO ) X");
            sql.AppendLine("GROUP BY X.ID, X.SIGLA_MODULO, X.COD_AREA, X.DESCRICAO, X.SUB_ACESSO");

            List<Hashtable> retonoBD = null;
            using (Query query = new Query(session))
            {
                query.setParametro("PSIGLA_MODULO", Sigla_Modulo);
                query.setParametro("PID_USUARIO", Id_Usuario);
                retonoBD = await query.Values(sql.ToString());
            }

            return retonoBD.Select(p => new Area_Modulo
            {
                Id = Convert.ToInt64(p["ID"]),
                Cod_Area = Convert.ToString(p["COD_AREA"]),
                Descricao = Convert.ToString(p["DESCRICAO"]),
                Sigla_Modulo = Convert.ToString(p["SIGLA_MODULO"]),
                Sub_Acesso = Convert.ToString(p["SUB_ACESSO"])
            }).ToList();
            
        }


    }
}
