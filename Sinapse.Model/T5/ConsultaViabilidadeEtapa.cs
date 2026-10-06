using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System.Collections.Generic;

namespace Sinapse.Model
{
    public class ConsultaViabilidadeEtapa : BaseModel
    {
        private IList<ConsultaViabilidadeDados> _dadosConsulta;
        private IList<ConsultaViabilidadeMotivo> _motivos;        
        
        public virtual IList<ConsultaViabilidadeDados> DadosConsulta
        {
            get 
            {
                if(this._dadosConsulta == null)
                {
                    this._dadosConsulta = new List<ConsultaViabilidadeDados>();
                }
                return this._dadosConsulta;
            }
        }
        public virtual IList<ConsultaViabilidadeMotivo> Motivos
        {
            get
            {
                if (this._motivos == null)
                {
                    this._motivos = new List<ConsultaViabilidadeMotivo>();
                }
 
                return this._motivos;
            }
        }
        public virtual EtapasValidacaoConsultaViabilidadeEnum TipoEtapa { get; set; }
        public virtual ConsultaViabilidade Consulta { get; set; }
        public virtual bool GerouStatus { get; set; }
    }
}
