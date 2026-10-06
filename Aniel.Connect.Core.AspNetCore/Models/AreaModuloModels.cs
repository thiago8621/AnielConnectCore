using System;
using System.Collections.Generic;

namespace Aniel.Connect.AspNetCore.Models.T5
{
    public class AreaModuloModel
    {
        public virtual String Sigla_Modulo { get; set; }
        public virtual String Cod_Area { get; set; }
        public virtual String Descricao { get; set; }
        public virtual String Sub_Acesso { get; set; }
    }

    public class AcessoFilial
    {
        List<AcessoFilialDet> _filiais;
        public virtual String Cod_Area { get; set; }
        public virtual Boolean Acesso { get; set; }
        public virtual List<AcessoFilialDet> Filiais
        {
            get
            {
                if (_filiais == null)
                    _filiais = new List<AcessoFilialDet>();
                return _filiais;
            }
            set
            {
                _filiais = value;
            }
        }
    }

    public class AcessoFilialDet
    {
        // Código Empresa Aniel
        public virtual Int16 CodEmpresa { get; set; }
        // Id Filial Connect
        public virtual Int64 Id_Filial { get; set; }
    }
}