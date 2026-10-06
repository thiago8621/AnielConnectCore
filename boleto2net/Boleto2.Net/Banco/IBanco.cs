using Boleto2Net.DebitoAutomatico;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boleto2Net
{
    public interface IBanco
    {
        Cedente Cedente { get; set; }
        int Codigo { get; }
        string Nome { get; }
        string Digito { get; }
        List<string> IdsRetornoCnab400RegistroDetalhe { get; } // Identifica os registros que cada banco implementa no arquivo retorno, sendo que o primeiro ID da List<> identifica um novo boleto dentro do arquivo retorno.
        bool RemoveAcentosArquivoRemessa { get; }

        /// <summary>
        /// Formata o cedente (Ag�ncia, Conta, C�digo)
        /// </summary>
        void FormataCedente();
        /// <summary>
        /// Formata o campo livre do c�digo de barras
        /// </summary>
        string FormataCodigoBarraCampoLivre(Boleto boleto);
        /// <summary>
        /// Formata o nosso n�mero
        /// </summary>
        void FormataNossoNumero(Boleto boleto);
        /// <summary>
        /// Respons�vel pela valida��o de todos os dados referente ao banco, que ser�o usados no boleto
        /// </summary>
        void ValidaBoleto(Boleto boleto);

        /// <summary>
        /// Gera o header do arquivo de remessa
        /// </summary>
        string GerarHeaderRemessa(TipoArquivo tipoArquivo, int numeroArquivoRemessa, ref int numeroRegistro);
        /// <summary>
        /// Gera o Trailer do arquivo de remessa
        /// </summary>
        string GerarDetalheRemessa(TipoArquivo tipoArquivo, Boleto boleto, ref int numeroRegistro, bool instrucaoCancelameto = false);
        /// <summary>
        /// Gera o Trailer do arquivo de remessa
        /// </summary>
        string GerarTrailerRemessa(TipoArquivo tipoArquivo, int numeroArquivoRemessa,
                                            ref int numeroRegistroGeral, decimal valorBoletoGeral,
                                            int numeroRegistroCobrancaSimples, decimal valorCobrancaSimples,
                                            int numeroRegistroCobrancaVinculada, decimal valorCobrancaVinculada,
                                            int numeroRegistroCobrancaCaucionada, decimal valorCobrancaCaucionada,
                                            int numeroRegistroCobrancaDescontada, decimal valorCobrancaDescontada);

        void LerHeaderRetornoCNAB240(ArquivoRetorno arquivoRetorno, string registro);
        void LerDetalheRetornoCNAB240SegmentoT(ref Boleto boleto, string registro);
        void LerDetalheRetornoCNAB240SegmentoU(ref Boleto boleto, string registro);
        void LerHeaderRetornoCNAB400(string registro);
        void LerDetalheRetornoCNAB400Segmento1(ref Boleto boleto, string registro);
        void LerDetalheRetornoCNAB400Segmento7(ref Boleto boleto, string registro);
        void LerTrailerRetornoCNAB400(string registro);

        string FormatarNomeArquivoRemessa(int numeroSequencial, int remessaDia);

        string GerarMensagemCliente(TipoArquivo tipoArquivo, Boleto boleto, ref int numeroRegistro);

        #region DebitoAutomatico
        string GerarHeaderDebitoAuto(DebitoAutomaticoCriar gerarHeader);

        List<string> GerarDetalheDebitoAuto(DebitoAutomaticoCriar gerarDetalhe);
        string GerarTraillerDebitoAuto(DebitoAutomaticoCriar gerarTrailler);

        DebitoAutomaticoConsulta LerDebitoAutoRetorno(string[] linhasArquivoRet);
        HeaderDebitoConsulta LerHeaderDebitoAuto(string linhaHeader);

        List<DetalheDebitoAutoConsulta> LerDetalhesDebitoAuto(string[] linhasDetalhes);

        TraillerDebitoAuto LerTraillerDebitoAuto(string linhaTrailler);

        Dictionary<string, string> CodigoRetorno(string codigo);
        #endregion
    }
}
