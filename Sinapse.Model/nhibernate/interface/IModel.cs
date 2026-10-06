using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
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
