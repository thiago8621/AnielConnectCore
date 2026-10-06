using System;

namespace Sinapse.DAO.AspNetCore.Repository
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
