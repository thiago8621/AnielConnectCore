using System;

namespace Sinapse.DAO.AspNetCore
{
    public interface IConstraintIntegrity
    {
        void RunIntegrity(Object model);
    }
}
