using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerado;

namespace Sinapse.DAO.AspNetCore.map
{
    public class FilialMap : ClassMapBase<Filial>
    {
        public FilialMap() : base()
        {
            Table("TB_FILIAL");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_FILIAL");

            Map(x => x.CodFilial, "CODFILIAL");
            Map(x => x.Nome, "NOME").Length(40);
            Map(x => x.Cnpj, "CNPJ").Length(18);
            Map(x => x.CodigoCedente, "CODIGOCEDENTE").Length(12);
            Map(x => x.DigitoVerificadorCedente, "DIGITOVERIFICADORCEDENTE").Length(10);
            Map(x => x.Carteira, "CARTEIRA").Length(10);
            
            Map(x => x.Endereco, "ENDERECO").Length(50);
            Map(X => X.Complemento, "COMPLEMENTO").Length(50);
            Map(x => x.Numero, "NUMERO");
            Map(x => x.Cidade, "CIDADE").Length(20);
            Map(x => x.Bairro, "BAIRRO").Length(20);
            Map(x => x.Cep, "CEP").Length(10);
            Map(x => x.Uf, "UF").Length(2);

            Map(x => x.Natureza_tributacao, "NATUREZA_TRIBUTACAO").CustomType<Natureza_TributacaoEnum>();
            Map(x => x.Razao_Social, "RAZAO_SOCIAL").Length(100);
            Map(x => x.Optante_Simples_Nacional, "OPTANTE_SIMPLES");
            Map(x => x.Regime_Tributario, "REGIME_TRIBUTARIO").CustomType<Regime_TributarioEnum>();
            Map(x => x.Incentivo_Cultural, "INCENTIVO_CULTURAL");
            Map(x => x.Incentivo_Fiscal, "INCENTIVO_FISCAL");
            Map(x => x.Regime_Tributario_Especial, "REGIME_TRIBUTARIO_ESPECIAL").CustomType<Regime_Tributario_EspecialEnum>();
            Map(x => x.DDD, "DDD").Length(5);
            Map(x => x.Email, "EMAIL_NFE").Length(100);
            Map(x => x.CNAE_Fiscal, "CENAE_FISCAL").Length(20);
            Map(x => x.Tipo_Tributacao_ISS, "TIPO_TRIBUTACAO_ISS").CustomType<Tipo_Tributacao_ISS>();
            Map(x => x.Exigibilidade_ISS, "EXIGIBILIDADE_ISS").CustomType<Exigibilidade_ISS>();
            Map(x => x.Num_Processo_Suspensao, "NUM_PROCESSO_SUSPENSAO").Length(20);

            Map(x => x.Inscricao_Municipal, "INSCRICAO_MUNICIPAL").Length(15);
            Map(x => x.Inscricao_Estadual, "INSCRICAO").Length(15);
            Map(x => x.Telefone, "TELEFONE").Length(15);

            References(x => x.Empresa, "ID_EMPRESA").ForeignKey("FK_FILIAL_EMPRESA");
            References(x => x.Faixa_Comissionamento, "ID_FAIXA_COMISSIONAMENTO").ForeignKey("FK_FILIAL_FAIXA");
            References(x => x.Comissionamento, "ID_COMISSIONAMENTO").ForeignKey("FK_FILIAL_COMISSIONAMENTO");
            References(x => x.Meta, "ID_META").ForeignKey("FK_FILIAL_META");
        }
    }
}
