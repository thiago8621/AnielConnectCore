using System;
using PrimS.Telnet;
using System.Threading.Tasks;
using System.Linq;

namespace Sinapse.Util.RemoteConnection.Protocols
{
    public class Telnet : IProtocolClient
    {
        #region Fields

        private Client _client;

        private bool _loggedIn = false;

        private int _timeoutMs = (int)TimeSpan.FromSeconds(3).TotalMilliseconds;
        private string _lineTerminator = ">";

        private string username;
        private string password;

        #endregion

        #region Constructors

        public Telnet(string ip, string username, string password)
        {
            this.username = username;
            this.password = password;

            this._client = new Client(ip, 23, new System.Threading.CancellationToken());

            if (this._client.IsConnected)
                _client.MillisecondReadDelay = 500;
        }

        public Telnet(string ip, int port, string username, string password)
        {
            this.username = username;
            this.password = password;

            this._client = new Client(ip, port, new System.Threading.CancellationToken());

            if (this._client.IsConnected)
                _client.MillisecondReadDelay = 500;
        }

        #endregion Constructors

        #region IProtocol

        public async Task Connect()
        {                 
           this._loggedIn = await this._client.TryLoginAsync(username, password, _timeoutMs);
        }

        public async Task ConnectTL1()
        {
            this._loggedIn = await this.TryLoginAsyncTL1(username, password);
        }

        public async Task LogoutTL1()
        {
            await this.TryLogoutAsyncTL1();
        }

        public bool IsConnected()
        {
            return this._loggedIn;
        }

        public void Execute(string command)
        {
            if (!this.IsConnected())
                throw new Exception("Cliente não conectado ao host");

            this._client.WriteLineAsync(command).Wait();
        }

        public Task Execute(string command, Action<string> callback)
        {
            if (!this.IsConnected())
                throw new Exception("Cliente não conectado ao host");

            return Task.Run(async () => {
                await this._client.WriteLineAsync(command);
                var responseLine = await this._client.TerminatedReadAsync(_lineTerminator);
            
                callback.Invoke(ExtractResponseFromLine(command, responseLine));
            });
        }

        public async Task ExecuteAsync(string command)
        {
            if (!this.IsConnected())
                throw new Exception("Cliente não conectado ao host");

            await this._client.WriteLineAsync(command);
        }

        public async Task<string> ReadAsync()
        {
            return await this._client.ReadAsync();
        }

        public async Task<string> ReadAsync(int delayMs)
        {
            await Task.Delay(delayMs);
            return await this._client.ReadAsync();
        }

        #endregion

        #region IDispose

        public void Dispose()
        {
            this._client.Dispose();
        }

        #endregion

        private string ExtractResponseFromLine(string commandSent, string line)
        {
            // Formato da resposta do servidor: 
            
            // (comando enviado)\r\n
            // (resposta do comando)\r\n
            // $ 
            
            // Temos que remover o comando que foi enviado e a 
            // nova linha que o servidor retorna no final da resposta

            var response = line.Replace(commandSent + "\r\n", "");
            response = response.Remove(response.Length - 7);
            
            return response;
        }

        //Método responsável por realizar o login
        //em servidores TL1
        private async Task<bool> TryLoginAsyncTL1(string username, string password)
        {
            await Task.Delay(2000);
            await ExecuteAsync(";\n");
            await Task.Delay(2000);
            await ExecuteAsync($"LOGIN:::CTAG::UN={username},PWD={password};");
            await Task.Delay(5000);
            var retorno_terminal = (await this.ReadAsync()).Split('\r', '\n').ToList();

            if (retorno_terminal.Exists(p => p.ToUpperInvariant().Contains("COMPLD".ToUpperInvariant())))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> TryLogoutAsyncTL1()
        {

            await ExecuteAsync($"LOGOUT:::CTAG::;");
            await Task.Delay(3000);
            var retorno_terminal = (await this.ReadAsync()).Split('\r', '\n').ToList();

            if (retorno_terminal.Exists(p => p.ToUpperInvariant().Contains("COMPLD".ToUpperInvariant())))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void InteractiveExecute<TStream>(Action<TStream> action) where TStream : class
        {
            throw new NotImplementedException();
        }       
    }
}
