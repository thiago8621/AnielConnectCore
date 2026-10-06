using System;

namespace Sinapse.ModelLog
{
    /// <typeparam name="T">Tipo do Campo IdLog</typeparam>
    public abstract class LogGenerico<T> : ILogGenerico<T>
    {
        public LogGenerico()
        {

        }

        public virtual LogGenerico<T> Bind(T pIdLog, String pUsuario, TipoOperLog pTipoOper, String pChave, String pCampo, String pCampo_ValorAntes, String pCampo_ValorDepois,String pNomeUsuario,String pMotivo = null)
        {
            IdLog = pIdLog;
            UsuarioCriacao = pUsuario;
            DataHoraCriacao = DateTime.Now;
            TipoOperacao = pTipoOper;
            Chave = pChave;
            Campo = pCampo;
            Campo_ValorAntes = pCampo_ValorAntes;
            Campo_ValorDepois = pCampo_ValorDepois;
            NomeUsuarioCriacao = pNomeUsuario;
            Motivo = pMotivo;
            return this;
        }

        public virtual String Documento { get; set; }
        public virtual String Motivo { get; set; }
        public virtual long Id { get; set; }
        public virtual T IdLog { get; set; }
        public virtual string UsuarioCriacao { get; set; }
        public virtual string NomeUsuarioCriacao { get; set; }
        public virtual DateTime DataHoraCriacao { get; set; }
        public virtual TipoOperLog TipoOperacao { get; set; }

        /// <summary>
        /// Identificador do Objeto
        /// </summary>
        public virtual String Chave { get; set; }

        /// <summary>
        /// Texto que representa o Identificador do Objeto 
        /// </summary>
        public virtual String DescricaoResource { get; set; }

        public virtual String Campo { get; set; }
        public virtual String Campo_ValorAntes { get; set; }
        public virtual String Campo_ValorDepois { get; set; }
        public virtual String Log_Integracao { get; set; }
    }
}
