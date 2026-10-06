using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.ModelLog
{
    public interface ILog
    {
        Int64 Id { get; set; }

        Int64 IdLog { get; set; }

        DateTime DataHoraLog { get; set; }

        String Token { get; set; }

        String UsuarioCriacao { get; set; }

        String UsuarioLog { get; set; }

        String JSON { get; set; }

        String Observacao { get; set; }

        String TipoObjeto { get; set; }

        TipoOperLog TipoOperLog { get; set; }

        void FillCamposChaves(Object objModel);
        
    }
}
