using Sinapse.Model.enumerados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
{
    public interface IModelLog
    {
        Int64 Id { get; set; }

        String UsuarioCriacao { get; set; }

        DateTime? DataHoraCriacao { get; set; }

        TipoOperacao TipoOperacao { get; set; }

        String Token { get; set; }
    }
}
