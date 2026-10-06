using PrimS.Telnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Sinapse.Util.RemoteConnection.Protocols
{
    /// <summary>
    ///     Implementação nativa para comunicação Telnet. Utilizar esta quando a biblioteca Prims.Telnet não obtiver resultados satisfatórios.
    /// </summary> 
    public class TelnetTCP : IProtocolClient
    {
        #region Fields

        private System.Net.Sockets.TcpClient _client;
        private System.Net.Sockets.NetworkStream _stream;

        private bool _loggedIn = false;

        private int _timeoutMs = (int)TimeSpan.FromSeconds(2).TotalMilliseconds;
        private int _delayMs = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
        private string _lineTerminator = ">";
        private string _lineFeed = "\n";

        private string username;
        private string password;

        #endregion

        #region Constructors

        public TelnetTCP(string ip, string username, string password)
        {
            this.username = username;
            this.password = password;

            this._client = new System.Net.Sockets.TcpClient(ip, 23);
           

            if (this._client.Connected) 
            {
                _client.ReceiveTimeout = _timeoutMs;
                _client.ReceiveBufferSize = UInt16.MaxValue;
                _stream = _client.GetStream();
                _stream.ReadTimeout = _timeoutMs;
            }
                
        }

        public TelnetTCP(string ip, int port, string username, string password)
        {
            this.username = username;
            this.password = password;

            this._client = new System.Net.Sockets.TcpClient(ip, port);

            if (this._client.Connected)
            {
                _client.ReceiveTimeout = _timeoutMs;
                _client.SendTimeout = _timeoutMs;
                _client.ReceiveBufferSize = UInt16.MaxValue;
                _stream = _client.GetStream();
                _stream.ReadTimeout = _timeoutMs;
                _stream.WriteTimeout = _timeoutMs;
            }
        }
    

        #endregion Constructors

        #region IProtocol
        public async Task Connect()
        {
            this._loggedIn = await this.TryLoginAsync(this.username, this.password);
        }

        //Tenta realizar o login em servidores TL1
        public async Task ConnectTL1()
        {
            this._loggedIn = await this.TryLoginAsyncTL1(this.username, this.password);
        }

        public async Task LogoutTL1()
        {
            await this.TryLogoutAsyncTL1();
        }

        public void Dispose()
        {

            this._client?.Client?.Shutdown(SocketShutdown.Both);
            this._client.Close();
            this._client.Dispose();


            Task.Delay(2000).Wait();

        }

        public void Execute(string command)
        {
            // Send command
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(string.Format("{0}{1}", command, _lineFeed));
            this._stream.Write(data, 0, data.Length);
            Thread.Sleep(_delayMs);
        }

        public Task Execute(string command, Action<string> callback)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteAsync(string command)
        {
            // Send command
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(string.Format("{0}{1}", command, _lineFeed));
            await this._stream.WriteAsync(data, 0, data.Length);
            await Task.Delay(_delayMs);           
        }

        public void InteractiveExecute<TStream>(Action<TStream> action) where TStream : class
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            return this._loggedIn;
        }

        public async Task<string> ReadAsync()
        {
            Thread.Sleep(_delayMs);
            // Receive response
            Byte[] responseData = new byte[UInt16.MaxValue];
                        
            Int32 numberOfBytesRead = await _stream.ReadAsync(responseData, 0, responseData.Length);
            string response = System.Text.Encoding.ASCII.GetString(responseData, 0, numberOfBytesRead);            
                
            //response = ParseData(response);

            return await Task.FromResult(response);
        }

        public async Task<string> ReadAsync(int delayMs)
        {
            await Task.Delay(delayMs);
            // Receive response
            Byte[] responseData = new byte[UInt16.MaxValue];

            Int32 numberOfBytesRead = await _stream.ReadAsync(responseData, 0, responseData.Length);
            string response = System.Text.Encoding.ASCII.GetString(responseData, 0, numberOfBytesRead);            

            return await Task.FromResult(response);
        }
        #endregion IProtocol

        #region Private Methods
        private async Task<bool> TryLoginAsync(string username, string password)
        {
            string retorno_terminal = await this.ReadAsync();

            if((retorno_terminal.Trim()).EndsWith(":"))
            {
                await ExecuteAsync(username);

                if ((await this.ReadAsync()).Trim().EndsWith(":"))
                {
                    await ExecuteAsync(password);

                    return !(await this.ReadAsync()).Trim().EndsWith(":");
                }
            }

            return false;
        }

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




        #endregion Private Methods
    }
}
