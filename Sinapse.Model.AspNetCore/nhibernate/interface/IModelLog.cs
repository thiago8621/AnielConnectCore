using Sinapse.Model.AspNetCore.enumerados;
using System;

namespace Sinapse.Model.AspNetCore.nhibernate
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
