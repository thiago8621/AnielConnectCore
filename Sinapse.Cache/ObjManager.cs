using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Cache.Caching
{
    public static class ObjManager
    { 
               
        /// <summary>
        /// retorna um objeto do cache
        /// </summary>
        /// <param name="pKey">Identificação do objeto</param>
        /// <param name="pCategory">nome da sessão</param>
        /// <returns>objeto capturado na sessão</returns>
        public static object GetObject(string pCategory, string pKey)
        {
            if (Contains(pCategory, pKey))
            {
                return Caching.GetCacheValue(pCategory, pKey);
            }
            else
            { 
                return null;
            }
        }        
        
        /// <summary>
        /// Insere um objeto em cache
        /// </summary>
        /// <param name="pObject">objeto que será inserido</param>
        /// <param name="pKey">Identificação do objeto</param>
        /// <param name="pCategory">nome da sessão onde ele será armazenado</param>
        public static void AddObject(object pObject, string pKey, string pCategory)
        {
            Caching.Replace(pKey, pCategory, pObject);
        }

        /// <summary>
        /// Verifica se um objeto já existe em cache
        /// </summary>
        /// <param name="pKey">Identificação do objeto</param>
        /// <param name="pCategory">nome da sessão</param>
        /// <returns>retorna verdadeiro se o objeto existir em cache ou falso caso não exista ainda</returns>
        public static bool Contains(string pCategory, string pKey)
        {
            return Caching.CacheContains(pCategory, pKey);
        }
        
        /// <summary>
        /// limpa do cache um objeto do tipoMat Domain
        /// </summary>
        /// <param name="seqTransfere">Objeto do tipoMat Domain</param>
        public static void ClearObject(string pCategoty, string pKey)
        {
            Caching.Clear(pCategoty, pKey);
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
