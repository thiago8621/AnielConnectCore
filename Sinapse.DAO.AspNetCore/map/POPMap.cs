using Sinapse.Model.AspNetCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.DAO.AspNetCore.map
{
    public class POPMap : ClassMapBase<POP>
    {
        public POPMap() : base()
        {
            Table("TB_POP");

            //Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_POP");

            Map(p => p.Nome, "NOME");

            References(p => p.Empresa, "ID_EMPRESA");
            References(p => p.Filial, "ID_FILIAL");

            Map(p => p.CodCt, "CODCT");
            Map(p => p.Projeto, "PROJETO");

            #region Endereço

            Map(p => p.Cep, "CEP");

            Map(p => p.Endereco, "ENDERECO");
            Map(p => p.Numero, "NUMERO");

            Map(p => p.IdBairro, "ID_BAIRRO");
            Map(p => p.Bairro, "BAIRRO");

            Map(p => p.IdCidade, "ID_CIDADE");
            Map(p => p.Cidade, "CIDADE");

            Map(p => p.UF, "UF");
            Map(p => p.Complemento, "COMPLEMENTO");

            Map(p => p.Latitude, "LATITUDE");
            Map(p => p.Longitude, "LONGITUDE");

            #endregion
        }
    }
}
