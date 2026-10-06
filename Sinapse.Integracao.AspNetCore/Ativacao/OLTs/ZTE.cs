using Aniel.Connect.Core.AspNetCore;
using log4net;
using PrimS.Telnet;
using Renci.SshNet;
using Sinapse.Core.AspNetCore;
using Sinapse.Integracao.AspNetCore.Ativacao.Interfaces;
using Sinapse.Integracao.AspNetCore.Ativacao.Models;
using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Util.AspNetCore.RemoteConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sinapse.Integracao.AspNetCore.Ativacao.OLTs
{
    //TODO: Será necessário mais de uma implementação pois os comandos diferem pelo modelo da OLT ZTE.
    public class ZTE : IntegracaoAtivacao
    {
        private OLT olt;

        public ZTE(OLT olt)
        {
            this.olt = olt;
        }

        public Response AutorizarONU(ConfigurarONUModel onu, string scriptOLT, string scriptONU)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU)
        {
            var response = new Response();
            var result = false;

            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.olt.IpTL1))
                    {
                        using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                throw new NotImplementedException();
                            }
                            else
                            {

                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 700;

                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 2000);
                                bool modo_terminal = await SetTerminalMode(client);

                                if (!onu.OntID.HasValue && scriptOLT.ToUpperInvariant().Contains("<ONT_ID>".ToUpperInvariant()))
                                {
                                    //Pegar próximo Id de ONU para autorização
                                    onu.OntID = await GetProximoOnuIdDisp(client, onu.IdSlot.Value, onu.IdPon.Value);
                                }

                                var linhasScriptOLT = scriptOLT.Split('\n');
                                foreach (string linha in linhasScriptOLT)
                                {
                                    if (linha.ToUpperInvariant().Contains("<ONT_ID>"))
                                    {
                                        await client.WriteLineAsync(linha.Replace("<ONT_ID>", Convert.ToString(onu.OntID)));
                                        continue;
                                    }
                                    await client.WriteLineAsync(linha);
                                }

                                var resposta = await client.ReadAsync();

                                if (resposta.ToUpperInvariant().Contains("error".ToUpperInvariant()) ||
                                    resposta.ToUpperInvariant().Contains("failed".ToUpperInvariant()))
                                {
                                    throw new EAnielException(resposta);
                                }

                                result = true;
                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new EAnielException(ex.Message);
            }

            return response;
        }

        public async Task<Response> DesautorizarONU(ConfigurarONUModel onu)
        {
            Response response = new Response();
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.olt.IpTL1))
                    {
                        using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                throw new NotImplementedException();
                            }
                            else
                            {

                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;
                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 2000);
                                bool modo_terminal = await SetTerminalMode(client);

                                await client.WriteLineAsync($"interface gpon_olt-1/{onu.IdSlot}/{onu.IdPon}");
                                await client.WriteLineAsync($"no onu {onu.OntID}");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("error".ToUpperInvariant()) || p.ToUpperInvariant().Contains("failed".ToUpperInvariant())))
                                    throw new EAnielException(string.Join("\n", resposta.ToArray()));

                                response.result = true;
                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.result = false;
                response.exception = $" {ex.Message}";
            }
            return response;
        }

        public async Task<DiagnosticoONU> DiagnosticoONU(ConfigurarONUModel onu)
        {
            DiagnosticoONU retorno = new DiagnosticoONU();
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.olt.IpTL1))
                    {
                        using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                throw new NotImplementedException();
                            }
                            else
                            {

                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;
                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 2000);
                                bool modo_terminal = await SetTerminalMode(client);

                                /*BR-PA-CAH-CCL-OLT-02-ZTE(config)#show pon power attenuation gpon_onu-1/1/1:1
                                           OLT                  ONU              Attenuation
                                --------------------------------------------------------------------------
                                 up      Rx :-14.077(dbm)      Tx:2.804(dbm)        16.881(dB)

                                 down    Tx :5.340(dbm)        Rx:-11.829(dbm)      17.169(dB)*/

                                await client.WriteLineAsync($"show pon power attenuation gpon_onu-1/{onu.IdSlot}/{onu.IdPon}:{onu.OntID}");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("up".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("up".ToUpperInvariant())).ToUpperInvariant()
                                        .Replace("RX :", String.Empty)
                                        .Replace("TX :", String.Empty)
                                        .Replace("RX:", String.Empty)
                                        .Replace("TX:", String.Empty)
                                        .Replace("(DBM)", String.Empty)
                                        .Replace("(DB)", String.Empty);

                                    var campos = linha.Split(' ', '\t', ':').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Rx_OLT = TratarCampoDiagosticoONU(campos.ElementAtOrDefault(1));
                                    retorno.Tx = TratarCampoDiagosticoONU(campos.ElementAtOrDefault(2));
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("down".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("down".ToUpperInvariant())).ToUpperInvariant()
                                        .Replace("RX :", String.Empty)
                                        .Replace("TX :", String.Empty)
                                        .Replace("RX:", String.Empty)
                                        .Replace("TX:", String.Empty)
                                        .Replace("(DBM)", String.Empty)
                                        .Replace("(DB)", String.Empty);

                                    var campos = linha.Split(' ', '\t', ':').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Tx_OLT = TratarCampoDiagosticoONU(campos.ElementAtOrDefault(1));
                                    retorno.Rx = TratarCampoDiagosticoONU(campos.ElementAtOrDefault(2));
                                }

                                /*BR-PA-CAH-CCL-OLT-02-ZTE(config)#show gpon onu detail-info gpon_onu-1/1/2:1

                                    ONU interface:          gpon_onu-1/1/2:1
                                      Name:                 38896298067
                                      Splitter:
                                      Type:                 F670LV1.1.01
                                      Configured speed mode:auto
                                      Current speed mode:   GPON
                                      Admin state:          enable
                                      Phase state:          working
                                      Config state:         success
                                      Authentication mode:  sn
                                      SN Bind:              enable with SN check
                                      Serial number:        ZTEGC4B4BE9D
                                      Password:
                                      Description:
                                      Vport mode:           gemport
                                      DBA Mode:             Hybrid
                                      ONU Status:           enable
                                      OMCI BW Profile:      704kbps
                                      OMCC Encrypt:         disable
                                      Line Profile:         N/A
                                      Service Profile:      N/A
                                      ONU Distance:         161m
                                      Online Duration:      0h 11m 07s
                                      FEC:                  disable
                                      FEC actual mode:      disable
                                      1PPS+ToD:             disable
                                      Auto replace:         disable
                                      Multicast encryption: disable
                                      Multicast encryption current state:N/A
                                    ------------------------------------------
                                           Authpass Time          OfflineTime             Cause
                                       1   2023-08-31 17:01:05    2023-09-04 13:15:14     UnKnown
                                       2   2023-09-04 13:15:28    2023-09-05 17:49:25     LOS
                                       3   2023-09-05 17:51:46    2023-09-06 10:36:37     LOS
                                 */

                                await client.WriteLineAsync($"show gpon onu detail-info gpon_onu-1/{onu.IdSlot}/{onu.IdPon}:{onu.OntID}");

                                resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("ONU Distance".ToUpperInvariant())))
                                {
                                    var linha = resposta.FirstOrDefault(p => p.ToUpperInvariant().Contains("ONU Distance".ToUpperInvariant()));
                                    var campos = linha.Split(':').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Type = campos.ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Online Duration".ToUpperInvariant())))
                                {
                                    var linha = resposta.FirstOrDefault(p => p.ToUpperInvariant().Contains("Online Duration".ToUpperInvariant()));
                                    var campos = linha.Split(':').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Uptime = campos.ElementAtOrDefault(1);
                                }

                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new EAnielException(ex.Message);
            }

            return retorno;
        }

        public async Task<List<Models.StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listONTIDs, Func<string, Task<string>> consultarOntId = null, List<(string, string)> listaAuxiliar = null)
        {
            List<Models.StatusONU> retorno = new List<Models.StatusONU>();
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                                
                if (!String.IsNullOrEmpty(this.olt.IpTL1))
                {
                    using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                    {
                        if (client.IsConnected)
                        {
                            client.MillisecondReadDelay = 500;

                            throw new NotImplementedException();
                        }
                        else
                        {

                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }
                else
                {
                    using (RemoteConnection client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                    {
                        await client.Connect();
                        if (client.IsConnected())
                        {                            
                            bool modo_terminal = await SetTerminalMode(client);

                            /*BR-PA-CAH-CCL-OLT-02-ZTE(config)#show gpon onu state gpon_olt-1/1/2
                            OnuIndex     Admin state  OMCC state  Phase state  Speed mode
                            ---------------------------------------------------------------
                            1/1/2:1       enable       enable      working      GPON
                            ONU Number: 1/1

                            POSSIVEIS STATUS:
                            AuthFail = Falha de autenticação
                            DyingGasp = Desligada ou com Perda de Sinal por motivo externo (Ex: rompimento de cabos)
                            LOS = Loss of State (Offline)
                            Logging = Medindo distância entre OLT e ONU
                            OffLine
                            speed-mode  ONU speed mode = ?
                            syncMib = Medindo distância entre OLT e ONU e sincronizando dados
                            working = Online
                                */

                            await client.ExecuteCommandAsync($"show gpon onu state gpon_olt-1/{numSlot}/{numPON}");

                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            LogIntegracaoAtivacao(string.Join("\n", resposta));

                            if (resposta.Exists(p => p.ToUpperInvariant().Contains("Invalid input detected".ToUpperInvariant()) || p.ToUpperInvariant().Contains("Error 20202".ToUpperInvariant()) || p.ToUpperInvariant().Contains("Error 20200".ToUpperInvariant())))
                            {
                                //Em alguns modelos de OLT ZTE a sintaxe dos comandos "gpon" é diferente, com os caracteres "-" e "_" em posição invertida.
                                //Nestes casos a rotina tenta a sintaxe com padrão diferente.
                                await client.ExecuteCommandAsync($"show gpon onu state gpon-olt_1/{numSlot}/{numPON}");

                                resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                            }

                            var lista_linhas_onu = resposta.FindAll(p => p.Contains($"1/{numSlot}/{numPON}:"));

                            if (resposta.Exists(p => p.ToUpperInvariant().Contains("error".ToUpperInvariant())))
                            {
                                throw new EAnielException(resposta.FirstOrDefault(p => p.ToUpperInvariant().Contains("error".ToUpperInvariant())));
                            }

                            if (lista_linhas_onu.Count > 0)
                            {
                                foreach (var ontId in listONTIDs)
                                {
                                    var linha = lista_linhas_onu.FirstOrDefault(p => p.Contains($"1/{numSlot}/{numPON}:{ontId}"));

                                    if (!String.IsNullOrEmpty(linha))
                                    {
                                        var campos = linha.Split(' ', '\t').ToList();
                                        campos.RemoveAll(s => s == " " || s == String.Empty);

                                        if (campos.ElementAtOrDefault(3).ToUpperInvariant().Contains("working".ToUpperInvariant()) ||
                                            campos.ElementAtOrDefault(3).ToUpperInvariant().Contains("syncMib".ToUpperInvariant()) ||
                                            campos.ElementAtOrDefault(3).ToUpperInvariant().Contains("Logging".ToUpperInvariant()))
                                        {
                                            retorno.Add(new Models.StatusONU
                                            {
                                                ONTID = ontId,
                                                Status = Model.AspNetCore.enumerados.StatusONU.Online
                                            });
                                        }
                                        else if (campos.ElementAtOrDefault(3).ToUpperInvariant().Contains("OffLine".ToUpperInvariant()) ||
                                            campos.ElementAtOrDefault(3).ToUpperInvariant().Contains("LOS".ToUpperInvariant()))
                                        {
                                            retorno.Add(new Models.StatusONU
                                            {
                                                ONTID = ontId,
                                                Status = Model.AspNetCore.enumerados.StatusONU.Offline
                                            });
                                        }
                                        else
                                        {
                                            retorno.Add(new Models.StatusONU
                                            {
                                                ONTID = ontId,
                                                Status = Model.AspNetCore.enumerados.StatusONU.Indisponivel
                                            });
                                        }
                                    }
                                        else
                                        {
                                            retorno.Add(new Models.StatusONU
                                            {
                                                ONTID = ontId,
                                                Status = Model.AspNetCore.enumerados.StatusONU.Indisponivel
                                            });
                                }
                            }
                                }

                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                LogIntegracaoAtivacao(ex);
                throw new EAnielException(ex.Message, ex);
            }

            return retorno;
        }

        public async Task<List<object>> ListaOnusConectadas()
        {
            var lista = new List<Object>();
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.olt.IpTL1))
                    {
                        using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                throw new NotImplementedException();
                            }
                            else
                            {

                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;
                                //bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 2000);
                                //bool modo_terminal = await SetTerminalMode(client);

                                throw new NotImplementedException();

                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new EAnielException(ex.Message);
            }

            return new List<object>();
        }

        public async Task<List<AutorizarONUListModel>> ONUsPendentesRegistro()
        {
            var listaOnus = new List<AutorizarONUListModel>();
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.olt.IpTL1))
                    {
                        using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                throw new NotImplementedException();
                            }
                            else
                            {

                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;
                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 2000);
                                bool modo_terminal = await SetTerminalMode(client);

                                /*BR-PA-CAH-CCL-OLT-02-ZTE(config)#show pon onu uncfg
                                OltIndex            Model                SN                 PW
                                -----------------------------------------------------------------------------
                                gpon_olt-1/1/1      NOCLEICODEG240WG     ALCLB3D88D7F       DEFAULT
                                 */

                                await client.WriteLineAsync("show pon onu uncfg");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);

                                var lista_onus_terminal = resposta.Where(p => p.ToUpperInvariant().Contains("gpon_olt-".ToUpperInvariant())).ToList();

                                if (lista_onus_terminal.Count > 0)
                                {
                                    int index = 1;
                                    foreach (var linha_onu in lista_onus_terminal)
                                    {
                                        var campos_onu = linha_onu.Split(' ', '\t').ToList();
                                        campos_onu.RemoveAll(s => s == " " || s == String.Empty);

                                        var interface_onu = campos_onu.ElementAtOrDefault(0)?.Replace("gpon_olt-", String.Empty)?.Split('/')?.ToList();

                                        string numSlot = interface_onu.ElementAtOrDefault(1);
                                        string numPon = interface_onu.ElementAtOrDefault(2);

                                        var onu_type = campos_onu.ElementAtOrDefault(1);
                                        var serial = campos_onu.ElementAtOrDefault(2);

                                        listaOnus.Add(new AutorizarONUListModel
                                        {
                                            Id = $"{numSlot}/{numPon}/{index}",
                                            Desc_OLT = this.olt.Descricao,
                                            Num_OLT = this.olt.NumOLT,
                                            Id_OLT = this.olt.Id,
                                            Slot = numSlot,
                                            PON = numPon,
                                            Mac_Serial = serial,
                                            Desc_ONU = onu_type
                                        });

                                        index++;
                                    }
                                }
                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new EAnielException(ex.Message);
            }

            return listaOnus;
        }

        public async Task<Response> ReiniciarONU(ConfigurarONUModel onu)
        {
            var response = new Response();
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.olt.IpTL1))
                    {
                        using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                throw new NotImplementedException();
                            }
                            else
                            {

                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;
                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 2000);
                                bool modo_terminal = await SetTerminalMode(client);

                                await client.WriteLineAsync($"pon-onu-mng gpon_onu-1/{onu.IdSlot}/{onu.IdPon}:{onu.OntID}");
                                await client.WriteLineAsync("reboot");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Confirm to reboot".ToUpperInvariant())))
                                    await client.WriteLineAsync("yes");

                                var resultado = (await client.ReadAsync());

                                if (resultado.ToUpperInvariant().Contains("error".ToUpperInvariant()) || resultado.ToUpperInvariant().Contains("failed".ToUpperInvariant()) || resultado.ToUpperInvariant().Contains("unavailable".ToUpperInvariant()))
                                    throw new Exception(resultado);

                                response.result = true;
                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.result = false;
                response.message = $"{ex.Message} - {ex.StackTrace}";
            }

            return response;
        }

        public async Task<Response> ResetarONU(ConfigurarONUModel onu)
        {
            var response = new Response();
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.olt.IpTL1))
                    {
                        using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                throw new NotImplementedException();
                            }
                            else
                            {

                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;
                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 2000);
                                bool modo_terminal = await SetTerminalMode(client);

                                await client.WriteLineAsync($"pon-onu-mng gpon_onu-1/{onu.IdSlot}/{onu.IdPon}:{onu.OntID}");
                                await client.WriteLineAsync("restore factory");

                                var resposta = (await client.ReadAsync());

                                if (resposta.ToUpperInvariant().Contains("error".ToUpperInvariant()) || resposta.ToUpperInvariant().Contains("failed".ToUpperInvariant()) || resposta.ToUpperInvariant().Contains("unavailable".ToUpperInvariant()))
                                    throw new Exception(resposta);

                                response.result = true;
                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.result = false;
                response.message = $"{ex.Message} - {ex.StackTrace}";
            }
            return response;
        }

        public async Task<InfoConfigONU> GetConfigEquipamento(ConfigurarONUModel onu)
        {
            InfoConfigONU retorno = new InfoConfigONU();
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.olt.IpTL1))
                    {
                        using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                throw new NotImplementedException();
                            }
                            else
                            {

                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;
                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 1000);
                                bool modo_terminal = await SetTerminalMode(client);

                                /*BR-PA-CAH-CCL-OLT-02-ZTE(config)#show gpon remote-onu wan-ip gpon_onu-1/1/2:1
                                    WAN index:             1
                                    Mode:                  PPPoE
                                    Authentication:        auto
                                    Username:              yago.dias
                                    Password:              12345
                                    Service name:
                                    Status:                connected
                                    Ping response:         enable
                                    Traceroute response:   enable
                                    VLAN tag mode:         tag - TAG MODE DA VLAN
                                    CVLAN:                 108 - ID VLAN
                                    CVLAN priority:        0
                                    SVLAN:                 0
                                    Current IP:            100.70.181.251
                                    Current mask:          255.255.255.255
                                    Current gateway:       10.0.0.1
                                    Current primary DNS:   1.1.1.1
                                    Current secondary DNS: 170.82.180.20
                                    Domain name:
                                    Host name:             omci_ipv4_pppoe_1 - PPPOE SERVICE NAME
                                    MAC address:           e019.54e2.022d
                                    IP host ID:            1
                                 */

                                await client.WriteLineAsync($"show gpon remote-onu wan-ip gpon_onu-1/{onu.IdSlot}/{onu.IdPon}:{onu.OntID}");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Mode".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Mode".ToUpperInvariant()));
                                    retorno.Modo_Conexao = linha.Split(':').ElementAtOrDefault(1);
                                    retorno.Tipo_Conexao = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Authentication".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Authentication".ToUpperInvariant()));
                                    retorno.PPPOEMODE = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Username".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Username".ToUpperInvariant()));
                                    retorno.PPPOE_Usuario = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Service name".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Service name".ToUpperInvariant()));
                                    retorno.Nome_Servico = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("CVLAN".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("CVLAN".ToUpperInvariant()));
                                    retorno.VLAN_ID = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Current IP".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Current IP".ToUpperInvariant()));
                                    retorno.IP_Estatico = linha.Split(':').ElementAtOrDefault(1);
                                    retorno.Tipo_IP = String.IsNullOrEmpty(linha.Split(':').ElementAtOrDefault(1)) ? "Não informado" : retorno.IP_Estatico.Contains(":") ? "IPV6" : "IPV4";
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Current mask".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Current mask".ToUpperInvariant()));
                                    retorno.IP_Mascara = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Current gateway".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Current gateway".ToUpperInvariant()));
                                    retorno.IP_Gateway = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Current primary DNS".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Current primary DNS".ToUpperInvariant()));
                                    retorno.DNS_Master = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Current secondary DNS".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Current secondary DNS".ToUpperInvariant()));
                                    retorno.DNS_Slave = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Host name".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Host name".ToUpperInvariant()));
                                    retorno.PPPOE_ServicoName = linha.Split(':').ElementAtOrDefault(1);
                                }

                                retorno.Porta_ONU = Convert.ToString(onu.OntID);
                                //await client.WriteLineAsync($"show gpon onu detail-info gpon_onu-1/{onu.IdSlot}/{onu.IdPon}:{onu.OntID}");

                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new EAnielException(ex.Message);
            }

            return retorno;
        }

        public async Task<InfoHardwareONU> GetInfoHardware(ConfigurarONUModel onu)
        {
            InfoHardwareONU retorno = new InfoHardwareONU();
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.olt.IpTL1))
                    {
                        using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                throw new NotImplementedException();
                            }
                            else
                            {

                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;
                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 1000);
                                bool modo_terminal = await SetTerminalMode(client);

                                /*BR-PA-CAH-CCL-OLT-02-ZTE(config)#show pon onu information gpon_onu-1/1/2:1

                                ONU interface:                  gpon_onu-1/1/2:1
                                Auth information:               sn(ZTEGC4B4BE9D)
                                SN reported:                    ZTEGC4B4BE9D
                                ONU ID:                         22
                                ONU type configured:            F670LV1.1.01
                                ONU type reported:              F670LV1.1.01
                                State :                         working
                                Hardware version:               V1.1.01
                                Software version:               V1.1.20P3N6B
                                ------------------------------------------
                                        Authpass Time          OfflineTime             Cause
                                    1   2023-08-31 17:01:05    2023-09-04 13:15:14     UnKnown
                                    2   2023-09-04 13:15:28    2023-09-05 17:49:25     LOS
                                    3   2023-09-05 17:51:46    2023-09-06 10:36:37     LOS
                                    4   2023-09-06 10:37:31    0000-00-00 00:00:00
                                 */

                                await client.WriteLineAsync($"show pon onu information gpon_onu-1/{onu.IdSlot}/{onu.IdPon}:{onu.OntID}");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Hardware version".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("ONU type configured".ToUpperInvariant()));
                                    retorno.Modelo = linha.Split(':').ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Software version".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Software version".ToUpperInvariant()));
                                    retorno.Versao_Software = linha.Split(':').ElementAtOrDefault(1);
                                }
                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw new EAnielException(ex.Message);
            }

            return retorno;
        }

        /// <summary>
        /// Busca o ONTID do equipamento pelo Serial.
        /// </summary>
        /// <param name="serial"></param>
        /// <returns></returns>
        public async Task<Int32?> GetOntIdBySerial(string serial)
        {
            int? retorno = null;
            try
            {
                var protocolo = this.olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this.olt.PortaSSH.GetValueOrDefault(22) : this.olt.PortaTelnet.GetValueOrDefault(23);

                if (!String.IsNullOrEmpty(this.olt.IpTL1))
                {
                    using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                    {
                        if (client.IsConnected)
                        {
                            //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                            //client.MillisecondReadDelay = delayTime;
                            client.MillisecondReadDelay = 500;

                            throw new NotImplementedException();
                        }
                        else
                        {

                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }
                else
                {
                    using (RemoteConnection client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                    {

                        await client.Connect();
                        if (client.IsConnected())
                        {
                            //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                            //client.MillisecondReadDelay = delayTime;                                
                            bool modo_terminal = await SetTerminalMode(client);

                            /*VAA-BBN-OLT-03(config)#show gpon onu by sn ZTEGD3495FDF
                                SearchResult
                                -----------------
                                gpon_onu-1/2/3:19
                                VAA-BBN-OLT-03(config)#show gpon onu by sn ZTEGD3495FDF1
                                %Error 230263: Parameter error.
                                VAA-BBN-OLT-03(config)#show gpon onu by sn ZTEGD3495FD1
                                %Error 230285: No related information to show.
                                VAA-BBN-OLT-03(config)#
                                */
                            await client.ExecuteCommandAsync($"show gpon onu by sn {serial.ToUpperInvariant()}");

                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            LogIntegracaoAtivacao(string.Join("\n", resposta));

                            if (resposta.Exists(p => p.ToUpperInvariant().Contains("gpon_onu-".ToUpperInvariant()) || p.ToUpperInvariant().Contains("gpon-onu_".ToUpperInvariant())))
                            {
                                var linha = resposta.First(p => p.ToUpperInvariant().Contains("gpon_onu-".ToUpperInvariant()) || p.ToUpperInvariant().Contains("gpon-onu_".ToUpperInvariant()));
                                string OntID = linha.Split(':').ElementAtOrDefault(1);

                                if (!String.IsNullOrEmpty(OntID))
                                    retorno = Convert.ToInt32(OntID);
                            }
                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogIntegracaoAtivacao(ex);
                throw new EAnielException(ex.Message);
            }

            return retorno;
        }

        #region metodos-privados
        private async Task<bool> SetTerminalMode(Client client)
        {
            bool sucesso = false;
            await client.WriteLineAsync("terminal length 500");
            await client.WriteLineAsync("configure terminal");
            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

            if (resposta.Exists(p => p.ToUpperInvariant().Contains("Enter configuration commands".ToUpperInvariant())))
                sucesso = true;

            return sucesso;
        }

        private async Task<bool> SetTerminalMode(RemoteConnection client)
        {
            bool sucesso = false;
            await client.ExecuteCommandAsync("terminal length 500");
            await client.ExecuteCommandAsync("configure terminal");
            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

            if (resposta.Exists(p => p.ToUpperInvariant().Contains("Enter configuration commands".ToUpperInvariant())))
                sucesso = true;

            return sucesso;
        }

        /// <summary>
        ///     Função que pega o proximo Id disponível para autorização (Só implementado para Telnet no momento).
        /// </summary>
        private async Task<Int32> GetProximoOnuIdDisp(Client client, long numSlot, long numPon)
        {
            Int32 IdOnu = 0;

            //Lista os ids disponíveis para alocação
            //BR-PA-CAH-CCL-OLT-02-ZTE(config)#show pon onu information gpon_olt-1/{numSlot}/{numPon}

            //RETORNO SEM IDS:                                   
            //BR-PA-CAH-CCL-OLT-02-ZTE(config)#show pon onu information gpon_olt-1/1/3
            //% Error 230285: No related information to show.

            //RETORNO COM IDS:
            /*BR-PA-CAH-CCL-OLT-02-ZTE(config)#show pon onu information gpon_olt-1/1/1
                        MAC            LLID                                         Offline
            ONU         SN report      ONU ID Auth configuration State              times
            -------------------------------------------------------------------------------
            1/1/1:1     SN(ZTEGCCEFEB5 124    SN(ZTEGCCEFEB5E)   working            1
                        E)*/

            await client.WriteLineAsync($"show pon onu information gpon_olt-1/{numSlot}/{numPon}");

            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
            resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);

            if (resposta.Exists(p => p.ToUpperInvariant().Contains("No related information to show".ToUpperInvariant()) || p.ToUpperInvariant().Contains("Error 230285".ToUpperInvariant())))
                return 1; //Não há nenhuma ONU conectada/autorizada na PON informada.

            var lista_formatada = resposta.FindAll(p => p.ToUpperInvariant().Contains($"1/{numSlot}/{numPon}:".ToUpperInvariant()));

            if (lista_formatada.Count > 0)
            {
                List<Int32> lista_ids_onu = new List<int>();

                foreach (var linha_onu in lista_formatada)
                {
                    var campos_onu = linha_onu.Split(' ', '\t').ToList();
                    campos_onu.RemoveAll(s => s == " " || s == String.Empty);

                    var interface_onu = campos_onu.ElementAtOrDefault(0)?.Split('/', ':')?.ToList();

                    var id_onu = 0;
                    if (Int32.TryParse(interface_onu.LastOrDefault(), out id_onu))
                        lista_ids_onu.Add(id_onu);

                    if (lista_ids_onu.Count > 0)
                    {
                        //Cada PON pode ter de 1-256 ONUs conectadas.
                        var lista_range = Enumerable.Range(1, 256).ToList();

                        //Pega o primeiro Id disponível a partir da exceção (Tem na lista_range mas não existe na lista_ids_onu).
                        IdOnu = lista_range.Except(lista_ids_onu).First();
                    }
                    else
                    {
                        //Se lista está vazia, não há nenhuma ONU conectada/autorizada na PON informada.
                        IdOnu = 1;
                    }
                }
            }
            else
            {
                //Se lista está vazia, não há nenhuma ONU conectada/autorizada na PON informada.
                IdOnu = 1;
            }

            return IdOnu;
        }

        private string TratarCampoDiagosticoONU(string campo)
        {
            if (String.IsNullOrEmpty(campo))
                return String.Empty;

            if (campo.ToUpperInvariant().Contains("N/A"))
                return String.Empty;

            if (campo.ToUpperInvariant().Contains("no signal".ToUpperInvariant()))
                return "Sem sinal";

            return System.Text.RegularExpressions.Regex.Replace(campo, "[(a-zA-Z)]", String.Empty); ;
        }

        private void LogIntegracaoAtivacao(string msg)
        {
            LogManager.GetLogger("IntegracaoOLT").Info($"[ZTE] - {msg}");
        }

        private void LogIntegracaoAtivacao(Exception ex)
        {
            LogManager.GetLogger("IntegracaoOLT").Error($"[ZTE] - {ex.Message}", ex);
        }
        #endregion        
    }
}
