using FluentNHibernate;
using FluentNHibernate.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.core
{
    public class CustomForeignKeyConvention : ForeignKeyConvention
    {
        protected override string GetKeyName(Member property, Type type)
        {
            if (property == null || type.Name == null)
            {
                return "";
            }

            String fk = $"FK_{property.Name}_Id";
            //try
            //{
            //    fk = $"FK_{type.Name.Substring(0, 6)}_{property.Name}_Id";

            //    if (fk.Length > 31)
            //    {
            //        fk = $"FK_{type.Name.Substring(0, 6)}_{property.Name.Substring(19)}_Id";
            //    }

            //    return fk;
            //}
            //catch
            //{
                return fk;
            //}
        }
    }
}
