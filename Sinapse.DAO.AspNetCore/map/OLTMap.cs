using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerados;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.DAO.AspNetCore.map
{
    public class OLTMap : ClassMapBase<OLT>
    {
        public OLTMap() : base()
        {
            Table("TB_OLT");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_OLT");

            Map(x => x.OLTID, "OLTID");
            Map(x => x.Descricao, "DESCRICAO").Length(100);
            Map(x => x.Marca_Modelo, "MARCA_MODELO").CustomType<Marca_Modelo_OLT>();
            Map(x => x.Versao_OLT, "VERSAO_OLT").CustomType<Versao_OLT>();

            Map(x => x.NumOLT, "NUM_OLT");
            Map(x => x.CodigoAuxiliar, "CODIGO_AUXILIAR");

            References(x => x.POP, "ID_POP").ForeignKey("FK_OLT_POP");

            #region Dados Comunicação

            Map(x => x.IP, "IP");
            Map(x => x.Login, "LOGIN").Length(25);
            Map(x => x.Senha, "SENHA").Length(25);
            Map(x => x.PortaTelnet, "PORTA_TELNET").Length(10);
            Map(x => x.PortaAPI, "PORTA_API").Length(10);
            Map(x => x.PortaHTTP, "PORTA_HTTP").Length(10);
            Map(x => x.PortaSSH, "PORTA").Length(10);



            #endregion

            #region Dados TL1

            Map(x => x.IpTL1, "IP_TL1");
            Map(x => x.LoginTL1, "LOGIN_TL1").Length(25);
            Map(x => x.SenhaTL1, "SENHA_TL1").Length(25);
            Map(x => x.PortaTL1, "PORTA_TL1").Length(10);

            Map(x => x.Gabinete, "GABINETE");
            Map(x => x.Subrack, "SUBRACK");

            #endregion

            References(x => x.Filial, "ID_FILIAL").ForeignKey("FK_OLT_FILIAL");

            Map(x => x.Status, "STATUS").CustomType<Status_Default>();
            Map(x => x.ConverteOlt, "UTILIZA_CONVERSAO_MAC").CustomType<Status_Default>();

            Map(x => x.DHCP_DNS_Primario, "DHCP_DNS_PRIMARIO").Length(15);
            Map(x => x.DHCP_DNS_Secundario, "DHCP_DNS_SECUNDARIO").Length(15);
            Map(x => x.DHCP_Lan_IP, "DHCP_LAN_IP").Length(15);
            Map(x => x.DHCP_Lan_Mascara, "DHCP_LAN_MASCARA").Length(15);
            Map(x => x.DHCP_Inicio_Range, "DHCP_INICIO_RANGE").Length(15);
            Map(x => x.DHCP_Final_Range, "DHCP_FINAL_RANGE").Length(15);

            Map(x => x.Teste_IP1, "TESTE_IP1").Length(15);
            Map(x => x.Teste_IP2, "TESTE_IP2").Length(15);

            Map(x => x.Tipo_Identificacao, "TIPO_IDENTIFICACAO").CustomType<ONU_Tipo_Identificacao>();
            Map(x => x.Tipo, "TIPO").CustomType<Tipo_OLT>();

            HasMany(x => x.GruposArea).KeyColumn("ID_OLT").Cascade.All().Inverse();
        }
    }
}
