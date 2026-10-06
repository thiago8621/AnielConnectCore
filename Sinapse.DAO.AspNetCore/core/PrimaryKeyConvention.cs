using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using Sinapse.Model.AspNetCore.nhibernate;
using System;

namespace Sinapse.DAO.AspNetCore.core
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
