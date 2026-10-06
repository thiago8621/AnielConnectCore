using Microsoft.AspNetCore.Http;

namespace Sinapse.Cache.AspNetCore.Caching
{
    public static class ObjManager
    { 
               
        /// <summary>
        /// retorna um objeto do cache
        /// </summary>
        /// <param name="httpContext">Contexto HTTP que será utilizado para encontrar o cache</param>
        /// <param name="pKey">Identificação do objeto</param>
        /// <param name="pCategory">nome da sessão</param>
        /// <returns>objeto capturado na sessão</returns>
        public static object GetObject(HttpContext httpContext, string pCategory, string pKey)
        {
            if (Contains(httpContext, pCategory, pKey))
            {
                return Caching.GetCacheValue(httpContext, pCategory, pKey);
            }
            else
            { 
                return null;
            }
        }        
        
        /// <summary>
        /// Insere um objeto em cache
        /// </summary>
        /// <param name="httpContext">Contexto HTTP onde será adicionado o cache</param>
        /// <param name="pObject">objeto que será inserido</param>
        /// <param name="pKey">Identificação do objeto</param>
        /// <param name="pCategory">nome da sessão onde ele será armazenado</param>
        public static void AddObject(HttpContext httpContext, object pObject, string pKey, string pCategory)
        {
            Caching.Replace(httpContext, pKey, pCategory, pObject);
        }

        /// <summary>
        /// Verifica se um objeto já existe em cache
        /// </summary>
        /// <param name="httpContext">Contexto HTTP que contém o cache</param>
        /// <param name="pKey">Identificação do objeto</param>
        /// <param name="pCategory">nome da sessão</param>
        /// <returns>retorna verdadeiro se o objeto existir em cache ou falso caso não exista ainda</returns>
        public static bool Contains(HttpContext httpContext, string pCategory, string pKey)
        {
            return Caching.CacheContains(httpContext, pCategory, pKey);
        }
        
        /// <summary>
        /// limpa do cache um objeto do tipoMat Domain
        /// </summary>
        public static void ClearObject(HttpContext httpContext, string pCategoty, string pKey)
        {
            Caching.Clear(httpContext, pCategoty, pKey);
        }

        /// <summary>
        /// Retorna um nome de sesão baseando-se no tipoMat do objeto informado
        /// </summary>
        /// <param name="pObject">Objeto base para criação do nome</param>
        /// <returns>nome da sessão</returns>
        private static string GetCategory(object pObject)
        { 
            return pObject.GetType().ToString();
        }

    }
}
