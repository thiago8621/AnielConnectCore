using Sinapse.Model.AspNetCore;

namespace Sinapse.DAO.AspNetCore.map
{
    public class FuncionarioMap : ClassMapBase<Funcionario>
    {
        public FuncionarioMap() : base()
        {
            Table("TB_FUNCIONARIO");

            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.Num_Fre, "NUM_FRE").Length(6).Index("IDX_FUNCIONARIO_NUM_FRE");
            Map(x => x.Nome, "NOME").Length(50);
            Map(x => x.Tel_Celular, "TEL_CELULAR").Length(15);
            Map(x => x.Situacao, "SITUACAO").Index("IDX_FUNCIONARIO_SITUACAO");
            Map(x => x.Vendedor, "VENDEDOR").Index("IDX_FUNCIONARIO_VENDEDOR");
            Map(x => x.E_SUPERVISOR, "E_SUPERVISOR").Index("IDX_FUNCIONARIO_SUPERVISOR");
            //Map(x => x.Codigo, "CODIGO").Length(50);

            Map(x => x.Bairro, "BAIRRO");
            Map(x => x.Cep, "CEP");
            Map(x => x.Complemento, "COMPLEMENTO");
            Map(x => x.Cpf_Cnpj, "CPF");
            Map(x => x.Endereco, "ENDERECO");
            Map(x => x.Municipio, "MUNICIPIO");
            Map(x => x.Rg, "RG");
            Map(x => x.Uf, "UF");
            Map(x => x.Numero, "NUMERO");
            Map(x => x.Id_CentroCusto, "ID_CENTRO_CUSTO");

            Map(x => x.Id_Comissionamento, "ID_COMISSIONAMENTO").Index("IDX_FUNCIONARIO_COM");

            References(x => x.Meta, "ID_META").ForeignKey("FK_FUNCIONARIO_META");
            References(x => x.Faixa, "ID_FAIXA").Cascade.Evict().ForeignKey("FK_FUNCIONARIO_FAIXA");
            References(x => x.Departamento, "ID_DEPARTAMENTO").ForeignKey("FK_FUNCIONARIO_DEPTO");
            References(x => x.Supervisor, "ID_SUPERVISOR").ForeignKey("FK_FUNCIONARIO_SUPERV");
            References(x => x.Filial, "ID_FILIAL").ForeignKey("FK_FUNCIONARIO_FILIAL");

        }
    }
}
