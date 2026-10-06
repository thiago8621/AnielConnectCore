using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Core.Entity
{
    public class ParValor
    {
        public String Key { get; set; }
        public Double Valor { get; set; }
    }

    public class ParValor2<T,Y,Z>
    {
        public T Key { get; set; }
        public Y Key2 { get; set; }
        public Z Valor { get; set; }
    }
}
