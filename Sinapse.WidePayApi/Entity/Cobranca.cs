using Sinapse.WidePayApi.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Entity
{
    public class Cobranca
    {
        //Campos utilizados somente no retorno de consulta de cobranças
        public string id { get; set; }
        public List<HistoricoCobranca> historico { get; set; }
        public string cobranca { get; set; }
        public string carne { get; set; }
        public string assinatura { get; set; }
        public Double valor { get; set; }
        public Double? recebido { get; set; }
        public DateTime? recebimento { get; set; }
        public DateTime? compensacao { get; set; }
        public DateTime? criacao { get; set; }
        public StatusCobranca status { get; set; }
        public bool compensado { get; set; }
        //endof

        private string referencia;
        private string forma;
        private string pessoa;
        private string cliente;
        private string cpf_cnpj;
        private string vencimento;
        private string enviar;
        private string email;
        private string mensagem;
        private Boleto boleto;
        private Pix pix;
        private TipoCobranca boletoPix;

        /// <summary>
        /// Telefone do Pagador
        /// </summary>
        public string telefone { get; set; }
        /// <summary>
        /// Endereço do Pagador
        /// </summary>
        public Endereco endereco { get; set; }
        /// <summary>
        /// Itens da Cobrança (Produto, Serviço...)
        /// </summary>
        public List<Item> itens { get; set; }
        /// <summary>
        /// Define a URL que o WidePay chamará quando Cobrança muda de status
        /// </summary>
        public string notificacao { get; set; }
        /// <summary>
        /// Redirecionamento (SOMENTE QUANDO CARTÂO)
        /// Endereço URL a ser chamado após o pagamento pelo cartão
        /// </summary>
        public string redirecionamento { get; set; }

        /// <summary>
        /// Cobrança
        /// </summary>
        /// <param name="tipoCobranca">Define o tipo da cobrança (Boleto, Carnê ou Cartão)</param>
        /// <param name="referencia">Identificador de referencia no sistema interno</param>
        /// <returns></returns>
        public Cobranca(string tipoCobranca, string referencia)
        {
            this.referencia = referencia;

            if (tipoCobranca == "Cartão")
                this.forma = "Cartão";
            else
                this.forma = "Boleto,Pix";
        }

        [Newtonsoft.Json.JsonConstructor]
        public Cobranca(string forma, string cliente,
            string pessoa, string cpf, string cnpj, string email, string referencia, string vencimento,
            string status, string compensado)
        {
            this.forma = forma;
            this.cliente = cliente;
            this.pessoa = pessoa;

            if (cpf != null)
                this.cpf_cnpj = cpf;

            if (cnpj != null)
                this.cpf_cnpj = cnpj;

            this.email = email;
            this.referencia = referencia;
            this.vencimento = vencimento;

            switch (status.ToUpper())
            {
                case "AGUARDANDO":
                    this.status = StatusCobranca.Aguardando;
                    break;
                case "CANCELADO":
                    this.status = StatusCobranca.Cancelado;
                    break;
                case "EM ANÁLISE":
                    this.status = StatusCobranca.Analise;
                    break;
                case "EM ANALISE":
                    this.status = StatusCobranca.Analise;
                    break;
                case "ESTORNADO":
                    this.status = StatusCobranca.Estornado;
                    break;
                case "RECEBIDO":
                    this.status = StatusCobranca.Recebido;
                    break;
                case "RECEBIDO MANUALMENTE":
                    this.status = StatusCobranca.RecebidoManual;
                    break;
                case "RECUSADO":
                    this.status = StatusCobranca.Recusado;
                    break;
                case "VENCIDO":
                    this.status = StatusCobranca.Vencido;
                    break;
            }
            if (compensado == null)
                this.compensado = false;
            else if (compensado.ToUpper().Equals("SIM"))
                this.compensado = true;
            else
                this.compensado = false;
        }

        public Cobranca(TipoCobranca boletoPix, string referencia)
        {
            this.boletoPix = boletoPix;
            this.referencia = referencia;
        }

        /// <summary>
        /// Define o tipo do Pagador
        /// </summary>
        /// <param name="tipo">Pessoa Física ou Pessoa Jurídica</param>
        /// <param name="referencia">CPF ou CNPJ do Pagagor</param>
        /// <returns></returns>
        public void setPessoa(TipoPessoa tipo, string cpf_cnpj, string nome)
        {
            this.cpf_cnpj = cpf_cnpj;

            if (tipo == TipoPessoa.fisica)
            {
                
                this.pessoa = "Física";
            } else
            {
                this.pessoa = "Jurídica";
            }

            this.cliente = nome;
        }

        /// <summary>
        /// Data de Vencimento do Boleto
        /// </summary>
        /// <param name="data">Data de Vencimento</param>
        /// <returns></returns>
        public void setDataVencimento(DateTime? dataVencimento)
        {
            if (this.forma == "Boleto,Pix")
            {
                this.vencimento = dataVencimento.Value.ToShortDateString();
            }
            else throw new InvalidOperationException("Não é possível informar Data de Vencimento para este tipo de cobrança.");
        }
        public DateTime getDataVencimento()
        {
            return Convert.ToDateTime(this.vencimento);
        }

        /// <summary>
        /// E-Mail do Pagador
        /// </summary>
        /// <param name="email">E-Mail do Pagador</param>
        /// <param name="enviar">Define se a cobrança será enviada ao Pagador</param>
        /// /// <param name="mensagem">Define a mensagem enviada ao Pagador</param>
        /// <returns></returns>
        public void setEMail(string email, bool enviar, string mensagem)
        {
            this.email = email;

            if (enviar)
                this.enviar = "E-mail";
            else 
                this.enviar = "";

            this.mensagem = mensagem;
        }

        /// <summary>
        /// Informações adicionais do Boleto
        /// </summary>
        /// <returns></returns>
        public void setInfoBoleto(Boleto boleto)
        {
            if (this.forma == "Boleto,Pix")
            {
                this.boleto = boleto;
            }
            else throw new InvalidOperationException("Não permitido para este tipo de cobrança.");
        }

        public string getCliente()
        {
            return this.cliente;
        }

        public string getCpfCnpj()
        {
            return this.cpf_cnpj;
        }

        public string getEMail()
        {
            return this.email;
        }

        public string getReferencia()
        {
            return this.referencia;
        }

        public string getEnviar()
        {
            return this.enviar;
        }
    }
}
