using Sinapse.Model.enumerados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.Ativacao.Models
{
    public class TesteONUModels
    {
        public string Id { get; set; }
        public string Num_OS { get; set; }
        public DateTime? Data_Hora_Teste { get; set; }
        public string Usuario_Responsavel { get; set; }

        //Resumo dos testes
        public bool Teste_Sistema { get; set; }
        public bool Teste_Provisionamento { get; set; }
        public bool Teste_Operacional { get; set; }
        public bool Teste_Optico { get; set; }
        public bool Teste_Trafego_Dados { get; set; }
        public bool Teste_WIFI { get; set; }
        public string MotivoReprovaTesteSis { get; set; }
        public string MotivoReprovaTesteProv { get; set; }
        public string MotivoReprovaTesteOpe { get; set; }
        public string MotivoReprovaTesteOpt { get; set; }
        public string MotivoReprovaTesteDados { get; set; }
        public string MotivoReprovaTesteWifi2G { get; set; }
        public string MotivoReprovaTesteWifi5G { get; set; }

        //Informações do Sistema
        public string Posicao_ONU { get; set; }
        public string Versao_Firmware_Ativo { get; set; }
        public string Numero_Versao { get; set; }
        public string Serial { get; set; }
        public string MAC { get; set; }
        public bool Info_Sistema_Resultado { get; set; }

        //Informações Operacionais
        public Sinapse.Model.enumerados.StatusONU Status_Administrativo { get; set; }
        public Sinapse.Model.enumerados.StatusONU Status_Operacional { get; set; }
        public bool Info_Operacionais_Resultado { get; set; }

        //Informações Ópticas
        public string Sinal_RX_ONU { get; set; }
        public string Sinal_TX_ONU { get; set; }
        public string Temperatura { get; set; }
        public string Voltagem { get; set; }
        public string Corrente_Laser { get; set; }
        public string Sinal_RX_OLT { get; set; }
        public string Diferenca_Sinal { get; set; }
        public bool Info_Opticas_Resultado { get; set; }

        //Informações Wi-Fi 2.4GHz
        public string Interface_24 { get; set; }
        public string Mac_Interface { get; set; }

        //Basic Service Set Identifier (endereço físico do ponto de acesso)
        public string Bssid { get; set; }

        //Indicador de intensidade do sinal recebido
        public string Rssi { get; set; }
        public string Qualidade_Sinal { get; set; }
        public string Velocidade_Download { get; set; }
        public string Velocidade_Upload { get; set; }
        public string Latencia { get; set; }
        public bool Info_24_Resultado { get; set; }

        //Informações Wi-Fi 5GHz
        public string Interface_5G { get; set; }
        public string Mac_Interface_5G { get; set; }

        //Basic Service Set Identifier (endereço físico do ponto de acesso)
        public string Bssid_5G { get; set; }

        //Indicador de intensidade do sinal recebido
        public string Rssi_5G { get; set; }
        public string Qualidade_Sinal_5G { get; set; }
        public string Velocidade_Download_5G { get; set; }
        public string Velocidade_Upload_5G { get; set; }
        public string Latencia_5G { get; set; }
        public bool Info_5G_Resultado { get; set; }
    }
}

