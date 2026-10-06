using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;

namespace Sinapse.Model
{
    public class ParametrosConsultaCredito : BaseModel
    {

        public virtual Int64 PontosPrimeiraFaixaCriterio { get; set; }
        public virtual Int64 PontosSegundaFaixaCriterio { get; set; }
        public virtual Int64 PontosTerceiraFaixaCriterio { get; set; }
        public virtual Boolean UtilizaFaixaCriterio1 { get; set; }
        public virtual Boolean UtilizaFaixaCriterio2 { get; set; }
        public virtual Boolean UtilizaFaixaCriterio3 { get; set; }
        public virtual AprovacaoCriterioParametroCredito TipoAprovacaoFaixa1 { get; set; }
        public virtual AprovacaoCriterioParametroCredito TipoAprovacaoFaixa2 { get; set; }
        public virtual AprovacaoCriterioParametroCredito TipoAprovacaoFaixa3 { get; set; }

        private IList<CriteriosParamCredito> _listacriterios;
        public virtual IList<CriteriosParamCredito> listaCriterios
        {
            get
            {
                if (_listacriterios == null)
                    _listacriterios = new List<CriteriosParamCredito>();
                return _listacriterios;
            }
            set
            {
                _listacriterios = value;
            }
        }

    }
}
