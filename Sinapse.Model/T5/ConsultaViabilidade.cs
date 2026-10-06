using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;

namespace Sinapse.Model
{
    /// <summary>
    /// classe de teste.
    /// </summary>
    
    public class ConsultaViabilidade : BaseModel
    {
        private IList<ConsultaViabilidadeDados> _dadosConsulta;
        private IList<ConsultaViabilidadeMotivo> _motivos;
        private IList<ConsultaViabilidadeEtapa> _etapas;
        //private int _numEtapasVinculadas;

        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
        public virtual StatusConsultaViabilidade Status { get; set; }
        /// <summary>
        /// Dados agora estão presentes na Etapa. Porém vendas anteriores ainda podem conter dados nessa propriedade.
        /// </summary>
        public virtual IList<ConsultaViabilidadeDados> DadosConsulta
        {
            get
            {
                if (this._dadosConsulta == null)
                    this._dadosConsulta = new List<ConsultaViabilidadeDados>();

                return this._dadosConsulta;
            }

            set
            {
                this._dadosConsulta = value;
            }
        }
        /// <summary>
        /// Motivos também estão presentes na Etapa. Atualmente somente os motivos que constam da configuração caem nessa listagem.
        /// </summary>
        public virtual IList<ConsultaViabilidadeMotivo> Motivos
        {
            get
            {
                if (this._motivos == null)
                    this._motivos = new List<ConsultaViabilidadeMotivo>();

                return this._motivos;
            }

            set
            {
                this._motivos = value;
            }
        }
        /// <summary>
        /// Instancia atual necessária para o fluxo de consulta de viabilidade
        /// </summary>
        public virtual IList<ConsultaViabilidadeEtapa> Etapas
        {
            get
            {
                if (this._etapas == null) 
                {
                    this._etapas = new List<ConsultaViabilidadeEtapa>();
                }
                //this._numEtapasVinculadas = this._etapas.Count;
                return this._etapas;
            }

            set
            {
                this._etapas = value;
            }
        }
    }
}
