using Aniel.Connect.Core.AspNetCore;
using log4net;
using NHibernate;
using PrimS.Telnet;
using Renci.SshNet;
using Sinapse.Core.AspNetCore;
using Sinapse.DAO.AspNetCore.repository;
using Sinapse.Integracao.AspNetCore.Ativacao.Interfaces;
using Sinapse.Integracao.AspNetCore.Ativacao.Models;
using Sinapse.Model.AspNetCore;
using Sinapse.Util.AspNetCore.RemoteConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sinapse.Integracao.AspNetCore.Ativacao.OLTs
{
    public class Datacom : IntegracaoAtivacao
    {
        private OLT _olt;

        public Datacom(OLT olt)
        {
            this._olt = olt;
        }

        public Response AutorizarONU(ConfigurarONUModel onu, string scriptOLT, string scriptONU)
        {
            var response = new Response();

            using (ISession session = HibernateUtil.GetSession())
            {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.GetValueOrDefault(22) : this._olt.PortaTelnet.GetValueOrDefault(23);

                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                //TODO: Implementar para SSH
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(23), new System.Threading.CancellationToken()))
                    {
                        if (client.IsConnected)
                        {
                            client.MillisecondReadDelay = 500;
                            var linhasScriptOLT = scriptOLT.Split('\n');
                            foreach (string linha in linhasScriptOLT)
                            {

                            }
                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }
            }
            throw new NotImplementedException();
        }

        public async Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU)
        {
            var response = new Response();
            var result = false;

            using (ISession session = HibernateUtil.GetSession())
            {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.GetValueOrDefault(22) : this._olt.PortaTelnet.GetValueOrDefault(23);

                if ((scriptOLT.Contains("service-port") || (!String.IsNullOrEmpty(scriptONU) && scriptONU.Contains("service-port"))) && onu.ServicePort.HasValue == false)
                {
                    throw new EAnielException($"O script utiliza service-port porém esta informação não está cadastrada para o Perfil de Ativação: {onu?.DescricaoPerfilAtivacao}");
                }

                if ((scriptOLT.Contains("gem") || (!String.IsNullOrEmpty(scriptONU) && scriptONU.Contains("gem"))) && onu?.GemPort.HasValue == false)
                {
                    throw new EAnielException($"O script utiliza gem port porém esta informação não está cadastrada para o Perfil de Ativação: {onu?.DescricaoPerfilAtivacao}");
                }

                if ((scriptOLT.Contains("vlan-id") || (!String.IsNullOrEmpty(scriptONU) && scriptONU.Contains("vlan-id"))) && onu?.VLAN_Uplink.HasValue == false)
                {
                    throw new EAnielException($"O script utiliza service-port porém esta informação não está cadastrada para a PON: {onu?.DescricaoPON}");
                }

                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                //TODO: Implementar para SSH
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(23), new System.Threading.CancellationToken()))
                    {
                        if (client.IsConnected)
                        {
                            client.MillisecondReadDelay = 500;
                            bool logado = await client.TryLoginAsync(this._olt.Login, this._olt.Senha, 1000);

                            if (!scriptOLT.Contains("config"))
                            {
                                await client.WriteLineAsync("config");
                            }

                            var linhasScriptOLT = scriptOLT.Split('\n');
                            foreach (string linha in linhasScriptOLT)
                            {
                                if (linha.Contains("<ONT_ID>"))
                                {
                                    if (!onu.OntID.HasValue)
                                    {
                                        //Lista os ids disponíveis para alocação
                                        //OLT-DTC-LAGOS-FRANCES-MHD(config-gpon-1/1/1)# onu ?

                                        //RETORNO SEM IDS:
                                        //% No entries found
                                        //Possible completions:
                                        //<0-127>   ONU ID

                                        //RETORNO COM IDS:
                                        //Possible completions:
                                        //<0-127>   ONU ID
                                        //---
                                        //0 1

                                        await client.WriteLineAsync("onu ?");
                                        var resposta_onuID = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                        if (resposta_onuID.Exists(p => p.ToUpperInvariant().Contains("NO ENTRIES FOUND")))
                                        {
                                            onu.OntID = 0;
                                        }
                                        else if (resposta_onuID.Exists(p => p.ToUpperInvariant().Contains("POSSIBLE COMPLETIONS")))
                                        {
                                            Regex r = new Regex(@"[a-zA-Z]");

                                            var index_header = resposta_onuID.IndexOf(resposta_onuID.First(p => p.Contains("---")));
                                            //var index_fim = resposta_onuID.IndexOf(resposta_onuID.Find(p => p.Contains("<cr>")));
                                            resposta_onuID = resposta_onuID.Skip(index_header).ToList();

                                            var index_fim = resposta_onuID.IndexOf(resposta_onuID.First(p => r.IsMatch(p)));
                                            //TODO: Colocar um try/catch

                                            var lista_ids_string = resposta_onuID.GetRange(1, index_fim - 1);
                                            lista_ids_string.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);

                                            var lista_ids = new List<Int32>();
                                            foreach (var item in lista_ids_string)
                                            {
                                                var lista_formatada = item.Replace("<cr>", String.Empty).Split('\t');
                                                foreach (var id in lista_formatada) lista_ids.Add(Convert.ToInt32(id));
                                            };

                                            for (var i = 0; i <= 127; i++)
                                            {
                                                if (!lista_ids.Contains(i))
                                                {
                                                    onu.OntID = i;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    await client.WriteLineAsync(linha.Replace("<ONT_ID>", Convert.ToString(onu.OntID)));
                                    continue;
                                }
                                await client.WriteLineAsync(linha);
                            }

                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                        var resposta_commit = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        if (resposta_commit.Exists(p => p.ToUpperInvariant().Contains("COMMIT COMPLETE")))
                        {
                            result = true;
                        }
                    }
                }
            }



            return new Response
            {
                result = result
            };
            //throw new NotImplementedException();
        }

        public async Task<Response> DesautorizarONU(ConfigurarONUModel onu)
        {
            /*
             config
             do show interface gpon onu (?)
             interface gpon 1/W/X
             no onu Y
             commit
             top
             no service-port Z
                 
             W = Slot
             X = Porta PON
             Y = Id da ONU
             Z = Nº Service Port            
             */
            Response retorno = new Response();
            using (ISession session = HibernateUtil.GetSession())
            {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.GetValueOrDefault(22) : this._olt.PortaTelnet.GetValueOrDefault(23);

                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                //TODO: Implementar para SSH
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(23), new System.Threading.CancellationToken()))
                    {
                        if (client.IsConnected)
                        {
                            client.MillisecondReadDelay = 500;

                            bool logado = await client.TryLoginAsync(this._olt.Login, this._olt.Senha, 1000);

                            await client.WriteLineAsync("config");
                            await client.WriteLineAsync($"interface gpon 1/{onu.IdSlot}/{onu.IdPon}");
                            await client.WriteLineAsync($"no onu {onu.OntID}");
                            await client.WriteLineAsync("commit");

                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                            if (resposta.Exists(p => p.ToUpperInvariant().Contains("COMMIT COMPLETE") || p.ToUpperInvariant().Contains("NO MODIFICATIONS")))
                            {
                                retorno.result = true;
                                return retorno;
                            }

                            if (resposta.Exists(p => p.ToUpperInvariant().Contains("ABORT") || (p.ToUpperInvariant().Contains("SERVICE-PORT") && p.ToUpperInvariant().Contains("NOT CONFIGURED"))))
                            {
                                await client.WriteLineAsync("top");
                                await client.WriteLineAsync($"no service-port {onu.VLAN_Uplink}");
                                await client.WriteLineAsync("commit");

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("COMMIT COMPLETE")))
                                {
                                    retorno.result = true;
                                    return retorno;
                                }
                                else
                                {
                                    var mensagem_erro = resposta.FirstOrDefault(p => p.ToUpperInvariant().Contains("ABORT"));
                                    retorno.message = mensagem_erro ?? string.Join(";", resposta);
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
            return retorno;
        }

        public async Task<DiagnosticoONU> DiagnosticoONU(ConfigurarONUModel onu)
        {
            DiagnosticoONU retorno = new DiagnosticoONU();

            try
            {
                using (ISession session = HibernateUtil.GetSession())
                {
                    var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                    var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.GetValueOrDefault(22) : this._olt.PortaTelnet.GetValueOrDefault(23);

                    if (protocolo == Protocol.Ssh)
                    {
                        using (var rc = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                        {
                            rc.Connect();

                            if (rc.IsConnected())
                            {
                                rc.InteractiveExecute<ShellStream>((shellStream) =>
                                {
                                    //TODO: Implementar para SSH
                                    throw new NotImplementedException();
                                });
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(23), new System.Threading.CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                bool logado = await client.TryLoginAsync(this._olt.Login, this._olt.Senha, 1000);

                                await client.WriteLineAsync("config");
                                await client.WriteLineAsync($"do show interface gpon 1/{onu.IdSlot}/{onu.IdPon} onu {onu.OntID}");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();


                                string rx = String.Empty;
                                string tx = String.Empty;
                                string voltagem = String.Empty;
                                string temperatura = String.Empty;

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("RX OPTICAL POWER")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("RX OPTICAL POWER"));
                                    var campos_onu = linha.Split(' ', ':').ToList();
                                    campos_onu.RemoveAll(s => s == " " || s == String.Empty);
                                    rx = campos_onu.Last();
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("TX OPTICAL POWER")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("TX OPTICAL POWER"));
                                    var campos_onu = linha.Split(' ', ':').ToList();
                                    campos_onu.RemoveAll(s => s == " " || s == String.Empty);
                                    tx = campos_onu.Last();
                                }

                                retorno = new DiagnosticoONU
                                {
                                    Voltagem = voltagem,
                                    Temperatura = temperatura,
                                    Rx = rx,
                                    Tx = tx
                                };
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

        public async Task<List<StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listSerial, Func<string, Task<string>> consultarOntId = null, List<(string, string)> listaAuxiliar = null)
        {
            List<StatusONU> retorno = new List<StatusONU>();

            using (ISession session = HibernateUtil.GetSession())
            {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.GetValueOrDefault(22) : this._olt.PortaTelnet.GetValueOrDefault(23);

                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                //TODO: Implementar para SSH
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(23), new System.Threading.CancellationToken()))
                    {
                        if (client.IsConnected)
                        {
                            await Task.Delay(500);

                            bool logado = await client.TryLoginAsync(this._olt.Login, this._olt.Senha, 2000);

                            await client.WriteLineAsync("config");
                            await Task.Delay(1000);
                            await client.WriteLineAsync($"do show interface gpon 1/{numSlot}/{numPON} onu");
                            await Task.Delay(5000);

                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            LogIntegracaoAtivacao(string.Join("\n", resposta));

                            foreach (var serial in listSerial)
                            {
                                string linha = resposta.FirstOrDefault(p => p.ToUpperInvariant().Contains(serial));

                                if (!String.IsNullOrEmpty(linha))
                                {
                                    var campos_onu = linha.Split(' ').ToList();
                                    campos_onu.RemoveAll(s => s == " " || s == String.Empty);

                                    Sinapse.Model.AspNetCore.enumerados.StatusONU status = Sinapse.Model.AspNetCore.enumerados.StatusONU.Indisponivel;

                                    switch (campos_onu[3].ToUpperInvariant())
                                    {
                                        case "UP":
                                            status = Sinapse.Model.AspNetCore.enumerados.StatusONU.Online;
                                            break;
                                        case "DOWN":
                                            status = Sinapse.Model.AspNetCore.enumerados.StatusONU.Offline;
                                            break;
                                        default:
                                            status = Sinapse.Model.AspNetCore.enumerados.StatusONU.Indisponivel;
                                            break;
                                    }

                                    retorno.Add(new StatusONU
                                    {
                                        Serial = campos_onu[2],
                                        Status = status
                                    });
                                }
                                else
                                {
                                    retorno.Add(new StatusONU
                                    {
                                        Serial = serial,
                                        Status = Sinapse.Model.AspNetCore.enumerados.StatusONU.Indisponivel
                                    });
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

            return retorno;
        }

        public async Task<List<object>> ListaOnusConectadas()
        {
            /*
            config
            do show interface gpon 1/X/Y onu (Busca em Slot/Pon específicos)
            OU
            do show interface gpon onu (Busca em toda a OLT)

            X = Slot
            Y = Porta PON

            RETORNO (CONTENDO DADOS): 
            OLT-DTC-LAGOS-FRANCES-MHD(config)# do show interface gpon onu
              Itf     ONU ID   Serial Number   Oper State   Software Download State      Name
            --------  ------   -------------   ----------   --------------------------   ------------------------------------------------
            1/1/1     0        ITBS5F0488CD    Up           None                         ONU-BRIDGE                                 
            1/1/1     1        MKPGB49EB5C6    Up           None                         ONT   

            RETORNO (NENHUMA ONU):
            OLT-DTC-LAGOS-FRANCES-MHD(config)# do show interface gpon onu
            % No entries found.
             */

            throw new NotImplementedException();
        }

        public async Task<List<AutorizarONUListModel>> ONUsPendentesRegistro()
        {
            /*
             config             
             do show interface gpon 1/X/Y discovered-onus (Busca em Slot/Pon específicos)
             OU
             do show interface gpon discovered-onus (Busca em toda a OLT)

             X = Slot
             Y = Porta PON

            RETORNO (CONTENDO DADOS): 
            OLT-DTC-LAGOS-FRANCES-MHD(config)# do show interface gpon discovered-onus
              Chassis / Slot / Port     Serial Number 
            ------------------------    -------------  
                    1/1/1               ITBS5F0488CD                                  
                    1/1/1               MKPGB49EB5C6   

            RETORNO (NENHUMA ONU):
            OLT-DTC-LAGOS-FRANCES-MHD(config)# do show interface gpon discovered-onus
            % No entries found.
             */

            var listaONU = new List<AutorizarONUListModel>();

            using (ISession session = HibernateUtil.GetSession())
            {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.GetValueOrDefault(22) : this._olt.PortaTelnet.GetValueOrDefault(23);

                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                //TODO: Implementar para SSH
                                throw new NotImplementedException();
                            });
                        }
                    }
                }
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(23), new System.Threading.CancellationToken()))
                    {
                        if (client.IsConnected)
                        {
                            client.MillisecondReadDelay = 500;
                            bool logado = await client.TryLoginAsync(this._olt.Login, this._olt.Senha, 1000);

                            await client.WriteLineAsync("config");
                            //await client.TerminatedReadAsync(new Regex("config"), TimeSpan.FromMilliseconds(1000));
                            await client.WriteLineAsync($"do show interface gpon discovered-onus");

                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                            int? index_header = null;
                            if (!resposta.Exists(p => p.Contains("No entries found")))
                                index_header = resposta.IndexOf(resposta.First(linha => linha.Contains("Slot / Port") || linha.Contains("Serial Number")));

                            if (index_header.HasValue)
                            {
                                var lista_onus = resposta.Skip(index_header.Value + 3).ToList();
                                lista_onus.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty || s.Contains("(config)"));

                                foreach (var onu in lista_onus)
                                {
                                    var campos_onu = onu.Split('/', ' ').ToList();
                                    campos_onu.RemoveAll(s => s == " " || s == String.Empty);

                                    var numSlot = campos_onu[1];
                                    var numPon = campos_onu[2];
                                    var serial = campos_onu[3];

                                    listaONU.Add(new AutorizarONUListModel
                                    {
                                        Id = $"1/{numSlot}/{numPon}",
                                        Desc_OLT = this._olt.Descricao,
                                        Num_OLT = this._olt.NumOLT,
                                        Id_OLT = this._olt.Id,
                                        Slot = numSlot,
                                        PON = numPon,
                                        Mac_Serial = serial,
                                    });
                                }
                            }

                            /*foreach(var linha in resposta)
                            {
                                if (linha.Contains("No entries found")) break;

                                if (String.IsNullOrEmpty(linha)) continue;

                                if (linha.Contains("Slot / Port") || linha.Contains("Serial Number")) index_header = resposta.IndexOf(linha);

                                if (linha.Contains("---------")) continue;
                            }*/

                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }

            }

            return listaONU;
        }

        public async Task<Response> ReiniciarONU(ConfigurarONUModel onu)
        {
            Response response = new Response();

            try
            {
                using (ISession session = HibernateUtil.GetSession())
                {
                    var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                    var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.GetValueOrDefault(22) : this._olt.PortaTelnet.GetValueOrDefault(23);

                    if (protocolo == Protocol.Ssh)
                    {
                        using (var rc = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                        {
                            rc.Connect();

                            if (rc.IsConnected())
                            {
                                rc.InteractiveExecute<ShellStream>((shellStream) =>
                                {
                                    //TODO: Implementar para SSH
                                    throw new NotImplementedException();
                                });
                            }
                        }
                    }
                    else
                    {
                        using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(23), new System.Threading.CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                bool logado = await client.TryLoginAsync(this._olt.Login, this._olt.Senha, 1000);

                                await client.WriteLineAsync("config");
                                await client.WriteLineAsync($"interface gpon 1/{onu.IdSlot}/{onu.IdPon}");
                                await client.WriteLineAsync($"onu-reset onu {onu.OntID}");

                                var resposta_pergunta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta_pergunta.Exists(p => p.ToUpperInvariant().Contains("ARE YOU SURE YOU WANT TO RESET THIS ONU")))
                                {
                                    await client.WriteLineAsync("yes");

                                    var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                    if (resposta.Exists(p => p.ToUpperInvariant().Contains("ABORT")))
                                    {
                                        throw new EAnielException(resposta.FirstOrDefault(p => p.ToUpperInvariant().Contains("ABORT")));
                                    }
                                    else
                                    {
                                        response = new Response
                                        {
                                            result = true,
                                            message = "ONU reiniciada com sucesso"
                                        };
                                    }
                                }
                                else
                                {
                                    string mensagem = $"Não foi possível reiniciar a ONU - Serial: {onu.Serial} - MAC: {onu.Mac} - ID: {onu.Id} - Response Telnet: {resposta_pergunta}";
                                    throw new EAnielException(mensagem);
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
                response.result = false;
                response.message = $"{ex.Message} - {ex.StackTrace}";
            }


            return response;
        }

        public Task<Response> ResetarONU(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
        }

        public async Task<InfoConfigONU> GetConfigEquipamento(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
        }

        public async Task<InfoHardwareONU> GetInfoHardware(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
        }

        private void LogIntegracaoAtivacao(string msg)
        {
            LogManager.GetLogger("IntegracaoOLT").Info($"[DATACOM] - {msg}");
        }

        private void LogIntegracaoAtivacao(Exception ex)
        {
            LogManager.GetLogger("IntegracaoOLT").Error($"[DATACOM] - {ex.Message}", ex);
        }
    }
}
