using Sinapse.Model.AspNetCore;

namespace Sinapse.DAO.AspNetCore.map
{
    public class EmpresaMap : ClassMapBase<Empresa>
    {
        public EmpresaMap() : base()
        {
            Table("TB_EMPRESA");

            Id(u => u.Id, "ID").GeneratedBy.Assigned();

            Map(x => x.CodEmpresa, "CODEMPRESA");
            Map(x => x.Nome, "NOME").Length(30);
            Map(x => x.Razao_Social, "RAZAO_SOCIAL").Length(60);
            Map(x => x.Logotipo, "LOGOTIPO");
            Map(x => x.Data_Alteracao, "DATA_ALTERACAO");
        }
    }
}
