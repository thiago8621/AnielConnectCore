using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using Boleto2Net.DebitoAutomatico;
using Boleto2Net.Exceptions;
using Boleto2Net.Extensions;
using static System.String;

[assembly: WebResource("BoletoNet.Imagens.237.jpg", "image/jpg")]

namespace Boleto2Net
{
    internal sealed class BancoInter : IBanco
    {
        internal static Lazy<IBanco> Instance { get; } = new Lazy<IBanco>(() => new BancoInter());
        public Cedente Cedente { get; set; }

        public int Codigo { get; } = 087; // Código do Banco Inter é 077

        public string Nome { get; } = "Banco Inter";

        public string Digito { get; } = "112"; // Não é oficial, mas alguns sistemas usam "9" ou vazio. Pode deixar em branco se necessário.

        public string Carteia = "112";
        public List<string> IdsRetornoCnab400RegistroDetalhe { get; } = new List<string> { "1" }; // CNAB400 geralmente usa tipo "1" para detalhes

        public bool RemoveAcentosArquivoRemessa { get; } = true; // Mantém true para evitar erros no retorno


        public void FormataCedente()
        {
            var contaBancaria = Cedente.ContaBancaria;
            
               //if (!CarteiraFactory<BancoInter>.CarteiraEstaImplementada(contaBancaria.CarteiraComVariacaoPadrao))
            //    throw Boleto2NetException.CarteiraNaoImplementada(contaBancaria.CarteiraComVariacaoPadrao);

            contaBancaria.FormatarDados("PAG�VEL PREFERENCIALMENTE NA INTER.", "", "", 7);

            var codigoCedente = Cedente.Codigo;
            Cedente.Codigo = codigoCedente.Length <= 20 ? codigoCedente.PadLeft(20, '0') : throw Boleto2NetException.CodigoCedenteInvalido(codigoCedente, 20);

            Cedente.CodigoFormatado = $"{contaBancaria.Agencia}-{contaBancaria.DigitoAgencia} / {contaBancaria.Conta}-{contaBancaria.DigitoConta}";
        }

        public void ValidaBoleto(Boleto boleto)
        {
        }

        public void FormataNossoNumero(Boleto boleto)
        {
            var carteira = CarteiraFactory<BancoInter>.ObterCarteira(boleto.CarteiraComVariacao);
            carteira.FormataNossoNumero(boleto);
        }

        public string FormataCodigoBarraCampoLivre(Boleto boleto)
        {
            var carteira = CarteiraFactory<BancoInter>.ObterCarteira(boleto.CarteiraComVariacao);
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
                        header += GerarHeaderRemessaCNAB240(numeroArquivoRemessa, ref numeroRegistroGeral);
                        // Cabe�alho do Lote
                        header += Environment.NewLine;
                        header += GerarHeaderLoteCNAB240(numeroArquivoRemessa, ref numeroRegistroGeral);
                        break;
                    case TipoArquivo.CNAB400:
                        header += GerarHeaderRemessaCNAB400(numeroArquivoRemessa, ref numeroRegistroGeral);
                     
                        break;
                    default:
                        throw new Exception("Tipo de arquivo inexistente.");
                }
                return header;
            }
            catch (Exception ex)
            {
                throw Boleto2NetException.ErroAoGerarRegistroHeaderDoArquivoRemessa(ex);
            }
        }
        private string GerarSegmentoB(int numeroLote, int sequencialRegistro, Cedente favorecido, ref int numeroRegistroGeral)
        {
            try
            {
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0'); // Código do banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, numeroLote, '0'); // Número do lote
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0'); // Tipo de registro
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, sequencialRegistro, '0'); // Nº sequencial registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "B", ' '); // Segmento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 003, 0, "", ' '); // Campo em branco

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, favorecido.ContaBancaria.TipoDocumento, '0'); // Tipo doc: 1=CPF, 2=CNPJ
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, favorecido.CPFCNPJ, '0'); // CPF/CNPJ

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0033, 035, 0, favorecido.Endereco.LogradouroEndereco, ' ') ; // Logradouro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0068, 005, 0, favorecido.Endereco.LogradouroNumero, ' '); // Número
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 015, 0, favorecido.Endereco.LogradouroComplemento, ' '); // Complemento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0088, 015, 0, favorecido.Endereco.Bairro, ' '); // Bairro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0103, 015, 0, favorecido.Endereco.Cidade, ' '); // Cidade
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0118, 008, 0, favorecido.Endereco.CEP, '0'); // CEP
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0126, 002, 0, favorecido.Endereco.UF, ' '); // UF
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0128, 105, 0, "", ' '); // Campo em branco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0233, 008, 0, "00416968", '0'); // Código ISPB do Banco Inter

                reg.CodificarLinha();
                numeroRegistroGeral++;
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar SEGMENTO B do arquivo de remessa CNAB240 para o Banco Inter.", ex);
            }
        }

        private string GerarSegmentoA(int numeroLote, int sequencialRegistro, Cedente favorecido, DateTime dataDebito, ref int numeroRegistroGeral)
        {
            try
            {
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0'); // Código do banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, numeroLote, '0'); // Número do lote
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0'); // Tipo de registro
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, sequencialRegistro, '0'); // Nº seq. do reg. no lote
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "A", ' '); // Código segmento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0015, 001, 0, "0", '0'); // Tipo de movimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0016, 002, 0, "00", '0'); // Código instrução de movimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 003, 0, "000", '0'); // Código da câmara centralizadora
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0021, 003, 0, "077", '0'); // Código do banco favorecido
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 005, 0, favorecido.ContaBancaria.Agencia, '0'); // Agência
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0029, 001, 0, favorecido.ContaBancaria.DigitoAgencia, ' '); // DV Agência
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0030, 012, 0, favorecido.ContaBancaria.Conta, '0'); // Conta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0042, 001, 0, favorecido.ContaBancaria.DigitoConta, ' '); // DV Conta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0043, 001, 0, "", ' '); // Branco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0044, 030, 0, favorecido.Nome, ' '); // Nome favorecido
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 020, 0, "", ' '); // Número documento (empresa)
                reg.Adicionar(TTiposDadoEDI.ediDataAAAAMMDD_________, 0094, 008, 0, dataDebito, '0'); // Data de débito
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0102, 003, 0, "BRL", ' '); // Tipo da moeda
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0105, 015, 0, "0", '0'); // Quantidade da moeda (geralmente zeros)

                reg.CodificarLinha();
                numeroRegistroGeral++;
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar SEGMENTO A do arquivo de remessa CNAB240 para o Banco Inter.", ex);
            }
        }
        public string GerarDetalheRemessa(TipoArquivo tipoArquivo, Boleto boleto, ref int numeroRegistro, bool instrucaoCancelameto = false)
        {
            try
            {
                string detalhe = Empty, strline = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB400:
                        detalhe += GerarDetalheRemessaCNAB400Registro1Inter(boleto, numeroRegistro);
                        if (!IsNullOrWhiteSpace(strline))
                        {
                            detalhe += Environment.NewLine;
                            detalhe += strline;
                        }
                       
                        break;
                    default:
                        throw new Exception("Tipo de arquivo inexistente.");
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
                var trailer = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        // Trailler do Lote
                        trailer += GerarTrailerLoteRemessaCNAC240(1, numeroRegistroGeral);
                        // Trailler do Arquivo
                        trailer += Environment.NewLine;
                        trailer += GerarTrailerArquivoRemessaCNAB240(numeroRegistroGeral,valorBoletoGeral);
                        break;

                    case TipoArquivo.CNAB400:
                        trailer = GerarTrailerRemessaCNAB400(ref numeroRegistroGeral);
                        break;
                    default:
                        throw new Exception("Tipo de arquivo inexistente.");
                }
                return trailer;
            }
            catch (Exception ex)
            {
                throw Boleto2NetException.ErroAoGerrarRegistroTrailerDoArquivoRemessa(ex);
            }
        }



        #region CNAB240

        #region RemessaCNAB240
        private string GerarTrailerArquivo(int quantidadeLotes, int quantidadeRegistros)
        {
            try
            {
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "237", '0'); // Código do banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "9999", '0'); // Lote = 9999
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "9", '0'); // Tipo de registro = 9
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, string.Empty, ' '); // Branco

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 006, 0, quantidadeLotes, '0'); // Qtd. de lotes
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 006, 0, quantidadeRegistros, '0'); // Qtd. de registros

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0030, 211, 0, string.Empty, ' '); // Branco

                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar TRAILER DO ARQUIVO CNAB240 para o Banco Inter.", ex);
            }
        }


        private string GerarHeaderRemessaCNAB240(int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0'); // 01 - Código do banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0000", '0'); // 02 - Lote de serviço
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "0", '0');    // 03 - Tipo de registro

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, "", ' ');     // 04 - Campo em branco

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, "2", '0');    // 05 - Tipo de documento (CNPJ)

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, Cedente.CPFCNPJ, '0'); // 06 - CNPJ da empresa

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0033, 020, 0, Cedente.Codigo, ' '); // 07 - Código do convênio

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0053, 005, 0, Cedente.ContaBancaria.Agencia, '0'); // 08 - Agência

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0058, 001, 0, Cedente.ContaBancaria.DigitoAgencia, ' '); // 09 - Dígito agência

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0059, 012, 0, Cedente.ContaBancaria.Conta, '0'); // 10 - Conta corrente

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0071, 001, 0, Cedente.ContaBancaria.DigitoConta, ' '); // 11 - Dígito conta

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0072, 001, 0, "", ' '); // 12 - Campo em branco

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 030, 0, Cedente.Nome, ' '); // 13 - Nome da empresa

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0103, 030, 0, "BANCO INTER", ' '); // 14 - Nome do banco

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0133, 010, 0, "", ' '); // 15 - Campo em branco

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0143, 001, 0, "1", '0'); // 16 - Identificação do arquivo remessa

                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0144, 008, 0, DateTime.Now, '0'); // 17 - Data de geração

                reg.Adicionar(TTiposDadoEDI.ediHoraHHMMSS___________, 0152, 006, 0, DateTime.Now, '0'); // 18 - Hora de geração

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0158, 006, 0, numeroArquivoRemessa, '0'); // 19 - Nº sequencial remessa

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0164, 003, 0, "107", '0'); // 20 - Nº versão layout

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0167, 005, 0, "01600", '0'); // 21 - Densidade de gravação

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0172, 020, 0, "", ' '); // 22 - Uso reservado do banco

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0192, 020, 0, Cedente.Observacoes, ' '); // 23 - Uso reservado da empresa

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0212, 029, 0, "", ' '); // 24 - Uso exclusivo FEBRABAN


                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB240 para o Banco Inter.", ex);
            }
        }


        private string GerarHeaderLoteCNAB240(int numeroLote, ref int numeroRegistroGeral)
        {
            try
            {
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0'); // Código do Banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, numeroLote, '0'); // Lote
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "1", '0'); // Tipo de registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 001, 0, "C", ' '); // Tipo da operação
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0010, 002, 0, "30", '0'); // Tipo de serviço (ex: pagamento)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0012, 002, 0, "01", '0'); // Forma de lançamento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0014, 003, 0, "046", '0'); // Versão layout lote
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0017, 001, 0, "", ' '); // Branco

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, Cedente.TipoCPFCNPJ("0"), '0'); // Tipo doc
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, Cedente.CPFCNPJ, '0'); // CPF/CNPJ
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0033, 020, 0, "", ' '); // Código convênio

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0053, 005, 0, Cedente.ContaBancaria.Agencia, '0'); // Agência
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0058, 001, 0, Cedente.ContaBancaria.DigitoAgencia, ' '); // Dígito Agência
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0059, 012, 0, Cedente.ContaBancaria.Conta, '0'); // Conta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0071, 001, 0, Cedente.ContaBancaria.DigitoConta, ' '); // Dígito Conta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0072, 001, 0, "", ' '); // Branco

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 030, 0, Cedente.Nome, ' '); // Nome empresa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0103, 040, 0, Cedente.Observacoes, ' '); // Informação opcional

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0143, 030, 0, Cedente.Endereco.LogradouroEndereco, ' '); // Rua
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0173, 005, 0, Cedente.Endereco.LogradouroNumero, '0'); // Número
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0178, 015, 0, Cedente.Endereco.LogradouroComplemento, ' '); // Complemento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0193, 020, 0, Cedente.Endereco.Cidade, ' '); // Cidade
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0213, 005, 0, "", '0'); // CEP
reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0218, 003, 0, "", ' '); // Complemento do CEP
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0221, 002, 0, Cedente.Endereco.UF, ' '); // UF
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0223, 008, 0, "", ' '); // Branco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0231, 010, 0, "", ' '); // Ocorrências para retorno

                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER DE LOTE do arquivo de remessa CNAB240 para o Banco Inter.", ex);
            }
        }


        private string GerarDetalheSegmentoPRemessaCNAB240(Boleto boleto, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0'); // Banco Inter
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0'); // Lote
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');     // Tipo registro
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroGeral, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "P", ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 001, 0, string.Empty, ' '); // Exclusivo CNAB
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0016, 002, 0, "01", '0'); // Código movimento remessa

                // Dados da conta (verificar no contrato do Banco Inter)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 005, 0, boleto.Banco.Cedente.ContaBancaria.Agencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0023, 001, 0, boleto.Banco.Cedente.ContaBancaria.DigitoAgencia, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 012, 0, boleto.Banco.Cedente.ContaBancaria.Conta, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0036, 001, 0, boleto.Banco.Cedente.ContaBancaria.DigitoConta, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0037, 001, 0, string.Empty, ' '); // Filler

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0038, 003, 0, boleto.Carteira, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0041, 005, 0, 0, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0046, 011, 0, boleto.NossoNumero, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0057, 001, 0, boleto.NossoNumeroDV, '0');

                // Tipo carteira / forma cadastro / tipo documento - verificar com o Banco Inter
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0058, 001, 0, (int)boleto.Banco.Cedente.ContaBancaria.TipoCarteiraPadrao, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0059, 001, 0, (int)boleto.Banco.Cedente.ContaBancaria.TipoFormaCadastramento, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0060, 001, 0, (int)boleto.Banco.Cedente.ContaBancaria.TipoDocumento, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0061, 001, 0, (int)boleto.Banco.Cedente.ContaBancaria.TipoImpressaoBoleto, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0062, 001, 0, (int)boleto.Banco.Cedente.ContaBancaria.TipoDistribuicao, ' ');

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0063, 015, 0, boleto.NumeroDocumento, ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0078, 008, 0, boleto.DataVencimento, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0086, 015, 2, boleto.ValorTitulo, '0');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0101, 005, 0, 0, '0'); // Agência cobradora (geralmente 0)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0106, 001, 0, "0", ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0107, 002, 0, (int)boleto.EspecieDocumento, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0109, 001, 0, boleto.Aceite, ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0110, 008, 0, boleto.DataEmissao, '0');

                if (boleto.ValorJurosDia == 0)
                {
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0118, 001, 2, "3", '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0119, 008, 0, "0", '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0127, 015, 2, 0, '0');
                }
                else
                {
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0118, 001, 2, "1", '0');
                    reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0119, 008, 0, boleto.DataJuros, '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0127, 015, 2, boleto.ValorJurosDia, '0');
                }

                if (boleto.ValorDesconto == 0)
                {
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0142, 001, 0, "0", '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0143, 008, 0, "0", '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0151, 015, 2, "0", '0');
                }
                else
                {
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0142, 001, 0, "1", '0');
                    reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0143, 008, 0, boleto.DataDesconto, '0');
                    reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0151, 015, 2, boleto.ValorDesconto, '0');
                }

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0166, 015, 2, boleto.ValorIOF, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0181, 015, 2, boleto.ValorAbatimento, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0196, 025, 0, boleto.NumeroControleParticipante, ' ');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0221, 001, 0, (int)boleto.CodigoProtesto, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0222, 002, 0, boleto.DiasProtesto, '0');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0224, 001, 0, (int)boleto.CodigoBaixaDevolucao, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0225, 003, 0, boleto.DiasBaixaDevolucao, '0');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0228, 002, 0, "09", '0'); // Código moeda
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0230, 010, 2, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0240, 001, 0, string.Empty, ' ');

                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do Segmento P para o Banco Inter no arquivo CNAB240.", ex);
            }
        }


        private string GerarDetalheSegmentoQRemessaCNAB240(Boleto boleto, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;

                var reg = new TRegistroEDI();

                // Posição 001-003: Código do banco (077 - Banco Inter)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0');

                // Posição 004-007: Lote
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0');

                // Posição 008: Tipo de registro (3 = detalhe)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');

                // Posição 009-013: Número sequencial do registro no lote
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroGeral, '0');

                // Posição 014: Código do segmento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "Q", ' ');

                // Posição 015: Uso exclusivo FEBRABAN - deixar em branco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 001, 0, "", ' ');

                // Posição 016-017: Código de movimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0016, 002, 0, "01", '0'); // entrada de título

                // Posição 018: Tipo de inscrição do sacado (1=CPF, 2=CNPJ)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, boleto.Sacado.TipoCPFCNPJ("0"), '0');

                // Posição 019-033: Número do CPF/CNPJ do sacado
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 015, 0, boleto.Sacado.CPFCNPJ, '0');

                // Posição 034-073: Nome do sacado
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0034, 040, 0, boleto.Sacado.Nome, ' ');

                // Posição 074-113: Endereço do sacado
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 040, 0, boleto.Sacado.Endereco.FormataLogradouro(40), ' ');

                // Posição 114-128: Bairro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0114, 015, 0, boleto.Sacado.Endereco.Bairro, ' ');

                // Posição 129-136: CEP (sem hífen)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0129, 008, 0, boleto.Sacado.Endereco.CEP.Replace("-", ""), ' ');

                // Posição 137-151: Cidade
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0137, 015, 0, boleto.Sacado.Endereco.Cidade, ' ');

                // Posição 152-153: UF
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0152, 002, 0, boleto.Sacado.Endereco.UF, ' ');

                // Posição 154: Tipo de inscrição do avalista (1=CPF, 2=CNPJ)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0154, 001, 0, boleto.Avalista.TipoCPFCNPJ("0"), '0');

                // Posição 155-169: CPF/CNPJ do avalista
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0155, 015, 0, boleto.Avalista.CPFCNPJ, '0');

                // Posição 170-209: Nome do avalista
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0170, 040, 0, boleto.Avalista.Nome, ' ');

                // Posição 210-212: Código banco correspondente (geralmente 000 se não houver)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0210, 003, 0, "000", '0');

                // Posição 213-232: Nosso número no banco correspondente (em branco ou conforme orientação)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0213, 020, 0, "", ' ');

                // Posição 233-240: Uso exclusivo FEBRABAN
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0233, 008, 0, "", ' ');

                // Gera a linha final
                reg.CodificarLinha();
                var vLinha = reg.LinhaRegistro;

                return vLinha;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do Segmento Q no arquivo de remessa do CNAB240 (Banco Inter).", ex);
            }
        }

        private string GerarDetalheSegmentoRRemessaCNAB240(Boleto boleto, ref int numeroRegistroGeral)
        {
            try
            {
                string codMulta = boleto.ValorMulta > 0 ? "1" : "0";

                var msg = boleto.MensagemArquivoRemessa.PadRight(500, ' ');
                var msg3 = msg.Substring(00, 40).FitStringLength(40, ' ');
                var msg4 = msg.Substring(40, 40).FitStringLength(40, ' ');

                if ((codMulta == "0") && IsNullOrWhiteSpace(msg3) && IsNullOrWhiteSpace(msg4))
                    return "";

                numeroRegistroGeral++;

                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0'); // Banco Inter
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0'); // Lote
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');    // Tipo de registro
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroGeral, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "R", ' ');     // Segmento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 001, 0, "", ' ');      // Uso exclusivo FEBRABAN
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0016, 002, 0, "01", '0');    // Cód. movimento

                // Multa
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, codMulta, '0'); // Código da multa
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0019, 008, 0, boleto.DataMulta, '0'); // Data da multa
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0027, 015, 2, boleto.ValorMulta, '0'); // Valor/Percentual multa

                // Mensagens (usualmente usadas pelo Banco Inter)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0042, 040, 0, msg3, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0082, 040, 0, msg4, ' ');

                // Reservado (vazio)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0122, 059, 0, "", ' ');

                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do Segmento R no arquivo de remessa do CNAB240 - Banco Inter.", ex);
            }
        }


        private string GerarDetalheSegmentoSRemessaCNAB240(Boleto boleto, ref int numeroRegistroGeral)
        {
            try
            {
                // Pega os primeiros 200 caracteres da mensagem (linhas 5 a 9)
                var msg5A9 = boleto.MensagemArquivoRemessa.PadRight(500, ' ').Substring(0, 200).FitStringLength(200, ' ');

                if (IsNullOrWhiteSpace(msg5A9))
                    return "";

                numeroRegistroGeral++;

                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0'); // Banco Inter
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0001", '0'); // Lote
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');    // Tipo de Registro
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroGeral, '0'); // Nº sequencial do registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "S", ' ');    // Segmento S
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 001, 0, "", ' ');     // Uso exclusivo FEBRABAN
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0016, 002, 0, "01", '0');   // Código de movimento remessa
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, "3", '0');    // Tipo de impressão: 3 = mensagem personalizada
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0019, 200, 0, msg5A9, ' '); // Mensagem
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0219, 022, 0, "", ' ');     // Complemento (reservado)

                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do Segmento S no arquivo de remessa do CNAB240 - Banco Inter.", ex);
            }
        }
        private string GerarTrailerLoteRemessaCNAC240(
    int totalLotes, int totalRegistros)
        {
            try
            {
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0'); // Código do banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "9999", '0'); // Lote de serviço
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "5", '0');    // Tipo de registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, string.Empty, ' '); // Branco (9)

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 006, 0, totalLotes, '0');     // Qtde de lotes
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 006, 0, totalRegistros, '0'); // Qtde de registros

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0030, 211, 0, string.Empty, ' '); // Branco (211)

                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar TRAILER DE LOTE (CNAB240).", ex);
            }
        }

        private string GerarTrailerArquivoRemessaCNAB240(int totalRegistros, decimal somatorioValores)
        {
            try
            {
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "077", '0'); // Código do banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "9999", '0'); // Lote de serviço (trailer = 9999)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "9", '0');    // Tipo de registro = 9
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, string.Empty, ' '); // Campo em branco

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 006, 0, totalRegistros, '0'); // Qtde de registros

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 018, 2, somatorioValores, '0'); // Somatório dos valores

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0042, 018, 0, 0, '0'); // Qtde moedas = zeros
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0060, 006, 0, string.Empty, ' '); // Aviso débito = branco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0066, 175, 0, string.Empty, ' '); // Campo em branco final

                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar TRAILER do arquivo CNAB240.", ex);
            }
        }
        #endregion

        #region RetornoCNAB240
        public void LerHeaderRetornoCNAB240(ArquivoRetorno arquivoRetorno, string registro)
        {
            // Layout CNAB240 - Banco Inter (Cód. 077)
            arquivoRetorno.Banco.Cedente = new Cedente();

            // Tipo de inscrição da empresa (1 = CPF, 2 = CNPJ) - posição 18 (1 dígito)
            string tipoInscricao = registro.Substring(17, 1);

            // CPF (11 dígitos) ou CNPJ (14 dígitos)
            if (tipoInscricao == "1") // CPF
                arquivoRetorno.Banco.Cedente.CPFCNPJ = registro.Substring(21, 11);
            else // CNPJ
                arquivoRetorno.Banco.Cedente.CPFCNPJ = registro.Substring(18, 14);

            // Código do convênio no banco - posição 33 a 52 (20 dígitos)
            arquivoRetorno.Banco.Cedente.Codigo = registro.Substring(32, 20).Trim();

            // Nome da empresa - posição 73 a 102 (30 caracteres)
            arquivoRetorno.Banco.Cedente.Nome = registro.Substring(72, 30).Trim();

            // Nome do banco (opcional) - posição 103 a 132 (30 caracteres)
            // arquivoRetorno.Banco.Nome = registro.Substring(102, 30).Trim();

            // Dados bancários
            arquivoRetorno.Banco.Cedente.ContaBancaria = new ContaBancaria();

            // Agência - posição 53 a 57 (5 dígitos)
            arquivoRetorno.Banco.Cedente.ContaBancaria.Agencia = registro.Substring(52, 5);

            // Dígito verificador da agência - posição 58 (1 dígito)
            arquivoRetorno.Banco.Cedente.ContaBancaria.DigitoAgencia = registro.Substring(57, 1);

            // Conta corrente - posição 59 a 70 (12 dígitos)
            arquivoRetorno.Banco.Cedente.ContaBancaria.Conta = registro.Substring(58, 12);

            // Dígito verificador da conta - posição 71 (1 dígito)
            arquivoRetorno.Banco.Cedente.ContaBancaria.DigitoConta = registro.Substring(70, 1);

            // Data de geração do arquivo - posição 144 a 151 (formato: DDMMAAAA)
            arquivoRetorno.DataGeracao = Utils.ToDateTime(
                Utils.ToInt32(registro.Substring(143, 8)).ToString("##-##-####")
            );

            // Número sequencial do arquivo - posição 158 a 163 (6 dígitos)
            arquivoRetorno.NumeroSequencial = Utils.ToInt32(registro.Substring(157, 6));
        }

        public void LerDetalheRetornoCNAB240SegmentoT(ref Boleto boleto, string registro)
        {
            try
            {
                // Nº Controle do Participante
                boleto.NumeroControleParticipante = registro.Substring(105, 25);

                // Carteira
                boleto.Carteira = registro.Substring(57, 1);

                switch (boleto.Carteira)
                {
                    case "1":
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaSimples;
                        break;
                    case "2":
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaVinculada;
                        break;
                    case "3":
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaCaucionada;
                        break;
                    case "4":
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaDescontada;
                        break;
                    case "5":
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaVendor;
                        break;
                    default:
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaSimples;
                        break;
                }

                // Identificação do Título no Banco
                string tmp = registro.Substring(37, 20);
                boleto.NossoNumero = tmp.Substring(8, 11);
                boleto.NossoNumeroDV = tmp.Substring(19, 1);

                // Identificação de Ocorrência
                boleto.CodigoOcorrencia = registro.Substring(15, 2);
                boleto.DescricaoOcorrencia = Cnab.OcorrenciaCnab240(boleto.CodigoOcorrencia);
                boleto.CodigoOcorrenciaAuxiliar = registro.Substring(213, 10);

                // Número do Documento
                boleto.NumeroDocumento = registro.Substring(58, 15);
                boleto.EspecieDocumento = TipoEspecieDocumento.NaoDefinido;

                // Valor do Título
                boleto.ValorTitulo = Convert.ToDecimal(registro.Substring(81, 15)) / 100;
                boleto.ValorTarifas = Convert.ToDecimal(registro.Substring(198, 15)) / 100;

                // Data Vencimento do Título
                boleto.DataVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(73, 8)).ToString("##-##-####"));

                // Dados Sacado
                boleto.Sacado = new Sacado();
                string str = registro.Substring(133, 15);
                boleto.Sacado.CPFCNPJ = str.Substring(str.Length - 14, 14);
                boleto.Sacado.Nome = registro.Substring(148, 40);

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
                // Juros de mora - posição 18 a 32 (15)
                boleto.ValorJurosDia = Convert.ToDecimal(registro.Substring(17, 15)) / 100;

                // Valor do Desconto Concedido - posição 33 a 47 (15)
                boleto.ValorDesconto = Convert.ToDecimal(registro.Substring(32, 15)) / 100;

                // Valor do Abatimento Concedido - posição 48 a 62 (15)
                boleto.ValorAbatimento = Convert.ToDecimal(registro.Substring(47, 15)) / 100;

                // Valor do IOF - posição 63 a 77 (15)
                boleto.ValorIOF = Convert.ToDecimal(registro.Substring(62, 15)) / 100;

                // Valor Pago pelo Sacado - posição 78 a 92 (15)
                boleto.ValorPago = Convert.ToDecimal(registro.Substring(77, 15)) / 100;

                // Valor Líquido a ser creditado - posição 93 a 107 (15)
                boleto.ValorPagoCredito = Convert.ToDecimal(registro.Substring(92, 15)) / 100;

                // Valor de Outras Despesas - posição 108 a 122 (15)
                boleto.ValorOutrasDespesas = Convert.ToDecimal(registro.Substring(107, 15)) / 100;

                // Valor de Outros Créditos - posição 123 a 137 (15)
                boleto.ValorOutrosCreditos = Convert.ToDecimal(registro.Substring(122, 15)) / 100;

                // Data da Ocorrência no banco - posição 138 a 145 (8)
                boleto.DataProcessamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(137, 8)).ToString("##-##-####"));

                // Data do Crédito - posição 146 a 153 (8)
                boleto.DataCredito = Utils.ToDateTime(Utils.ToInt32(registro.Substring(145, 8)).ToString("##-##-####"));

                // Registro bruto
                boleto.RegistroArquivoRetorno += registro + Environment.NewLine;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 240 / Segmento U (Banco Inter).", ex);
            }
        }

        #endregion

        #endregion


        #region CNAB400
        public void LerHeaderRetornoCNAB400(string registro)
        {
            try
            {
                if (!registro.StartsWith("02RETORNO"))
                    throw new Exception("O arquivo não é do tipo \"02RETORNO\"");



            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler HEADER do arquivo de RETORNO / CNAB 400 (Banco Inter).", ex);
            }
        }

        public void LerDetalheRetornoCNAB400Segmento1(ref Boleto boleto, string registro)
        {
            try
            {
                // Nº Controle do Participante
                boleto.NumeroControleParticipante = registro.Substring(37, 25).Trim();

                // Carteira (geralmente "1" para cobrança simples no Inter)
                boleto.Carteira = registro.Substring(107, 1);
                switch (boleto.Carteira)
                {
                    case "1":
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaSimples;
                        break;
                    case "2":
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaVinculada;
                        break;
                    default:
                        boleto.TipoCarteira = TipoCarteira.CarteiraCobrancaSimples;
                        break;
                }

                // Nosso Número (sem DV)
                boleto.NossoNumero = registro.Substring(70, 11);
                boleto.NossoNumeroDV = registro.Substring(81, 1);
                boleto.NossoNumeroFormatado = $"{boleto.Carteira}/{boleto.NossoNumero}-{boleto.NossoNumeroDV}";

                // Código e descrição da ocorrência
                boleto.CodigoOcorrencia = registro.Substring(108, 2);
                boleto.DescricaoOcorrencia = DescricaoOcorrenciaCnab400(boleto.CodigoOcorrencia);
                boleto.CodigoOcorrenciaAuxiliar = registro.Substring(318, 10).Trim();

                // Número do documento
                boleto.NumeroDocumento = registro.Substring(116, 10).Trim();

                // Espécie
                boleto.EspecieDocumento = AjustaEspecieCnab400(registro.Substring(173, 2));

                // Valores
                boleto.ValorTitulo = Convert.ToDecimal(registro.Substring(152, 13)) / 100;
                boleto.ValorTarifas = Convert.ToDecimal(registro.Substring(175, 13)) / 100;
                boleto.ValorOutrasDespesas = Convert.ToDecimal(registro.Substring(188, 13)) / 100;
                boleto.ValorIOF = Convert.ToDecimal(registro.Substring(214, 13)) / 100;
                boleto.ValorAbatimento = Convert.ToDecimal(registro.Substring(227, 13)) / 100;
                boleto.ValorDesconto = Convert.ToDecimal(registro.Substring(240, 13)) / 100;
                boleto.ValorPago = Convert.ToDecimal(registro.Substring(253, 13)) / 100;
                boleto.ValorJurosDia = Convert.ToDecimal(registro.Substring(266, 13)) / 100;
                boleto.ValorOutrosCreditos = Convert.ToDecimal(registro.Substring(279, 13)) / 100;

                // Datas
                boleto.DataProcessamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(110, 6)).ToString("##-##-##"));
                boleto.DataVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(146, 6)).ToString("##-##-##"));
                boleto.DataCredito = Utils.ToDateTime(Utils.ToInt32(registro.Substring(295, 6)).ToString("##-##-##"));

                // Registro bruto (para fins de log ou debug)
                boleto.RegistroArquivoRetorno += registro + Environment.NewLine;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 400 (Banco Inter).", ex);
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
            return $"CB{DateTime.Now.Date.Day:00}{DateTime.Now.Date.Month:00}{numeroSequencial.ToString().PadLeft(9, '0').Right(2)}.rem";
        }

        private string GerarHeaderRemessaCNAB400(int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;

                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 001, 0, "0", '0');               // 01. Identificação do Registro
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0002, 001, 0, "1", '0');               // 02. Identificação do arquivo remessa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0003, 007, 0, "REMESSA", ' ');         // 03. Literal remessa
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0010, 002, 0, "01", '0');              // 04. Código de serviço
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0012, 015, 0, "COBRANCA", ' ');        // 05. Literal serviço

                // ⚠️ Cedente.Codigo deve conter o código do convênio fornecido pelo banco (até 20 números)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0027, 020, 0, Cedente.Codigo, '0');

                // ⚠️ Cedente.Nome deve ter no máximo 30 caracteres SEM acento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0047, 030, 0, Cedente.Nome, ' ');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0077, 003, 0, "077", '0');             // 08. Código do banco na compensação
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0080, 015, 0, "INTER", ' ');           // 09. Nome do banco por extenso
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0095, 006, 0, DateTime.Now, '0');      // 10. Data da gravação do arquivo
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0101, 010, 0, string.Empty, ' ');      // 11. Campo em branco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0111, 007, 0, numeroArquivoRemessa, '0'); // 12. Número sequencial da remessa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0118, 277, 0, string.Empty, ' ');      // 13. Reservado (branco)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistroGeral, '0'); // 14. Número sequencial do registro

                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa CNAB400 para o Banco Inter.", ex);
            }
        }
        private string GerarDetalheRemessaCNAB400Registro1Inter(Boleto boleto, int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0001, 001, 0, "1", ' '); // Identificação do registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 019, 0, "", ' '); // Branco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0021, 003, 0, boleto.Carteira, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 004, 0, boleto.Banco.Cedente.ContaBancaria.Agencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0028, 009, 0, boleto.Banco.Cedente.ContaBancaria.Conta.PadLeft(9, '0'), '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0037, 001, 0, boleto.Banco.Cedente.ContaBancaria.DigitoConta, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0038, 025, 0, boleto.NumeroControleParticipante ?? "", ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0063, 003, 0, "", ' ');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0066, 001, 0, boleto.PercentualMulta > 0 ? "2" : (boleto.ValorMulta > 0 ? "1" : "0"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0067, 013, 2, boleto.ValorMulta, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0080, 004, 2, boleto.PercentualMulta, '0');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0084, 006, 0, boleto.DataMulta, '0');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0090, 011, 0, boleto.NossoNumero, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0101, 008, 0, "", ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0109, 002, 0, boleto.CodigoOcorrencia, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0111, 010, 0, boleto.NumeroDocumento, ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0121, 006, 0, boleto.DataVencimento, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0127, 013, 2, boleto.ValorTitulo, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0140, 002, 0, boleto.DiasProtesto.ToString("00"), '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0142, 006, 0, "", ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0148, 002, 0, "01", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0150, 001, 0, "N", ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0151, 006, 0, "", ' '); // Campo em branco conforme Inter
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0157, 003, 0, "", ' ');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0160, 001, 0, boleto.PercentualJurosDia > 0 ? "2" : (boleto.ValorJurosDia > 0 ? "1" : "0"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0161, 013, 2, boleto.ValorJurosDia, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0174, 004, 2, boleto.PercentualMulta, '0');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0178, 006, 0, boleto.DataJuros, '0');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0184, 001, 0, boleto.ValorDesconto > 0 ? "1" : "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0185, 013, 2, boleto.ValorDesconto, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0198, 004, 2, 0, '0'); // Percentual desconto não usado
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAA___________, 0202, 006, 0, boleto.DataDesconto, '0');

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0208, 013, 0, 0, '0'); // Campo em branco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0221, 002, 0, "2", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0223, 014, 0, boleto.Sacado.CPFCNPJ, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0237, 040, 0, boleto.Sacado.Nome, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0277, 038, 0, boleto.Sacado.Endereco, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0315, 002, 0, boleto.Sacado.Endereco.UF, ' ');

                // 🔧 CEP: garantir apenas números
                string cepNumerico = new string((boleto.Sacado.Endereco.CEP ?? "").Where(char.IsDigit).ToArray());
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0317, 008, 0, cepNumerico.PadLeft(8, '0'), '0');

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0325, 070, 0, boleto.CodigoInstrucao1 ?? "", ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistroGeral.ToString().PadLeft(6, '0'), '0');

                reg.CodificarLinha();

                return reg.LinhaRegistro.Length < 400
                    ? reg.LinhaRegistro.PadRight(400, ' ')
                    : reg.LinhaRegistro.Substring(0, 400);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar detalhe CNAB400 Inter - REGISTRO 1.", ex);
            }
        }



        private string GerarDetalheRemessaCNAB400Registro2(Boleto boleto, ref int numeroRegistroGeral)
        {
            numeroRegistroGeral++;

            var reg = new TRegistroEDI();

          
            // 001 a 001 - Tipo de registro
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 001, 0, "2", '0');

            // 002 a 079 - 2ª mensagem
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 078, 0, boleto.MensagemArquivoRemessa ?? string.Empty, ' ');

            // 080 a 157 - 3ª mensagem
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0080, 078, 0, boleto.MensagemArquivoRemessa ?? string.Empty, ' ');

            // 158 a 235 - 4ª mensagem
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0158, 078, 0, boleto.MensagemArquivoRemessa ?? string.Empty, ' ');

            // 236 a 313 - 5ª mensagem
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0236, 078, 0, boleto.MensagemArquivoRemessa ?? string.Empty, ' ');

            // 314 a 319 - Data limite para desconto 2 (DDMMAA ou zeros)
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0314, 006, 0, "0", '0');

            // 320 a 332 - Valor desconto 2
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0320, 013, 2, "0", '0');

            // 333 a 336 - Percentual desconto 2
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0333, 004, 2, "0", '0');

            // 337 a 346 - Campo em branco
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0337, 010, 0, string.Empty, ' ');

            // 347 a 352 - Data limite para desconto 3
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0347, 006, 0, "0", '0');

            // 353 a 365 - Valor desconto 3
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0353, 013, 2, "0", '0');

            // 366 a 369 - Percentual desconto 3
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0366, 004, 2, "0", '0');
            // 370 a 379 - Campo em branco
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0370, 010, 0, string.Empty, ' ');

            // 380 a 390 - Nosso número (Inter preenche no retorno)
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0380, 011, 0, "0", '0');

            // 391 a 394 - Campo em branco
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0391, 004, 0, string.Empty, ' ');

            // 395 a 400 - Número sequencial do registro
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistroGeral, '0');

            reg.CodificarLinha();
            return reg.LinhaRegistro.PadRight(400, ' ');
        }



        private string GerarTrailerRemessaCNAB400(ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;

                var reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 001, 0, "9", '0'); // Tipo registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 393, 0, string.Empty, ' '); // Conteúdo em branco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistroGeral, '0'); // Número sequencial

                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geração do registro TRAILER do arquivo de REMESSA (Banco Inter).", ex);
            }
        }


        private string DescricaoOcorrenciaCnab400(string codigo)
        {
            switch (codigo)
            {
                case "02": return "Entrada Confirmada";
                case "03": return "Entrada Rejeitada";
                case "06": return "Liquidação normal";
                case "09": return "Baixado Automaticamente via Arquivo";
                case "10": return "Baixado conforme instruções da Agência";
                case "11": return "Em Ser - Arquivo de Títulos pendentes";
                case "12": return "Abatimento Concedido";
                case "13": return "Abatimento Cancelado";
                case "14": return "Vencimento Alterado";
                case "15": return "Liquidação em Cartório";
                case "17": return "Liquidação após baixa ou Título não registrado";
                case "18": return "Acerto de Depositária";
                case "19": return "Confirmação Recebimento Instrução de Protesto";
                case "20": return "Confirmação Recebimento Instrução Sustação de Protesto";
                case "21": return "Acerto do Controle do Participante";
                case "23": return "Entrada do Título em Cartório";
                case "24": return "Entrada rejeitada por CEP Irregular";
                case "27": return "Baixa Rejeitada";
                case "28": return "Débito de tarifas/custas";
                case "30": return "Alteração de Outros Dados Rejeitados";
                case "32": return "Instrução Rejeitada";
                case "33": return "Confirmação Pedido Alteração Outros Dados";
                case "34": return "Retirado de Cartório e Manutenção Carteira";
                case "35": return "Desagendamento de débito automático";
                case "68": return "Acerto dos dados de rateio de Crédito";
                case "69": return "Cancelamento dos dados de rateio";
                default: return "";
            }
        }

        private TipoEspecieDocumento AjustaEspecieCnab400(string codigoEspecie)
        {
            switch (codigoEspecie)
            {
                case "01": return TipoEspecieDocumento.DM; // Duplicata Mercantil
                case "02": return TipoEspecieDocumento.NP; // Nota Promissória
                case "03": return TipoEspecieDocumento.NS; // Nota de Seguro
                case "05": return TipoEspecieDocumento.RC; // Recibo
                case "10": return TipoEspecieDocumento.LC; // Letra de Câmbio
                case "12": return TipoEspecieDocumento.DS; // Duplicata de Serviço
                default: return TipoEspecieDocumento.OU; // Outros
            }
        }

        private string AjustaEspecieCnab400(TipoEspecieDocumento especieDocumento)
        {
            switch (especieDocumento)
            {
                case TipoEspecieDocumento.DM: return "01";
                case TipoEspecieDocumento.NP: return "02";
                case TipoEspecieDocumento.NS: return "03";
                case TipoEspecieDocumento.RC: return "05";
                case TipoEspecieDocumento.LC: return "10";
                case TipoEspecieDocumento.DS: return "12";
                default: return "99"; // Outros
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
        private string GerarMensagemRemessaCNAB240(Boleto boleto, int numeroRegistro)
        {
            var reg = new TRegistroEDI();

            // Tipo de Registro: "3" (detalhe), Segmento livre personalizado
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, 3, '0')); // Código do banco (use conforme necessário)
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0004, 004, 0, "Y", ' ')); // Tipo de Segmento (Ex: Y ou outro)
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0008, 003, 0, "01", ' ')); // Código de movimento (mensagem livre)

            // Nosso número (pode ser parte do controle do detalhe)
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0011, 020, 0, boleto.NossoNumero?.PadLeft(20, '0') ?? string.Empty, ' '));

            // Instruções (Mensagens livres ao sacado) - total de 4x 80 caracteres
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0031, 080, 0, boleto.InstrucoesBoleto?.ElementAtOrDefault(0) ?? string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0111, 080, 0, boleto.InstrucoesBoleto?.ElementAtOrDefault(1) ?? string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0191, 080, 0, boleto.InstrucoesBoleto?.ElementAtOrDefault(2) ?? string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0271, 080, 0, boleto.InstrucoesBoleto?.ElementAtOrDefault(3) ?? string.Empty, ' '));

            // Seu Número
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0351, 015, 0, boleto.SeuNumero.ToString().PadLeft(15, '0'), ' '));

            // Número sequencial do registro no arquivo
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0366, 006, 0, numeroRegistro, '0'));

            reg.CodificarLinha();

            string detalhe = Utils.SubstituiCaracteresEspeciais(reg.LinhaRegistro);
            return detalhe;
        }

        public string GerarMensagemCliente(TipoArquivo tipoArquivo, Boleto boleto, ref int numeroRegistro)
        {
            try
            {
                numeroRegistro++;

                string detalhe = string.Empty;

                // Define Carteira e Variação padrão do Banco Inter
                string carteira = "112";
                string variacao = "A";
                string carteiraComVariacao = carteira + variacao;

                // Garante que a conta bancária esteja instanciada
                boleto.Banco.Cedente.ContaBancaria.CarteiraPadrao = carteira;
                boleto.Banco.Cedente.ContaBancaria.VariacaoCarteiraPadrao = "112";
                
                boleto.VariacaoCarteira = variacao;

                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB400:
                             detalhe = GerarDetalheRemessaCNAB400Registro2(boleto,ref numeroRegistro);
                        break;

                    case TipoArquivo.CNAB240:
                        detalhe = GerarMensagemRemessaCNAB240(boleto, numeroRegistro);
                        break;

                    default:
                        throw new Exception("Tipo de arquivo CNAB não suportado.");
                }

                return detalhe;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geração do detalhe do arquivo de remessa.", ex);
            }
        }

        private string GerarMensagemRemessaCNAB400(Boleto boleto, int numeroRegistro)
        {
            var reg = new TRegistroEDI();

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0001, 001, 0, "2", ' ')); // Tipo de Registro
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 011, 0, string.Empty, ' ')); // Identificação
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0013, 008, 0, boleto.NossoNumero?.PadLeft(8, '0') ?? string.Empty, ' ')); // Nosso Número (até 8 posições no Inter)

            // Instruções ao sacado – normalmente até 4 linhas de 80 caracteres cada
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0021, 080, 0, boleto.InstrucoesBoleto?.ElementAtOrDefault(0) ?? string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0101, 080, 0, boleto.InstrucoesBoleto?.ElementAtOrDefault(1) ?? string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0181, 080, 0, boleto.InstrucoesBoleto?.ElementAtOrDefault(2) ?? string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0261, 080, 0, boleto.InstrucoesBoleto?.ElementAtOrDefault(3) ?? string.Empty, ' '));

            // Número de controle do participante (Seu Número)
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0342, 010, 0, boleto.SeuNumero, '0'));
            // Reservado / Filler
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0366, 029, 0, string.Empty, ' '));

            // Número sequencial do registro no arquivo
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistro, '0'));

            reg.CodificarLinha();

            string detalhe = Utils.SubstituiCaracteresEspeciais(reg.LinhaRegistro);
            return detalhe;
        }

        #endregion
    }

}