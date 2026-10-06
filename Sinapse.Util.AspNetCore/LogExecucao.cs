using System;
using System.Collections.Generic;
using System.IO;

namespace Sinapse.Util.AspNetCore
{
    public class LogExecucao : IDisposable
    {
        private StreamWriter log = null;
        private String caminho = String.Empty;
        private String arquivo;


        public LogExecucao(String modelIntegracao, String caminho)
        {
            arquivo = @"\" + modelIntegracao + ".txt";
            if (log == null)
            {
                if (!Directory.Exists(caminho))
                    Directory.CreateDirectory(caminho);

                this.caminho = caminho;

                try
                {
                    File.AppendAllLines(caminho + arquivo, new List<String>() { "", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - Iniciando a gravação dos logs ..." });
                }
                catch
                {
                    //throw new Exception("Não foi possível salvar o log em: " + caminho);
                }
            }
        }

        public void RegistraLog(String Msg)
        {
            try
            {
                File.AppendAllLines(caminho + arquivo, new List<String>() { Msg });
            }
            catch
            {
                //throw new Exception("Não foi possível salvar o log em: " + caminho);
            }
        }

        public void SalvarLog()
        {
            try
            {
                File.AppendAllLines(caminho + arquivo, new List<String>() { DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - Finalizando a gravação dos logs ..." });
            }
            catch
            {
                throw new Exception("Não foi possível salvar o log em: " + caminho);
            }
        }

        #region IDisposable Support
        private Boolean disposedValue;// To detect redundant calls

        // IDisposable
        protected virtual void Dispose(Boolean disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {

                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
            }
            this.disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
