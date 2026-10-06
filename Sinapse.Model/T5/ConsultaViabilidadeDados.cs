using Sinapse.Model.nhibernate;
using System.Collections.Generic;

namespace Sinapse.Model
{
    public class ConsultaViabilidadeDados : BaseModel
    {
        private IList<ConsultaViabilidadePortaDisponivel> _portasDisponiveis;


        public virtual ConsultaViabilidade Consulta { get; set; }
        public virtual ConsultaViabilidadeEtapa Etapa { get; set; }
        public virtual Elemento_Rede Caixa { get; set; }
        public virtual bool PortaReservada { get; set; }
        /// <summary>
        ///     Distância em quilômetros da latitude e longitude do endereço de instalação para
        ///     a latitude e longitude da caixa
        /// </summary>
        public virtual double Distancia { get; set; }
        public virtual IList<ConsultaViabilidadePortaDisponivel> PortasDisponiveis
        {
            get
            {
                if (this._portasDisponiveis == null)
                    this._portasDisponiveis = new List<ConsultaViabilidadePortaDisponivel>();

                return this._portasDisponiveis;
            }

            set
            {
                this._portasDisponiveis = value;
            }
        }
    }
}
