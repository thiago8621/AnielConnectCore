using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.UI;
using Boleto2Net.DebitoAutomatico;
using Boleto2Net.Exceptions;
using Boleto2Net.Extensions;
using static System.String;

[assembly: WebResource("BoletoNet.Imagens.104.jpg", "image/jpg")]

namespace Boleto2Net
{
    internal sealed class BancoCaixa : IBanco
    {
        internal static Lazy<IBanco> Instance { get; } = new Lazy<IBanco>(() => new BancoCaixa());

        public Cedente Cedente { get; set; }
        public int Codigo { get; } = 104;
        public string Nome { get; } = "Caixa Econ�mica Federal";
        public string Digito { get; } = "0";
        public List<string> IdsRetornoCnab400RegistroDetalhe { get; } = new List<string>();
        public bool RemoveAcentosArquivoRemessa { get; } = true;

        public BancoCaixa()
        {
            IdsRetornoCnab400RegistroDetalhe.Add("1");
        }

        public void FormataCedente()
        {
            var contaBancaria = Cedente.ContaBancaria;
            if (!CarteiraFactory<BancoCaixa>.CarteiraEstaImplementada(contaBancaria.CarteiraComVariacaoPadrao))
                throw Boleto2NetException.CarteiraNaoImplementada(contaBancaria.CarteiraComVariacaoPadrao);

            contaBancaria.FormatarDados("PREFERENCIALMENTE NAS CASAS LOTERICAS ATE O VALOR LIMITE", "", "SAC CAIXA: 0800 726 0101 (informa��es, reclama��es, sugest�es e elogios)<br>Para pessoas com defici�ncia auditiva ou de fala: 0800 726 2492<br>Ouvidoria: 0800 725 7474<br>caixa.gov.br<br>", 6);

            var codigoCedente = Cedente.Codigo;
            Cedente.Codigo = codigoCedente.Length <= 6 ? codigoCedente.PadLeft(6, '0') : throw Boleto2NetException.CodigoCedenteInvalido(codigoCedente, 9);

            if (Cedente.CodigoDV == Empty)
                throw new Exception($"D�gito do c�digo do cedente ({codigoCedente}) n�o foi informado.");

            Cedente.CodigoFormatado = $"{contaBancaria.Agencia} / {codigoCedente}-{Cedente.CodigoDV}";

        }

        public void ValidaBoleto(Boleto boleto)
        {
        }

        public void FormataNossoNumero(Boleto boleto)
        {
            var carteira = CarteiraFactory<BancoCaixa>.ObterCarteira(boleto.Carteira);
            carteira.FormataNossoNumero(boleto);
        }

        public string FormataCodigoBarraCampoLivre(Boleto boleto)
        {
            var carteira = CarteiraFactory<BancoCaixa>.ObterCarteira(boleto.Carteira);
            return carteira.FormataCodigoBarraCampoLivre(boleto);
        }

        public string GerarHeaderRemessa(TipoArquivo tipoArquivo, int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                var header = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        // Cabe�alho do Arquivo
                        header += GerarHeaderRemessaCNAB240SIGCB(numeroArquivoRemessa, ref numeroRegistroGeral);
                        // Cabe�alho do Lote
                        header += Environment.NewLine;
                        header += GerarHeaderLoteRemessaCNAB240SIGCB(numeroArquivoRemessa, ref numeroRegistroGeral);
                        break;
                    case TipoArquivo.CNAB400:
                        header += GerarHeaderRemessaCNAB400(numeroArquivoRemessa, ref numeroRegistroGeral);
                        break;
                    default:
                        throw new Exception("Header - Tipo de arquivo inexistente.");
                }
                return header;
            }
            catch (Exception ex)
            {
                throw Boleto2NetException.ErroAoGerarRegistroHeaderDoArquivoRemessa(ex);
            }
        }

        public string GerarDetalheRemessa(TipoArquivo tipoArquivo, Boleto boleto, ref int numeroRegistro, bool instrucaoCancelameto = false)
        {
            try
            {
                var detalhe = Empty;
                var strline = "";
                //base.GerarDetalheRemessa(boleto, numeroRegistro, tipoArquivo);
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        // Segmento P (Obrigat�rio)
                        detalhe += GerarDetalheSegmentoPRemessaCNAB240SIGCB(boleto, ref numeroRegistro);

                        // Segmento Q (Obrigat�rio)
                        detalhe += Environment.NewLine;
                        detalhe += GerarDetalheSegmentoQRemessaCNAB240SIGCB(boleto, ref numeroRegistro);

                        // Segmento R (Opcional)
                        strline = GerarDetalheSegmentoRRemessaCNAB240SIGCB(boleto, ref numeroRegistro);
                        if (!IsNullOrWhiteSpace(strline))
                        {
                            detalhe += Environment.NewLine;
                            detalhe += strline;
                        }

                        // Segmento S (Opcional)
                        strline = GerarDetalheSegmentoSRemessaCNAB240SIGCB(boleto, ref numeroRegistro);
                        if (!IsNullOrWhiteSpace(strline))
                        {
                            detalhe += Environment.NewLine;
                            detalhe += strline;
                        }

                        break;
                    case TipoArquivo.CNAB400:
                        detalhe += GerarDetalheRemessaCNAB400Registro1(boleto, ref numeroRegistro);
                        if (!IsNullOrWhiteSpace(strline))
                        {
                            detalhe += Environment.NewLine;
                            detalhe += strline;
                        }
                        break;
                    default:
                        throw new Exception("Caixa Econ�mica Federal - Detalhe - Tipo de arquivo inexistente.");
                }

                return detalhe;
            }
            catch (Exception ex)
            {
                throw Boleto2NetException.ErroAoGerarRegistroDetalheDoArquivoRemessa(ex);
            }
        }

        public string GerarTrailerRemessa(TipoArquivo tipoArquivo, int numeroArquivoRemessa,
            ref int numeroRegistroGeral, decimal valorBoletoGeral,
            int numeroRegistroCobrancaSimples, decimal valorCobrancaSimples,
            int numeroRegistroCobrancaVinculada, decimal valorCobrancaVinculada,
            int numeroRegistroCobrancaCaucionada, decimal valorCobrancaCaucionada,
            int numeroRegistroCobrancaDescontada, decimal valorCobrancaDescontada)
        {
            try
            {
                var trailler = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        // Trailler do Lote
                        trailler += GerarTrailerLoteRemessaCNAC240SIGCB(ref numeroRegistroGeral,
                            numeroRegistroCobrancaSimples, valorCobrancaSimples,
                            numeroRegistroCobrancaCaucionada, valorCobrancaCaucionada,
                            numeroRegistroCobrancaDescontada, valorCobrancaDescontada);
                        // Trailler do Arquivo
                        trailler += Environment.NewLine;
                        trailler += GerarTrailerRemessaCNAB240SIGCB(ref numeroRegistroGeral);
                        break;
                    case TipoArquivo.CNAB400:
                        trailler = GerarTrailerRemessaCNAB400(ref numeroRegistroGeral);
                        break;
                    default:
                        throw new Exception("Caixa Econ�mica Federal - Trailler - Tipo de arquivo inexistente.");
                }
                return trailler;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do Trailler do arquivo de REMESSA.", ex);
            }
        }

        #region SIG14 - Fun��es de apoio

        #region CNAB 400

        private string GerarHeaderRemessaCNAB400(int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 001, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0002, 001, 0, "1", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0003, 007, 0, "REMESSA", ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0010, 002, 0, "01", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0012, 015, 0, "COBRANCA", ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0027, 004, 0, Cedente.ContaBancaria.Agencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0031, 006, 0, Cedente.Codigo, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0037, 010, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0047, 030, 0, Cedente.Nome, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0077, 003, 0, "104", ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0080, 015, 0, "CEF", ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0095, 006, 0, DateTime.Now, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0101, 289, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0390, 005, 0, numeroArquivoRemessa, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistroGeral, '0');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB400.", ex);
            }
        }

        private string GerarDetalheRemessaCNAB400Registro1(Boleto boleto, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 001, 0, "1", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0002, 002, 0, boleto.Banco.Cedente.TipoCPFCNPJ("00"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 014, 0, boleto.Banco.Cedente.CPFCNPJ, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 004, 0, boleto.Banco.Cedente.ContaBancaria.Agencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0022, 006, 0, boleto.Banco.Cedente.Codigo, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0028, 001, 0, "2", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0029, 001, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0030, 002, 0, Empty, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0032, 025, 0, boleto.NumeroControleParticipante, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0057, 002, 0, "14", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0059, 015, 0, boleto.NossoNumero, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 002, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0076, 001, 0, "1", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0077, 030, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0107, 002, 0, "01", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0109, 002, 0, boleto.CodigoOcorrencia, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0111, 010, 0, boleto.NumeroDocumento, ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0121, 006, 0, boleto.DataVencimento, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0127, 013, 2, boleto.ValorTitulo, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0140, 003, 0, "104", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0143, 005, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0148, 002, 0, (int)boleto.EspecieDocumento, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0150, 001, 0, boleto.Aceite, ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0151, 006, 0, boleto.DataEmissao, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0157, 002, 0, boleto.CodigoInstrucao1, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0159, 002, 0, boleto.CodigoInstrucao2, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0161, 013, 2, boleto.ValorJurosDia, '0');

                if (boleto.ValorDesconto == 0)
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0174, 006, 0, "0", '0'); // Sem Desconto
                else
                    reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0174, 006, 0, boleto.DataDesconto, '0'); // Com Desconto

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0180, 013, 2, boleto.ValorDesconto, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0193, 013, 2, boleto.ValorIOF, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0206, 013, 2, boleto.ValorAbatimento, '0');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0219, 002, 0, boleto.Sacado.TipoCPFCNPJ("00"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0221, 014, 0, boleto.Sacado.CPFCNPJ, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0235, 040, 0, boleto.Sacado.Nome, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0275, 040, 0, boleto.Sacado.Endereco.FormataLogradouro(40), ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0315, 012, 0, boleto.Sacado.Endereco.Bairro, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0327, 008, 0, boleto.Sacado.Endereco.CEP.Replace("-", ""), '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0335, 015, 0, boleto.Sacado.Endereco.Cidade, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0350, 002, 0, boleto.Sacado.Endereco.UF, ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0352, 006, 0, boleto.DataMulta, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0358, 010, 2, boleto.ValorMulta, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0368, 022, 0, boleto.Avalista.Nome, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0390, 002, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0394, 001, 0, "1", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistroGeral, '0');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do arquivo CNAB400 - Registro 1.", ex);
            }
        }

        private string GerarTrailerRemessaCNAB400(ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 001, 0, "9", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 393, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistroGeral, '0');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do registro TRAILER do arquivo de REMESSA.", ex);
            }
        }

        #endregion

        #region CNAB 240

        private string GerarHeaderRemessaCNAB240SIGCB(int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "104", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0000", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, Cedente.TipoCPFCNPJ("0"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, Cedente.CPFCNPJ, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0033, 020, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0053, 005, 0, Cedente.ContaBancaria.Agencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0058, 001, 0, Cedente.ContaBancaria.DigitoAgencia, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0059, 006, 0, Cedente.Codigo, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0065, 007, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0072, 001, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 030, 0, Cedente.Nome, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0103, 030, 0, "CAIXA ECONOMICA FEDERAL", ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0133, 010, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0143, 001, 0, "1", '0');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0144, 008, 0, DateTime.Now, ' ');
                reg.Adicionar(TTiposDadoEDI.ediHoraHHMMSS___________, 0152, 006, 0, DateTime.Now, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0158, 006, 0, numeroArquivoRemessa, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0164, 003, 0, "050", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0167, 005, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0172, 020, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0192, 020, 0, "REMESSA-PRODUCAO", ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0212, 004, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0216, 025, 0, Empty, ' ');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        private string GerarHeaderLoteRemessaCNAB240SIGCB(int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "104", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "1", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 001, 0, "R", ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0010, 002, 0, "01", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0012, 002, 0, "00", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0014, 003, 0, "030", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0017, 001, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, Cedente.TipoCPFCNPJ("0"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 015, 0, Cedente.CPFCNPJ, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0034, 006, 0, Cedente.Codigo, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0040, 014, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0054, 005, 0, Cedente.ContaBancaria.Agencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0059, 001, 0, Cedente.ContaBancaria.DigitoAgencia, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0060, 006, 0, Cedente.Codigo, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0066, 007, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0073, 001, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 030, 0, Cedente.Nome, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0104, 040, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0144, 040, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0184, 008, 0, numeroArquivoRemessa, '0');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0192, 008, 0, DateTime.Now, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0200, 008, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0208, 033, 0, Empty, ' ');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do lote no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        private string GerarDetalheSegmentoPRemessaCNAB240SIGCB(Boleto boleto, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "104", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroGeral, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "P", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 001, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0016, 002, 0, boleto.CodigoOcorrencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 005, 0, boleto.Banco.Cedente.ContaBancaria.Agencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0023, 001, 0, boleto.Banco.Cedente.ContaBancaria.DigitoAgencia, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 006, 0, boleto.Banco.Cedente.Codigo, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0030, 008, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0038, 003, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0041, 017, 0, boleto.NossoNumero, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0058, 001, 0, (int)boleto.TipoCarteira, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0059, 001, 0, (int)boleto.Banco.Cedente.ContaBancaria.TipoFormaCadastramento, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0060, 001, 0, "2", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0061, 001, 0, (int)boleto.Banco.Cedente.ContaBancaria.TipoImpressaoBoleto, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0062, 001, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0063, 011, 0, boleto.NumeroDocumento, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 004, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0078, 008, 0, boleto.DataVencimento, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0086, 015, 2, boleto.ValorTitulo, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0101, 005, 2, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0106, 001, 0, "0", ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0107, 002, 0, (int)boleto.EspecieDocumento, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0109, 001, 0, boleto.Aceite, ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0110, 008, 0, boleto.DataEmissao, '0');
                if (boleto.ValorJurosDia == 0)
                {
                    // Sem Juros Mora
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0118, 001, 2, "3", '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0119, 008, 0, "0", '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0127, 015, 2, 0, '0');
                }
                else
                {
                    // Com Juros Mora ($)
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0118, 001, 2, "1", '0');
                    reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0119, 008, 0, boleto.DataJuros, '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0127, 015, 2, boleto.ValorJurosDia, '0');
                }
                if (boleto.ValorDesconto == 0)
                {
                    // Sem Desconto
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0142, 001, 0, "0", '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0143, 008, 0, "0", '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0151, 015, 2, "0", '0');
                }
                else
                {
                    // Com Desconto
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0142, 001, 0, "1", '0');
                    reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0143, 008, 0, boleto.DataDesconto, '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0151, 015, 2, boleto.ValorDesconto, '0');
                }
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0166, 015, 2, boleto.ValorIOF, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0181, 015, 2, boleto.ValorAbatimento, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0196, 025, 0, boleto.NumeroControleParticipante, ' ');
                switch (boleto.CodigoProtesto)
                {
                    case TipoCodigoProtesto.NaoProtestar:
                        reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0221, 001, 0, 3, '0');
                        break;
                    case TipoCodigoProtesto.ProtestarDiasCorridos:
                        reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0221, 001, 0, 1, '0');
                        break;
                    default:
                        reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0221, 001, 0, 0, '0');
                        break;
                }
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0222, 002, 0, boleto.DiasProtesto, '0');
                switch (boleto.CodigoBaixaDevolucao)
                {
                    case TipoCodigoBaixaDevolucao.NaoBaixarNaoDevolver:
                        reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0224, 001, 0, 2, '0');
                        break;
                    case TipoCodigoBaixaDevolucao.BaixarDevolver:
                        reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0224, 001, 0, 1, '0');
                        break;
                    default:
                        reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0224, 001, 0, 0, '0');
                        break;
                }
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0225, 003, 0, boleto.DiasBaixaDevolucao, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0228, 002, 0, "09", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0230, 010, 2, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0240, 001, 0, Empty, ' ');
                reg.CodificarLinha();
                var vLinha = reg.LinhaRegistro;
                return vLinha;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do Segmento P no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        private string GerarDetalheSegmentoQRemessaCNAB240SIGCB(Boleto boleto, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "104", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroGeral, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "Q", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 001, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0016, 002, 0, boleto.CodigoOcorrencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, boleto.Sacado.TipoCPFCNPJ("0"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 015, 0, boleto.Sacado.CPFCNPJ, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0034, 040, 0, boleto.Sacado.Nome, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 040, 0, boleto.Sacado.Endereco.FormataLogradouro(40), ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0114, 015, 0, boleto.Sacado.Endereco.Bairro, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0129, 008, 0, boleto.Sacado.Endereco.CEP.Replace("-", ""), ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0137, 015, 0, boleto.Sacado.Endereco.Cidade, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0152, 002, 0, boleto.Sacado.Endereco.UF, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0154, 001, 0, boleto.Avalista.TipoCPFCNPJ("0"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0155, 015, 0, boleto.Avalista.CPFCNPJ, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0170, 040, 0, boleto.Avalista.Nome, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0210, 003, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0213, 020, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0233, 008, 0, Empty, ' ');
                reg.CodificarLinha();
                var vLinha = reg.LinhaRegistro;
                return vLinha;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do Segmento Q no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        private string GerarDetalheSegmentoRRemessaCNAB240SIGCB(Boleto boleto, ref int numeroRegistroGeral)
        {
            try
            {
                string codMulta;
                if (boleto.ValorMulta > 0)
                    codMulta = "1";
                else
                    codMulta = "0";
                var msg = boleto.MensagemArquivoRemessa.PadRight(500, ' ');
                var msg3 = msg.Substring(00, 40).FitStringLength(40, ' ');
                var msg4 = msg.Substring(40, 40).FitStringLength(40, ' ');
                if ((codMulta == "0") & IsNullOrWhiteSpace(msg3) & IsNullOrWhiteSpace(msg4))
                    return "";

                numeroRegistroGeral++;
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "104", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroGeral, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "R", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 001, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0016, 002, 0, boleto.CodigoOcorrencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 008, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0027, 015, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0042, 001, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0043, 008, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0051, 015, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0066, 001, 0, codMulta, '0');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0067, 008, 0, boleto.DataMulta, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0075, 015, 2, boleto.ValorMulta, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0090, 010, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0100, 040, 0, msg3, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0140, 040, 0, msg4, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0180, 050, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0230, 011, 0, Empty, ' ');
                reg.CodificarLinha();
                var vLinha = reg.LinhaRegistro;
                return vLinha;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do Segmento Q no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        private string GerarDetalheSegmentoSRemessaCNAB240SIGCB(Boleto boleto, ref int numeroRegistroGeral)
        {
            try
            {
                var msg5A9 = boleto.MensagemArquivoRemessa.PadRight(500, ' ').Substring(80, 200).FitStringLength(200, ' ');
                if (IsNullOrWhiteSpace(msg5A9))
                    return "";

                numeroRegistroGeral++;
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "104", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroGeral, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "S", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 001, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0016, 002, 0, boleto.CodigoOcorrencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, "3", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0019, 200, 0, msg5A9, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0219, 022, 0, Empty, ' ');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do Segmento Q no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        private string GerarTrailerLoteRemessaCNAC240SIGCB(ref int numeroRegistroGeral,
            int numeroRegistroCobrancaSimples, decimal valorCobrancaSimples,
            int numeroRegistroCobrancaCaucionada, decimal valorCobrancaCaucionada,
            int numeroRegistroCobrancaDescontada, decimal valorCobrancaDescontada)
        {
            try
            {
                // O n�mero de registros no lote � igual ao n�mero de registros gerados + 2 (header e trailler do lote)
                var numeroRegistrosNoLote = numeroRegistroGeral + 2;
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "104", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "5", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 006, 0, numeroRegistrosNoLote, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 006, 0, numeroRegistroCobrancaSimples, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0030, 017, 2, valorCobrancaSimples, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0047, 006, 0, numeroRegistroCobrancaCaucionada, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0053, 017, 2, valorCobrancaCaucionada, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0070, 006, 0, numeroRegistroCobrancaDescontada, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0076, 017, 2, valorCobrancaDescontada, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0093, 031, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0124, 117, 0, Empty, ' ');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do lote no arquivo de remessa do CNAB400.", ex);
            }
        }

        private string GerarTrailerRemessaCNAB240SIGCB(ref int numeroRegistroGeral)
        {
            try
            {
                // O n�mero de registros no arquivo � igual ao n�mero de registros gerados + 4 (header e trailler do lote / header e trailler do arquivo)
                var numeroRegistrosNoArquivo = numeroRegistroGeral + 4;
                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "104", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "9999", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "9", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 006, 0, "1", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 006, 0, numeroRegistrosNoArquivo, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0030, 006, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0036, 205, 0, Empty, ' ');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB400.", ex);
            }
        }

        #endregion

        #endregion

        #region SIG14 - Retorno

        public void LerHeaderRetornoCNAB240(ArquivoRetorno arquivoRetorno, string registro)
        {
            ////144 - 151 Data de gera��o do arquivo N 008 DDMMAAAA
            //arquivoRetorno.DataGeracao = Utils.ToDateTime(Utils.ToInt32(registro.Substring(143, 8)).ToString("##-##-####"));
            ////158 - 163 N� seq�encial do arquivo N 006
            //arquivoRetorno.NumeroSequencial = Utils.ToInt32(registro.Substring(157, 6));
        }

        public void LerDetalheRetornoCNAB240SegmentoT(ref Boleto boleto, string registro)
        {
            try
            {
                //N� Controle do Participante
                boleto.NumeroControleParticipante = registro.Substring(105, 25);

                //Carteira
                boleto.Carteira = registro.Substring(57, 1);
                switch (boleto.Carteira)
                {
                    case "3":
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaCaucionada;
                        break;
                    case "4":
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaDescontada;
                        break;
                    default:
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaSimples;
                        break;
                }

                //Identifica��o do T�tulo no Banco
                boleto.NossoNumero = registro.Substring(39, 17);
                boleto.NossoNumeroDV = registro.Substring(56, 1);
                boleto.NossoNumeroFormatado = Format("{0}-{1}", boleto.NossoNumero, boleto.NossoNumeroDV);

                //Identifica��o de Ocorr�ncia
                boleto.CodigoOcorrencia = registro.Substring(15, 2);
                boleto.DescricaoOcorrencia = Cnab.OcorrenciaCnab240(boleto.CodigoOcorrencia);
                boleto.CodigoOcorrenciaAuxiliar = registro.Substring(213, 10);

                //N�mero do Documento
                boleto.NumeroDocumento = registro.Substring(58, 11);
                boleto.EspecieDocumento = TipoEspecieDocumento.NaoDefinido;

                //Valor do T�tulo
                boleto.ValorTitulo = Convert.ToDecimal(registro.Substring(81, 15)) / 100;
                boleto.ValorTarifas = Convert.ToDecimal(registro.Substring(198, 15)) / 100;

                //Data Vencimento do T�tulo
                boleto.DataVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(73, 8)).ToString("##-##-####"));

                // Registro Retorno
                boleto.RegistroArquivoRetorno = boleto.RegistroArquivoRetorno + registro + Environment.NewLine;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 240 / T.", ex);
            }
        }

        public void LerDetalheRetornoCNAB240SegmentoU(ref Boleto boleto, string registro)
        {
            try
            {
                //Valor do T�tulo
                boleto.ValorJurosDia = Convert.ToDecimal(registro.Substring(17, 15)) / 100;
                boleto.ValorDesconto = Convert.ToDecimal(registro.Substring(32, 15)) / 100;
                boleto.ValorAbatimento = Convert.ToDecimal(registro.Substring(47, 15)) / 100;
                boleto.ValorIOF = Convert.ToDecimal(registro.Substring(62, 15)) / 100;
                boleto.ValorPago = Convert.ToDecimal(registro.Substring(77, 15)) / 100;
                boleto.ValorPagoCredito = Convert.ToDecimal(registro.Substring(92, 15)) / 100;
                boleto.ValorOutrasDespesas = Convert.ToDecimal(registro.Substring(107, 15)) / 100;
                boleto.ValorOutrosCreditos = Convert.ToDecimal(registro.Substring(122, 15)) / 100;


                //Data Ocorr�ncia no Banco
                boleto.DataProcessamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(137, 8)).ToString("##-##-####"));

                // Data do Cr�dito
                boleto.DataCredito = Utils.ToDateTime(Utils.ToInt32(registro.Substring(145, 8)).ToString("##-##-####"));

                // Registro Retorno
                boleto.RegistroArquivoRetorno = boleto.RegistroArquivoRetorno + registro + Environment.NewLine;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 240 / U.", ex);
            }
        }

        public void LerHeaderRetornoCNAB400(string registro)
        {
            try
            {
                if (registro.Substring(0, 9) != "02RETORNO")
                    throw new Exception("O arquivo n�o � do tipo \"02RETORNO\"");
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler HEADER do arquivo de RETORNO / CNAB 400.", ex);
            }
        }

        public void LerDetalheRetornoCNAB400Segmento1(ref Boleto boleto, string registro)
        {
            try
            {
                //N� Controle do Participante
                boleto.NumeroControleParticipante = registro.Substring(31, 6);

                //Carteira
                boleto.Carteira = registro.Substring(106, 2);
                boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaSimples;

                //Identifica��o do T�tulo no Banco
                boleto.NossoNumero = registro.Substring(56, 17);
                boleto.NossoNumeroDV = registro.Substring(93, 1); //DV
                boleto.NossoNumeroFormatado = $"{boleto.Carteira}/{boleto.NossoNumero}-{boleto.NossoNumeroDV}";

                //Identifica��o de Ocorr�ncia
                boleto.CodigoOcorrencia = registro.Substring(108, 2);
                boleto.CodigoOcorrenciaAuxiliar = registro.Substring(79, 2);
                boleto.DescricaoOcorrencia = DescricaoOcorrenciaCnab400(boleto.CodigoOcorrencia, boleto.CodigoOcorrenciaAuxiliar);

                //N�mero do Documento
                boleto.NumeroDocumento = registro.Substring(116, 10);
                boleto.EspecieDocumento = AjustaEspecieCnab400(registro.Substring(173, 2));

                //Valores do T�tulo
                boleto.ValorTitulo = Convert.ToDecimal(registro.Substring(152, 13)) / 100;
                boleto.ValorTarifas = Convert.ToDecimal(registro.Substring(175, 13)) / 100;
                boleto.ValorOutrasDespesas = 0; // Convert.ToDecimal(registro.Substring(188, 13)) / 100;
                boleto.ValorIOF = Convert.ToDecimal(registro.Substring(214, 13)) / 100;
                boleto.ValorAbatimento = Convert.ToDecimal(registro.Substring(227, 13)) / 100;
                boleto.ValorDesconto = Convert.ToDecimal(registro.Substring(240, 13)) / 100;
                boleto.ValorPagoCredito = Convert.ToDecimal(registro.Substring(253, 13)) / 100;
                boleto.ValorJurosDia = Convert.ToDecimal(registro.Substring(266, 13)) / 100;
                boleto.ValorOutrosCreditos = Convert.ToDecimal(registro.Substring(279, 13)) / 100;

                //Data Ocorr�ncia no Banco
                boleto.DataProcessamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(110, 6)).ToString("##-##-##"));

                //Data Vencimento do T�tulo
                boleto.DataVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(146, 6)).ToString("##-##-##"));

                // Data do Cr�dito
                boleto.DataCredito = Utils.ToDateTime(Utils.ToInt32(registro.Substring(293, 6)).ToString("##-##-##"));

                // Registro Retorno
                boleto.RegistroArquivoRetorno = boleto.RegistroArquivoRetorno + registro + Environment.NewLine;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 400.", ex);
            }
        }

        public void LerDetalheRetornoCNAB400Segmento7(ref Boleto boleto, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerTrailerRetornoCNAB400(string registro)
        {
        }

        public string FormatarNomeArquivoRemessa(int numeroSequencial, int remessaDia)
        {
            return numeroSequencial.ToString();
        }

        private string DescricaoOcorrenciaCnab400(string codigo, string codigoRejeicao)
        {
            switch (codigo)
            {
                case "01":
                    return "Entrada Confirmada";
                case "02":
                    return "Baixa Manual Confirmada";
                case "03":
                    return "Abatimento Concedido";
                case "04":
                    return "Abatimento Cancelado";
                case "05":
                    return "Vencimento Alterado";
                case "06":
                    return "Uso da Empresa Alterado";
                case "07":
                    return "Prazo de Protesto Alterado";
                case "08":
                    return "Prazo de Devolu��o Alterado";
                case "09":
                    return "Altera��o Confirmada";
                case "10":
                    return "Altera��o com reemiss�o de boleto confirmada";
                case "11":
                    return "Altera��o da op��o de Protesto para Devolu��o Confirmada";
                case "12":
                    return "Altera��o da op��o de Devolu��o para Protesto Confirmada";
                case "20":
                    return "Em Ser";
                case "21":
                    return "Liquida��o";
                case "22":
                    return "Liquida��o em Cart�rio";
                case "23":
                    return "Baixa por Devolu��o";
                case "25":
                    return "Baixa por Protesto";
                case "26":
                    return "T�tulo enviado para Cart�rio";
                case "27":
                    return "Susta��o de Protesto";
                case "28":
                    return "Estorno de Protesto";
                case "29":
                    return "Estorno de Susta��o de Protesto";
                case "30":
                    return "Altera��o de T�tulo";
                case "31":
                    return "Tarifa sobre T�tulo Vencido";
                case "32":
                    return "Outras Tarifas de Altera��o";
                case "33":
                    return "Estorno de Baixa / Liquida��o";
                case "34":
                    return "Tarifas Diversas";
                case "35":
                    return "Liquida��o On-line";
                case "36":
                    return "Estorno de Liquida��o On-line";
                case "37":
                    return "Transfer�ncia para a cobran�a simples";
                case "38":
                    return "Transfer�ncia para a cobran�a descontada";
                case "51":
                    return "Reconhecido pelo sacado";
                case "52":
                    return "N�o reconhecido pelo sacado";
                case "53":
                    return "Recusado no DDA";
                case "A4":
                    return "Pagador DDA";

                case "99":
                    return DescricaoRejeicaoCnab400(codigoRejeicao);
                default:
                    return "";
            }
        }

        private string DescricaoRejeicaoCnab400(string codigo)
        {
            switch (codigo)
            {
                case "01":
                    return "Movimento sem Benefici�rio Correspondente";
                case "02":
                    return "Movimento sem T�tulo Correspondente";
                case "08":
                    return "Movimento para t�tulo j� com movimenta��o no dia";
                case "09":
                    return "Nosso N�mero n�o pertence ao Benefici�rio";
                case "10":
                    return "Inclus�o de t�tulo j� existente na base";
                case "12":
                    return "Movimento duplicado";
                case "13":
                    return "Entrada Inv�lida para Cobran�a Caucionada(Benefici�rio n�o possui conta Cau��o)";
                case "20":
                    return "CEP do Pagador n�o encontrado(n�o foi poss�vel a determina��o da Ag�ncia Cobradora para o t�tulo)";
                case "21":
                    return "Ag�ncia cobradora n�o encontrada (ag�ncia designada para cobradora n�o cadastrada no sistema)";
                case "22":
                    return "Ag�ncia Benefici�rio n�o encontrada (Ag�ncia do Benefici�rio n�o cadastrada no sistema)";
                case "26":
                    return "Data de vencimento inv�lida";
                case "44":
                    return "CEP do sacado inv�lido";
                case "45":
                    return "Data de Vencimento com prazo superior ao limite";
                case "49":
                    return "Movimento inv�lido para t�tulo Baixado / Liquidado";
                case "50":
                    return "Movimento inv�lido para t�tulo enviado a Cart�rio";
                case "54":
                    return "Faixa de CEP da Ag�ncia Cobradora n�o abrange CEP do Pagador";
                case "55":
                    return "T�tulo j� com op��o de Devolu��o";
                case "56":
                    return "Processo de Protesto em andamento";
                case "57":
                    return "T�tulo j� com op��o de Protesto";
                case "58":
                    return "Processo de devolu��o em andamento";
                case "59":
                    return "Novo prazo p / Protesto / Devolu��o inv�lido";
                case "76":
                    return "Altera��o do prazo de protesto inv�lida";
                case "77":
                    return "Altera��o do prazo de devolu��o inv�lida";
                case "81":
                    return "CEP do Pagador inv�lido";
                case "82":
                    return "CNPJ / CPF do Pagador inv�lido (d�gito n�o confere)";
                case "83":
                    return "N�mero do Documento(seu n�mero) inv�lido";
                case "84":
                    return "Protesto inv�lido para t�tulo sem N�mero do documento(seu n�mero)";
                default:
                    return "";
            }
        }

        private TipoEspecieDocumento AjustaEspecieCnab400(string codigoEspecie)
        {
            switch (codigoEspecie)
            {
                case "01":
                    return TipoEspecieDocumento.CH;
                case "02":
                    return TipoEspecieDocumento.DM;
                case "03":
                    return TipoEspecieDocumento.DMI;
                case "04":
                    return TipoEspecieDocumento.DS;
                case "05":
                    return TipoEspecieDocumento.DSI;
                case "06":
                    return TipoEspecieDocumento.DR;
                case "07":
                    return TipoEspecieDocumento.LC;
                case "08":
                    return TipoEspecieDocumento.NCC;
                case "09":
                    return TipoEspecieDocumento.NCE;
                case "10":
                    return TipoEspecieDocumento.NCI;
                case "11":
                    return TipoEspecieDocumento.NCR;
                case "12":
                    return TipoEspecieDocumento.NP;
                case "13":
                    return TipoEspecieDocumento.NPR;
                case "14":
                    return TipoEspecieDocumento.TM;
                case "15":
                    return TipoEspecieDocumento.TS;
                case "16":
                    return TipoEspecieDocumento.NS;
                case "17":
                    return TipoEspecieDocumento.RC;
                case "18":
                    return TipoEspecieDocumento.FAT;
                case "19":
                    return TipoEspecieDocumento.ND;
                case "20":
                    return TipoEspecieDocumento.AP;
                case "21":
                    return TipoEspecieDocumento.ME;
                case "22":
                    return TipoEspecieDocumento.PC;
                case "23":
                    return TipoEspecieDocumento.NF;
                case "24":
                    return TipoEspecieDocumento.DD;
                case "25":
                    return TipoEspecieDocumento.CPR;
                case "31":
                    return TipoEspecieDocumento.CC;
                case "32":
                    return TipoEspecieDocumento.BP;
                case "99":
                    return TipoEspecieDocumento.OU;
                default:
                    return TipoEspecieDocumento.OU;
            }
        }

        public string GerarHeaderDebitoAuto(DebitoAutomaticoCriar gerarHeader)
        {
            throw new NotImplementedException();
        }

        public List<string> GerarDetalheDebitoAuto(DebitoAutomaticoCriar gerarDetalhe)
        {
            throw new NotImplementedException();
        }

        public string GerarTraillerDebitoAuto(DebitoAutomaticoCriar gerarTrailler)
        {
            throw new NotImplementedException();
        }

        public DebitoAutomaticoConsulta LerDebitoAutoRetorno(string[] linhasArquivoRet)
        {
            throw new NotImplementedException();
        }

        public HeaderDebitoConsulta LerHeaderDebitoAuto(string linhaHeader)
        {
            throw new NotImplementedException();
        }

        public List<DetalheDebitoAutoConsulta> LerDetalhesDebitoAuto(string[] linhasDetalhes)
        {
            throw new NotImplementedException();
        }

        public TraillerDebitoAuto LerTraillerDebitoAuto(string linhaTrailler)
        {
            throw new NotImplementedException();
        }

        public string HeaderDebitoAuto(HeaderDebitoAutoCriar gerarHeader)
        {
            throw new NotImplementedException();
        }

        public List<string> DetalheDebitoAuto(List<DetalheDebitoAutoCadastro> gerarDetalhes)
        {
            throw new NotImplementedException();
        }

        public string TraillerDebitoAuto(TraillerDebitoAuto gerarTrailler)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> CodigoRetorno(string codigo)
        {
            throw new NotImplementedException();
        }

        public string GerarMensagemCliente(TipoArquivo tipoArquivo, Boleto boleto, ref int numeroRegistro)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}