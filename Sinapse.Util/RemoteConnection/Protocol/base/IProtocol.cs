using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sinapse.Util.RemoteConnection
{
    /// <summary>
    ///     Representa um protocolo de acesso remoto
    /// </summary
    public interface IProtocolClient : IDisposable
    {
        /// <summary>
        ///     Estabelece a conexão e efetua o login.
        /// </summary>
        Task Connect();

        /// <summary>
        ///     Estabelece a conexão e efetua o login em servidores TL1.
        /// </summary>
        Task ConnectTL1();

        /// <summary>
        ///     Efetua o logout em servidores TL1.
        /// </summary>
        Task LogoutTL1();


        /// <summary>
        ///     Retorna o resultado da tentativa de estabelcer conexão e login.
        /// </summary>
        bool IsConnected();

        /// <summary>
        ///     Executa ações que necessitam de um shell interativo
        /// </summary>
        /// <typeparam name="TStream">Tipo do Stream usado para interagir com o shell</typeparam>
        /// <param name="callback"></param>
        void InteractiveExecute<TStream>(Action<TStream> action) where TStream : class;

        /// <summary>
        ///     Executa comandos de forma síncrona.
        /// </summary>        
        /// <param name="command"> Comando a ser executado.</param>
        void Execute(string command);
        Task Execute(string command, Action<string> callback);

        /// <summary>
        ///     Executa comandos de forma assíncrona.
        /// </summary>        
        /// <param name="command"> Comando a ser executado.</param>
        Task ExecuteAsync(string command);

        /// <summary>
        ///     Lê o retorno do terminal de forma assíncrona.
        /// </summary>                
        Task<string> ReadAsync();

        /// <summary>
        ///     Lê o retorno do terminal de forma assíncrona após aguardar determinado tempo.
        /// </summary>        
        /// <param name="delayMs"> Delay em milisegundos.</param>
        Task<string> ReadAsync(int delayMs);
    }
}
