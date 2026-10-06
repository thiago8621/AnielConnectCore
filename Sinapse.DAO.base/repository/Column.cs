using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.repository
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class Column : Attribute
    {
        private ColumnTargets target;
        public Column(ColumnTargets value)
        {
            this.target = value;
        }

        public ColumnTargets getTarget() { return this.target; }
    }

    public enum ColumnTargets
    {
        Key = 1
    }
}
