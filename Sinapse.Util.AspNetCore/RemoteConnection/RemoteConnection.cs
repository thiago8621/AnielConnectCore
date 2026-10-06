using Sinapse.Util.AspNetCore.RemoteConnection.Protocols;
using System;
using System.Threading.Tasks;

namespace Sinapse.Util.AspNetCore.RemoteConnection
{
    /// <summary>
    ///     Classe de implementação para conexões remotas (SSH, Telnet, TL1). 
    /// </summary>   
    public class RemoteConnection : IDisposable
    {
        private IProtocolClient _client;

        #region Constructors

        /// <summary>
        ///     Retorna a consulta de pendência de endereço mais recente registrada no sistema para determinado endereço
        /// </summary>
        /// <param name="ip">IP do host para conexão remota</param>
        /// <param name="username">Usuário para efetuação de login na conexão</param>
        /// <param name="password">Senha para efetuação de login na conexão</param>
        /// <param name="protocol">Protocolo utilizado para estabelecer a conexão</param>
        public RemoteConnection(string ip, string username, string password, Protocol protocol)
        {
            if (protocol == Protocol.Ssh)
            {
                _client = new Ssh(ip, username, password);
            }
            else
            {
                _client = new Telnet(ip, username, password);
            }
        }

        /// <summary>
        ///     Retorna a consulta de pendência de endereço mais recente registrada no sistema para determinado endereço
        /// </summary>
        /// <param name="ip">IP do host para conexão remota</param>
        /// <param name="username">Usuário para efetuação de login na conexão</param>
        /// <param name="password">Senha para efetuação de login na conexão</param>
        /// <param name="protocol">Protocolo utilizado para estabelecer a conexão</param>
        /// <param name="usarTCP">Parâmetro opcional - quando true, utilizará uma implementação alternativa para Telnet em casos onde o Prims.Telnet não estiver retornando resultado precisos.</param>
        public RemoteConnection(string ip, int? port, string username, string password, Protocol protocol, bool usarTCP = false)
        {
            if (protocol == Protocol.Ssh)
            {
                _client = new Ssh(ip, port.GetValueOrDefault(22), username, password);
            }
            else
            {
                if (usarTCP)
                    _client = new TelnetTCP(ip, port.GetValueOrDefault(23), username, password);
                else
                    _client = new Telnet(ip, port.GetValueOrDefault(23), username, password);
            }
        }

        #endregion

        /// <summary>
        ///     Tenta estabelecer a conexão remota e efetuar login.
        /// </summary>       
        public async Task Connect()
        {
            await _client.Connect();
        }

        /// <summary>
        ///     Executa login em servidores TL1.
        /// </summary> 
        public async Task ConnectTL1()
        {
            await _client.ConnectTL1();
        }

        /// <summary>
        ///     Retorna o resultado da tentativa de estabelecer a conexão. 
        ///     true - Conexão estabelecida e login efetivado.
        /// </summary>        
        public bool IsConnected()
        {
            return _client.IsConnected();
        }

        /// <summary>
        ///     Executa comandos de forma síncrona no terminal da conexão.
        /// </summary>        
        public void ExecuteCommand(string command)
        {
            _client.Execute(command);
        }

        /// <summary>
        ///     Executa comandos de forma assíncrona no terminal da conexão.
        /// </summary>   
        public async Task ExecuteCommandAsync(string command)
        {
            await _client.ExecuteAsync(command);
        }

        /// <summary>
        ///     Lê o retorno de forma assíncrona no terminal da conexão.
        /// </summary> 
        public async Task<string> ReadAsync()
        {
            return await _client.ReadAsync();
        }

        /// <summary>
        ///     Lê o retorno de forma assíncrona no terminal da conexão após aguardar determinado tempo.      
        /// </summary> 
        /// <param name="delayMs">Delay em milisegundos</param>
        public async Task<string> ReadAsync(int delayMs)
        {
            return await _client.ReadAsync(delayMs);
        }

        public Task ExecuteCommand(string command, Action<string> callback)
        {
            return _client.Execute(command, callback);
        }

        public void ExecuteScript(string script)
        {
            // Verifica se o script possui um interpretador especificado 
            // no início do arquivo
            // Ex.: #!/bin/bash

            bool hasInterpreter = script.IndexOf("#!") == 0;

            if (hasInterpreter)
            {
                var firstLineBreak = script.IndexOf('\n');
                var interpreter = script.Substring(2, firstLineBreak);

                script = script.Substring(firstLineBreak + 1);

                _client.Execute($"echo \"{script}\" | {interpreter}");
            }
            else
            {
                _client.Execute($"echo \"{script}\" | /bin/sh");
            }
        }

        public Task ExecuteScript(string script, Action<string> callback)
        {
            // Verifica se o script possui um interpretador especificado 
            // no início do arquivo
            // Ex.: #!/bin/bash

            bool hasInterpreter = script.IndexOf("#!") == 0;

            if (hasInterpreter)
            {
                var firstLineBreak = script.IndexOf('\n');
                var interpreter = script.Substring(2, firstLineBreak);

                script = script.Substring(firstLineBreak + 1);

                return _client.Execute($"echo \"{script}\" | {interpreter}", callback);
            }
            else
            {
                return _client.Execute($"echo \"{script}\" | /bin/sh", callback);
            }
        }

        /// <summary>
        ///     Executa ações que necessitam de um shell interativo
        ///     <para>
        ///         TODO: Encontrar um jeito de usar o método sem precisar 
        ///               passar detalhes de implementação (classe do stream do shell)
        ///     </para>
        /// </summary>
        /// <typeparam name="TStream">Tipo do Stream usado para interagir com o shell</typeparam>
        /// <param name="action"></param>
        public void InteractiveExecute<TStream>(Action<TStream> action) where TStream : class
        {
            _client.InteractiveExecute(action);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
