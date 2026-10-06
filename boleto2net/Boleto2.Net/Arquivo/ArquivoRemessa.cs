using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Boleto2Net
{
    public class ArquivoRemessa // : AbstractArquivoRemessa, IArquivoRemessa
    {
        public IBanco Banco { get; set; }
        public TipoArquivo TipoArquivo { get; set; }
        public int NumeroArquivoRemessa { get; set; }
        public int RemessaDia { get; set; }

        public string NomeArquivo => this.Banco?.FormatarNomeArquivoRemessa(this.NumeroArquivoRemessa, this.RemessaDia); //

        public ArquivoRemessa(IBanco banco, TipoArquivo tipoArquivo, int numeroArquivoRemessa)
        {
            Banco = banco;
            TipoArquivo = tipoArquivo;
            NumeroArquivoRemessa = numeroArquivoRemessa;
        }

        /// <summary>
        ///  Retorna no primeiro parametro a lista de erros, no segundo com byte do arquivo gerado
        /// </summary>
        /// <param name="boletos"></param>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Dictionary<string, string> GerarArquivoRemessa(Boletos boletos, Stream arquivo, bool instrucaoCancelameto = false)
        {
            Dictionary<string, string> errosGerados = new Dictionary<string, string>();
            try
            {
                int numeroRegistroGeral = 0,
                    numeroRegistroCobrancaSimples = 0,
                    numeroRegistroCobrancaVinculada = 0,
                    numeroRegistroCobrancaCaucionada = 0,
                    numeroRegistroCobrancaDescontada = 0;
                decimal valorBoletoGeral = 0,
                    valorCobrancaSimples = 0,
                    valorCobrancaVinculada = 0,
                    valorCobrancaCaucionada = 0,
                    valorCobrancaDescontada = 0;

                int tamanhoRegistro;
                if (this.TipoArquivo == TipoArquivo.CNAB240)
                    tamanhoRegistro = 240;
                else
                    tamanhoRegistro = 400;

                StreamWriter arquivoRemessa = new StreamWriter(arquivo, Encoding.GetEncoding("ISO-8859-1"));
                string strline = String.Empty;

                // Header do Arquivo
                strline = Banco.GerarHeaderRemessa(this.TipoArquivo, this.NumeroArquivoRemessa, ref numeroRegistroGeral);
                if (String.IsNullOrWhiteSpace(strline))
                    throw new Exception("Registro HEADER obrigat�rio.");
                strline = FormataLinhaArquivoCNAB(strline, tamanhoRegistro);
                arquivoRemessa.WriteLine(strline);

                foreach (Boleto boleto in boletos)
                {
                    // Todos os boletos da cole��o devem ser do mesmo banco da gera��o do arquivo remessa
                    // A solu��o aqui � for�ar essa rela��o, mas talvez seja melhor subir uma exce��o detalhando o erro.                 
                    try {
                        boleto.Banco = this.Banco;

                        // Detalhe do arquivo
                        strline = boleto.Banco.GerarDetalheRemessa(this.TipoArquivo, boleto, ref numeroRegistroGeral, instrucaoCancelameto);

                        if (String.IsNullOrWhiteSpace(strline))
                            throw new Exception("Registro DETALHE obrigatório.");
                        //strline = FormataLinhaArquivoCNAB(strline, tamanhoRegistro);
                        arquivoRemessa.WriteLine(strline);

                        ///instrucoesBoleto
                        strline = boleto.Banco.GerarMensagemCliente(this.TipoArquivo, boleto, ref numeroRegistroGeral);
                        if (String.IsNullOrWhiteSpace(strline))
                            throw new Exception("Registro INSTRUCOES é obrigatório.");

                        arquivoRemessa.WriteLine(strline);

                        // Ajusta Totalizadores
                        valorBoletoGeral += boleto.ValorTitulo;
                        switch (boleto.TipoCarteira) {
                            case TipoCarteira.CarteiraCobrancaSimples:
                                numeroRegistroCobrancaSimples++;
                                valorCobrancaSimples += boleto.ValorTitulo;
                                break;
                            case TipoCarteira.CarteiraCobrancaVinculada:
                                numeroRegistroCobrancaVinculada++;
                                valorCobrancaVinculada += boleto.ValorTitulo;
                                break;
                            case TipoCarteira.CarteiraCobrancaCaucionada:
                                numeroRegistroCobrancaCaucionada++;
                                valorCobrancaCaucionada += boleto.ValorTitulo;
                                break;
                            case TipoCarteira.CarteiraCobrancaDescontada:
                                numeroRegistroCobrancaDescontada++;
                                valorCobrancaDescontada += boleto.ValorTitulo;
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex) {
                        errosGerados.Add(boleto.NossoNumero, "Erro ao gerar arquivo de remessa: "+ ex.Message);
                        numeroRegistroGeral--;
                        continue;
                    }
                }

                // Trailler do Arquivo
                strline = Banco.GerarTrailerRemessa(this.TipoArquivo, this.NumeroArquivoRemessa,
                                                            ref numeroRegistroGeral, valorBoletoGeral,
                                                            numeroRegistroCobrancaSimples, valorCobrancaSimples,
                                                            numeroRegistroCobrancaVinculada, valorCobrancaVinculada,
                                                            numeroRegistroCobrancaCaucionada, valorCobrancaCaucionada,
                                                            numeroRegistroCobrancaDescontada, valorCobrancaDescontada);
                if (!String.IsNullOrWhiteSpace(strline))
                {
                    strline = FormataLinhaArquivoCNAB(strline, tamanhoRegistro);
                    arquivoRemessa.WriteLine(strline);
                }

                arquivoRemessa.Close();
                arquivoRemessa.Dispose();
                arquivoRemessa = null;

                return errosGerados;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar arquivo remessa.", ex);
            }
        }

        private string FormataLinhaArquivoCNAB(string strLinha, int tamanhoRegistro)
        {
            strLinha = strLinha.ToUpper();
            if (Banco.RemoveAcentosArquivoRemessa)
            {
                strLinha = Utils.SubstituiCaracteresEspeciais(strLinha);
            }
            if (tamanhoRegistro != 0)
            {
                string[] strLinhas = strLinha.Split('\n');
                foreach (string s in strLinhas)
                {
                   var tamanhocaracteres = s.Replace("\r", "").Length;

                    if (tamanhocaracteres != tamanhoRegistro)
                        throw new Exception("Registro com tamanho incorreto:" + tamanhocaracteres.ToString() + " - " + s);
                }
            }
            return strLinha.ToUpper();
        }

    }
}
