using System;

namespace Sinapse.Model.AspNetCore.nhibernate
{
    public interface IModel
    {
        Int64 Id { get; set; }

        String UsuarioCriacao { get; set; }

        DateTime? DataHoraCriacao { get; set; }

        String UsuarioAlteracao { get; set; }

        DateTime? DataHoraAlteracao { get; set; }

        String Token { get; set; }
    }
}
