using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.ModelLog
{
    /// <typeparam name="T">Tipo do campo IdLog</typeparam>
    public interface ILogGenerico<T>
    {
        Int64 Id { get; set; }

        T IdLog { get; set; }

        String UsuarioCriacao { get; set; }
        String NomeUsuarioCriacao { get; set; }
        DateTime DataHoraCriacao { get; set; }
        TipoOperLog TipoOperacao { get; set; }
        
        /// <summary>
        /// Identificador do Objeto
        /// </summary>
        String Chave { get; set; }

        /// <summary>
        /// Texto que representa o Identificador do Objeto 
        /// </summary>
        String DescricaoResource { get; set; }

        String Campo { get; set; }
        String Campo_ValorAntes { get; set; }
        String Campo_ValorDepois { get; set; }
        String Motivo { get; set; }
    }
}
