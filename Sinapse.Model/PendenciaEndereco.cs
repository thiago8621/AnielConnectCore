using Sinapse.Model.nhibernate;
using System.Collections.Generic;

namespace Sinapse.Model
{
    /// <summary>
    ///     Registra um consulta de endereço realizada para determinado cliente
    /// </summary>
    public class PendenciaEndereco : BaseModel
    {
        private IList<PendenciaEnderecoDados> _dadosConsulta;

        public virtual string Logradouro { get; set; }
        public virtual string Numero { get; set; }
        public virtual IList<PendenciaEnderecoDados> DadosConsulta
        {
            get
            {
                if (this._dadosConsulta == null)
                    this._dadosConsulta = new List<PendenciaEnderecoDados>();

                return this._dadosConsulta;
            }

            set
            {
                this._dadosConsulta = value;
            }
        }
    }
}
