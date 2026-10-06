using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinapse.Core
{
    public interface IEmpresa
    {
        String NomeFantasia { get; set; }
        String RazaoSocial { get; set; }
        String Cnpj { get; set; }
        String Logo { get; set; }
    }
}
