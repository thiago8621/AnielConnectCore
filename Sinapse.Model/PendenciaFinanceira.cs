using Sinapse.Model.nhibernate;
using System.Collections.Generic;

namespace Sinapse.Model
{
    /// <summary>
    ///     Registra um consulta de pendência financeira realizada para determinado cliente
    /// </summary>
    public class PendenciaFinanceira : BaseModel
    {
        private IList<PendenciaFinanceiraDados> _dadosConsulta;

        public virtual string Cpf { get; set; }
        public virtual string Cnpj { get; set; }
        public virtual IList<PendenciaFinanceiraDados> DadosConsulta
        {
            get
            {
                if (this._dadosConsulta == null)
                    this._dadosConsulta = new List<PendenciaFinanceiraDados>();

                return this._dadosConsulta;
            }

            set
            {
                this._dadosConsulta = value;
            }
        }
    }
}
