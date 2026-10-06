using System.Collections.Generic;
using Boleto2Net.Exceptions;
using System.Web.UI;
using System;
using System.Threading.Tasks;
using Boleto2Net.DebitoAutomatico;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;

[assembly: WebResource("Boleto2Net.Imagens.748.jpg", "image/jpg")]
namespace Boleto2Net
{
    internal sealed class BancoSicredi : IBanco
    {
        internal static Lazy<IBanco> Instance { get; } = new Lazy<IBanco>(() => new BancoSicredi());

        public Cedente Cedente { get; set; }
        public int Codigo { get; } = 748;
        public string Nome { get; } = "Sicredi";
        public string Digito { get; } = "X";
        public List<string> IdsRetornoCnab400RegistroDetalhe { get; } = new List<string> { "1" };
        public bool RemoveAcentosArquivoRemessa { get; } = true;

        public void FormataCedente()
        {
            var contaBancaria = Cedente.ContaBancaria;

            if (!CarteiraFactory<BancoSicredi>.CarteiraEstaImplementada(contaBancaria.CarteiraComVariacaoPadrao))
                throw Boleto2NetException.CarteiraNaoImplementada(contaBancaria.CarteiraComVariacaoPadrao);

            contaBancaria.FormatarDados("PAGÁVEL EM QUALQUER BANCO ATÉ O VENCIMENTO.", "", "", 9);

            Cedente.CodigoFormatado = $"{contaBancaria.Agencia}.{contaBancaria.OperacaoConta}.{Cedente.Codigo}";
        }

        public string FormataCodigoBarraCampoLivre(Boleto boleto)
        {
            var carteira = CarteiraFactory<BancoSicredi>.ObterCarteira(boleto.CarteiraComVariacao);
            return carteira.FormataCodigoBarraCampoLivre(boleto);
        }

        public void FormataNossoNumero(Boleto boleto)
        {
            var carteira = CarteiraFactory<BancoSicredi>.ObterCarteira(boleto.CarteiraComVariacao);
            carteira.FormataNossoNumero(boleto);
        }

        public string FormatarNomeArquivoRemessa(int sequencial, int remessaDia)
        {
            var agora = DateTime.Now;

            var mes = agora.Month.ToString();
            if (mes == "10") mes = "O";
            if (mes == "11") mes = "N";
            if (mes == "12") mes = "D";
            var dia = agora.Day.ToString().PadLeft(2, '0');

            //Caso for gerado mais de um arquivo de remessa alterar a extensão do aquivo para "RM" + o contador do numero do arquivo de remessa gerado no dia
            var nomeArquivoRemessa = string.Format("{0}{1}{2}.{3}", Cedente.CodigoBeneficiario, mes, dia, remessaDia != 0 ? $"RM{remessaDia}" : "CRM");

            return nomeArquivoRemessa;
        }

        public string GerarDetalheRemessa(TipoArquivo tipoArquivo, Boleto boleto, ref int numeroRegistro, bool instrucaoCancelameto = false)
        {
            numeroRegistro++;

            string _detalhe = " ";

            switch (tipoArquivo) {
                case TipoArquivo.CNAB400:
                    boleto.VariacaoCarteira = "A";
                    _detalhe = GerarDetalheRemessaCNAB400(boleto, numeroRegistro, tipoArquivo, instrucaoCancelameto);
                    break;
            }

            return _detalhe;
        }

        public string GerarDetalheRemessaCNAB400(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo, bool instrucaoCancelameto = false)
        {
            string detalhe = string.Empty;
            //Redireciona para o Detalhe da remessa Conforme o "Tipo de Documento" = "Tipo de Cobrança do CNAB400":
            //  A = 'A' - SICREDI com Registro
            // C1 = 'C' - SICREDI sem Registro Impressão Completa pelo Sicredi
            // C2 = 'C' - SICREDI sem Registro Pedido de bloquetos pré-impressos
            if (boleto.VariacaoCarteira.Equals("A"))
                detalhe = GerarDetalheRemessaCNAB400_A(boleto, numeroRegistro, tipoArquivo, instrucaoCancelameto);
            else if (boleto.VariacaoCarteira.Equals("C1"))
                detalhe = GerarDetalheRemessaCNAB400_C1(boleto, numeroRegistro, tipoArquivo);
            else if (boleto.VariacaoCarteira.Equals("C2"))
                detalhe = GerarDetalheRemessaCNAB400_C2(boleto, numeroRegistro, tipoArquivo);
            return detalhe;
        }

        private string GerarDetalheRemessaCNAB400_A(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo, bool instrucaoCancelameto = false)
        {
            TRegistroEDI reg = new TRegistroEDI();
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0001, 001, 0, "1", ' '));                                                    //001-001
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 001, 0, "A", ' '));                                                    //002-002  'A' - SICREDI com Registro
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0003, 001, 0, "A", ' '));                                                    //003-003  'A' - Simples
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0004, 001, 0, "A", ' '));                                                    //004-004  'A' – Normal
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0005, 012, 0, string.Empty, ' '));                                           //005-016
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0017, 001, 0, "A", ' '));                                                    //017-017  Tipo de moeda: 'A' - REAL
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0018, 001, 0, "A", ' '));                                                    //018-018  Tipo de desconto: 'A' - VALOR
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0019, 001, 0, "A", ' '));                                                    //019-019  Tipo de juros: 'A' - VALOR
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0020, 028, 0, string.Empty, ' '));                                           //020-047
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0048, 009, 0, boleto.NossoNumero, '0'));                                     //048-056

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0057, 006, 0, string.Empty, ' '));                                           //057-062
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediDataAAAAMMDD_________, 0063, 008, 0, boleto.DataProcessamento, ' '));                               //063-070
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0071, 001, 0, string.Empty, ' '));                                           //071-071
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0072, 001, 0, 'N', ' '));                                                    //072-072 'S' - Para postar o título diretamente ao pagador / 'N' - Não Postar e remeter para o beneficiário
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 001, 0, string.Empty, ' '));                                           //073-073

            switch (boleto.Banco.Cedente.ContaBancaria.TipoImpressaoBoleto) {
                case TipoImpressaoBoleto.Banco:
                    reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 001, 0, 'A', ' '));                                            //074-074 'A' - Impressão é feita pelo Sicredi
                    break;
                case TipoImpressaoBoleto.Empresa:
                    reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 001, 0, 'B', ' '));                                            //'B' – Impressão é feita pelo Beneficiário
                    break;
            }

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0075, 002, 0, 0, '0'));                                                     //075-076
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0077, 002, 0, 0, '0'));                                                     //077-078
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0079, 004, 0, string.Empty, ' '));                                          //079-082
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0083, 010, 2, boleto.ValorDesconto, '0'));                                  //083-092
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0093, 004, 0, boleto.PercentualMulta, '0'));                                //093-096
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0097, 012, 0, string.Empty, ' '));                                          //097-108
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0109, 002, 0, instrucaoCancelameto ? "02" : "01", ' '));                                                  //109-110 01 - Cadastro de título;
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0111, 010, 0, boleto.SeuNumero, '0'));                                    //111-120
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediDataDDMMAA___________, 0121, 006, 0, boleto.DataVencimento, ' '));                                 //121-126
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0127, 013, 2, boleto.ValorTitulo, '0'));                                    //127-139
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0140, 009, 0, string.Empty, ' '));                                          //140-148
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0149, 001, 0, EspecieDocumentoSicredi(boleto.EspecieDocumento), 'A'));      //149-149
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0150, 001, 0, "S", ' '));                                         //150-150
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediDataDDMMAA___________, 0151, 006, 0, boleto.DataProcessamento, ' '));                              //151-156

            //Instruções de protesto
            string vInstrucao1 = "";
            string vInstrucao2 = "";
            switch (boleto.CodigoProtesto) {
                case TipoCodigoProtesto.NaoProtestar:
                    vInstrucao1 = "00";
                    vInstrucao2 = "0";
                    break;
                case TipoCodigoProtesto.ProtestarDiasCorridos:
                    vInstrucao1 = "06";
                    vInstrucao2 = boleto.DiasProtesto.ToString();
                    break;
            }
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0157, 002, 0, vInstrucao1, '0'));                                           //157-158
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0159, 002, 0, vInstrucao2, '0'));                                           //159-160

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0161, 013, 2, boleto.PercentualJurosDia, '0'));                             //161-173

            //DataDesconto
            string vDataDesconto = "000000";
            if (!boleto.DataDesconto.Equals(DateTime.MinValue))
                vDataDesconto = boleto.DataDesconto.ToString("ddMMyy");

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0174, 006, 0, vDataDesconto, '0'));                                         //174-179
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0180, 013, 2, boleto.ValorDesconto, '0'));                                  //180-192
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0193, 013, 0, 0, '0'));                                                     //193-205
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0206, 013, 2, boleto.ValorAbatimento, '0'));                                //206-218

            //Regra Tipo de Inscrição Sacado
            string vCpfCnpjSac = "0";
            if (boleto.Sacado.CPFCNPJ.Length.Equals(11)) vCpfCnpjSac = "1";
            else if (boleto.Sacado.CPFCNPJ.Length.Equals(14)) vCpfCnpjSac = "2";

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0219, 001, 0, vCpfCnpjSac, '0'));                                           //219-219
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0220, 001, 0, "0", '0'));                                                   //220-220

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0221, 014, 0, boleto.Sacado?.CPFCNPJ == null ?
                                                                                                          throw new Exception($"CNPJ é Obrigatório") :
                                                                                                          boleto.Sacado?.CPFCNPJ, '0')); //221-234

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0235, 040, 0, boleto.Sacado?.Nome == null ?
                                                                                                          throw new Exception($"Nome é Obrigatório") :
                                                                                                          boleto.Sacado?.Nome.ToUpper(), ' ')); //235-274

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0275, 040, 0, boleto.Sacado?.Endereco?.LogradouroEndereco == null ?
                                                                                                          throw new Exception($"Endereço Obrigatório") :
                                                                                                          boleto.Sacado?.Endereco?.LogradouroEndereco.ToUpper(), ' ')); //275-314

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0315, 005, 0, 0, '0'));                                                     //315-319
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0320, 006, 0, 0, '0'));                                                     //320-325
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0326, 001, 0, string.Empty, ' '));                                          //326-326

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0327, 008, 0, boleto.Sacado.Endereco.CEP == null ?
                                                                                                          throw new Exception($"CEP Obrigatório") :
                                                                                                          Regex.Replace(boleto.Sacado.Endereco.CEP, "-", ""), '0'));//327-334

            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0335, 005, 0, 0, '0')); //327-334
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0340, 014, 0, string.Empty, ' ')); //340-353
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0354, 041, 0, string.Empty, ' '));  //354-394
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistro, '0')); //395-400

            reg.CodificarLinha();

            string _detalhe = Utils.SubstituiCaracteresEspeciais(reg.LinhaRegistro);

            return _detalhe;
        }

        private string EspecieDocumentoSicredi(TipoEspecieDocumento EspecieDocumento)
        {
            switch (EspecieDocumento) {
                case TipoEspecieDocumento.DMI:
                    return "A";
                case TipoEspecieDocumento.DR:
                    return "B";
                case TipoEspecieDocumento.NP:
                    return "C";
                case TipoEspecieDocumento.NPR:
                    return "D";
                case TipoEspecieDocumento.NS:
                    return "E";
                case TipoEspecieDocumento.RC:
                    return "G";
                case TipoEspecieDocumento.LC:
                    return "H";
                case TipoEspecieDocumento.DSI:
                    return "J";
                case TipoEspecieDocumento.OU:
                    return "K";
            }

            return string.Empty;
        }

        private string GerarDetalheRemessaCNAB400_C1(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            throw new NotImplementedException("Função não implementada.");
        }
        private string GerarDetalheRemessaCNAB400_C2(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            throw new NotImplementedException("Função não implementada.");
        }

        public string GerarHeaderRemessa(TipoArquivo tipoArquivo, int numeroArquivoRemessa, ref int numeroRegistro)
        {
            try {
                numeroRegistro++;

                string _header = " ";

                switch (tipoArquivo) {
                    case TipoArquivo.CNAB400:
                        _header = GerarHeaderRemessaCNAB400(0, numeroArquivoRemessa);
                        break;
                }

                return _header;

            }
            catch (Exception ex) {
                throw new Exception("Erro durante a geração do HEADER do arquivo de REMESSA.", ex);
            }
        }

        private string GerarHeaderRemessaCNAB400(int numeroConvenio, int numeroArquivoRemessa)
        {
            try {
                TRegistroEDI reg = new TRegistroEDI();
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0001, 001, 0, "0", ' '));                             //001-001
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 001, 0, "1", ' '));                             //002-002
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0003, 007, 0, "REMESSA", ' '));                       //003-009
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0010, 002, 0, "01", ' '));                            //010-011
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0012, 015, 0, "COBRANCA", ' '));                      //012-026
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0027, 005, 0, Cedente.CodigoBeneficiario, ' '));                  //027-031
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0032, 014, 0, Cedente.CPFCNPJ, ' '));                 //032-045
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0046, 031, 0, "", ' '));                              //046-076
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0077, 003, 0, "748", ' '));                           //077-079
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0080, 015, 0, "SICREDI", ' '));                       //080-094
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediDataAAAAMMDD_________, 0095, 008, 0, DateTime.Now, ' '));                    //095-102
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0103, 008, 0, "", ' '));                              //103-110
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0111, 007, 0, numeroArquivoRemessa.ToString(), '0')); //111-117
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0118, 273, 0, "", ' '));                              //118-390
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0391, 004, 0, "2.00", ' '));                          //391-394
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0395, 006, 0, "000001", '0'));                        //395-400
                reg.CodificarLinha();

                string vLinha = reg.LinhaRegistro;
                string _header = Utils.SubstituiCaracteresEspeciais(vLinha);

                return _header;
            }
            catch (Exception ex) {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB400.", ex);
            }
        }

        public string GerarTrailerRemessa(TipoArquivo tipoArquivo, int numeroArquivoRemessa, ref int numeroRegistroGeral, decimal valorBoletoGeral, int numeroRegistroCobrancaSimples, decimal valorCobrancaSimples, int numeroRegistroCobrancaVinculada, decimal valorCobrancaVinculada, int numeroRegistroCobrancaCaucionada, decimal valorCobrancaCaucionada, int numeroRegistroCobrancaDescontada, decimal valorCobrancaDescontada)
        {
            try {
                numeroRegistroGeral++;

                string trailer = " ";

                switch (tipoArquivo) {
                    case TipoArquivo.CNAB400:
                        trailer = GerarTrailerRemessa400(numeroRegistroGeral);
                        break;
                }

                return trailer;

            }
            catch (Exception ex) {
                throw new Exception("", ex);
            }
        }

        private string GerarTrailerRemessa400(int numeroRegistroGeral)
        {
            try {
                TRegistroEDI reg = new TRegistroEDI();
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0001, 001, 0, "9", ' '));                         //001-001
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 001, 0, "1", ' '));                         //002-002
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0003, 003, 0, "748", ' '));                       //003-006
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0006, 005, 0, Cedente.CodigoBeneficiario, ' '));  //006-010                
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0011, 384, 0, string.Empty, ' '));                //011-394
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistroGeral, '0'));         //395-400

                reg.CodificarLinha();

                string vLinha = reg.LinhaRegistro;
                string _trailer = Utils.SubstituiCaracteresEspeciais(vLinha);

                return _trailer;
            }
            catch (Exception ex) {
                throw new Exception("Erro durante a geração do registro TRAILER do arquivo de REMESSA.", ex);
            }
        }

        public void LerDetalheRetornoCNAB240SegmentoT(ref Boleto boleto, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerDetalheRetornoCNAB240SegmentoU(ref Boleto boleto, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerDetalheRetornoCNAB400Segmento1(ref Boleto boleto, string registro)
        {
            try {
                // Identificação do Título no Banco
                boleto.NossoNumero = registro.Substring(47, 9);
                boleto.NossoNumeroDV = registro.Substring(55, 1);
                boleto.NossoNumeroFormatado = string.Format("{0}/{1}-{2}", boleto.NossoNumero.Substring(0, 2), boleto.NossoNumero.Substring(2, 6), boleto.NossoNumeroDV);

                // Identificação de Ocorrência
                boleto.CodigoOcorrencia = registro.Substring(108, 2);
                boleto.DescricaoOcorrencia = DescricaoOcorrenciaCnab400(boleto.CodigoOcorrencia);

                // Data Ocorrência no Banco
                boleto.DataProcessamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(110, 6)).ToString("##-##-##"));

                // Número do Documento
                boleto.NumeroDocumento = registro.Substring(116, 10).Trim();

                //Data Vencimento do Título
                boleto.DataVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(146, 6)).ToString("##-##-##"));

                //Valores do Título
                boleto.ValorTitulo = Convert.ToDecimal(registro.Substring(152, 13)) / 100;
                boleto.ValorAbatimento = Convert.ToDecimal(registro.Substring(227, 13)) / 100;
                boleto.ValorDesconto = Convert.ToDecimal(registro.Substring(240, 13)) / 100;
                boleto.ValorPago = Convert.ToDecimal(registro.Substring(253, 13)) / 100;
                boleto.ValorJurosDia = Convert.ToDecimal(registro.Substring(266, 13)) / 100;
                boleto.ValorOutrosCreditos = Convert.ToDecimal(registro.Substring(279, 13)) / 100;

                boleto.ValorPago += boleto.ValorJurosDia;

                // Data do Crédito
                boleto.DataCredito = Utils.ToDateTime(Utils.ToInt32(registro.Substring(294, 6)).ToString("##-##-##"));

                //boleto.CodigoMotivo = Motivos da Ocorrencia
                boleto.CodigoMotivo = registro.Substring(318, 10);
                boleto.MotivoDescricao = GetMotivo(boleto.CodigoMotivo);
                // Identificação de Ocorrência - Código Auxiliar
                boleto.CodigoOcorrenciaAuxiliar = registro.Substring(381, 10);


                // Registro Retorno
                boleto.RegistroArquivoRetorno = boleto.RegistroArquivoRetorno + registro + Environment.NewLine;
            }
            catch (Exception ex) {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 400.", ex);
            }
        }

        private string DescricaoOcorrenciaCnab400(string codigo)
        {
            switch (codigo) {
                case "02":
                    return "Confirmação de entrada";
                case "03":
                    return "Entrada rejeitada";
                case "04":
                    return "Baixa de título liquidado por edital";
                case "06":
                    return "Liquidação normal";
                case "07":
                    return "Liquidação parcial";
                case "08":
                    return "Baixa por pagamento, liquidação pelo saldo";
                case "09":
                    return "Devolução automática";
                case "10":
                    return "Baixado conforme instruções";
                case "11":
                    return "Arquivo levantamento";
                case "12":
                    return "Concessão de abatimento";
                case "13":
                    return "Cancelamento de abatimento";
                case "14":
                    return "Vencimento alterado";
                case "15":
                    return "Pagamento em cartório";
                case "16":
                    return "Alteração de dados";
                case "17":
                    return "Liquidação após baixa";
                case "18":
                    return "Alteração de instruções";
                case "19":
                    return "Confirmação de instrução protesto";
                case "20":
                    return "Confirmação de instrução para sustar protesto";
                case "21":
                    return "Aguardando autorização para protesto por edital";
                case "22":
                    return "Protesto sustado por alteração de vencimento e prazo de cartório";
                case "23":
                    return "Confirmação da entrada em cartório";
                case "24":
                    return "Entrada rejeitada por CEP irregular";
                case "25":
                    return "Devolução, liquidado anteriormente";
                case "26":
                    return "Devolvido pelo cartório – erro de informação";
                case "27":
                    return "Baixa rejeitada";
                case "28":
                    return "Tarifa";
                case "29":
                    return "Rejeição do pagador";
                case "30":
                    return "Alteração rejeitada";
                case "31":
                    return "Título em trânsito pago em cartório";
                case "32":
                    return "Instrução rejeitada";
                case "33":
                    return "Confirmação de pedido de alteração de outros dados ";
                case "34":
                    return "Retirado de cartório e manutenção em carteira";
                case "35":
                    return "Aceite do pagador";
                case "40":
                    return "Baixa de títulos protestados";
                case "41":
                    return "Despesa de aponte";
                case "42":
                    return "Alteração de título";
                case "43":
                    return "Relação de títulos";
                case "44":
                    return "Manutenção mensal";
                case "45":
                    return "Sustação de cartório e envio de título a cartório";
                case "46":
                    return "Fornecimento de formulário pré-impresso";
                case "47":
                    return "Confirmação de entrada – Pagador DDA";
                case "68":
                    return "Acerto dos dados do rateio de crédito";
                case "69":
                    return "Cancelamento dos dados do rateio";
                default:
                    return "";
            }
        }

        private List<string> GetMotivo(string codigos)
        {
            List<string> motivos = new List<string>();

            //em uma linha de 10 caracteres é podem ter 5 motivos distintos, geralmente os dois primeiros
            int count = 2;
            for (int i = 0; i <= codigos.Length; i += count) {

                string codigo = "";

                if (i == codigos.Length)
                    codigo = codigos.Substring(codigos.Length - 2);
                else
                    codigo = codigos.Substring(i, count);

                if (i != 0 && codigo == "00")
                    continue;

                switch (codigo) {
                    case "00":
                        motivos.Add("Não há Motivo de Ocorrência");
                        break;
                    case "01":
                        motivos.Add("Código do banco inválido");
                        break;
                    case "02":
                        motivos.Add("Código do registro detalhe inválido");
                        break;
                    case "03":
                        motivos.Add("Código da ocorrência inválido");
                        break;
                    case "04":
                        motivos.Add("Código de ocorrência não permitida para a carteira");
                        break;
                    case "05":
                        motivos.Add("Código de ocorrência não numérico");
                        break;
                    case "07":
                        motivos.Add("Cooperativa/agência/conta/dígito inválidos");
                        break;
                    case "08":
                        motivos.Add("Nosso número inválido");
                        break;
                    case "09":
                        motivos.Add("Nosso número duplicado");
                        break;
                    case "10":
                        motivos.Add("Carteira inválida");
                        break;
                    case "14":
                        motivos.Add("Título protestado");
                        break;
                    case "15":
                        motivos.Add("Cooperativa/carteira/agência/conta/nosso número inválidos");
                        break;
                    case "16":
                        motivos.Add("Data de vencimento inválida");
                        break;
                    case "17":
                        motivos.Add("Data de vencimento anterior à data de emissão");
                        break;
                    case "18":
                        motivos.Add("Vencimento fora do prazo de operação");
                        break;
                    case "20":
                        motivos.Add("Valor do título inválido");
                        break;
                    case "21":
                        motivos.Add("Espécie do título inválida");
                        break;
                    case "22":
                        motivos.Add("Espécie não permitida para a carteira");
                        break;
                    case "24":
                        motivos.Add("Data de emissão inválida");
                        break;
                    case "29":
                        motivos.Add("Valor do desconto maior/igual ao valor do título");
                        break;
                    case "31":
                        motivos.Add("Concessão de desconto - existe desconto anterior");
                        break;
                    case "33":
                        motivos.Add("Valor do abatimento inválido");
                        break;
                    case "34":
                        motivos.Add("Valor do abatimento maior/igual ao valor do título");
                        break;
                    case "36":
                        motivos.Add("Concessão de abatimento - existe abatimento anterior");
                        break;
                    case "38":
                        motivos.Add("Prazo para protesto inválido");
                        break;
                    case "39":
                        motivos.Add("Pedido para protesto não permitido para o título");
                        break;
                    case "40":
                        motivos.Add("Título com ordem de protesto emitida");
                        break;
                    case "41":
                        motivos.Add("Pedido cancelamento/sustação sem instrução de protesto");
                        break;
                    case "44":
                        motivos.Add("Cooperativa de crédito/agência beneficiária não prevista");
                        break;
                    case "45":
                        motivos.Add("Nome do pagador inválido");
                        break;
                    case "46":
                        motivos.Add("Tipo/número de inscrição do pagador inválidos");
                        break;
                    case "47":
                        motivos.Add("Endereço do pagador não informado");
                        break;
                    case "48":
                        motivos.Add("CEP irregular");
                        break;
                    case "49":
                        motivos.Add("Número de Inscrição do pagador/avalista inválido");
                        break;
                    case "50":
                        motivos.Add("Pagador/avalista não informado");
                        break;
                    case "60":
                        motivos.Add("Movimento para título não cadastrado");
                        break;
                    case "63":
                        motivos.Add("Entrada para título já cadastrado");
                        break;
                    case "A":
                        motivos.Add("Aceito");
                        break;
                    case "D":
                        motivos.Add("Desprezado");
                        break;
                    case "A1":
                        motivos.Add("Praça do pagador não cadastrada.");
                        break;
                    case "A2":
                        motivos.Add("Tipo de cobrança do título divergente com a praça do pagador.");
                        break;
                    case "A3":
                        motivos.Add("Cooperativa/agência depositária divergente: atualiza o cadastro de praças da Coop./agência beneficiária");
                        break;
                    case "A4":
                        motivos.Add("Beneficiário não cadastrado ou possui CGC/CIC inválido");
                        break;
                    case "A5":
                        motivos.Add("Pagador não cadastrado");
                        break;
                    case "A6":
                        motivos.Add("Data da instrução/ocorrência inválida");
                        break;
                    case "A7":
                        motivos.Add("Ocorrência não pode ser comandada");
                        break;
                    case "A8":
                        motivos.Add("Recebimento da liquidação fora da rede Sicredi - via compensação eletrônica");
                        break;
                    case "B4":
                        motivos.Add("Tipo de moeda inválido");
                        break;
                    case "B5":
                        motivos.Add("Tipo de desconto/juros inválido");
                        break;
                    case "B6":
                        motivos.Add("Mensagem padrão não cadastrada");
                        break;
                    case "B7":
                        motivos.Add("Seu número inválido");
                        break;
                    case "B8":
                        motivos.Add("Percentual de multa inválido");
                        break;
                    case "B9":
                        motivos.Add("Valor ou percentual de juros inválido");
                        break;
                    case "C1":
                        motivos.Add("Data limite para concessão de desconto inválida");
                        break;
                    case "C2":
                        motivos.Add("Aceite do título inválido");
                        break;
                    case "C3":
                        motivos.Add("Campo alterado na instrução “31 – alteração de outros dados” inválido");
                        break;
                    case "C4":
                        motivos.Add("Título ainda não foi confirmado pela centralizadora");
                        break;
                    case "C5":
                        motivos.Add("Título rejeitado pela centralizadora");
                        break;
                    case "C6":
                        motivos.Add("Título já liquidado");
                        break;
                    case "C7":
                        motivos.Add("Título já baixado");
                        break;
                    case "C8":
                        motivos.Add("Existe mesma instrução pendente de confirmação para este título");
                        break;
                    case "C9":
                        motivos.Add("Instrução prévia de concessão de abatimento não existe ou não confirmada");
                        break;
                    case "D1":
                        motivos.Add("Título dentro do prazo de vencimento (em dia)");
                        break;
                    case "D2":
                        motivos.Add("Espécie de documento não permite protesto de título");
                        break;
                    case "D3":
                        motivos.Add("Título possui instrução de baixa pendente de confirmação");
                        break;
                    case "D4":
                        motivos.Add("Quantidade de mensagens padrão excede o limite permitido");
                        break;
                    case "D5":
                        motivos.Add("Quantidade inválida no pedido de boletos pré-impressos da cobrança sem registro");
                        break;
                    case "D6":
                        motivos.Add("Tipo de impressão inválida para cobrança sem registro");
                        break;
                    case "D7":
                        motivos.Add("Cidade ou Estado do pagador não informado");
                        break;
                    case "D8":
                        motivos.Add("Seqüência para composição do nosso número do ano atual esgotada");
                        break;
                    case "D9":
                        motivos.Add("Registro mensagem para título não cadastrado");
                        break;
                    case "E2":
                        motivos.Add("Registro complementar ao cadastro do título da cobrança com e sem registro não cadastrado");
                        break;
                    case "E3":
                        motivos.Add("Tipo de postagem inválido, diferente de S, N e branco");
                        break;
                    case "E4":
                        motivos.Add("Pedido de boletos pré-impressos");
                        break;
                    case "E5":
                        motivos.Add("Confirmação/rejeição para pedidos de boletos não cadastrado");
                        break;
                    case "E6":
                        motivos.Add("Pagador/avalista não cadastrado");
                        break;
                    case "E7":
                        motivos.Add("Informação para atualização do valor do título para protesto inválido");
                        break;
                    case "E8":
                        motivos.Add("Tipo de impressão inválido, diferente de A, B e branco");
                        break;
                    case "E9":
                        motivos.Add("Código do pagador do título divergente com o código da cooperativa de crédito");
                        break;
                    case "F1":
                        motivos.Add("Liquidado no sistema do cliente");
                        break;
                    case "F2":
                        motivos.Add("Baixado no sistema do cliente");
                        break;
                    case "F3":
                        motivos.Add("Instrução inválida, este título está caucionado/descontado");
                        break;
                    case "F4":
                        motivos.Add("Instrução fixa com caracteres inválidos");
                        break;
                    case "F6":
                        motivos.Add("Nosso número / número da parcela fora de seqüência – total de parcelas inválido");
                        break;
                    case "F7":
                        motivos.Add("Falta de comprovante de prestação de serviço");
                        break;
                    case "F8":
                        motivos.Add("Nome do beneficiário incompleto / incorreto.");
                        break;
                    case "F9":
                        motivos.Add("CNPJ / CPF incompatível com o nome do pagador / Sacador Avalista");
                        break;
                    case "G1":
                        motivos.Add("CNPJ / CPF do pagador Incompatível com a espécie");
                        break;
                    case "G2":
                        motivos.Add("Título aceito: sem a assinatura do pagador");
                        break;
                    case "G3":
                        motivos.Add("Título aceito: rasurado ou rasgado");
                        break;
                    case "G4":
                        motivos.Add("Título aceito: falta título (cooperativa/ag. beneficiária deverá enviá-lo)");
                        break;
                    case "G5":
                        motivos.Add("Praça de pagamento incompatível com o endereço");
                        break;
                    case "G6":
                        motivos.Add("Título aceito: sem endosso ou beneficiário irregular");
                        break;
                    case "G7":
                        motivos.Add("Título aceito: valor por extenso diferente do valor numérico");
                        break;
                    case "G8":
                        motivos.Add("Saldo maior que o valor do título");
                        break;
                    case "G9":
                        motivos.Add("Tipo de endosso inválido");
                        break;
                    case "H1":
                        motivos.Add("Nome do pagador incompleto / Incorreto");
                        break;
                    case "H2":
                        motivos.Add("Sustação judicial");
                        break;
                    case "H3":
                        motivos.Add("Pagador não encontrado");
                        break;
                    case "H4":
                        motivos.Add("Alteração de carteira");
                        break;
                    case "H5":
                        motivos.Add("Recebimento de liquidação fora da rede Sicredi – VLB Inferior – Via Compensação");
                        break;
                    case "H6":
                        motivos.Add("Recebimento de liquidação fora da rede Sicredi – VLB Superior – Via Compensação");
                        break;
                    case "H7":
                        motivos.Add("Espécie de documento necessita beneficiário ou avalista PJ");
                        break;
                    case "H8":
                        motivos.Add("Recebimento de liquidação fora da rede Sicredi – Contingência Via Compe");
                        break;
                    case "H9":
                        motivos.Add("Dados do título não conferem com disquete");
                        break;
                    case "I1":
                        motivos.Add("Pagador e Sacador Avalista são a mesma pessoa");
                        break;
                    case "I2":
                        motivos.Add("Aguardar um dia útil após o vencimento para protestar");
                        break;
                    case "I3":
                        motivos.Add("Data do vencimento rasurada");
                        break;
                    case "I4":
                        motivos.Add("Vencimento – extenso não confere com número");
                        break;
                    case "I5":
                        motivos.Add("Falta data de vencimento no título");
                        break;
                    case "I6":
                        motivos.Add("DM/DMI sem comprovante autenticado ou declaração");
                        break;
                    case "I7":
                        motivos.Add("Comprovante ilegível para conferência e microfilmagem");
                        break;
                    case "I8":
                        motivos.Add("Nome solicitado não confere com emitente ou pagador");
                        break;
                    case "I9":
                        motivos.Add("Confirmar se são 2 emitentes. Se sim, indicar os dados dos 2");
                        break;
                    case "J1":
                        motivos.Add("Endereço do pagador igual ao do pagador ou do portador");
                        break;
                    case "J2":
                        motivos.Add("Endereço do apresentante incompleto ou não informado");
                        break;
                    case "J3":
                        motivos.Add("Rua/número inexistente no endereço");
                        break;
                    case "J4":
                        motivos.Add("Falta endosso do favorecido para o apresentante");
                        break;
                    case "J5":
                        motivos.Add("Data da emissão rasurada");
                        break;
                    case "J6":
                        motivos.Add("Falta assinatura do pagador no título");
                        break;
                    case "J7":
                        motivos.Add("Nome do apresentante não informado/incompleto/incorreto");
                        break;
                    case "J8":
                        motivos.Add("Erro de preenchimento do título");
                        break;
                    case "J9":
                        motivos.Add("Título com direito de regresso vencido");
                        break;
                    case "K1":
                        motivos.Add("Título apresentado em duplicidade");
                        break;
                    case "K2":
                        motivos.Add("Título já protestado");
                        break;
                    case "K3":
                        motivos.Add("Letra de câmbio vencida – falta aceite do pagador");
                        break;
                    case "K4":
                        motivos.Add("Falta declaração de saldo assinada no título");
                        break;
                    case "K5":
                        motivos.Add("Contrato de câmbio – Falta conta gráfica");
                        break;
                    case "K6":
                        motivos.Add("Ausência do documento físico");
                        break;
                    case "K7":
                        motivos.Add("Pagador falecido");
                        break;
                    case "K8":
                        motivos.Add("Pagador apresentou quitação do título");
                        break;
                    case "K9":
                        motivos.Add("Título de outra jurisdição territorial");
                        break;
                    case "L1":
                        motivos.Add("Título com emissão anterior a concordata do pagador");
                        break;
                    case "L2":
                        motivos.Add("Pagador consta na lista de falência");
                        break;
                    case "L3":
                        motivos.Add("Apresentante não aceita publicação de edital");
                        break;
                    case "L4":
                        motivos.Add("Dados do Pagador em Branco ou inválido");
                        break;
                    case "L5":
                        motivos.Add("Código do Pagador na agência beneficiária está duplicado");
                        break;
                    case "M1":
                        motivos.Add("Reconhecimento da dívida pelo pagador");
                        break;
                    case "M2":
                        motivos.Add("Não reconhecimento da dívida pelo pagador");
                        break;
                    case "M3":
                        motivos.Add("Inclusão de desconto 2 e desconto 3 inválida");
                        break;
                    case "X1":
                        motivos.Add("Regularização centralizadora – Rede Sicredi");
                        break;
                    case "X2":
                        motivos.Add("Regularização centralizadora – Compensação");
                        break;
                    case "X3":
                        motivos.Add("Regularização centralizadora – Banco correspondente");
                        break;
                    case "X4":
                        motivos.Add("Regularização centralizadora - VLB Inferior - via compensação");
                        break;
                    case "X5":
                        motivos.Add("Regularização centralizadora - VLB Superior - via compensação");
                        break;
                    case "X0":
                        motivos.Add("Pago com cheque");
                        break;
                    case "X6":
                        motivos.Add("Pago com cheque – bloqueado 24 horas");
                        break;
                    case "X7":
                        motivos.Add("Pago com cheque – bloqueado 48 horas");
                        break;
                    case "X8":
                        motivos.Add("Pago com cheque – bloqueado 72 horas");
                        break;
                    case "X9":
                        motivos.Add("Pago com cheque – bloqueado 96 horas");
                        break;
                    case "XA":
                        motivos.Add("Pago com cheque – bloqueado 120 horas");
                        break;
                    case "XB":
                        motivos.Add("Pago com cheque – bloqueado 144 horas");
                        break;
                    default:
                        motivos.Add("Código desconhecido");
                        break;
                }
            }

            return motivos;
        }

        public void LerDetalheRetornoCNAB400Segmento7(ref Boleto boleto, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerHeaderRetornoCNAB240(ArquivoRetorno arquivoRetorno, string registro)
        {
            throw new NotImplementedException();
        }

        public void LerHeaderRetornoCNAB400(string registro)
        {
            try {
                if (registro.Substring(0, 9) != "02RETORNO")
                    throw new Exception("O arquivo não é do tipo \"02RETORNO\"");
            }
            catch (Exception ex) {
                throw new Exception("Erro ao ler HEADER do arquivo de RETORNO / CNAB 400.", ex);
            }
        }

        public void LerTrailerRetornoCNAB400(string registro)
        {

        }

        public void ValidaBoleto(Boleto boleto)
        {

        }

        public string GerarMensagemCliente(TipoArquivo tipoArquivo, Boleto boleto, ref int numeroRegistro)
        {
            try {
                numeroRegistro++;

                string _detalhe = " ";

                switch (tipoArquivo) {
                    case TipoArquivo.CNAB400:
                        boleto.VariacaoCarteira = "A";
                        _detalhe = GerarMensagemRemessaCNAB400(boleto, numeroRegistro);
                        break;
                }

                return _detalhe;
            }
            catch (Exception ex) {
                throw new Exception("Erro durante a geração do DETALHE arquivo de REMESSA.", ex);
            }
        }

        private string GerarMensagemRemessaCNAB400(Boleto boleto, int numeroRegistro)
        {
            TRegistroEDI reg = new TRegistroEDI();
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0001, 001, 0, "2", ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 011, 0, string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0013, 009, 0, boleto.NossoNumero, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0022, 080, 0, boleto.InstrucoesBoleto != null ? boleto.InstrucoesBoleto[0] : string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0102, 080, 0, boleto.InstrucoesBoleto != null ? boleto.InstrucoesBoleto[1] : string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0182, 080, 0, boleto.InstrucoesBoleto != null ? boleto.InstrucoesBoleto[2] : string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0262, 080, 0, boleto.InstrucoesBoleto != null ? boleto.InstrucoesBoleto[3] : string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0342, 010, 0, boleto.SeuNumero, '0'));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0352, 043, 0, string.Empty, ' '));
            reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistro, '0'));

            reg.CodificarLinha();

            string _detalhe = Utils.SubstituiCaracteresEspeciais(reg.LinhaRegistro);

            return _detalhe;
        }

        #region DebitoAutomatico
        public string GerarHeaderDebitoAuto(DebitoAutomaticoCriar gerarHeader)
        {
            return HeaderDebitoAuto(gerarHeader.HeaderDebitoAutoCadastro);
        }

        public List<string> GerarDetalheDebitoAuto(DebitoAutomaticoCriar gerarDetalhe)
        {
            return DetalheDebitoAuto(gerarDetalhe.DetalhesDebitoCadastro);
        }

        public string GerarTraillerDebitoAuto(DebitoAutomaticoCriar gerarTrailler)
        {
            return TraillerDebitoAuto(gerarTrailler.TraillerDebitoAutoCadastro);
        }

        public DebitoAutomaticoConsulta LerDebitoAutoRetorno(string[] linhasArquivoRet)
        {
            DebitoAutomaticoConsulta consultas = new DebitoAutomaticoConsulta();

            try {
                int countLinhasArquivo = linhasArquivoRet.Length;

                if (countLinhasArquivo < 0)
                    throw new ArgumentException("Arquivo em formato inválido ou não possuí tamanho correto");

                string linhaHeader = linhasArquivoRet.First();
                string linhaTrailler = linhasArquivoRet.Last();
                string[] linhasDetalhes = linhasArquivoRet.Skip(1).Take(linhasArquivoRet.Length - 2).ToArray();

                HeaderDebitoConsulta headerDebito = LerHeaderDebitoAuto(linhaHeader);
                List<DetalheDebitoAutoConsulta> detalheDebito = LerDetalhesDebitoAuto(linhasDetalhes);
                TraillerDebitoAuto traillerDebito = LerTraillerDebitoAuto(linhaTrailler);

                consultas.TraillerDebitoAutoConsulta = traillerDebito;
                consultas.HeaderDebitoAutoConsulta = headerDebito;
                consultas.DetalhesDebitoConsulta.AddRange(detalheDebito);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return consultas;
        }

        /// <summary>
        /// Metodo para Leitura de Headers de Arquivo do Retorno
        /// </summary>
        /// <param name="linhaHeader"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public HeaderDebitoConsulta LerHeaderDebitoAuto(string linhaHeader)
        {
            HeaderDebitoConsulta header = new HeaderDebitoConsulta();

            try {
                if (linhaHeader.Length < 150)
                    throw new Exception("Header do arquivo esta com formato invalido, menor que 150 caracteres");

                if (linhaHeader.Substring(0, 1) != "A")
                    throw new Exception("Registro informado não compatível com o Header 'A' esperado de Débito Automático");

                header.TipoRegistro = linhaHeader.Substring(0, 1);
                header.CodigoRemessa = linhaHeader.Substring(1, 1);
                header.CodigoConvenio = linhaHeader.Substring(2, 20);
                header.NomeEmpresa = linhaHeader.Substring(22, 20);
                header.CodigoBanco = Convert.ToInt64(linhaHeader.Substring(42, 3));
                header.NomeBanco = linhaHeader.Substring(45, 20);
                header.DataGeracao = DateTime.ParseExact(linhaHeader.Substring(65, 8), "yyyyMMdd", null).ToString("dd/MM/yyyy");
                header.NumeroSequencialArquivo = Convert.ToInt64(linhaHeader.Substring(73, 6));
                header.VersaoLayout = Convert.ToInt64(linhaHeader.Substring(79, 2));
                header.IdentificacaoServico = linhaHeader.Substring(81, 17);

            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return header;
        }

        /// <summary>
        /// Metodo para Leitura de Detalhes do Arquivo de Retorno
        /// </summary>
        /// <param name="linhasDetalhes"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<DetalheDebitoAutoConsulta> LerDetalhesDebitoAuto(string[] linhasDetalhes)
        {
            List<DetalheDebitoAutoConsulta> detalhesConsulta = new List<DetalheDebitoAutoConsulta>();

            try {
                foreach (var linha in linhasDetalhes) {
                    if (linha.Length < 150)
                        throw new Exception("Detalhes do arquivo esta com formato invalido, menor que 150 caracteres");

                    if (linha.Substring(0, 1) != "F")
                        throw new Exception("Registro informado não compatível com o Retorno de Débito Automático");

                    DetalheDebitoAutoConsulta detalhe = new DetalheDebitoAutoConsulta
                    {
                        CodigoRegistro = linha.Substring(0, 1),
                        IdentificacaoCliente = linha.Substring(1, 25),
                        AgenciaDebito = linha.Substring(26, 4),
                        ContaCliente = linha.Substring(30, 14),
                        DataVencimento = DateTime.ParseExact(linha.Substring(44, 8), "yyyyMMdd", null).ToString("dd/MM/yyyy"),
                        ValorOriginalDebito = Convert.ToDecimal(linha.Substring(52, 15)),
                        CodigoRetorno = CodigoRetorno(linha.Substring(67, 2)),
                        InformacoesComplementares = linha.Substring(69, 60),
                        CodigoMovimento = linha.Substring(149, 1)
                    };

                    detalhesConsulta.Add(detalhe);
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

            return detalhesConsulta;
        }

        /// <summary>
        /// Metodo para Leitura de Trailler do Arquivo de Retorno
        /// </summary>
        /// <param name="linhaTrailler"></param>
        /// <returns></returns>
        public TraillerDebitoAuto LerTraillerDebitoAuto(string linhaTrailler)
        {
            TraillerDebitoAuto trailler = new TraillerDebitoAuto();

            if (linhaTrailler.Length < 150)
                throw new Exception("Trailer do arquivo está com formato invalido, menor que 150 caracteres");

            if (linhaTrailler.Substring(0, 1) != "Z")
                throw new Exception("Registro informado não compatível com o Trailler esperado de Débito Automático");

            trailler.CodigoRegistro = linhaTrailler.Substring(0, 1);
            trailler.TotalRegistro = Convert.ToInt64(linhaTrailler.Substring(1, 6));
            trailler.TotalValoresRegistro = Convert.ToDecimal(linhaTrailler.Substring(7, 17));

            return trailler;
        }

        /// <summary>
        /// Cria o header do Arquivo de 
        /// Remessa
        /// </summary>
        /// <returns></returns>
        public string HeaderDebitoAuto(HeaderDebitoAutoCriar gerarHeader)
        {
            var reg = new TRegistroEDI();

            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 001, 001, 0, "A", ' '); //fixo para header
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 002, 001, 0, "1", ' '); //fixo para definir remessa no header
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 003, 020, 0, "X68", ' '); //codigo convenio
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 023, 020, 0, gerarHeader.NomeEmpresa, ' '); //nome da empresa
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 043, 003, 0, gerarHeader.CodigoBanco, ' '); //codigo do banco
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 046, 020, 0, gerarHeader.NomeBanco, ' '); //descricao do banco
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 066, 008, 0, gerarHeader.DataGeracao.ToString("yyyyMMdd"), '0'); //data gerado YYYYMMDD
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 074, 006, 0, gerarHeader.NumeroSequencialArquivo, '0'); //codigo NSA gerado de 1 em 1
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 080, 002, 0, "04", '0'); //modelo versao layout
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 082, 017, 0, "DEBITO AUTOMATICO", ' '); //descricao e idenficicacao serv
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 099, 052, 0, string.Empty, ' '); //espaco em branco
            reg.CodificarLinha();

            return reg.LinhaRegistro;
        }

        /// <summary>
        /// Cria o corpo "Detalhe" do Arquivo de 
        /// Remessa
        /// </summary>
        /// <returns></returns>
        public List<string> DetalheDebitoAuto(List<DetalheDebitoAutoCadastro> gerarDetalhes)
        {
            List<string> linhasDetalhes = new List<string>();

            foreach (var detalhe in gerarDetalhes) {
                var reg = new TRegistroEDI();

                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 001, 001, 0, "E", ' '); //fixo para detalhe de remssa Deb Auto
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 002, 025, 0, detalhe.IdentificacaoCliente, ' '); //-Identificação do Cliente na Empresa 11 caracteres
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 027, 004, 0, detalhe.AgenciaDebito, ' '); // informacao da agencia do banco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 031, 014, 0, detalhe.ContaCliente + detalhe.DigitoConta, ' '); // informacao da conta/corrente/pag/salario/poup
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 045, 008, 0, detalhe.DataVencimento.ToString("yyyyMMdd"), ' '); // datavencimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 053, 015, 0, detalhe.ValorOriginalDebito.ToString("0.00"), '0'); // valordebito
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 068, 002, 0, "03", '0'); // moeda
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 070, 049, 0, detalhe.InformacoesComplementares, ' '); // informacao da insti destinataria (empresa)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 119, 010, 0, string.Empty, ' '); // informacao da insti destinataria (empresa)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 129, 001, 0, string.Empty, ' '); // informacao da insti destinataria (empresa)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 130, 020, 0, string.Empty, ' '); // informacao da insti destinataria (empresa)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 150, 001, 0, "0", ' '); //codigoMovimento
                reg.CodificarLinha();

                linhasDetalhes.Add(reg.LinhaRegistro);
            }

            return linhasDetalhes;
        }

        /// <summary>
        /// Cria o Trailler do Arquivo de 
        /// Remessa
        /// </summary>
        /// <returns></returns>
        public string TraillerDebitoAuto(TraillerDebitoAuto gerarTrailler)
        {
            var reg = new TRegistroEDI();
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 001, 001, 0, "Z", ' '); //fixo para header
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 002, 006, 0, gerarTrailler.TotalRegistro, '0'); //quantidade de registros contando header e trailler
            reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 008, 017, 0, gerarTrailler.TotalValoresRegistro.ToString("0.00"), '0'); //valores somados dos registros
            reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 025, 126, 0, string.Empty, ' '); //fixo para header
            reg.CodificarLinha();

            return reg.LinhaRegistro;
        }

        /// <summary>
        /// Faz a leitura e conversao, dado um 
        /// codigo
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public Dictionary<string, string> CodigoRetorno(string codigo)
        {
            Dictionary<string, string> response = new Dictionary<string, string>();

            switch (codigo) {
                case "00":
                    response.Add("00", "Débito efetuado");
                    break;
                case "01":
                    response.Add("01", "Débito não efetuado - Insuficiência de fundos");
                    break;
                case "02":
                    response.Add("02", "Débito não efetuado - Conta corrente não cadastrada");
                    break;
                case "04":
                    response.Add("04", "Débito não efetuado - Outras restrições");
                    break;
                case "05":
                    response.Add("05", "Débito não efetuado – valor do débito excede valor limite aprovado.");
                    break;
                case "10":
                    response.Add("10", "Débito não efetuado - Agência em regime de encerramento");
                    break;
                case "12":
                    response.Add("12", "Débito não efetuado - Valor inválido");
                    break;
                case "13":
                    response.Add("13", "Débito não efetuado - Data de lançamento inválida");
                    break;
                case "14":
                    response.Add("14", "Débito não efetuado - Agência inválida");
                    break;
                case "15":
                    response.Add("15", "Débito não efetuado - Conta corrente inválida");
                    break;
                case "18":
                    response.Add("18", "Débito não efetuado - Data do débito anterior à do processamento");
                    break;
                case "19":
                    response.Add("19", "Débito não efetuado – Agência/Conta não pertence ao CPF/CNPJ informado");
                    break;
                case "20":
                    response.Add("20", "Débito não efetuado – Conta corrente conjunta não solidária");
                    break;
                case "30":
                    response.Add("30", "Débito não efetuado - Sem contrato de débito automático");
                    break;
                case "31":
                    response.Add("31", "Débito efetuado em data diferente da data informada – feriado na praça de débito");
                    break;
                case "96":
                    response.Add("96", "Manutenção do Cadastro");
                    break;
                case "97":
                    response.Add("97", "Cancelamento - Não encontrado");
                    break;
                case "98":
                    response.Add("98", "Cancelamento - Não efetuado, fora do tempo hábil");
                    break;
                case "99":
                    response.Add("99", "Cancelamento - Cancelado conforme solicitação");
                    break;
                default:
                    response.Add("", "Código desconhecido");
                    break;
            }

            return response;
        }
        #endregion
    }
}