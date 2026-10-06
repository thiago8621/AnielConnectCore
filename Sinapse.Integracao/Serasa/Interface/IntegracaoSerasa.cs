using Sinapse.Integracao.Serasa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Integracao.Serasa.Interface
{
    public interface IntegracaoSerasa
    {
        public RetornoCpfSerasaModel GetConsultaSerasaCpf(string cpf);
        public RetornoCnpjSerasaModel GetConsultaSerasaCnpj(string cnpj);
    }
}
