using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sinapse.DAO;

namespace Sinapse.DAO.Repository
{
    public class ModelIntegrityAttribute : Attribute
    {
        private Type model;

        private OperationIntegrity operation;

        public ModelIntegrityAttribute(Type model, OperationIntegrity operation)
        {
            this.model = model;

            this.operation = operation;
        }

        public Type GetTypeModel()
        {
            return model;
        }

        public OperationIntegrity GetOperation()
        {
            return operation;
        }
    }
}
