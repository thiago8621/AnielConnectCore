using NHibernate.Engine;
using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;

namespace Sinapse.Model
{
    /// <summary>
    ///     Perfil que limita os serviços que um usuário pode acessar baseado em parâmetros
    ///     configurados na classe <see cref="ParametrosConsultaPreQualParam"/>
    /// </summary>
    public class ParametrosConsultaPreQual : BaseModel
    {
        private IEnumerable<ParametrosConsultaPreQualParamTipoPessoa> _paramsTipoPessoa;
        //private IEnumerable<ParametrosConsultaPreQualParamMeioCaptacao> _paramsMeioCaptacao;
        private IEnumerable<ParametrosConsultaPreQualParamCategoriaCliente> _paramsCategoriaCliente;
        private IEnumerable<ParametrosConsultaPreQualParamRegiao> _paramsRegiao;
        //private IEnumerable<ParametrosConsultaPreQualParamEquipeVenda> _paramsEquipeVenda;
        private IEnumerable<ParametrosConsultaPreQualParamSetorDepto> _paramsSetorDepto;


        //Usado para ordenar as listas de acordo com o maior conteúdo obtido
        public virtual int order { get; set; }
        public virtual string Descricao { get; set; }
        public virtual bool Padrao { get; set; }
        public virtual Status_Default Status { get; set; }

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de contrato/projeto associados ao perfil
        /// </summary>
        public virtual IEnumerable<ParametrosConsultaPreQualParamTipoPessoa> ParamsTipoPessoa
        {
            get
            {
                if (this._paramsTipoPessoa == null)
                    this._paramsTipoPessoa = new List<ParametrosConsultaPreQualParamTipoPessoa>();

                return this._paramsTipoPessoa;
            }
            set
            {
                this._paramsTipoPessoa = value;
            }
        }

        ///// <summary>
        /////     Lista somente para leitura dos parâmetros de filtro de área associados ao perfil
        ///// </summary>
        //public virtual IEnumerable<ParametrosConsultaPreQualParamMeioCaptacao> ParamsMeioCaptacao
        //{
        //    get
        //    {
        //        if (this._paramsMeioCaptacao == null)
        //            this._paramsMeioCaptacao = new List<ParametrosConsultaPreQualParamMeioCaptacao>();

        //        return this._paramsMeioCaptacao;
        //    }
        //    set
        //    {
        //        this._paramsMeioCaptacao = value;
        //    }
        //}

        /// <summary>
        ///     Lista somente para leitura dos parâmetros de filtro de setor associados ao perfil
        /// </summary>
        public virtual IEnumerable<ParametrosConsultaPreQualParamSetorDepto> ParamsSetorDepto
        {
            get
            {
                if (this._paramsSetorDepto == null)
                    this._paramsSetorDepto = new List<ParametrosConsultaPreQualParamSetorDepto>();

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
        public virtual IEnumerable<ParametrosConsultaPreQualParamCategoriaCliente> ParamsCategoriaCliente
        {
            get
            {
                if (this._paramsCategoriaCliente == null)
                    this._paramsCategoriaCliente = new List<ParametrosConsultaPreQualParamCategoriaCliente>();

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
        public virtual IEnumerable<ParametrosConsultaPreQualParamRegiao> ParamsRegiao
        {
            get
            {
                if (this._paramsRegiao == null)
                    this._paramsRegiao = new List<ParametrosConsultaPreQualParamRegiao>();

                return this._paramsRegiao;
            }
            set
            {
                this._paramsRegiao = value;
            }
        }

        ///// <summary>
        /////     Lista somente para leitura dos parâmetros de filtro de Equipe de Venda associados ao perfil
        ///// </summary>
        //public virtual IEnumerable<ParametrosConsultaPreQualParamEquipeVenda> ParamsEquipeVenda
        //{
        //    get
        //    {
        //        if (this._paramsEquipeVenda == null)
        //            this._paramsEquipeVenda = new List<ParametrosConsultaPreQualParamEquipeVenda>();

        //        return this._paramsEquipeVenda;
        //    }
        //    set
        //    {
        //        this._paramsEquipeVenda = value;
        //    }
        //}
    }
}
