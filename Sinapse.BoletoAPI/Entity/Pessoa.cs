using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.BoletoAPI.Entity
{
    public class Pessoa
    {
        public string nome { get; set; }
        public string cpfcnpj { get; set; }
        public Endereco endereco { get; set; }
        public string telefone { get; set; }
        public string email { get; set; }
    }
}
