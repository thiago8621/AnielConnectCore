using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sinapse.Model;
using Sinapse.ModelLog;

namespace Sinapse.Model.nhibernate
{
    [Serializable]
    [ObjetoPrincipalLog(TypeLog = typeof(EmpresaLog),
                        OBSMessageResourceType = typeof(Resources.Resources),
                        OBSMessageResourceName = "LOGOBS_EMPRESA")]
    public class Empresa : BaseModel, IEndereco
    {
        private ICollection<EmpresaTelefone> _listaTelefones;

        public virtual Int32 Codigo { get; set; }

        public virtual string Fantasia { get; set; }

        public virtual string RazaoSocial { get; set; }

        public virtual string CNPJ { get; set; }

        #region IEndereco
        public virtual string Cep { get; set; }

        public virtual string Pais { get; set; }

        public virtual string Estado { get; set; }

        public virtual string Cidade { get; set; }

        public virtual string Bairro { get; set; }

        public virtual string Logradouro { get; set; }

        public virtual string Numero { get; set; }

        public virtual string Complemento { get; set; }

        public virtual double Longitude { get; set; }

        public virtual double Latitude { get; set; }

        #endregion

        public virtual ICollection<EmpresaTelefone> Telefones
        {
            get
            {
                if (_listaTelefones == null)
                    _listaTelefones = new List<EmpresaTelefone>();
                return _listaTelefones;
            }
            set
            {
                _listaTelefones = value;
            }
        }

    }
}
