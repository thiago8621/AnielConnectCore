using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sinapse.DAO.AspNetCore.Repository
{
    public class ModelIntegrityService
    {
        public static void RunIntegrity(Object model, OperationIntegrity operacao)
        {
            String Assemblys = System.Configuration.ConfigurationManager.AppSettings["AssemblysBusiness"];

            List<Type> tipos = new List<Type>();
            foreach (String _assembly in Assemblys.Split(';'))
                tipos.AddRange(Assembly.Load(_assembly).GetTypes().ToList());

            foreach (Type t in tipos)
            {
                if (typeof(IConstraintIntegrity).IsAssignableFrom(t))
                {
                    ModelIntegrityAttribute[] atributos = (ModelIntegrityAttribute[])t.GetCustomAttributes(typeof(ModelIntegrityAttribute), true);

                    foreach (ModelIntegrityAttribute attr in atributos)
                    {
                        if (attr.GetTypeModel() == model.GetType() && attr.GetOperation() == operacao)
                        {
                            IConstraintIntegrity contraintIntegrity = (t.GetConstructor(System.Type.EmptyTypes).Invoke(null) as IConstraintIntegrity);

                            contraintIntegrity.RunIntegrity(model);
                        }
                    }
                }
            }
        }
    }
}
