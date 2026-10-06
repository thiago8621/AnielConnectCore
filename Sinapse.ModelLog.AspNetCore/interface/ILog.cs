using System;

namespace Sinapse.ModelLog.AspNetCore
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
