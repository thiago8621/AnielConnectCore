using System.Collections.Generic;

namespace Aniel.Connect.Core.AspNetCore.Models
{
    /// <summary>
    ///     Classe que carrega os caminhos para os conteúdos estáticos de uma View
    /// </summary>
    public class ViewContent
    {
        /// <summary>
        ///     Caminho para o arquivo Javascript principal da View
        /// </summary>
        public string JsPath { get; set; }

        /// <summary>
        ///     Caminhos dos arquivos CSS utilizados na View
        /// </summary>
        public IEnumerable<string> CssPath { get; set; }
    }
}
