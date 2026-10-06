using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.Model.AspNetCore
{
    public class OLT : BaseModel
    {
        public virtual Int64 OLTID { get; set; }
        public virtual String Descricao { get; set; }
        public virtual Marca_Modelo_OLT Marca_Modelo { get; set; }
        public virtual Versao_OLT? Versao_OLT { get; set; }
        public virtual String NumOLT { get; set; }
        public virtual String CodigoAuxiliar { get; set; }
        public virtual POP POP { get; set; }

        #region Dados Comunicação

        public virtual String IP { get; set; }
        public virtual String Login { get; set; }
        public virtual String Senha { get; set; }
        public virtual Int32? PortaSSH { get; set; }

        public virtual Int32? PortaTelnet { get; set; }
        public virtual Int32? PortaAPI { get; set; }
        public virtual Int32? PortaHTTP { get; set; }

        #endregion

        #region Dados Servidor TL1

        public virtual String IpTL1 { get; set; }
        public virtual String LoginTL1 { get; set; }
        public virtual String SenhaTL1 { get; set; }
        public virtual Int32? PortaTL1 { get; set; }
        public virtual String Gabinete { get; set; }
        public virtual String Subrack { get; set; }

        #endregion

        public virtual Filial Filial { get; set; }

        public virtual Status_Default Status { get; set; }
        public virtual Status_Default ConverteOlt { get; set; }

        public virtual String DHCP_DNS_Primario { get; set; }
        public virtual String DHCP_DNS_Secundario { get; set; }
        public virtual String DHCP_Lan_IP { get; set; }
        public virtual String DHCP_Lan_Mascara { get; set; }
        public virtual String DHCP_Inicio_Range { get; set; }
        public virtual String DHCP_Final_Range { get; set; }

        public virtual String Teste_IP1 { get; set; }
        public virtual String Teste_IP2 { get; set; }

        public virtual ONU_Tipo_Identificacao? Tipo_Identificacao { get; set; }
        public virtual Tipo_OLT? Tipo { get; set; }
        private IList<OLT_Grupo_Area> _gruposarea;
        public virtual IList<OLT_Grupo_Area> GruposArea
        {
            get
            {
                if (_gruposarea == null)
                    _gruposarea = new List<OLT_Grupo_Area>();
                return _gruposarea;
            }
            set
            {
                _gruposarea = value;
            }
        }
    }
}
