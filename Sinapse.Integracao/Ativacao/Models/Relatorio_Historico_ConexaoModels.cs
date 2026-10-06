using Aniel.Connect.Core;
using Sinapse.Integracao.Ativacao.Interfaces;
using Sinapse.Model.enumerados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.Ativacao.Models
{
    public class RelatorioConexao_DataTableRequest : DataTableRequest
    {
        public long? IdCliente { get; set; }
        public long? IdPlano { get; set; }
        public long? IdOLT { get; set; }
        public long? IdPon { get; set; }
        public long? IdSlot { get; set; }
        public string Estado { get; set; }
        public string Cidade { get; set; }
        public string Bairro { get; set; }
        public long? IdConcentradora { get; set; }
        public long? IdPoolIp { get; set; }
        public long? IdCaixaFtth { get; set; }        
        public Int32? Status { get; set; }
        public Int32? StatusOnu { get; set; }
        public int? StatusAutenticacaoRadius { get; set; }
    }

    public class RelatorioHistoricoConexao_DataTableRequest : DataTableRequest
    {
        public virtual Int64 Id_Login_Radius { get; set; }
    }

    public class RelatorioConexaoModel
    {
        public virtual Int64 Id { get; set; }
        public virtual String Desc_Cliente { get; set; }
        public virtual String Bloqueado { get; set; }
        public virtual String Mac { get; set; }
        public virtual String Ponto_Acesso { get; set; }
        public virtual String Endereco_Ip { get; set; }
        public virtual String Login_Radius { get; set; }
        public virtual long Id_Login_Radius { get; set; }
        public virtual string Data_Inicial { get; set; }
        public virtual string Data_Fim { get; set; }
        public virtual string StatusOnu { get; set; }
        public virtual string Desc_POP { get; set; }
        public virtual string Desc_Pool_Ip { get; set; }
        public virtual string Desc_OLT { get; set; }
        public virtual string Desc_Plano { get; set; }
        public virtual long? Id_Porta_FTTH { get; set; }
        public virtual string Num_Porta_FTHH { get; set; }
        public virtual string Desc_Caixa_FTTH { get; set; }
        public virtual string Ultima_Autenticacao { get; set; }
        public virtual string Data_Ini_Ultimo_Consumo { get; set; }
        public virtual string Data_Fim_Ultimo_Consumo { get; set; }
        public virtual long? IdClientePlanoProduto { get; set; }

    }

    public class RelatorioHistoricoConexaoModel
    {
        //public virtual Int64 Id { get; set; } Verificar se mantém ou não
        public virtual String Endereco_Ipv4 { get; set; }
        public virtual String Endereco_Ipv6 { get; set; }
        public virtual String Mac { get; set; }
        public virtual String Hora_Login { get; set; }
        public virtual String Hora_Logout { get; set; }
        public virtual String Tempo_Conectado { get; set; }
        public String Upload { get; set; }
        public String Download { get; set; }
        public String Motivo_Desconexao { get; set; }
    }

    public class ConexaoTempoRealModel
    {
        public virtual String Upload_Atual_Str { get; set; }
        public virtual double Upload_Atual { get; set; }
        public virtual String Download_Atual_Str { get; set; }
        public virtual double Download_Atual { get; set; }
        public virtual String Download_Maximo_Str { get; set; }
        public virtual double Download_Maximo { get; set; }
        public virtual String Upload_Maximo_Str { get; set; }
        public virtual double Upload_Maximo { get; set; }
        public virtual String Download_Total_Str { get; set; }
        public virtual double Download_Total { get; set; }
        public virtual String Upload_Total_Str { get; set; }
        public virtual double Upload_Total { get; set; }
        public virtual String DataHora_Inicio { get; set; }
        public virtual String DataHora_Fim { get; set; }
        public virtual ConsumoPeriodoModel Consumo_Periodo { get; set; }

    }

    public class ConexaoTempoReal_Request
    {
        public virtual Medida_Velocidade_ConexaoEnum Unidade_Medida_Velocidade { get; set; }
        public virtual Int64 Id_Login_Radius { get; set; }
        public virtual string Desc_Login_Radius { get; set; }
        public virtual string Desc_Cliente { get; set; }
        public virtual DateTime? Data_Inicial { get; set; }
        public virtual string Data_Inicial_String { get; set; }
        public virtual string Ip_Login_Radius { get; set; }
        public virtual string Nas { get; set; }
        public virtual Double LastTotalDownloadOctects { get; set; }
        public virtual Double LastTotalUploadOctects { get; set; }        
    }

    public class GraficoConsumoPeriodo_Request
    {
        public virtual HistoricoConsumoPeriodoEnum Periodo_Consumo { get; set; }
        public virtual Int64 Id_Login_Radius { get; set; }
        public virtual string Desc_Login_Radius { get; set; }
        public virtual DateTime? Data_Inicial { get; set; }
        public virtual DateTime? Data_Fim { get; set; }
        public virtual string Data_Inicial_String { get; set; }
        public virtual string Data_Fim_String { get; set; }
    }

    public class ConsumoPeriodoModel
    {
        public virtual String DownloadString { get; set; }
        public virtual String UploadString { get; set; }
        public virtual Double Download { get; set; }
        public virtual Double Upload { get; set; }
        public virtual String DataHoraConsumoFormatada { get; set; }
        public virtual DateTime DataHoraConsumo { get; set; }
        public virtual Double LastTotalDownloadOctects { get; set; }
        public virtual Double LastTotalUploadOctects { get; set; }
        public virtual bool AtualizarConsumoTotal { get; set; }
    }

    public class GraficoConsumoPeriodoModel
    {
        public virtual String Download_Atual { get; set; }
        public virtual String Download_Medio { get; set; }
        public virtual String Download_Maximo { get; set; }
        public virtual String Upload_Atual { get; set; }
        public virtual String Upload_Medio { get; set; }
        public virtual String Upload_Maximo { get; set; }
        public virtual List<ConsumoPeriodoModel> ListaConsumoPeriodo { get; set; }
        public virtual String DataHora_Inicio { get; set; }
        public virtual String DataHora_Fim { get; set; }
        public virtual HistoricoConsumoPeriodoEnum Periodo_Consumo { get; set; }
    }

    public class TempoOnlineOfflineModel
    {
        public virtual string Data_Conexao_String { get; set; }
        public virtual DateTime? Data_Conexao { get; set; }
        public virtual int Horas_Online_Offline { get; set; }
    }

    public class GraficoTempoOnlineOfflineModel
    {
        public virtual List<TempoOnlineOfflineModel> ListaTempoOnlineOffline { get; set; }
        public virtual String DataHora_Inicio { get; set; }
        public virtual String DataHora_Fim { get; set; }
        public virtual HistoricoConsumoPeriodoEnum Periodo_Consumo { get; set; }
    }

    public class GraficoTempoOnlineOffline_Request
    {
        public virtual Int64 Id_Login_Radius { get; set; }
        public virtual string Desc_Login_Radius { get; set; }
        public virtual string Desc_Cliente { get; set; }
        public virtual string Ip_Login_Radius { get; set; }
        public virtual DateTime? Data_Inicial { get; set; }
        public virtual DateTime? Data_Fim { get; set; }
        public virtual string Data_Inicial_String { get; set; }
        public virtual string Data_Fim_String { get; set; }
        public virtual HistoricoConsumoPeriodoEnum Periodo_Consumo { get; set; }
    }

    public class GraficoPing_Request
    {
        public virtual Int64 Id_Login_Radius { get; set; }
        public virtual bool pingConcentradora { get; set; }
        public virtual string Desc_Login_Radius { get; set; }
        public virtual string Desc_Cliente { get; set; }
        public virtual string Ip_Login_Radius { get; set; }
        public virtual string Ip_Concentradora { get; set; }
    }

    public class GraficoPingModel
    {
        public virtual Double latencia { get; set; }
        public virtual string DataHoraRequisicao { get; set; }
    }

    public class AcessoEquipamentoClienteModel
    {
        public virtual Int64 Id_Login_Radius { get; set; }
        public virtual string Desc_Login_Radius { get; set; }
        public virtual string Desc_Cliente { get; set; }
        public virtual string Ultimo_Ip { get; set; }
        public virtual string Endereco_Mac { get; set; }
        public virtual string Senha_Equipamento { get; set; }
        public virtual string Login_Equipamento { get; set; }
        public virtual string Porta_Http { get; set; }
        public virtual string Framed_Ipv6 { get; set; }
        public virtual string Delegated_Ipv6 { get; set; }


    }

    public class GraficoPotenciaRxTxModel
    {
        public virtual Int64 Id_Login_Radius { get; set; }
        public virtual string Desc_Onu { get; set; }
        public virtual string MAC { get; set; }
        public virtual DateTime? Data_Inicial { get; set; }
        public virtual DateTime? Data_Fim { get; set; }
        public virtual string Data_Inicial_String { get; set; }
        public virtual string Data_Fim_String { get; set; }
        public virtual HistoricoConsumoPeriodoEnum Periodo_Consumo { get; set; }
    }

    public class PotenciaRxTxModel_Response
    {
        public virtual long Id_Log_Potencia_Rx_Tx { get; set; } //Ver se deve manter
        public virtual string DataHoraConsulta { get; set; }
        public virtual double Potencia_Rx { get; set; }
        public virtual double Potencia_Tx { get; set; }
    }

    public class GraficoPotenciaRxTxModel_Response
    {
        public virtual List<PotenciaRxTxModel_Response> ListaPotenciaRxTx { get; set; }
        public virtual String DataHora_Inicio { get; set; }
        public virtual String DataHora_Fim { get; set; }
        public virtual HistoricoConsumoPeriodoEnum Periodo_Consumo { get; set; }
    }

    public class RelatorioPainelHistoricoAutenticacao
    {
        public virtual Int64 Id { get; set; }
        public virtual String Endereco_Ip { get; set; }
        public virtual String Login_Radius { get; set; }
        public virtual String Senha { get; set; }
        public virtual String Desc_Concentradora { get; set; }
        public virtual string Desc_Pool_Ip { get; set; }
        public virtual string Data_Autenticacao { get; set; }
        public virtual string Status_Autenticacao { get; set; }


    }

    public class RelatorioPainelHistoricoAutenticacao_DataTableRequest : DataTableRequest
    {
        public String Endereco_Ip { get; set; }
        public String Login_Radius { get; set; }
        public String Senha { get; set; }
        public String Desc_Concentradora { get; set; }
        public string Desc_Pool_Ip { get; set; }
        public string Data_Autenticacao { get; set; }
        public virtual string Status_Autenticacao { get; set; }
    }
}