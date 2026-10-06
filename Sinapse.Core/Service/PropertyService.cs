using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sinapse.Core
{
    public static class PropertyService
    {
        /// <summary>
        /// Utilizado no MakeLog
        /// </summary>
        private static String[] ignores = new String[] { "Id", "UsuarioCriacao", "UsuarioAlteracao", "DataHoraCriacao", "DataHoraAlteracao", "Token", "Logs" };


        public static PropertyInfo StringToProperty(Type pTipo, string pProperty)
        {
            string temp = pProperty;
            Type tipoTemp = pTipo;

            PropertyInfo retorno = null;
            if (pProperty.Contains("."))
            {
                temp = pProperty.Substring(0, pProperty.IndexOf("."));
                tipoTemp = pTipo.GetProperty(temp).PropertyType;
                retorno = StringToProperty(tipoTemp, pProperty.Substring(pProperty.IndexOf(".") + 1));
            }
            else
            {
                retorno = tipoTemp.GetProperty(temp);
            }

            return retorno;
        }

        public static void StringToProperty(object pDominio, string pProperty, ref object pDominioRetorno, ref PropertyInfo pPropertyRetorno)
        {
            string temp = pProperty;

            PropertyInfo retornoProp = null;
            object retornoDom = null;

            if (pProperty.Contains("."))
            {
                temp = pProperty.Substring(0, pProperty.IndexOf("."));

                retornoDom = (object)pDominio.GetType().GetProperty(temp).GetValue(pDominio, null);
                if (retornoDom == null)
                {
                    retornoDom = (object)pDominio.GetType().GetProperty(temp).PropertyType.GetConstructor(Type.EmptyTypes).Invoke(null);
                    pDominio.GetType().GetProperty(temp).SetValue(pDominio, retornoDom, null);
                }
                StringToProperty(retornoDom, pProperty.Substring(pProperty.IndexOf(".") + 1), ref pDominioRetorno, ref pPropertyRetorno);
            }
            else
            {
                retornoDom = pDominio;
                if (pDominio == null)
                {
                    retornoDom = (object)pDominio.GetType().GetConstructor(Type.EmptyTypes).Invoke(null);
                }
                retornoProp = pDominio.GetType().GetProperty(temp);

                pDominioRetorno = retornoDom;
                pPropertyRetorno = retornoProp;
            }
        }

        public static void StringToProperty(Type TypeDominio, string pProperty, ref object pDominioRetorno, ref PropertyInfo pPropertyRetorno)
        {
            string temp = pProperty;

            PropertyInfo retornoProp = null;
            Object retornoDom = null;
            
            Object _dominio = TypeDominio.GetConstructor(Type.EmptyTypes).Invoke(null);


            if (pProperty.Contains("."))
            {
                temp = pProperty.Substring(0, pProperty.IndexOf("."));

                retornoDom = (object)_dominio.GetType().GetProperty(temp).GetValue(_dominio, null);
                if (retornoDom == null)
                {
                    retornoDom = (object)_dominio.GetType().GetProperty(temp).PropertyType.GetConstructor(Type.EmptyTypes).Invoke(null);
                    _dominio.GetType().GetProperty(temp).SetValue(_dominio, retornoDom, null);
                }
                StringToProperty(retornoDom, pProperty.Substring(pProperty.IndexOf(".") + 1), ref pDominioRetorno, ref pPropertyRetorno);
            }
            else
            {
                retornoDom = _dominio;
                if (_dominio == null)
                {
                    retornoDom = (object)_dominio.GetType().GetConstructor(Type.EmptyTypes).Invoke(null);
                }
                retornoProp = _dominio.GetType().GetProperty(temp);

                pDominioRetorno = retornoDom;
                pPropertyRetorno = retornoProp;
            }
        }


        public static List<String> PropertiesInModel(Type TypeDominio)
        {
            List<String> retorno = new List<String>();

            foreach (PropertyInfo prop in TypeDominio.GetProperties())
            {
                retorno.Add(prop.Name);
            }

            return retorno;
        }

        /*public static void CopyPropertyModelToLog(IModel pObj, ILogModel pObjLog)
        {
            foreach (PropertyInfo prop in pObj.GetType().GetProperties())
            {
                Object valor = prop.GetValue(pObj, Type.EmptyTypes);

                if (ObjectService.IsInterfaceOf(prop.PropertyType, typeof(IModel)))
                {
                    object[] atts = prop.GetCustomAttributes(typeof(IdLogModelAttribute), false);

                    if (atts.Length > 0)
                    {
                        String nomeProp = ((IdLogModelAttribute)atts[0]).GetPropFK();
                        try
                        {
                            pObjLog.GetType().GetProperty(nomeProp).SetValue(pObjLog, ((IModel)valor).Id, null);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                else if (prop.PropertyType.IsGenericType)
                {
                    continue;
                }
                else
                {
                    if (!ignores.Contains(prop.Name))
                    {
                        try
                        {
                            pObjLog.GetType().GetProperty(prop.Name).SetValue(pObjLog, valor, null);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }

        public static ILogModel MakeLog(IModel pObj)
        {
            object[] atts = pObj.GetType().GetCustomAttributes(typeof(LogModelAttribute), true);

            if (atts.Length > 0)
            {
                LogModelAttribute att = (LogModelAttribute)atts[0];
                Type tipoLog = att.GetTipoLog();
                ILogModel log = (ILogModel)tipoLog.GetConstructor(Type.EmptyTypes).Invoke(null);

                PropertyService.CopyPropertyModelToLog(pObj, log);

                return log;
            }
            else
            {
                return null;
            }
        }

        public static ILogModel MakeLogByDif(IModel pObjClean, IModel pObjDirty, TipoLog pTipoLog)
        {
            if (pObjClean == null || pObjDirty == null)
            {
                return null;
            }

            ILogModel logClean = MakeLog(pObjClean);
            ILogModel logDirty = MakeLog(pObjDirty);

            if (logClean == null || logDirty == null)
            {
                return null;
            }

            ILogModel retorno = (ILogModel)logClean.GetType().GetConstructor(Type.EmptyTypes).Invoke(null);
            Boolean temDiferentes = false;

            foreach (PropertyInfo prop in logClean.GetType().GetProperties())
            {
                if (ignores.Contains(prop.Name))
                    continue;

                Object valorClean = prop.GetValue(logClean, Type.EmptyTypes);
                Object valorDirty = prop.GetValue(logDirty, Type.EmptyTypes);

                object[] att = prop.GetCustomAttributes(typeof(DescricaoLogAttribute), false);

                try
                {
                    if (!Object.Equals(valorClean, valorDirty) || att.Length > 0)
                    {
                        if (!Object.Equals(valorClean, valorDirty))
                            temDiferentes = true;

                        retorno.GetType().GetProperty(prop.Name).SetValue(retorno, valorClean, null);
                    }
                }
                catch
                {
                    continue;
                }
            }

            if (temDiferentes || pTipoLog == TipoLog.Exclusao)
                return retorno;
            else
                return null;
        }
        */

    }
}
