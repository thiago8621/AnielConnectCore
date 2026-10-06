using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class BancoMap : ClassMapBase<Banco>
    {
        public BancoMap() : base()
        {
            Table("TB_BANCO");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_BANCO");

            Map(x => x.Descricao, "DESCRICAO").Length(100);
            Map(x => x.Codigo, "CODIGO").Length(5);
            Map(x => x.Status, "STATUS").CustomType<Status_Default>();
            Map(x => x.Carteira, "CARTEIRA");
            Map(x => x.Nosso_Numero_Max, "NOSSO_NUMERO_MAX");

            Map(x => x.ConvenioPadraoCNAB, "CONVENIO_PADRAO_CNAB");
            Map(x => x.ConvenioCarteiraCodigo, "CONVENIO_CARTEIRA_CODIGO");
            Map(x => x.TransmissaoInstantania, "TRANSMISSAO_INSTANTANIA");
            Map(x => x.ConvenioAPIKEY, "CONVENIO_API_KEY");
            Map(x => x.ConvenioAPILD, "CONVENIO_API_LD");
            Map(x => x.ConvenioAPISecret, "CONVENIO_API_SECRET");
            Map(x => x.CampoEstacao, "CAMPO_ESTACAO");

            Map(x => x.PagamentoAutomatico, "UTILIZA_PAG_AUTOMATICO");
            Map(x => x.Chave, "CHAVE");
            Map(x => x.ClientID, "CLIENT_ID");
            Map(x => x.ClientSecret, "CLIENT_SECRET");

            Map(x => x.NomeCertificado, "NOME_CERTIFICADO");
            Map(x => x.Senha, "SENHA");
            Map(x => x.Email, "EMAIL");

            Map(x => x.NomeCertificadoCP, "NOME_CERTIFICADO_CP");
            Map(x => x.SenhaCP, "SENHA_CP");
            Map(x => x.EmailCP, "EMAIL_CP");
            Map(x => x.CodigoCOMPE, "NUM_COMPE").Length(5);
            Map(x => x.CodigoISPB, "NUM_ISPB").Length(10);
        }
    }
}
