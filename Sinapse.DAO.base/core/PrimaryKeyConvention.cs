using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.core
{
    public class PrimaryKeyConvention : IIdConvention
    {
        public void Apply(IIdentityInstance instance)
        {
            Type type = instance.EntityType;
            var persitentType = typeof(IModel);
            var isMappableModel = persitentType.IsAssignableFrom(type);

            if (isMappableModel)
            {
                instance.Column(type.Name + "_Id");
                //instance.GeneratedBy.Assigned();
            }
        }
    }
}
