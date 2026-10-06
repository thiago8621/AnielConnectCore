using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class ModeloMensagemMap : ClassMapBase<ModeloMensagem>
    {
        public ModeloMensagemMap() : base()
        {
            Table("TB_MODELO_MENSAGEM");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_MODELO_MENSAGEM");

            Map(x => x.Nome, "NOME").Length(512);
            Map(x => x.Texto, "TEXTO").Length(1024);
            Map(x => x.Status, "STATUS");
            Map(x => x.IdWhatsAppBusinessAccount, "ID_WHATS_BUSSINESS_ACCOUNT");
        }
    }
}
