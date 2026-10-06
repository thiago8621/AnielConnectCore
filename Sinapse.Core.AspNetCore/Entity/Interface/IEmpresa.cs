using System;

namespace Sinapse.Core.AspNetCore
{
    public interface IEmpresa
    {
        String NomeFantasia { get; set; }
        String RazaoSocial { get; set; }
        String Cnpj { get; set; }
        String Logo { get; set; }
    }
}
