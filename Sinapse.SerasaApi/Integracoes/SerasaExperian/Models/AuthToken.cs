using System.Collections.Generic;

namespace Aniel.Connect.Integracoes.SerasaExperian.Models
{
    public class AuthToken
    {
        public string accessToken { get; set; }
        public string tokenType { get; set; }
        public string expiresIn { get; set; }
        public List<string> scope { get; set; }
        public DescricoesErros Erros { get; set; } = new DescricoesErros();
    }
}