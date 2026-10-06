using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;
using System;
using System.Collections.Generic;

namespace Sinapse.Model.AspNetCore
{
    public class Perfil_DashBoard_Connect : BaseModel
    {

        public virtual String Descricao { get; set; }
        public virtual DashBoard_Gerencial DashBoard_Gerencial { get; set; }
        public virtual DashboardInicialConnect? Dashboard_Inicial { get; set; }        
        public virtual Perfil_DashBoard_Sistema Sistema { get; set; }
        private  IList<Detalhes_Dash_Gerencial> _detalhes_Dash_Gerencial { get; set; }
        public virtual IList<Detalhes_Dash_Gerencial> Detalhes_Dash_Gerencial
        {
            get
            {
                if (_detalhes_Dash_Gerencial == null)
                    _detalhes_Dash_Gerencial = new List<Detalhes_Dash_Gerencial>();
                return _detalhes_Dash_Gerencial;
            }
            set
            {
                _detalhes_Dash_Gerencial = value;
            }
        }



        private IList<Detalhes_Dash_Inicial> _detalhes_Dash_Inicial;

        public virtual IList<Detalhes_Dash_Inicial> Detalhes_Dash_Inicial
        {
            get
            {
                if (_detalhes_Dash_Inicial == null)
                    _detalhes_Dash_Inicial = new List<Detalhes_Dash_Inicial>();
                return _detalhes_Dash_Inicial;
            }
            set
            {
                _detalhes_Dash_Inicial = value;
            }
        }



        public virtual Status_Default Status { get; set; }
    }
}
