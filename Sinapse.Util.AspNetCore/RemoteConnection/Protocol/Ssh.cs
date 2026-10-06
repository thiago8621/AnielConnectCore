using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinapse.Util.AspNetCore.RemoteConnection.Protocols
{
    public class Ssh : IProtocolClient
    {
        private SshClient _client;
        private ShellStream _sh;

        #region Constructors

        public Ssh(string ip, string username, string password)
        {
            _client = new SshClient(ip, username, password);
            _client.KeepAliveInterval = TimeSpan.FromMinutes(2);

            _client.Connect();

            if (this.IsConnected())
            {
                int bufferSize = UInt16.MaxValue;
                _sh = _client.CreateShellStream("onu_list", 80, 20, 800, 600, bufferSize);
            }
        }

        public Ssh(string ip, int port, string username, string password)
        {
            _client = new SshClient(ip, port, username, password);
            _client.KeepAliveInterval = TimeSpan.FromMinutes(2);

            _client.Connect();

            if (this.IsConnected())
            {
                int bufferSize = UInt16.MaxValue;
                _sh = _client.CreateShellStream("onu_list", 80, 20, 800, 600, bufferSize);
            }
        }

        #endregion Constructors

        #region IProtocol

        public Task Connect()
        {
            if (!_client.IsConnected)
            {
                _client.Connect();
            }

            return Task.CompletedTask;
        }

        public Task ConnectTL1()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            return _client.IsConnected;
        }

        public void Execute(string command)
        {
            if (!this.IsConnected())
                throw new Exception("Cliente não conectado ao host");

            _client.RunCommand(command);
        }

        public Task Execute(string command, Action<string> callback)
        {
            if (!this.IsConnected())
                throw new Exception("Cliente não conectado ao host");

            return Task.Run(() => {
                string result = _client.CreateCommand(command).Execute();
                callback.Invoke(result);
            });
        }

        public void InteractiveExecute<TStream>(Action<TStream> action) where TStream : class
        {
            //int bufferSize = UInt16.MaxValue;

            //using (var sh = _client.CreateShellStream("onu_list", 80, 20, 800, 600, bufferSize))
            //{
            action.Invoke(_sh as TStream);
            //}
        }

        public Task ExecuteAsync(string command)
        {
            if (!this.IsConnected())
                throw new Exception("Cliente não conectado ao host");

            this._sh.WriteLine(command);
            return Task.CompletedTask;
        }

        public async Task<string> ReadAsync()
        {
            List<string> retorno = new List<string>();

            string readLine;
            while ((readLine = _sh.ReadLine(TimeSpan.FromMilliseconds(500))) != null)
                retorno.Add(readLine);

            return await Task.FromResult(string.Join("\n", retorno));
        }

        public async Task<string> ReadAsync(int delayMs)
        {
            await Task.Delay(delayMs);
            List<string> retorno = new List<string>();

            string readLine;
            while ((readLine = _sh.ReadLine(TimeSpan.FromMilliseconds(500))) != null)
                retorno.Add(readLine);

            return await Task.FromResult(string.Join("\n", retorno));
        }

        #endregion

        #region IDispose

        public void Dispose()
        {
            _sh.Close();
            _sh.Dispose();
            _client.Disconnect();
            _client.Dispose();
        }

        #endregion
    }
}
