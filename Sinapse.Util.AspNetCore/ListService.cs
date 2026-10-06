using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Util.AspNetCore
{
    public static class ListService
    {
        public static Dictionary<Int32, List<T>> GetListaQuebrada<T>(List<T> lista, Int32 quantidade)
        {
            Dictionary<Int32, List<T>> retorno = new Dictionary<Int32, List<T>>();

            List<T> listatmp = lista;

            if (listatmp.Count < quantidade)
            {
                retorno.Add(1, listatmp);
                return retorno;
            }
            var i = 1;

            while (listatmp.Count > quantidade)
            {
                retorno.Add(i, listatmp.Take(quantidade).ToList());
                listatmp.RemoveRange(0, quantidade);
                i++;
            }

            if (listatmp.Count > 0)
            {
                retorno.Add(i, listatmp.Take(quantidade).ToList());
                listatmp.RemoveRange(0, listatmp.Count);
            }

            return retorno;
        }
    }
}
