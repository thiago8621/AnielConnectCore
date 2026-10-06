using Aniel.Connect.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.ControllersWebApi
{
    /// <summary>
    ///     Metodos para implementar colunas dinamicas no Datatable
    /// </summary>
    /// <typeparam name="R">Objeto de retorno do Datatable</typeparam>
    public abstract class DynamicColumnsApiController<R> : BaseApiController
    {
        /// <summary>
        ///     Define as colunas Dinamicas do Datatable
        ///     baseado nos valores que vem do banco.
        /// </summary>
        /// <typeparam name="T">Tipo de valor das colunas dinamicas</typeparam>
        /// <param name="valoresBanco">Valores que estao no banco</param>
        /// <param name="listaValores">Lista de controle das colunas dinamicas</param>
        /// <param name="header">Lista de colunas do cabecalho do Datatable</param>
        /// <param name="data">Nome do parametro data do Datatable. Sera concatenado de acordo com o indice </param>
        /// <param name="title"></param>

        protected void SetColunas<T>(
            List<T> valoresBanco,
            List<Coluna_Dinamica<T>> listaValores,
            List<Column> header,
            String data,
            String title
        )
        {
            if (valoresBanco.Count > 0)
            {
                foreach (var valor in valoresBanco)
                {
                    int index_valor_banco = valoresBanco.FindIndex(val => val.Equals(valor)) + 1;

                    listaValores.Add(new Coluna_Dinamica<T>
                    {
                        Index_Coluna = index_valor_banco,
                        Titulo = valor                        
                    });

                    header.Add(new Column
                    {
                        title = String.Format(title, valor),
                        data = $"{data}_{index_valor_banco}",
                        data_value = Convert.ToString(valor),
                        orderable = true
                    });
                }
            }
        }

        /// <summary>
        ///     Joga os valores nas colunas que foram definidas pelo SetColunas
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="retorno">Lista de retorno do Datatable</param>
        /// <param name="listColunas">Lista das colunas</param>
        /// <param name="nomeProp"></param>
        protected abstract void SetValoresColunas<T>(
            List<R> retorno,
            List<Coluna_Dinamica<T>> listColunas,
            string nomeProp
        );

    }
}
