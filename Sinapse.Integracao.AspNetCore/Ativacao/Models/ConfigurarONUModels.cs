using Sinapse.Model.AspNetCore.enumerados;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.Integracao.AspNetCore.Ativacao.Models
{
    public class ConfigurarONUModel
    {
        public Int64 Id { get; set; }
        public Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_OLT? MarcaModelo { get; set; }
        public Sinapse.Model.AspNetCore.enumerados.ONU_Tipo_Identificacao? Tipo_Identificacao { get; set; }
        public String Usuario { get; set; }
        public String Descricao { get; set; }

        public String Serial { get; set; }
        public String Mac { get; set; }
        public String Serial_Convertido { get; set; }

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
        public Sinapse.Model.AspNetCore.enumerados.StatusONU? StatusOnu { get; set; }
        public bool PrecisaAtualizarONTID { get; set; }

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
        public string Posicao_Estoque { get; set; }
        public string Informacao { get; set; }
        public string Desc_ONU { get; set; }
        public bool Pode_Autorizar_Sem_OS { get; set; }
        public long Id_Cliente_Plano_Produto { get; set; }
    }

    public class StatusONU
    {
        public string Serial { get; set; }
        public string ONTID { get; set; }
        public Sinapse.Model.AspNetCore.enumerados.StatusONU Status { get; set; }
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
}
