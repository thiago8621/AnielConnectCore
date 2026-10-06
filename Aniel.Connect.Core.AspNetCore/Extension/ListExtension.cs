using System.Collections.Generic;
using System.Linq;

namespace Aniel.Connect.Core.AspNetCore.Extension
{
    public static class ListExtension
    {
        public static IList<T> GetPage<T>(this IList<T> list, int page, int pageSize)
        {
            return list.Skip(page * pageSize).Take(pageSize).ToList();
        }
    }
}
