using NHibernate.Classic;
using Resources;
using Sinapse.ModelLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace Sinapse.Model.nhibernate
{
    public class BaseModel : IModel, ILifecycle
    {
        public virtual Int64 Id { get; set; }

        public virtual String UsuarioCriacao { get; set; }

        public virtual DateTime? DataHoraCriacao { get; set; }

        public virtual String UsuarioAlteracao { get; set; }

        public virtual DateTime? DataHoraAlteracao { get; set; }

        public virtual String Token { get; set; }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public virtual LifecycleVeto OnDelete(NHibernate.ISession s)
        {
            return LifecycleVeto.NoVeto;
        }

        public virtual void OnLoad(NHibernate.ISession s, object id)
        {

        }

        public virtual LifecycleVeto OnSave(NHibernate.ISession s)
        {
            DataHoraCriacao = DateTime.Now;

            return LifecycleVeto.NoVeto;
        }

        public virtual LifecycleVeto OnUpdate(NHibernate.ISession s)
        {
            DataHoraAlteracao = DateTime.Now;

            return LifecycleVeto.NoVeto;
        }

        public virtual void Log_ComparaProperty<T, TLog>(List<TLog> logs, String pUsuario, String pNomeUsuario, TipoOperLog pTipoOperLog, String pChave, String pDescricaoCampo, T pOld, T pNew, Expression<Func<T, object>> pExpression)
            where T : BaseModel
            where TLog : LogGenerico<Int64>, new()
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
            else if (pExpression.Body.NodeType == ExpressionType.Call)
            {

                //Não está funcionando... Ver como chama metodo sem saber os parametros
                var methodInfo = (pExpression.Body as MethodCallExpression).Method;

                var valOld = methodInfo.Invoke(pOld, null);
                var valNew = methodInfo.Invoke(pNew, null);

                if (Convert.ToString(valOld) != Convert.ToString(valNew))
                {
                    var log = new TLog();
                    logs.Add((TLog)log.Bind(pNew.Id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, Convert.ToString(valOld), Convert.ToString(valNew), pNomeUsuario));
                }
            }

            if (memberExpr != null)
            {
                var prop = (PropertyInfo)memberExpr.Member;

                var valOld = Convert.ToString(prop.GetValue(pOld));
                var valNew = Convert.ToString(prop.GetValue(pNew));

                if (prop.PropertyType.IsEnum)
                {
                    valOld = GetEnumDescription(prop.GetValue(pOld));
                    valNew = GetEnumDescription(prop.GetValue(pNew));
                }

                if (valOld != valNew)
                {
                    var log = new TLog();
                    logs.Add((TLog)log.Bind(pNew.Id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, Convert.ToString(prop.GetValue(pOld)), Convert.ToString(prop.GetValue(pNew)), pNomeUsuario));
                }
            }
        }

        public virtual void Log_ComparaProperty<T, TLog, TInner>(List<TLog> logs, String pUsuario, String pNomeUsuario, TipoOperLog pTipoOperLog, String pChave, String pDescricaoCampo, T pOld, T pNew, Expression<Func<T, BaseModel>> pExpression, Expression<Func<TInner, String>> yExpression)
            where T : BaseModel
            where TInner : BaseModel
            where TLog : LogGenerico<Int64>, new()
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
            else if (pExpression.Body.NodeType == ExpressionType.Call)
            {

                //Não está funcionando... Ver como chama metodo sem saber os parametros
                var methodInfo = (pExpression.Body as MethodCallExpression).Method;

                var valOld = methodInfo.Invoke(pOld, null);
                var valNew = methodInfo.Invoke(pNew, null);

                if (Convert.ToString(valOld) != Convert.ToString(valNew))
                {
                    var log = new TLog();
                    logs.Add((TLog)log.Bind(pNew.Id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, Convert.ToString(valOld), Convert.ToString(valNew), pNomeUsuario));
                }
            }

            if (memberExpr != null)
            {
                var prop = (PropertyInfo)memberExpr.Member;

                var valOld = ((TInner)prop.GetValue(pOld));
                var valNew = ((TInner)prop.GetValue(pNew));

                var valOldId = valOld != null ? valOld.Id.ToString() : null;
                var valNewId = valNew != null ? valNew.Id.ToString() : null;

                Func<TInner, string> expressionCompilada = yExpression.Compile();

                if (valOldId != valNewId)
                {
                    var log = new TLog();
                    logs.Add((TLog)log.Bind(pNew.Id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, valOld != null ? expressionCompilada(valOld) : null, valNew != null ? expressionCompilada(valNew) : null, pNomeUsuario));
                }
            }
        }

        public virtual void Log_ComparaPropertyTipado<T, TLog, TInner>(List<TLog> logs, String pUsuario, String pNomeUsuario, TipoOperLog pTipoOperLog, String pChave, String pDescricaoCampo, T pOld, T pNew, Expression<Func<T, TInner>> pExpression, Expression<Func<TInner, String>> yExpression)
            where T : BaseModel
            where TLog : LogGenerico<Int64>, new()
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
            else if (pExpression.Body.NodeType == ExpressionType.Call)
            {

                //Não está funcionando... Ver como chama metodo sem saber os parametros
                var methodInfo = (pExpression.Body as MethodCallExpression).Method;

                var valOld = methodInfo.Invoke(pOld, null);
                var valNew = methodInfo.Invoke(pNew, null);

                if (Convert.ToString(valOld) != Convert.ToString(valNew))
                {
                    var log = new TLog();
                    logs.Add((TLog)log.Bind(pNew.Id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, Convert.ToString(valOld), Convert.ToString(valNew), pNomeUsuario));
                }
            }

            if (memberExpr != null)
            {
                var prop = (PropertyInfo)memberExpr.Member;


                var valOld = ((TInner)prop.GetValue(pOld));
                var valNew = ((TInner)prop.GetValue(pNew));



                Func<TInner, string> expressionCompilada = yExpression.Compile();
                var valorCompiladoOld = valOld != null ? expressionCompilada(valOld) : null;
                var valorCompiladoNew = valNew != null ? expressionCompilada(valNew) : null;

                if (valorCompiladoOld != valorCompiladoNew)
                {
                    var log = new TLog();
                    logs.Add((TLog)log.Bind(pNew.Id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, valorCompiladoOld, valorCompiladoNew, pNomeUsuario));
                }
            }
        }

        public virtual void Log_ComparaProperty<T, TLog, TInner>(List<TLog> logs, String pUsuario, String pNomeUsuario, TipoOperLog pTipoOperLog, String pChave, String pDescricaoCampo, T pOld, T pNew, Expression<Func<T, object>> pExpression, Expression<Func<TInner, String>> yExpression, Expression<Func<TInner, object>> zExpression)
            where T : BaseModel
            where TLog : LogGenerico<Int64>, new()
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
            else if (pExpression.Body.NodeType == ExpressionType.Call)
            {

                //Não está funcionando... Ver como chama metodo sem saber os parametros
                var methodInfo = (pExpression.Body as MethodCallExpression).Method;

                var valOld = methodInfo.Invoke(pOld, null);
                var valNew = methodInfo.Invoke(pNew, null);

                if (Convert.ToString(valOld) != Convert.ToString(valNew))
                {
                    var log = new TLog();
                    logs.Add((TLog)log.Bind(pNew.Id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, Convert.ToString(valOld), Convert.ToString(valNew), pNomeUsuario));
                }
            }

            MemberExpression memberExprInner = null;

            if (zExpression.Body.NodeType == ExpressionType.Convert)
            {
                memberExprInner = ((UnaryExpression)zExpression.Body).Operand as MemberExpression;
            }
            else if (zExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExprInner = zExpression.Body as MemberExpression;
            }
            else if (zExpression.Body.NodeType == ExpressionType.Call)
            {

                //Não está funcionando... Ver como chama metodo sem saber os parametros
                var methodInfo = (zExpression.Body as MethodCallExpression).Method;

                var valOld = methodInfo.Invoke(pOld, null);
                var valNew = methodInfo.Invoke(pNew, null);

                if (Convert.ToString(valOld) != Convert.ToString(valNew))
                {
                    var log = new TLog();
                    logs.Add((TLog)log.Bind(pNew.Id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, Convert.ToString(valOld), Convert.ToString(valNew), pNomeUsuario));
                }
            }


            if (memberExpr != null)
            {
                var prop = (PropertyInfo)memberExpr.Member;
                var propInner = (PropertyInfo)memberExprInner.Member;

                var valOld = ((TInner)prop.GetValue(pOld));
                var valNew = ((TInner)prop.GetValue(pNew));

                var valOldId = valOld != null ? propInner.GetValue(valOld).ToString() : null;
                var valNewId = valNew != null ? propInner.GetValue(valNew).ToString() : null;

                Func<TInner, string> expressionCompilada = yExpression.Compile();

                if (valOldId != valNewId)
                {
                    var log = new TLog();
                    logs.Add((TLog)log.Bind(pNew.Id, pUsuario, pTipoOperLog, pChave, pDescricaoCampo, valOld != null ? expressionCompilada(valOld) : null, valNew != null ? expressionCompilada(valNew) : null, pNomeUsuario));
                }
            }
        }

        public static string GetEnumDescription(object value)
        {
            if (value == null)
                return null;

            FieldInfo fi = value.GetType().GetField(value.ToString());

            DisplayAttribute[] attributes =
                (DisplayAttribute[])fi.GetCustomAttributes(
                typeof(DisplayAttribute),
                false);

            try
            {
                if (attributes != null &&
                    attributes.Length > 0)
                {
                    return ResourceService.GetResource(attributes[0].ResourceType, attributes[0].Name);
                }
                else
                    return value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }

        public virtual void Log_InsereTipoOper<T, TLog>(List<TLog> logs, String pUsuario, String pNomeUsuario, TipoOperLog pTipoOperLog, String pChave)
            where T : BaseModel
            where TLog : LogGenerico<Int64>, new()
        {
            var log = new TLog();
            logs.Add((TLog)log.Bind(this.Id, pUsuario, pTipoOperLog, pChave, GetEnumDescription(pTipoOperLog), String.Empty, String.Empty, pNomeUsuario));
        }

    }
}
