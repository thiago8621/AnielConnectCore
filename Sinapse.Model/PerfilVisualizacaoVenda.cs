using NHibernate.Engine;
using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;

namespace Sinapse.Model
{
    /// <summary>
    ///     Perfil que limita os serviços que um usuário pode acessar baseado em parâmetros
    ///     configurados na classe <see cref="PerfilVisualizacaoVendaParam"/>
    /// </summary>
    public class PerfilVisualizacaoVenda : BaseModel
    {
        private IEnumerable<PerfilVisualizacaoVendaParamTipoPessoa> _paramsTipoPessoa;
        private IEnumerable<PerfilVisualizacaoVendaParamMeioCaptacao> _paramsMeioCaptacao;
        private IEnumerable<PerfilVisualizacaoVendaParamCategoriaCliente> _paramsCategoriaCliente;
        private IEnumerable<PerfilVisualizacaoVendaParamRegiao> _paramsRegiao;
        private IEnumerable<PerfilVisualizacaoVendaParamEquipeVenda> _paramsEquipeVenda;
        private IEnumerable<PerfilVisualizacaoVendaParamSetorDepto> _paramsSetorDepto;



        public virtual string Descricao { get; set; }
        public virtual bool PadraoVisualizaLead { get; set; }
        public virtual bool PadraoVisualizaVenda { get; set; }
        public virtual bool PerfilSolicitaAutorizacao { get; set; }
        public virtual int? Qtd_Dias_Bloqueio_Lead { get; set; }
        public virtual Status_Default Status { get; set; }
        public virtual bool PadraoVisualizaTodasTemperaturas { get; set; }
        public virtual bool TemperaturaQuente { get; set; }
        public virtual bool TemperaturaMorno { get; set; }
        public virtual bool TemperaturaFrio { get; set; }
        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de contrato/projeto associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoVendaParamTipoPessoa> ParamsTipoPessoa
        {
            get
            {
                if (this._paramsTipoPessoa == null)
                    this._paramsTipoPessoa = new List<PerfilVisualizacaoVendaParamTipoPessoa>();

                return this._paramsTipoPessoa;
            }
            set
            {
                this._paramsTipoPessoa = value;
            }
        }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de área associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoVendaParamMeioCaptacao> ParamsMeioCaptacao
        {
            get
            {
                if (this._paramsMeioCaptacao == null)
                    this._paramsMeioCaptacao = new List<PerfilVisualizacaoVendaParamMeioCaptacao>();

                return this._paramsMeioCaptacao;
            }
            set
            {
                this._paramsMeioCaptacao = value;
            }
        }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de setor associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoVendaParamSetorDepto> ParamsSetorDepto
        {
            get
            {
                if (this._paramsSetorDepto == null)
                    this._paramsSetorDepto = new List<PerfilVisualizacaoVendaParamSetorDepto>();

                return this._paramsSetorDepto;
            }
            set
            {
                this._paramsSetorDepto = value;
            }
        }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de CategoriaCliente associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoVendaParamCategoriaCliente> ParamsCategoriaCliente
        {
            get
            {
                if (this._paramsCategoriaCliente == null)
                    this._paramsCategoriaCliente = new List<PerfilVisualizacaoVendaParamCategoriaCliente>();

                return this._paramsCategoriaCliente;
            }
            set
            {
                this._paramsCategoriaCliente = value;
            }
        }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de Regiao associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoVendaParamRegiao> ParamsRegiao
        {
            get
            {
                if (this._paramsRegiao == null)
                    this._paramsRegiao = new List<PerfilVisualizacaoVendaParamRegiao>();

                return this._paramsRegiao;
            }
            set
            {
                this._paramsRegiao = value;
            }
        }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de Equipe de Venda associados ao perfil
        /// </summary>
        public virtual IEnumerable<PerfilVisualizacaoVendaParamEquipeVenda> ParamsEquipeVenda
        {
            get
            {
                if (this._paramsEquipeVenda == null)
                    this._paramsEquipeVenda = new List<PerfilVisualizacaoVendaParamEquipeVenda>();

                return this._paramsEquipeVenda;
            }
            set
            {
                this._paramsEquipeVenda = value;
            }
        }
    }
}
