using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Sinapse.Core.Entity;
using System.Web;
using OfficeOpenXml;


namespace Sinapse.Core.Service
{
    public class ExcelService : IDisposable
    {
        public ExcelTemplate ExcelTemplate { get; set; }

        public Boolean TituloValido
        {
            get
            {
                if (ExcelTemplate.Titulos.Count == 0)
                    return false;
                else
                    return true;
            }
            set
            {

            }
        }

        public Int32 QtdeRegistos
        {
            get
            {
                return ExcelTemplate.Valores.Count;
            }
            set
            {

            }
        }

        public ExcelService()
        {
            ExcelTemplate = new ExcelTemplate()
            {
                Titulos = new List<ExcelTitulo>(),
                Valores = new List<ExcelValor>()
            };
        }

        public void ImportExcelXls(HttpPostedFileBase fileBase)
        {
            using (var package = new ExcelPackage(fileBase.InputStream))
            {
                // get the first worksheet in the workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                Int32 maxColum = 1;
                while (worksheet.Cells[1, maxColum].Value != null) // Varre Colunas
                {
                    ExcelTemplate.Titulos.Add(new ExcelTitulo()
                    {
                        Coluna = maxColum,
                        Descricao = worksheet.Cells[1, maxColum].Value.ToString()
                    });
                    maxColum++;
                }

                
                Int32 linhaAtual = 2;

                while (worksheet.Cells[linhaAtual, 1].Value != null) // Varre Linha
                {
                    ExcelValor valoresLinha = new ExcelValor()
                        {
                            Valor = new Dictionary<int,object>()
                        };

                    for (int colunaAtual = 1; colunaAtual < maxColum; colunaAtual++)
                    {
                        valoresLinha.Valor.Add(colunaAtual, worksheet.Cells[linhaAtual, colunaAtual].Value);
                    }

                    ExcelTemplate.Valores.Add(valoresLinha);

                    linhaAtual++;
                }
                


            } // the using 
        }

        #region Disposable
        bool disposed = false;

        // método privado para controle
        // da liberação dos recursos
        private void Dispose(bool disposing)
        {
            // Verifique se Dispose já foi chamado.
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Liberando recursos gerenciados

                }

                // Seta a variável booleana para true,
                // indicando que os recursos já foram liberados
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
