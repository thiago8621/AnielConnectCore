using Sinapse.Model.enumerados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.Ativacao.Models
{
    public class ConfigurarONUModel
    {
        public Int64 Id { get; set; }
        public Marca_Modelo_OLT? MarcaModelo { get; set; }
        public ONU_Tipo_Identificacao? Tipo_Identificacao { get; set; }
        public String Usuario { get; set; }
        public String Descricao { get; set; }

        public String Serial { get; set; }
        public String Mac { get; set; }
        public String SerialConvertido { get; set; }

        public string IpOLT { get; set; }
        public Int64? IdOLT { get; set; }
        public Int64? IdSlot { get; set; }
        public Int64? IdPon { get; set; }
        public string PonIDFiberhome { get; set; }
        public string DescricaoPON { get; set; }
        public int? OntID { get; set; }
        public Int64? IdCliente { get; set; }
        public Int64? IdPlano { get; set; }
        public Int64? IdLogin { get; set; }       
        public Int64? IdCaixaFTTH { get; set; }
        public Int64? IdPortaFTTH { get; set; }

        public Int32? GemPort { get; set; }
        public Int32? ServicePort { get; set; }
        public Int32? SenorId { get; set; }

        public Int64? IdPerfilAtivacao { get; set; }
        public string DescricaoPerfilAtivacao { get; set; }
        public string PerfilAtivacao_IpGerencia { get; set; }
        public string PerfilAtivacao_Login { get; set; }
        public string PerfilAtivacao_Senha { get; set; }
        public int? PerfilAtivacao_PortaTelnet { get; set; }


        public string Gabinete { get; set; }
        public String Subrack { get; set; }
        public String IpGerencia { get; set; }
        public String Login { get; set; }
        public String Senha { get; set; }
        public String SenhaPPPOE { get; set; }
        public String UsuarioRoteador { get; set; }
        public String SenhaRoteador { get; set; }
        public String SSID_WIFI { get; set; }
        public Int32? PortaTelnet { get; set; }
        public Int32? PortaHttp { get; set; }

        public Int16? VLAN_Uplink { get; set; }
        public Int16? VLAN_PPPOE { get; set; }
        public Int16? VLAN_DHCP { get; set; }
        public Int16? VLAN_IPTV { get; set; }
        public Int16? VLAN_VOIP { get; set; }
        public Int16? VLAN_TR69 { get; set; }
        public Int16? VLAN_Outros { get; set; }
        public string VLAN_Profile { get; set; }

        public string ClienteCpfCnpj { get; set; }
        public string VelocidadeDownload { get; set; }
        public string VelocidadeUpload { get; set; }
        public string DescricaoPlano { get; set; }          

        public string LineProfile { get; set; }

        public String ScriptOLT { get; set; }
        public String ScriptONU { get; set; }
        public Sinapse.Model.enumerados.StatusONU? StatusOnu { get; set; }
        public string UltimaAtualizacaoStatus { get; set; }

    }

    public class DiagnosticoONU
    {
        public string Rx { get; set; }
        public string Tx { get; set; }
        public string Temperatura { get; set; }
        public string Voltagem { get; set; }
        public string Bias_Current { get; set; }
        public string Type { get; set; }
        public string Rx_OLT { get; set; }
        public string Tx_OLT { get; set; }
        public string Uptime { get; set; }
        public string MotivoErro { get; set; }
    }

    public class AutorizarONUListModel 
    {
        public string Id { get; set; }
        public string Desc_OLT { get; set; }
        public string Desc_Slot { get; set; }
        public string Desc_PON { get; set; }
        public string Num_OLT { get; set; }
        public long Id_OLT { get; set; }
        public string Slot { get; set; }
        public string PON { get; set; }
        public string Mac_Serial { get; set; }
        public string Mac_Serial_Convertido { get; set; }
        public string Posicao_Estoque { get; set; }
        public string Informacao { get; set; }
        public string Desc_ONU { get; set; }
        public bool Pode_Autorizar_Sem_OS { get; set; }
        public long Id_Cliente_Plano_Produto { get; set; }
        public long? Id_Perfil_Ativacao { get; set; }
        /// <summary>
        /// Define se a requisição de provisionamento foi originada do Touch.
        /// </summary>
        public bool Origem_Mobile { get; set; }
        public int? NumPorta { get; set; }
        public long IdCaixa { get; set; }
        public string Wireless_SSID { get; set; }
        public string Senha_WPA2 { get; set; }
        public string Detalhe { get; set; }
        public string Usuario { get; set; }
        public string CodCt { get; set; } 
        public string Projeto { get; set; }
        public string NumObra { get; set; }
        public string CodMat { get; set; }
        public string CodCpl { get; set; }
        public string Num_Os { get; set; }
        public string PPPOE {  get; set; }
        public long? Id_Login_Radius { get; set; }
        public string NumCpfCnpj { get; set; }
        public Int64? IdCliente { get; set; }
        /// <summary>
        /// Caso já exista uma ONU com o mesmo serial na base, força a remoção no banco e provisiona novamente.
        /// </summary>
        public bool Forcar_Remocao_ONU_Existente { get; set; }
    }

    public class StatusONU
    {
        public string Serial { get; set; }
        public string ONTID { get; set; }
        public Sinapse.Model.enumerados.StatusONU Status { get; set; }
        public string StatusStr { get; set; }
        public DateTime? DataHoraUltimaAtualizacao { get; set; }
        public string DataHoraUltimaAtualizacaoStr { get; set; }
    }

    public class InfoHardwareONU
    {
        public virtual long Id_Onu { get; set; }
        public string Nome { get; set; }
        public string IP { get; set; }
        public string Modelo { get; set; }
        public string Versao_Software { get; set; }
        public string Memoria { get; set; }
        public string CPU { get; set; }
        public string Temperatura { get; set; }
    }

    public class InfoConfigONU
    {
        public virtual long Id_Onu { get; set; }
        public virtual string PPPOE_Proxy { get; set; }
        public virtual string PPPOE_Usuario { get; set; }
        public virtual string PPPOEMODE { get; set; }
        public virtual string PPPOE_ServicoName { get; set; }

        public virtual string IP_Estatico { get; set; }
        public virtual string IP_Mascara { get; set; }
        public virtual string IP_Gateway { get; set; }
        public virtual string DNS_Master { get; set; }
        public virtual string DNS_Slave { get; set; }
        public virtual string Porta_ONU { get; set; }


        public virtual string QoS { get; set; }
        public virtual string Nome_Servico { get; set; }
        public virtual string Modo_Conexao { get; set; }
        public virtual string Tipo_Conexao { get; set; }
        public virtual string VLAN_ID { get; set; }
        public virtual string VLAN_COS { get; set; }
        public virtual string NAT { get; set; }
        public virtual string Tipo_IP { get; set; }
    }

    public class OnuExistenteModel
    {
        public bool MesmoSerial { get; set; }
        public bool MesmoLogin { get; set; }
    }

    public class GetDadosONUOLT
    {
        public string Serial {  get; set; }
        public string Slot { get; set; }
        public string Pon {  get; set; }
        public string Onu_Id { get; set; }
        public bool Onu_Encontrada { get; set; }

    }

    public class GetStatusADMeOperacionalONU
    {
        public bool Status_Adm {  get; set; }
        public bool Status_Ope { get; set; }
        public string Motivo_Erro { get; set; }
    }

    public class SaldoVincCliente
    {
        public String CODMAT { get; set; }
        public String CODMAT_RET { get; set; }
        public String CODMAT_REAP { get; set; }
        public String descricao { get; set; }
        public String cpl { get; set; }
        public String unid { get; set; }
        public bool subCodExclusivo { get; set; }
        public String saldo { get; set; }
        public bool ativacaoSinal { get; set; }
        public Tipo_Equipamento? tipoEquipamento { get; set; }
    }

    public enum Tipo_Equipamento
    {
        ONU = 1,
        Roteador = 2,
        TV = 3,
        Telefonia = 4,
        Radio = 5
    }
}