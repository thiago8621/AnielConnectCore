using Sinapse.DAO.map;
using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Aniel.Connect.DAO.map
{
    public class POPMap : ClassMapBase<POP>
    {
        public POPMap() : base()
        {
            Table("TB_POP");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_POP");

            Map(p => p.Nome, "NOME");
            Map(p => p.CodigoAuxiliar, "CODIGO_AUXILIAR");

            References(p => p.Empresa, "ID_EMPRESA");
            References(p => p.Filial, "ID_FILIAL");

            Map(p => p.CodCt, "CODCT");
            Map(p => p.Projeto, "PROJETO");

            Map(p => p.Status, "STATUS").CustomType<Status_Default>();

            #region Endereço

            Map(p => p.Cep, "CEP");
            
            Map(p => p.Endereco, "ENDERECO");
            Map(p => p.Numero, "NUMERO");
            
            Map(p => p.IdBairro, "ID_BAIRRO");
            Map(p => p.Bairro, "BAIRRO");

            Map(p => p.IdCidade, "ID_CIDADE");
            Map(p => p.Cidade, "CIDADE");

            Map(p => p.IdPais, "ID_PAIS");
            Map(p => p.Pais, "PAIS");
            Map(p => p.UF, "UF");
            Map(p => p.Complemento, "COMPLEMENTO");

            Map(p => p.Latitude, "LATITUDE");
            Map(p => p.Longitude, "LONGITUDE");

            #endregion
        }
    }
}
