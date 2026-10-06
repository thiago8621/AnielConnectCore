using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.Models
{
    public class Coluna_Dinamica<T>
    {
        public Int32 Index_Coluna { get; set; }
        public T Titulo { get; set; }
    }
}
