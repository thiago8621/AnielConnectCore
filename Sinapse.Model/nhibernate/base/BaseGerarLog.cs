using Sinapse.ModelLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
{
    /// <summary>
    ///     Base para classes do hibernate que geram log
    /// </summary>
    /// <typeparam name="TLog">Modelo da tabela de log</typeparam>
    /// <typeparam name="TId">Tipo do id da tabela</typeparam>
    public abstract class BaseGerarLog<TLog, TId> where TLog : LogGenerico<TId>, new()
    {
        public virtual void Log_ComparaProperty<T>(List<TLog> logs, String pUsuario,String pNomeUsuario, TipoOperLog pTipoOperLog, String pChave, TId id, String pDescricaoCampo, T pOld, T pNew, Expression<Func<T, object>> pExpression)
        {
            MemberExpression memberExpr = null;
            
            if (pExpression.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr = ((UnaryExpression)pExpression.Body).Operand as MemberExpression;
            }
            else if (pExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = pExpression.Body as MemberExpression;
            }

            var prop = (PropertyInfo)memberExpr.Member;

            if (Convert.ToString(prop.GetValue(pOld)) != Convert.ToString(prop.GetValue(pNew)))
            {
                var log = new TLog();
                logs.Add((TLog)log.Bind(id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, Convert.ToString(prop.GetValue(pOld)), Convert.ToString(prop.GetValue(pNew)), pNomeUsuario));
            }
        }
    }
}
