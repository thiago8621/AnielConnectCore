using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.BoletoAPI.Entity
{
    public class Endereco
    {
        public string bairro { get; set; }
        public string cep { get; set; }
        public string cidade { get; set; }
        public string complemento { get; set; }
        public string estado { get; set; }
        public string numero { get; set; }
        public string endereco { get; set; }
    }
}
