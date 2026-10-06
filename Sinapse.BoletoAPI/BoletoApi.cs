using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Boleto2Net;
using NReco.PdfGenerator;
using Sinapse.BoletoAPI.Entity;
using Sinapse.BoletoAPI.Enum;

namespace Sinapse.BoletoAPI
{
    public class BoletoApi
    {
        private Boleto2Net.IBanco banco;

        /// <summary>
        /// Construtor do Boleto
        /// </summary>
        /// <param name="pessoaJuridica">Beneficiário</param>
        /// <param name="codigoBanco">Código do banco da empresa</param>
        /// <param name="agencia">Agencia da empresa</param>
        /// <param name="digitoAgencia">Digito da agencia</param>
        /// <param name="conta">Conta-Corrente da empresa</param>
        /// <param name="digitoConta">Digito da Conta-Corrente</param>
        /// <param name="codigoCedente">Código informado pelo banco da Empresa</param>
        /// <param name="dvCedente">Digito do Código Cedente</param>
        /// <param name="carteira">Código informado pelo banco da Empresa</param>
        /// <param name="posto">Posto do Banco conveniado</param>
        /// <param name="codigoBeneficiario">Codigo Beneficiario</param>
        /// <param name="instrucoesBoleto">Instrucoes para o Cliente no Boleto/remessa</param>
        public BoletoApi(Pessoa pessoaJuridica, int codigoBanco, string agencia, string digitoAgencia,
            string conta, string digitoConta, string codigoCedente, string dvCedente, string carteira,
            string posto = null, string codigoBeneficiario = null, string[] instrucoesBoleto = null)
        {
            Boleto2Net.ContaBancaria ctaBancaria = new Boleto2Net.ContaBancaria();
            ctaBancaria.Agencia = agencia;
            ctaBancaria.DigitoAgencia = digitoAgencia;
            ctaBancaria.Conta = conta;
            ctaBancaria.DigitoConta = digitoConta;
            ctaBancaria.CarteiraPadrao = carteira;
            ctaBancaria.TipoCarteiraPadrao = Boleto2Net.TipoCarteira.CarteiraCobrancaSimples;
            ctaBancaria.TipoFormaCadastramento = Boleto2Net.TipoFormaCadastramento.ComRegistro;
            ctaBancaria.TipoImpressaoBoleto = Boleto2Net.TipoImpressaoBoleto.Empresa;
            ctaBancaria.OperacaoConta = posto;
            Boleto2Net.Cedente cedente = new Boleto2Net.Cedente();
            cedente.ContaBancaria = ctaBancaria;
            cedente.CPFCNPJ = pessoaJuridica.cpfcnpj;
            cedente.Nome = pessoaJuridica.nome;
            cedente.Codigo = codigoCedente;
            cedente.CodigoDV = dvCedente;
            cedente.CodigoBeneficiario = codigoBeneficiario;
            cedente.Endereco = new Boleto2Net.Endereco
            {
                Bairro = pessoaJuridica.endereco.bairro,
                CEP = pessoaJuridica.endereco.cep,
                UF = pessoaJuridica.endereco.estado,
                Cidade = pessoaJuridica.endereco.cidade,
                LogradouroEndereco = pessoaJuridica.endereco.endereco,
                LogradouroComplemento = pessoaJuridica.endereco.complemento,
                LogradouroNumero = pessoaJuridica.endereco.numero
            };
            banco = Boleto2Net.Banco.Instancia(codigoBanco);
            banco.Cedente = cedente;
            banco.Cedente.InstrucoesRemessa = instrucoesBoleto;
            banco.Cedente.CodigoBeneficiario = codigoBeneficiario;
            banco.Cedente.CodigoPosto = posto;
            banco.FormataCedente();
        }


        public BoletoApi(int codigoBanco, string carteiraPadrao)
        {
            Boleto2Net.ContaBancaria ctaBancaria = new Boleto2Net.ContaBancaria();
            ctaBancaria.CarteiraPadrao = carteiraPadrao;
            ctaBancaria.TipoCarteiraPadrao = Boleto2Net.TipoCarteira.CarteiraCobrancaSimples;
            ctaBancaria.VariacaoCarteiraPadrao = "";

            Boleto2Net.Cedente cedente = new Boleto2Net.Cedente();
            cedente.ContaBancaria = ctaBancaria;

            banco = Boleto2Net.Banco.Instancia(codigoBanco);
            banco.Cedente = cedente;
        }

        /// <summary>
        /// Método para geração do boleto manual
        /// </summary>
        /// <param name="cob">Informações da cobrança que será gerada</param>
        /// <param name="tipoBoleto">Tipo de Boleto que se deseja gerar (PDF,HTML ou REM)</param>
        /// <param name="nossoNumero">Código de controle que permite ao Banco e ao beneficiário identificar os dados da cobrança que deu origem ao boleto de pagamento</param>
        public BoletoResponse gerarCobranca(Cobranca cob, TipoBoleto tipoBoleto, string nossoNumero, bool carne = false) //, int? numRemessa = null
        {
            StringBuilder msgCaixa = new StringBuilder();

            Boleto2Net.Boleto boleto = new Boleto2Net.Boleto(banco);
            boleto.Sacado = new Boleto2Net.Sacado
            {
                CPFCNPJ = cob.sacado.cpfcnpj,
                Nome = cob.sacado.nome,
                Endereco = new Boleto2Net.Endereco
                {
                    Bairro = cob.sacado.endereco.bairro,
                    UF = cob.sacado.endereco.estado,
                    Cidade = cob.sacado.endereco.cidade,
                    CEP = cob.sacado.endereco.cep,
                    LogradouroEndereco = cob.sacado.endereco.endereco,
                    LogradouroNumero = cob.sacado.endereco.numero,
                    LogradouroComplemento = cob.sacado.endereco.complemento
                }
            };

            boleto.Avalista = boleto.Sacado;
            boleto.DataEmissao = DateTime.Now;
            boleto.DataProcessamento = DateTime.Now;
            boleto.DataVencimento = cob.vencimento;
            boleto.ValorTitulo = Convert.ToDecimal(cob.valor);
            boleto.NossoNumero = nossoNumero.ToString();
            boleto.NumeroDocumento = nossoNumero; //String.Format("BB{0}{1}",nossoNumero.ToString("D6"),(char)(64 + 1));
            boleto.EspecieDocumento = Boleto2Net.TipoEspecieDocumento.DM;
            boleto.Aceite = "A";

            if ((cob.dataDesconto != null) && (cob.desconto != null))
            {
                boleto.DataDesconto = cob.dataDesconto.Value;
                boleto.ValorDesconto = Convert.ToDecimal(cob.desconto);
                msgCaixa.AppendLine($"Conceder desconto de {boleto.ValorDesconto.ToString("R$ ##,##0.00")} até {boleto.DataDesconto.ToString("dd/MM/yyyy")}. ");
            }

            if (cob.multa != null)
            {
                boleto.ValorMulta = Convert.ToDecimal(cob.multa);

                msgCaixa.AppendLine($"Cobrar multa de {boleto.ValorMulta.ToString("R$ ##,##0.00")} após o vencimento. ");
            }

            if (cob.juros != null)
            {
                boleto.ValorJurosDia = Convert.ToDecimal(cob.juros);

                msgCaixa.AppendLine($"Cobrar juros de {boleto.ValorJurosDia.ToString("R$ ##,##0.00")} por dia de atraso. ");
            }

            msgCaixa.AppendLine(cob.instrucoes);

            boleto.MensagemInstrucoesCaixa = msgCaixa.ToString();

            boleto.ValidarDados();

            BoletoResponse resposta = new BoletoResponse();

            boleto.MensagemArquivoRemessa = "Mensagem para o arquivo remessa";
            boleto.NumeroControleParticipante = "CHAVEPRIMARIA =" + boleto.NossoNumero;
            boleto.InstrucoesBoleto = cob.InstrucoesCob;
            boleto.SeuNumero = cob.SeuNumero;

            Boleto2Net.Boletos boletos = new Boleto2Net.Boletos();
            boletos.Add(boleto);

            switch (tipoBoleto)
            {
                case TipoBoleto.HTML: resposta.html = gerarBoletoHTML(boleto, carne); break;
                case TipoBoleto.PDF: resposta.pdf = gerarBoletoPDF(boleto, carne); break;
                case TipoBoleto.REM: break; //resposta.rem = numRemessa != null ? gerarBoletoREM(boletos,numRemessa.Value) : throw new NullReferenceException("Número da Remessa não informado"); break;
                default:
                    resposta.html = gerarBoletoHTML(boleto, carne);
                    resposta.pdf = gerarBoletoPDF(boleto, carne);
                    //resposta.rem = numRemessa != null ? gerarBoletoREM(boletos, numRemessa.Value) : throw new NullReferenceException("Número da Remessa não informado");
                    break;
            }

            resposta.boleto = boleto;
            resposta.linhaDigitavel = boleto.CodigoBarra.LinhaDigitavel;

            // Aqui 
            resposta.boletos.Add(boleto);


            return resposta;
        }


        public async Task<Boleto2Net.Boleto> GerarBoletoCancelamento(Sinapse.BoletoAPI.Entity.Cobranca cob, string nossoNumero)
        {
            StringBuilder msgCaixa = new StringBuilder();

            Boleto2Net.Boleto boleto = new Boleto2Net.Boleto(banco);
            boleto.Sacado = new Boleto2Net.Sacado
            {
                CPFCNPJ = cob.sacado.cpfcnpj,
                Nome = cob.sacado.nome,
                Endereco = new Boleto2Net.Endereco
                {
                    Bairro = cob.sacado.endereco.bairro,
                    UF = cob.sacado.endereco.estado,
                    Cidade = cob.sacado.endereco.cidade,
                    CEP = cob.sacado.endereco.cep,
                    LogradouroEndereco = cob.sacado.endereco.endereco,
                    LogradouroNumero = cob.sacado.endereco.numero,
                    LogradouroComplemento = cob.sacado.endereco.complemento
                }
            };

            boleto.Avalista = boleto.Sacado;
            boleto.DataEmissao = DateTime.Now;
            boleto.DataProcessamento = DateTime.Now;
            boleto.DataVencimento = cob.vencimento;
            boleto.ValorTitulo = Convert.ToDecimal(cob.valor);
            boleto.NossoNumero = nossoNumero.ToString();
            boleto.NumeroDocumento = nossoNumero;
            boleto.EspecieDocumento = Boleto2Net.TipoEspecieDocumento.DM;
            boleto.Aceite = "A";

            if ((cob.dataDesconto != null) && (cob.desconto != null))
            {
                boleto.DataDesconto = cob.dataDesconto.Value;
                boleto.ValorDesconto = Convert.ToDecimal(cob.desconto);
                msgCaixa.AppendLine($"Conceder desconto de {boleto.ValorDesconto.ToString("R$ ##,##0.00")} até {boleto.DataDesconto.ToString("dd/MM/yyyy")}. ");
            }

            if (cob.multa != null)
            {
                boleto.ValorMulta = Convert.ToDecimal(cob.multa);

                msgCaixa.AppendLine($"Cobrar multa de {boleto.ValorMulta.ToString("R$ ##,##0.00")} após o vencimento. ");
            }

            if (cob.juros != null)
            {
                boleto.ValorJurosDia = Convert.ToDecimal(cob.juros);

                msgCaixa.AppendLine($"Cobrar juros de {boleto.ValorJurosDia.ToString("R$ ##,##0.00")} por dia de atraso. ");
            }

            msgCaixa.AppendLine(cob.instrucoes);

            boleto.MensagemInstrucoesCaixa = msgCaixa.ToString();

            boleto.ValidarDados();

            BoletoResponse resposta = new BoletoResponse();

            boleto.MensagemArquivoRemessa = "Mensagem para o arquivo remessa";
            boleto.NumeroControleParticipante = "CHAVEPRIMARIA =" + boleto.NossoNumero;
            boleto.InstrucoesBoleto = cob.InstrucoesCob;
            boleto.SeuNumero = cob.SeuNumero;

            return boleto;
        }


        public List<BoletoResponse> lerRetornoRemessa(Stream arqv)
        {
            List<BoletoResponse> resposta = new List<BoletoResponse>();

            Boleto2Net.ArquivoRetorno retorno;

            if (banco.Codigo == Boleto2Net.Banco.Instancia(Boleto2Net.Bancos.Santander).Codigo)
                retorno = new Boleto2Net.ArquivoRetorno(banco, Boleto2Net.TipoArquivo.CNAB240);
            else if (banco.Codigo == Boleto2Net.Banco.Instancia(Boleto2Net.Bancos.Caixa).Codigo)
                retorno = new Boleto2Net.ArquivoRetorno(banco, Boleto2Net.TipoArquivo.CNAB240);
            else
                retorno = new Boleto2Net.ArquivoRetorno(banco, Boleto2Net.TipoArquivo.CNAB400);

            foreach (Boleto2Net.Boleto b in retorno.LerArquivoRetorno(arqv))
            {
                resposta.Add(new BoletoResponse()
                {
                    boleto = b
                });
            }

            return resposta;
        }


        /// <summary>
        ///  Retorna no primeiro parametro a lista de erros, no segundo com byte do arquivo gerado
        /// </summary>
        /// <param name="boletos"></param>
        /// <param name="numRemessa"></param>
        /// <returns></returns>
        public (Dictionary<string, string>, byte[]) gerarBoletoREM(Boleto2Net.Boletos boletos, 
                                                                   int numRemessa, 
                                                                   bool instrucaoCancelameto = false)
        {
            Boleto2Net.ArquivoRemessa arquivoRemessa;

            if (banco.Codigo == Boleto2Net.Banco.Instancia(Boleto2Net.Bancos.Santander).Codigo)
                arquivoRemessa = new Boleto2Net.ArquivoRemessa(banco, Boleto2Net.TipoArquivo.CNAB240, numRemessa);
            else if (banco.Codigo == Boleto2Net.Banco.Instancia(Boleto2Net.Bancos.Caixa).Codigo)
                arquivoRemessa = new Boleto2Net.ArquivoRemessa(banco, Boleto2Net.TipoArquivo.CNAB240, numRemessa);
            else
                arquivoRemessa = new Boleto2Net.ArquivoRemessa(banco, Boleto2Net.TipoArquivo.CNAB400, numRemessa);

            MemoryStream memory = new MemoryStream();

            var response = arquivoRemessa.GerarArquivoRemessa(boletos, memory, instrucaoCancelameto);

            return (response, memory.ToArray());
        }

        private byte[] gerarBoletoPDF(Boleto2Net.Boleto boleto, bool carne)
        {
            string boletoHTML = gerarBoletoHTML(boleto, carne);

            byte[] boletoPDF = new HtmlToPdfConverter().GeneratePdf(boletoHTML);

            return boletoPDF;
        }

        public byte[] gerarBoletoPDF(string boletoHTML)
        {
            var converter = new HtmlToPdfConverter();
            converter.Margins.Top = 0;
            converter.Margins.Bottom = 0;
            byte[] boletoPDF = converter.GeneratePdf(boletoHTML);

            return boletoPDF;
        }


        private string gerarBoletoHTML(Boleto2Net.Boleto boleto, bool carne = false)
        {
            using (Boleto2Net.BoletoBancario bb = new Boleto2Net.BoletoBancario())
            {
                if (!carne)
                {
                    bb.Boleto = boleto;
                    bb.OcultarInstrucoes = false;
                    bb.MostrarComprovanteEntrega = false;
                    bb.MostrarEnderecoCedente = false;
                }
                else
                {
                    bb.Boleto = boleto;
                    bb.OcultarInstrucoes = true;
                    bb.MostrarComprovanteEntrega = false;
                    bb.MostrarEnderecoCedente = false;
                    bb.ExibirDemonstrativo = false;
                    bb.FormatoCarne = true;
                    bb.OcultarReciboSacado = true;
                    bb.OcultarEnderecoSacado = true;
                }


                StringBuilder html = new StringBuilder();

                html.Append(bb.MontaHtml());

                return html.ToString();
            }
        }
    }
}
