using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System.Collections.Generic;

namespace Sinapse.Model
{
    /// <summary>
    ///     Perfil que limita os serviços que um usuário pode acessar baseado em parâmetros
    ///     configurados na classe <see cref="PerfilVisualizacaoServicoParam"/>
    /// </summary>
    public class PerfilVisualizacaoServico : BaseModel
    {
        private IEnumerable<PerfilVisualizacaoServicoParamContratoProjeto> _paramsContratoProjeto;
        private IEnumerable<PerfilVisualizacaoServicoParamArea> _paramsArea;
        private IEnumerable<PerfilVisualizacaoServicoParamSetorDepto> _paramsSetorDepto;
        private IEnumerable<PerfilVisualizacaoServicoParamSupervisor> _paramsSupervisor;
        private IEnumerable<PerfilVisualizacaoServicoParamMonitor> _paramsMonitor;

        public virtual string Descricao { get; set; }
        public virtual Status_Default Status { get; set; }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de contrato/projeto associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoServicoParamContratoProjeto> ParamsContratoProjeto
        {
            get
            {
                if (this._paramsContratoProjeto == null)
                    this._paramsContratoProjeto = new List<PerfilVisualizacaoServicoParamContratoProjeto>();

                return this._paramsContratoProjeto;
            }
            set
            {
                this._paramsContratoProjeto = value;
            }
        }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de área associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoServicoParamArea> ParamsArea
        {
            get
            {
                if (this._paramsArea == null)
                    this._paramsArea = new List<PerfilVisualizacaoServicoParamArea>();

                return this._paramsArea;
            }
            set
            {
                this._paramsArea = value;
            }
        }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de setor associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoServicoParamSetorDepto> ParamsSetorDepto
        {
            get
            {
                if (this._paramsSetorDepto == null)
                    this._paramsSetorDepto = new List<PerfilVisualizacaoServicoParamSetorDepto>();

                return this._paramsSetorDepto;
            }
            set
            {
                this._paramsSetorDepto = value;
            }
        }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de supervisor associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoServicoParamSupervisor> ParamsSupervisor
        {
            get
            {
                if (this._paramsSupervisor == null)
                    this._paramsSupervisor = new List<PerfilVisualizacaoServicoParamSupervisor>();

                return this._paramsSupervisor;
            }
            set
            {
                this._paramsSupervisor = value;
            }
        }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de monitor associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoServicoParamMonitor> ParamsMonitor
        {
            get
            {
                if (this._paramsMonitor == null)
                    this._paramsMonitor = new List<PerfilVisualizacaoServicoParamMonitor>();

                return this._paramsMonitor;
            }
            set
            {
                this._paramsMonitor = value;
            }
        }
    }
}
