using System;
using System.Collections.Generic;

namespace Boleto2Net.DebitoAutomatico
{
    public abstract class HeaderDebitoAutomatico
    {
        /// <summary>
        /// "A" fixo para Registro de Debitos Automaticos
        /// </summary>
        public string TipoRegistro { get; set; }
        /// <summary>
        /// "1" fixo para definicao de Arquivos Remessa
        /// </summary>
        public string CodigoRemessa { get; set; }
        /// <summary>
        /// CodigoConvenio para controle interno
        /// </summary>
        public string CodigoConvenio { get; set; }
        /// <summary>
        /// Nome do Remetente/Empresa
        /// </summary>
        public string NomeEmpresa { get; set; }
        /// <summary>
        /// Codigo Compe Banco
        /// </summary>
        public long CodigoBanco { get; set; }
        /// <summary>
        /// Nome do Banco
        /// </summary>
        public string NomeBanco { get; set; }
        /// <summary>
        /// Numero Sequencial do Arquivo (NSA)
        /// </summary>
        public long NumeroSequencialArquivo { get; set; }
        /// <summary>
        /// Versao do Layout
        /// </summary>
        public long VersaoLayout { get; set; }
        /// <summary>
        /// FIXO "DEBITO AUTOMATICO" 
        /// </summary>
        public string IdentificacaoServico { get; set; }
    }

    public abstract class DetalheDebitoAutomatico
    {
        /// <summary>
        /// Fixo "F" para retornos com status e "E" para cadastro
        /// </summary>
        public string CodigoRegistro { get; set; }
        /// <summary>
        /// Identificacao cliente
        /// </summary>
        public string IdentificacaoCliente { get; set; }
        /// <summary>
        /// agencia de debito do cliente
        /// </summary>
        public string AgenciaDebito { get; set; }
        /// <summary>
        /// Conta Corrente, Poupanca, Salario Cliente
        /// </summary>
        public string ContaCliente { get; set; }
        /// <summary>
        /// Digito da Conta, caso possuir
        /// </summary>
        public string DigitoConta { get; set; }

        /// <summary>
        /// Valor original do lancamento ou debitado
        /// </summary>
        public Decimal ValorOriginalDebito { get; set; }
        /// <summary>
        /// Codigo de Retorno "00" + Mensagem "Debito Efetuado"
        /// </summary>
        public Dictionary<string, string> CodigoRetorno { get; set; }
        /// <summary>
        /// Informacoes Complementares passada no documento
        /// </summary>
        public string InformacoesComplementares { get; set; }
        /// <summary>
        /// Tipo de identificacao, caso tenha
        /// </summary>
        public string TipoIdentificacao { get; set; }
        /// <summary>
        /// Documento da identificacao, caso tenha
        /// </summary>
        public string DocIdentificacao { get; set; }
        /// <summary>
        /// Codigo do movimento 0 = debito normal, 1 = Cancelamento
        /// </summary>
        public string CodigoMovimento { get; set; }
    }

    public abstract class TraillerDebitoAutomatico 
    {
        /// <summary>
        /// Fixo “Z” para retorno de traillers
        /// </summary>
        public string CodigoRegistro { get; set; }
        /// <summary>
        /// No somatório dos registros, deverão ser também incluídos,
        /// os registros Header e Trailler.
        /// </summary>
        public long TotalRegistro { get; set; }
        /// <summary>
        /// Somatorio dos Valores dos Detalhes enviados
        /// </summary>
        public decimal TotalValoresRegistro { get;set;}
    }

    public class HeaderDebitoConsulta : HeaderDebitoAutomatico 
    {
        /// <summary>
        /// Data de Geracao do arquivo
        /// </summary>
        public string DataGeracao { get; set; }
    }
    public class HeaderDebitoAutoCriar : HeaderDebitoAutomatico
    {
        /// <summary>
        /// Data de Geracao do arquivo
        /// </summary>
        public DateTime DataGeracao { get; set; }
    }

    public class DetalheDebitoAutoConsulta : DetalheDebitoAutomatico 
    {
        /// <summary>
        /// Data de Vencimento ou Efetivacao do Debito
        /// </summary>
        public string DataVencimento { get; set; }
    }
    public class DetalheDebitoAutoCadastro : DetalheDebitoAutomatico
    {
        /// <summary>
        /// Data de Vencimento ou Efetivacao do Debito
        /// </summary>
        public DateTime DataVencimento { get; set; }
        public long CodigoMoeda { get; set; }
    }
    public class TraillerDebitoAuto : TraillerDebitoAutomatico { }

    public class DebitoAutomaticoConsulta
    {
        public HeaderDebitoConsulta HeaderDebitoAutoConsulta { get; set; }
        public List<DetalheDebitoAutoConsulta> DetalhesDebitoConsulta { get; set; } = new List<DetalheDebitoAutoConsulta> { };
        public TraillerDebitoAuto TraillerDebitoAutoConsulta { get; set; }
    } 

    public class DebitoAutomaticoCriar
    {
        public HeaderDebitoAutoCriar HeaderDebitoAutoCadastro { get; set; }
        public List<DetalheDebitoAutoCadastro> DetalhesDebitoCadastro { get; set; } = new List<DetalheDebitoAutoCadastro>();
        public TraillerDebitoAuto TraillerDebitoAutoCadastro { get; set; }
    }
}
