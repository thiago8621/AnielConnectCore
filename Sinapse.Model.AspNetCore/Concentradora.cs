using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.Model.AspNetCore
{
    public class Concentradora : BaseModel
    {
        private IList<Concentradora_Grupo_Area> _gruposarea;
        public virtual String Nome { get; set; }
        public virtual Marca_Modelo_Concentradora? Tipo { get; set; }

        public virtual POP POP { get; set; }
        public virtual String Ip { get; set; }

        public virtual String Secret { get; set; }

        public virtual String Login { get; set; }
        public virtual String Senha { get; set; }

        public virtual Radius Radius { get; set; }

        public virtual String PortaTelnet { get; set; }
        public virtual String PortaAPI { get; set; }
        public virtual String PortaHTTP { get; set; }
        public virtual String PortaSSH { get; set; }
        public virtual String PortaIncoming { get; set; }

        public virtual IList<Concentradora_Grupo_Area> GruposArea
        {
            get
            {
                if (_gruposarea == null)
                    _gruposarea = new List<Concentradora_Grupo_Area>();
                return _gruposarea;
            }
            set
            {
                _gruposarea = value;
            }
        }
    }
}
