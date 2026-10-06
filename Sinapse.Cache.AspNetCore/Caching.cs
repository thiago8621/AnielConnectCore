using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Sinapse.JSON.AspNetCore;
using System.Text;

namespace Sinapse.Cache.AspNetCore.Caching
{

    public static class Caching
    {
        private static Dictionary<object, object> CacheSimulation = null;

        private static List<CachingIndex> CacheIdx = new List<CachingIndex>();

        public static Object GetCacheValue(HttpContext httpContext, string pKey, string pId)
        {
            Object objeto = null;

            CachingIndex idx = CacheIdx.Find(p => p.id == pId);

            if (idx == null)
                return null;
            else
            {
                if (idx.tipo == CachingIndexEnum.Cookie)
                {
                    String obj = ReadCookie(httpContext, pId);
                    if (!String.IsNullOrEmpty(obj))
                    {
                        objeto = JsonService.DeserializeJasonMapper<Object>(obj);
                    }
                }
                else
                {
                    Dictionary<object, object> cache = GetCache(httpContext, pId, idx.tipo);

                    if (cache != null)
                        cache.TryGetValue(pKey, out objeto);
                }
            }

            return objeto;
        }

        public static Dictionary<object, object> GetCache(HttpContext httpContext, string pId, CachingIndexEnum pTipo)
        {
            if (httpContext != null)
            {
                if (pTipo == CachingIndexEnum.Session)
                {
                    byte[] sessionCacheBytes = httpContext.Session.Get(pId);
                    string serializedSessionCache = JsonService.SerializeObject(sessionCacheBytes);

                    object sessionCache = JsonService.DeserializeJasonMapper<Object>(serializedSessionCache);

                    return sessionCache as Dictionary<object, object>;
                }
                else if (pTipo == CachingIndexEnum.Cache)
                {
                    return httpContext.Items[pId] as Dictionary<object, object>;
                }
                else
                    return null;
            }
            else
            {
                return CacheSimulation;
            }
        }

        private static void RegisterKey(String pId, CachingIndexEnum pTipo)
        {
            if (FindKey(pId) == null)
                CacheIdx.Add(new CachingIndex() { id = pId, tipo = pTipo });
        }

        private static CachingIndex FindKey(String pId)
        {
            return CacheIdx.Find(p => p.id == pId);
        }

        public static void Add(HttpContext httpContext, string pKey, string pId, object pValue)
        {
            if (pValue == null)
            {
                RemoveCooking(httpContext, pId);
                if (GetCacheValue(httpContext, pKey, pId) != null)
                    Replace(httpContext, pKey, pId, pValue);

                return;
            }

            if (IsCookingObj(pValue))
            {
                RegisterKey(pId, CachingIndexEnum.Cookie);
                AddCooking(httpContext, pKey, pId, pValue);
            }
            else
            {
                CachingIndexEnum tipo;

                if (IsCachingObj(pValue))
                {
                    tipo = CachingIndexEnum.Cache;
                    CachingInitialize(httpContext, pId, true);
                }
                else
                {
                    tipo = CachingIndexEnum.Session;
                    CachingInitialize(httpContext, pId, false);
                }

                RegisterKey(pId, tipo);

                if (GetCacheValue(httpContext, pKey, pId) != null)
                    Replace(httpContext, pKey, pId, pValue);
                else
                    GetCache(httpContext, pId, tipo).Add(pKey, pValue);
            }
        }

        private static void AddCooking(HttpContext httpContext, string pKey, string pId, object pValue)
        {
            try
            {
                WriteCookie(httpContext, pId, JsonService.SerializeObject(pValue));
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Não foi possível adicionar o objeto {0} ao Cookie.\n{1}", pValue.GetType().Name, ex.Message));
            }
        }

        public static void WriteCookie(HttpContext httpContext, string name, string value)
        {
            httpContext.Response.Cookies.Append(name, value);
        }


        public static String ReadCookie(HttpContext httpContext, string name)
        {
            // Verificando se o cookie existe no cabeçalho da resposta
            if (httpContext.Response.Headers.ContainsKey("Set-Cookie"))
            {
                // Cada cookie adicionado no response segue o formato "Set-Cookie: <cookie-name>=<cookie-value>".
                // Portanto, é necessário verificar cada header Set-Cookie até encontrar o header que define o cookie com o nome esperado.
                foreach (string cookie in httpContext.Response.Headers["Set-Cookie"])
                {
                    if (cookie.StartsWith($"{name}="))
                    {
                        int cookieValueStartingIndex = cookie.IndexOf('=') + 1;
                        int cookieValueEndingIndex = cookie.IndexOf(';');
                        
                        return cookie.Substring(cookieValueStartingIndex, cookieValueEndingIndex);
                    }
                }
            }

            if (httpContext.Request.Cookies.ContainsKey(name))
            {
                var cookie = httpContext.Request.Cookies[name];
                return cookie;
            }

            return null;
        }


        private static Boolean RemoveCooking(HttpContext httpContext, string pKey)
        {
            try
            {
                httpContext.Response.Cookies.Delete(pKey);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Não foi possível remover o objeto com o nome {0}.\n {1}", pKey, ex.Message));
            }
        }

        public static void Replace(HttpContext httpContext, string key, string id, object value)
        {
            Clear(httpContext, key, id);
            Add(httpContext, key, id, value);
        }

        public static bool Clear(HttpContext httpContext, string pKey, string pId)
        {
            CachingIndex idx = FindKey(pId);

            if (idx == null)
            {
                return false;
            }
            else if (CacheContains(httpContext, pKey, pId, idx))
            {
                if (idx.tipo == CachingIndexEnum.Cookie)
                    return RemoveCooking(httpContext, pKey);
                else
                    return GetCache(httpContext, pId, idx.tipo).Remove(pKey);
            }
            else
            {
                return false;
            }
        }

        public static bool CacheContains(HttpContext httpContext, object pKey, string pId)
        {
            CachingIndex idx = FindKey(pId);

            if (idx == null)
                return false;

            Dictionary<object, object> cache = GetCache(httpContext, pId, idx.tipo);

            if (cache != null)
                return cache.ContainsKey(pKey);
            else
                return false;
        }

        public static bool CacheContains(HttpContext httpContext, object pKey, string pId, CachingIndex pIdx)
        {
            Dictionary<object, object> cache = GetCache(httpContext, pId, pIdx.tipo);

            if (cache != null)
                return cache.ContainsKey(pKey);
            else
                return false;
        }

        private static void CachingInitialize(HttpContext httpContext, string pId, Boolean pCacheObject)
        {
            if (httpContext != null)
            {
                //if (pCacheObject)
                //{
                //    if (HttpContext.Current.Cache[pId] == null)
                //    {
                //        HttpContext.Current.Cache[pId] = new Dictionary<object, object>();
                //    }
                //}
                //else
                //{
                //    if (HttpContext.Current.Session[pId] == null)
                //    {
                //        HttpContext.Current.Session[pId] = new Dictionary<object, object>();
                //    }
                //}
                
                if (pCacheObject)
                {
                    if (httpContext.Items[pId] == null)
                    {
                        httpContext.Items[pId] = new Dictionary<object, object>();
                    }
                }
                else
                { 
                    if (httpContext.Session.Get(pId) == null)
                    {
                        var emptyDictionary = new Dictionary<object, object>();
                        var serializedDictionaryString = JsonService.SerializeObject(emptyDictionary);

                        byte[] serializedDictionary = Encoding.UTF8.GetBytes(serializedDictionaryString);

                        httpContext.Session.Set(pId, serializedDictionary);
                    }
                }
            }
            else
            {
                if (CacheSimulation == null)
                    CacheSimulation = new Dictionary<object, object>();
            }
        }

        //private void RegistraCookie()
        //{
        //    HttpContext.Current.Response.Cookies[COOKIE_LOGIN]["Login"] = txtEmail.Text;
        //    HttpContext.Current.Response.Cookies[COOKIE_LOGIN]["Senha"] = txtSenha.Text;
        //    HttpContext.Current.Response.Cookies[COOKIE_LOGIN].Expires = DateTime.Now.AddDays(365);           
        //}

        //protected void PreencheCamposByCookies()
        //{
        //    objCookie = HttpContext.Current.Request.Cookies[COOKIE_LOGIN];
        //    if (objCookie != null)
        //    {
        //        txtEmail.Text = objCookie["Login"];
        //        txtSenha.Attributes.Add("value", objCookie["Senha"]);
        //        chkLembrarSenhar.Checked = true;
        //    }
        //}

        public static Boolean IsCachingObj(Object pObject)
        {
            if (pObject == null)
                return false;
            else if (pObject.GetType().IsGenericType)
                return pObject.GetType().GetGenericArguments()[0].GetCustomAttributes(typeof(CachingAttribute), false).Length > 0;
            else
                return pObject.GetType().GetCustomAttributes(typeof(CachingAttribute), false).Length > 0;
        }

        public static Boolean IsCookingObj(Object pObject)
        {
            if (pObject == null)
                return false;
            //else if (HttpContext.Current == null)
            //    return false;
            else if (pObject.GetType().IsGenericType)
                return pObject.GetType().GetGenericArguments()[0].GetCustomAttributes(typeof(CookingAttribute), false).Length > 0;
            else
                return pObject.GetType().GetCustomAttributes(typeof(CookingAttribute), false).Length > 0;
        }

        public static void ClearAll(HttpContext httpContext)
        {
            if (httpContext != null)
                httpContext.Session.Clear();
        }


        public static object GetLastValue(HttpContext httpContext, string pId)
        {
            Object objeto;

            CachingIndex idx = FindKey(pId);

            if (idx == null)
                return null;

            Dictionary<object, object> vDictionary = GetCache(httpContext, pId, idx.tipo);

            if (vDictionary != null)
                objeto = vDictionary.Last().Value;
            else
                objeto = null;

            return objeto;
        }

    }
}