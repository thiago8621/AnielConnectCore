using NHibernate.Proxy;
using System;
using System.Reflection;

namespace Sinapse.Core.AspNetCore
{
    public class ModelAttributeService
    {
        private static String GetAttDisplayName(DisplayNameAttribute atributo, String defaultName)
        {
            String _keyResource = null;

            if (atributo != null)
            {
                _keyResource = atributo.GetDisplayName();
            }
            else
            {
                _keyResource = defaultName;
            }

            return _keyResource;
        }

        public static TokenLessAttribute GetTokenLessAtt(Type tipo)
        {
            TokenLessAttribute[] atributo = (TokenLessAttribute[])tipo.GetCustomAttributes(typeof(TokenLessAttribute), false);

            if (atributo != null)
            {
                if (atributo.Length > 0)
                    return atributo[0];
                else
                    return null;
            }
            else
                return null;
        }

        public static String GetPropertyDisplayName(Type tipo, String propertyName)
        {
            Object _dominioRetorno = null;
            PropertyInfo _propertyRetorno = null;

            PropertyService.StringToProperty(tipo, propertyName,ref _dominioRetorno,ref _propertyRetorno);

            if (_dominioRetorno is INHibernateProxy)
            {
                ILazyInitializer lazyInitialiser = ((INHibernateProxy)_dominioRetorno).HibernateLazyInitializer;
                _dominioRetorno = lazyInitialiser.GetImplementation();
            }

            return GetPropertyDisplayName(_dominioRetorno.GetType(), _propertyRetorno);
        }

        public static Type GetTypePropertyDisplayName(Type tipo, String propertyName)
        {
            Object _dominioRetorno = null;
            PropertyInfo _propertyRetorno = null;

            PropertyService.StringToProperty(tipo, propertyName, ref _dominioRetorno, ref _propertyRetorno);

            if (_propertyRetorno == null)
                return typeof(String);
            else
                return _propertyRetorno.PropertyType;
        }

        public static String GetPropertyDisplayName(Type tipo, PropertyInfo property) 
        {
            Object[] atributos = property.GetCustomAttributes(false);
            
            foreach (Object obj in atributos)
            {
                if (obj.GetType() == typeof(DisplayNameAttribute))
                {
                    return GetAttDisplayName((DisplayNameAttribute)obj, property.Name);
                }
            }

            return property.Name;
        }

        public static String GetClassDisplayName(Type tipo)
        {
            Object[] atributos = tipo.GetCustomAttributes(false);

            foreach (Object obj in atributos)
            {
                if (obj.GetType() == typeof(System.ComponentModel.DisplayNameAttribute))
                {
                    return GetAttDisplayName((DisplayNameAttribute)obj, tipo.Name);
                }
            }

            return tipo.Name;
        }

        

    }
}
