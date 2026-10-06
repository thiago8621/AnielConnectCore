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
    public class Huawei : IntegracaoAtivacao
    {
        private OLT _olt;

        public Huawei(OLT olt)
        {
            this._olt = olt;
        }

        public Response AutorizarONU(ConfigurarONUModel onu, string scriptOLT, string scriptONU)
        {
            var response = new Response();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            #region SSH
            if (protocol == Protocol.Ssh)
            {
                using (RemoteConnection rc = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {
                    if (this._olt.Marca_Modelo == Marca_Modelo_OLT.Huawei)
                    {
                        rc.Connect();
                        rc.InteractiveExecute<ShellStream>((shellStream) =>
                        {
                            string[] scriptOltLines = scriptOLT.Split('\n');

                            int? ontID = null;

                            foreach (var line in scriptOltLines)
                            {
                                if (line.Contains("ont add"))
                                {

                                    if (line.Contains("\\r"))
                                    {
                                        var command = line.Replace("\\r", String.Empty);

                                        shellStream.WriteLine(command);
                                        shellStream.WriteLine(null);
                                    }
                                    else
                                    {
                                        shellStream.WriteLine(line);
                                    }

                                    string readLine;
                                    List<string> linhas = new List<string>();
                                    while ((readLine = shellStream.ReadLine()) != null)
                                    {
                                        linhas.Add(readLine);
                                        if (readLine.Contains("ONTID"))
                                        {
                                            var lineArray = readLine.Split(':');
                                            var value = lineArray[lineArray.Length - 1];

                                            ontID = Convert.ToInt32(value);
                                            onu.OntID = ontID;

                                            break;
                                        }
                                        else if (readLine.Contains("the error locates at"))
                                        {
                                            throw new Exception(string.Join("\n", linhas));
                                        }
                                        else if (readLine.Contains("line profile does not exist"))
                                        {
                                            throw new EAnielException($"Line Profile cadastrado para o Perfil de Ativação {Convert.ToString(onu.DescricaoPerfilAtivacao)} não existe.");
                                        }
                                    }
                                }
                                else if (line.Contains("<ONT_ID>"))
                                {
                                    if (ontID.HasValue)
                                    {
                                        var command = line.Replace("<ONT_ID>", Convert.ToString(ontID.Value));

                                        if (line.Contains("\\r"))
                                        {
                                            command = command.Replace("\\r", String.Empty);

                                            shellStream.WriteLine(command);
                                            //Thread.Sleep(1500);

                                            shellStream.WriteLine(null);
                                        }
                                        else
                                        {
                                            shellStream.WriteLine(line);
                                            //Thread.Sleep(1500);
                                        }
                                    }
                                    else
                                    {
                                        throw new EAnielException($"ONT_ID não atribuído para a ONU de serial {onu.Serial}");
                                    }
                                }
                                else
                                {
                                    if (line.Contains("\\r"))
                                    {
                                        var command = line.Replace("\\r", String.Empty);

                                        shellStream.WriteLine(command);
                                        shellStream.WriteLine(null);
                                    }
                                    else
                                    {
                                        shellStream.WriteLine(line);
                                    }
                                }

                                // Sleep para esperar o término da execução dos comandos
                                Thread.Sleep(1000);
                            }
                        });
                    }

                    // Execução p/ shell script
                    //rc.ExecuteScript(scriptOLT);
                }
            }
            #endregion
            else
            {
                #region TL1
                if (!String.IsNullOrEmpty(this._olt.IpTL1))
                {
                    throw new NotImplementedException();
                }
                #endregion
                #region Telnet
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(22), new CancellationToken()))
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
                #endregion
            }

            if (scriptONU != null)
            {
                using (RemoteConnection rc = new RemoteConnection(onu.PerfilAtivacao_IpGerencia, onu.PerfilAtivacao_PortaTelnet.Value, onu.PerfilAtivacao_Login, onu.PerfilAtivacao_Senha, Protocol.Telnet))
                {
                    rc.ExecuteScript(scriptONU);
                }
            }

            response.result = true;

            return response;
        }

        public async Task<Response> DesautorizarONU(ConfigurarONUModel onu)
        {
            List<int> listIdServicePort = new List<int>();

            var response = new Response();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;
            var result = true;

            #region SSH
            if (protocol == Protocol.Ssh)
            {
                using (RemoteConnection rc = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {
                    if (this._olt.Marca_Modelo == Marca_Modelo_OLT.Huawei)
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>(sh =>
                            {
                                sh.WriteLine("scroll");
                                sh.WriteLine("\r");

                                sh.WriteLine("enable");
                                sh.WriteLine("config");
                                sh.WriteLine("undo smart");

                                sh.WriteLine($"display service-port port 0/{onu.IdSlot}/{onu.IdPon} ont {onu.OntID}");
                                sh.WriteLine("\r");

                                // RETORNO DO COMANDO:

                                // Command:
                                //         display service-port port 0 / 16 / 13 ont 3
                                // Switch-Oriented Flow List
                                // -----------------------------------------------------------------------------
                                //   INDEX VLAN VLAN     PORT F/ S/ P VPI VCI    FLOW FLOW       RX TX   STATE
                                //         ID   ATTR     TYPE                    TYPE PARA
                                // -----------------------------------------------------------------------------
                                //   9639   690 common   gpon 0/16/13 3    2     vlan  690       11 11   up
                                //   9640   400 common   gpon 0/16/13 3    4     vlan  400       13 13   up
                                // -----------------------------------------------------------------------------
                                // Total : 2 (Up/Down : 2/0)

                                string currLine;

                                bool dataStarted = false;

                                while ((currLine = sh.ReadLine()) != null)
                                {
                                    if (currLine.Contains("error") || currLine.Contains("Failure"))
                                    {
                                        break;
                                    }
                                    else if (currLine.Trim().Split(' ')[0] == "INDEX")
                                    {
                                        dataStarted = true;
                                    }
                                    else if (dataStarted)
                                    {
                                        if (currLine.Contains("Total"))
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            if (currLine.Contains("--------------") || currLine.Contains("ID"))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                int idServicePort = Convert.ToInt32(currLine.Trim().Split(' ')[0]);
                                                listIdServicePort.Add(idServicePort);
                                            }
                                        }
                                    }
                                }

                                sh.WriteLine($"interface gpon 0/{onu.IdSlot}");

                                foreach (var idServicePort in listIdServicePort)
                                {
                                    sh.WriteLine($"undo service-port {onu.VLAN_Uplink}");
                                }

                                sh.WriteLine($"ont delete {onu.IdPon} {onu.OntID}");
                                while ((currLine = sh.ReadLine()) != null)
                                {
                                    if (currLine.Contains("error") || currLine.Contains("Failure"))
                                    {
                                        result = false;
                                        response.message = currLine;
                                        break;
                                    }
                                    else if (currLine.Contains("success"))
                                    {
                                        break;
                                    }
                                }
                            });

                            /*rc.ExecuteCommand($"interface gpon 0/{onu.Slot.NumSlot}");

                            foreach (var idServicePort in listIdServicePort)
                            {
                                rc.ExecuteCommand($"undo service-port {onu.VLAN_Uplink}");
                            }

                            rc.ExecuteCommand($"ont delete {onu.PON.Num_PON} {onu.OntID}");*/
                        }
                    }
                }
            }
            #endregion
            else
            {
                #region TL1
                if (!String.IsNullOrEmpty(this._olt.IpTL1))
                {
                    throw new NotImplementedException();
                }
                #endregion
                #region Telnet
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(22), new CancellationToken()))
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
                #endregion
            }

            response.result = result;

            return response;
        }

        public async Task<List<object>> ListaOnusConectadas()
        {
            throw new NotImplementedException();
        }

        public async Task<List<AutorizarONUListModel>> ONUsPendentesRegistro()
        {
            var listaONU = new List<AutorizarONUListModel>();

            using (ISession session = HibernateUtil.GetSession())
            using (var repository = new Conexao<OLT>(session))
            {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

                #region SSH
                if (protocolo == Protocol.Ssh)
                {
                    using (var rc = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {

                            rc.InteractiveExecute<ShellStream>((shellStream) =>
                            {
                                shellStream.WriteLine("scroll");
                                shellStream.WriteLine("\r");

                                shellStream.WriteLine("undo smart");
                                shellStream.WriteLine("enable");
                                shellStream.WriteLine("config");

                                shellStream.WriteLine("cls");

                                // Formato dos registros de retorno da ONU:
                                //
                                // ------------------------------ (separador de registros)
                                // Number               : (int)
                                // F/S/P                : (Num OLT: int)/(Num Slot: int)/(Num PON: int)
                                // Ont SN               : (string)
                                // Password             : (hex)
                                // Loid                 :
                                // Checkcode            :
                                // VendorID             : (string)
                                // Ont Version          : (string)
                                // Ont Software Version : (string)
                                // Ont EquipmentID      : (int)
                                // Ont Customized Info  : (string)
                                // Ont autofind time    : (Date)

                                shellStream.WriteLine("display ont autofind all");
                                shellStream.WriteLine("\r");

                                string currLine;
                                AutorizarONUListModel currOnu = null;

                                while ((currLine = shellStream.ReadLine()) != null)
                                {
                                    if (currLine.Contains("F/S/P"))
                                    {
                                        var lineArray = currLine.Split(':');

                                        var fsp = lineArray[lineArray.Length - 1];
                                        var fspArray = fsp.Split('/');

                                        var numSlot = fspArray[1];
                                        var numPon = fspArray[2];

                                        //CÓDIGO MOVIDO PARA APICONTROLLER QUE CHAMA ESTA ROTINA NO CONNECT
                                        /*var slot = repoSlot.selectFirst(p => p.OLT == this._olt &&
                                                                             p.NumSlot == Convert.ToInt64(numSlot)).Result;

                                        var pon = reposPon.selectFirst(p => p.OLT == this._olt &&
                                                                            p.Slot == slot &&
                                                                            p.Num_PON == Convert.ToInt64(numPon)).Result;*/

                                        // Essa linha é a primeira da qual a gente extrai os dados.


                                        // Inicialzando um novo registro de ONU

                                        currOnu = new AutorizarONUListModel
                                        {
                                            Id = fsp,

                                            Num_OLT = this._olt.NumOLT,
                                            Id_OLT = this._olt.Id,
                                            Slot = numSlot,
                                            PON = numPon,

                                            Desc_OLT = this._olt.Descricao,
                                            //Desc_Slot = slot?.Descricao,
                                            //Desc_PON = pon?.Descricao
                                        };
                                    }
                                    else if (currLine.Contains("Ont SN"))
                                    {
                                        var lineArray = currLine.Split(':');
                                        var value = lineArray[lineArray.Length - 1].Trim();

                                        var serial = value.Split(' ')[0];
                                        currOnu.Mac_Serial = serial;
                                    }
                                    else if (currLine.Contains("Ont autofind time"))
                                    {
                                        // Se já houver um registro de ONU, vamos adicioná-lo à lista do retorno

                                        if (currOnu != null)
                                        {
                                            //CÓDIGO MOVIDO PARA APICONTROLLER QUE CHAMA ESTA ROTINA NO CONNECT
                                            /*var posicaoEstoque = apiCodCpl.PosicaoEstoque(currOnu.Mac_Serial).Result;

                                            currOnu.Posicao_Estoque = posicaoEstoque.Posicao;
                                            currOnu.Informacao = posicaoEstoque.InfoAdicional;*/

                                            listaONU.Add(currOnu);
                                        }
                                    }
                                    else if (currLine.Contains("The number of GPON autofind ONT is") || currLine.Contains("The automatically found ONTs do not exist"))
                                    {
                                        break;
                                    }
                                }
                            });
                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão com a OLT");
                        }
                    }
                }
                #endregion
                else
                {
                    #region TL1
                    if (!String.IsNullOrEmpty(this._olt.IpTL1))
                    {
                        throw new NotImplementedException();
                    }
                    #endregion
                    #region Telnet
                    else
                    {
                        using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(22), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                client.MillisecondReadDelay = 500;

                                await client.WriteLineAsync("scroll");
                                await client.WriteLineAsync("\r");

                                await client.WriteLineAsync("undo smart");
                                await client.WriteLineAsync("enable");
                                await client.WriteLineAsync("config");

                                await client.WriteLineAsync("cls");

                                await client.WriteLineAsync("display ont autofind all");
                                await client.WriteLineAsync("\r");

                                AutorizarONUListModel currOnu = null;

                                var response = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (response.Exists(p => p.ToUpperInvariant().Contains("F/S/P")))
                                {

                                    var linhaFSP = response.Find(p => p.ToUpperInvariant().Contains("F/S/P"));

                                    var lineArray = linhaFSP.Split(':');

                                    var fsp = lineArray[lineArray.Length - 1];
                                    var fspArray = fsp.Split('/');

                                    var numSlot = fspArray[1];
                                    var numPon = fspArray[2];

                                    /*var slot = repoSlot.selectFirst(p => p.OLT == this._olt &&
                                                                         p.NumSlot == Convert.ToInt64(numSlot)).Result;

                                    var pon = reposPon.selectFirst(p => p.OLT == this._olt &&
                                                                        p.Slot == slot &&
                                                                        p.Num_PON == Convert.ToInt64(numPon)).Result;*/

                                    // Essa linha é a primeira da qual a gente extrai os dados.
                                    // Inicialzando um novo registro de ONU

                                    currOnu = new AutorizarONUListModel
                                    {
                                        Id = fsp,

                                        Num_OLT = this._olt.NumOLT,
                                        Id_OLT = this._olt.Id,
                                        Slot = numSlot,
                                        PON = numPon,

                                        Desc_OLT = this._olt.Descricao,
                                        //Desc_Slot = slot?.Descricao,
                                        //Desc_PON = pon?.Descricao
                                    };
                                }

                                if (response.Exists(p => p.ToUpperInvariant().Contains("Ont SN")))
                                {
                                    var linhaSerial = response.Find(p => p.ToUpperInvariant().Contains("Ont SN"));

                                    var lineArray = linhaSerial.Split(':');
                                    var value = lineArray[lineArray.Length - 1].Trim();

                                    var serial = value.Split(' ')[0];
                                    currOnu.Mac_Serial = serial;
                                }

                                if (response.Exists(p => p.ToUpperInvariant().Contains("Ont autofind time")))
                                {
                                    if (currOnu != null)
                                    {
                                        /*var posicaoEstoque = apiCodCpl.PosicaoEstoque(currOnu.Mac_Serial).Result;

                                        currOnu.Posicao_Estoque = posicaoEstoque.Posicao;
                                        currOnu.Informacao = posicaoEstoque.InfoAdicional;*/

                                        listaONU.Add(currOnu);
                                    }
                                }
                            }
                            else
                            {
                                throw new EAnielException("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    #endregion
                }
            }

            return listaONU;
        }

        public async Task<Response> ReiniciarONU(ConfigurarONUModel onu)
        {
            var response = new Response();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;
            var result = true;

            #region SSH
            if (protocol == Protocol.Ssh)
            {
                using (RemoteConnection rc = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {
                    if (this._olt.Marca_Modelo == Marca_Modelo_OLT.Huawei)
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>(sh =>
                            {
                                sh.WriteLine("scroll");
                                sh.WriteLine("\r");

                                sh.WriteLine("enable");
                                sh.WriteLine("config");
                                sh.WriteLine("undo smart");
                                sh.WriteLine($"interface gpon {onu.Subrack}/{onu.IdSlot}");
                                sh.WriteLine($"ont reset {onu.IdPon} {onu.OntID}");

                                string currLine;

                                bool dataStarted = false;

                                while ((currLine = sh.ReadLine()) != null)
                                {
                                    if (currLine.Contains("error") || currLine.Contains("Failure"))
                                    {
                                        result = false;
                                        response.message = currLine;
                                        break;
                                    }
                                    else if (currLine.Contains("Resetting the ONT(s) will interrupt all services on the ONT(s)"))
                                    {
                                        sh.WriteLine("y");
                                        Thread.Sleep(5000);
                                        sh.WriteLine("quit");
                                        break;
                                        // Thread.Sleep(1000);
                                        //sh.WriteLine("\r");
                                    }
                                }

                                var retornoStatus = false;
                                var tentativas = 0;

                                //Rotina para consultar se a ONU voltou a ficar Online, caso não tenha sucesso em até 10 tentativas aborta
                                while (!retornoStatus && tentativas <= 10)
                                {
                                    sh.WriteLine($"interface gpon {onu.Subrack}/{onu.IdSlot}");
                                    sh.WriteLine($"display ont info {onu.IdPon} all");
                                    sh.WriteLine("\r");

                                    // Formato ont info:
                                    //
                                    // ---------------------------------------------------------------------------
                                    // F/S/P   ONT         SN         Control   Run      Config   Match    Protect
                                    //         ID                     flag      state    state    state    side
                                    // ---------------------------------------------------------------------------                            

                                    const string endStr = "the total of ONTs are";
                                    const string errStr = "the error locates at";

                                    while ((currLine = sh.ReadLine()) != null)
                                    {
                                        // identifica se a linha é um registro da listagem 
                                        if (currLine.Contains(errStr) || currLine.Contains(endStr))
                                        {
                                            break;
                                        }
                                        else if ((currLine.Contains($"{onu.Subrack}/{onu.IdSlot}/{onu.IdPon}") && currLine.Contains(onu.Mac)) || (currLine.Contains($"{onu.Subrack}/ {onu.IdSlot}/{onu.IdPon}") && currLine.Contains(onu.Mac)))
                                        {
                                            currLine = currLine.Replace($"{onu.Subrack}/ {onu.IdSlot}/{onu.IdPon}", $"{onu.Subrack}/{onu.IdSlot}/{onu.IdPon}");
                                            var data = currLine
                                                           .Split(' ')
                                                           .Select(var => var.Trim())
                                                           .Where(var => var != String.Empty)
                                                           .ToArray();

                                            if (data[4] == "online")
                                            {
                                                retornoStatus = true;
                                                break;
                                            }
                                            else
                                            {
                                                sh.WriteLine("quit");
                                                sh.WriteLine("cls");

                                                tentativas++;
                                                if (tentativas == 1) //Primeira tentativa aguarda 30 segundos para tentar novamente, tentativas seguintes aguarda 10 segundos
                                                    Thread.Sleep(30000);
                                                else
                                                    Thread.Sleep(10000);
                                                break;
                                            }
                                        }
                                    }
                                }

                                result = retornoStatus;

                            });
                        }

                    }
                }
            }
            #endregion
            else
            {
                #region TL1
                if (!String.IsNullOrEmpty(this._olt.IpTL1))
                {
                    throw new NotImplementedException();
                }
                #endregion
                #region Telnet
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(22), new CancellationToken()))
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
                #endregion
            }

            //config
            //interface gpon 0/1
            //ont reset 0 4
            response.result = result;
            return response;
        }

        public async Task<Response> ResetarONU(ConfigurarONUModel onu)
        {
            var response = new Response();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;
            var result = true;

            #region SSH
            if (protocol == Protocol.Ssh)
            {
                using (RemoteConnection rc = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {
                    if (this._olt.Marca_Modelo == Marca_Modelo_OLT.Huawei)
                    {
                        rc.Connect();

                        if (rc.IsConnected())
                        {
                            rc.InteractiveExecute<ShellStream>(sh =>
                            {
                                sh.WriteLine("scroll");
                                sh.WriteLine("\r");

                                sh.WriteLine("enable");
                                sh.WriteLine("config");
                                sh.WriteLine("undo smart");
                                sh.WriteLine($"interface gpon {onu.Subrack}/{onu.IdSlot}");
                                sh.WriteLine($"ont factory-setting-restore {onu.IdPon} {onu.OntID}");

                                string currLine;

                                bool dataStarted = false;

                                while ((currLine = sh.ReadLine()) != null)
                                {
                                    if (currLine.Contains("error") || currLine.Contains("Failure"))
                                    {
                                        result = false;
                                        response.message = currLine;
                                        break;
                                    }
                                    else if (currLine.Contains($"ont factory-setting-restore {onu.IdPon} {onu.OntID}"))
                                    {
                                        sh.WriteLine("y");
                                        break;
                                    }
                                }

                                string scriptONU = null;
                                if (onu.ScriptONU != null)
                                {
                                    //var scriptONUBase64 = Encoding.UTF8.GetString(onu.ScriptONU);
                                    var scriptONUBase64 = onu.ScriptONU;

                                    scriptONU = Encoding.UTF8.GetString(Convert.FromBase64String(scriptONUBase64));
                                    scriptONU = SubstituirVariaveis(onu, scriptONU);

                                    rc.ExecuteScript(scriptONU);
                                }
                            });
                        }
                    }
                }
            }
            #endregion
            else
            {
                #region TL1
                if (!String.IsNullOrEmpty(this._olt.IpTL1))
                {
                    throw new NotImplementedException();
                }
                #endregion
                #region Telnet
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(22), new CancellationToken()))
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
                #endregion
            }

            //config
            //interface gpon 0/1
            //ont factory-setting-restore 0 4
            response.result = result;
            return response;
        }

        public async Task<List<Models.StatusONU>> GetStatusONUsByPONOld(string numSlot, string numPON, List<string> listSerial)
        {
            var retorno = new List<Models.StatusONU>();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            #region SSH
            if (protocol == Protocol.Ssh)
            {
                using (RemoteConnection rc = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {
                    rc.Connect();
                    rc.InteractiveExecute<ShellStream>((shellStream) =>
                    {
                        shellStream.WriteLine("scroll");
                        shellStream.WriteLine("\r");

                        shellStream.WriteLine("enable");
                        shellStream.WriteLine("config");

                        // TODO: DESCOBRIR DE ONDE TIRAR O FRAMEID
                        shellStream.WriteLine($"interface gpon 0/{numSlot}");

                        shellStream.WriteLine($"display ont info {numPON} all");
                        shellStream.WriteLine("\r");

                        // Formato ont info:
                        //
                        // ---------------------------------------------------------------------------
                        // F/S/P   ONT         SN         Control   Run      Config   Match    Protect
                        //         ID                     flag      state    state    state    side
                        // ---------------------------------------------------------------------------

                        string currLine;

                        const string endStr = "the total of ONTs are";
                        const string errStr = "the error locates at";

                        while ((currLine = shellStream.ReadLine()) != null)
                        {
                            // identifica se a linha é um registro da listagem 
                            if (currLine.Contains(errStr) || currLine.Contains(endStr))
                            {
                                break;
                            }
                            else if (currLine.Contains($"0/{numSlot}/{numPON}") || currLine.Contains($"0/ {numSlot}/{numPON}"))
                            {
                                currLine = currLine.Replace($"0/ {numSlot}/{numPON}", $"0/{numSlot}/{numPON}");
                                var data = currLine
                                               .Split(' ')
                                               .Select(var => var.Trim())
                                               .Where(var => var != String.Empty)
                                               .ToArray();

                                var serial = data[2];

                                if (listSerial.Contains(serial))
                                {
                                    retorno.Add(new Models.StatusONU
                                    {
                                        Serial = serial,
                                        Status = data[4] == "online" ? Sinapse.Model.AspNetCore.enumerados.StatusONU.Online : Sinapse.Model.AspNetCore.enumerados.StatusONU.Offline
                                    });
                                }
                            }
                        }
                    });
                }
            }
            #endregion
            else
            {
                #region TL1
                if (!String.IsNullOrEmpty(this._olt.IpTL1))
                {
                    throw new NotImplementedException();
                }
                #endregion
                #region Telnet
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(22), new CancellationToken()))
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
                #endregion
            }

            return retorno;
        }

        public async Task<List<Models.StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listSerial, Func<string, Task<string>> consultarOntId = null, List<(string, string)> listaAuxiliar = null)
        {
            var retorno = new List<Models.StatusONU>();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            try
            {
                using (RemoteConnection client = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {

                    await client.Connect();
                    if (client.IsConnected())
                    {
                        await client.ExecuteCommandAsync("enable");
                        await client.ExecuteCommandAsync("config");
                        await client.ExecuteCommandAsync("undo smart");
                        await client.ExecuteCommandAsync("scroll");
                        await client.ExecuteCommandAsync($"interface gpon 0/{numSlot}");
                        await client.ExecuteCommandAsync($"display ont info {numPON} all");

                        Thread.Sleep(2000);
                        // Formato ont info:
                        //
                        /*-----------------------------------------------------------------------------
                          F/S/P   ONT         SN         Control     Run      Config   Match    Protect
                                  ID                     flag        state    state    state    side
                          -----------------------------------------------------------------------------
                          0/14/9    0  5A544547C4484505  active      online   normal   mismatch no
                          0/14/9    1  5A544547D14F5C4E  active      online   normal   match    no
                          0/14/9    2  5A544547C4480318  active      online   normal   match    no
                          0/14/9    3  5A544547C4B4ADBE  active      online   normal   match    no
                          0/14/9    7  44443136B3D2786A  active      offline  initial  initial  no
                          0/14/9    8  44443136B3C776AC  active      online   normal   mismatch no
                          0/14/9    9  44443136B3E36C08  active      online   normal   mismatch no
                          0/14/9   15  5A544547CCE2961E  active      online   normal   match    no
                          -----------------------------------------------------------------------------
                          F/S/P   ONT-ID   Description
                          -----------------------------------------------------------------------------
                          0/14/9       0   Leonardo_Santos_da_Luz_zone_Zone_1_descr__extid_ZTEGC448
                                           4505
                          0/14/9       1   DEBORA_FREITAS_PANTOJA_zone_Zone_authd_20230922
                          0/14/9       2   Walace_Rodrigo_Monteiro_Alexandrino_zone_Zone_1_descr__a
                                           uthd_20220427
                          0/14/9       3   WENDEL_RIBEIRO_GALVAO_zone_Zone_1_descr__authd_20230617
                          0/14/9       7   Larissa_Roberta_Fernandes_Cordeiro_zone_Zone_1_descr__ex
                                           tid_DD16B3D2786A
                         */

                        var reader = (await client.ReadAsync());
                        var resposta = reader.Split('\r', '\n').ToList();
                        LogIntegracaoAtivacao(string.Join("\n", resposta));

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("the error locates at".ToUpperInvariant())))
                        {
                            LogIntegracaoAtivacao(resposta.FirstOrDefault(p => p.ToUpperInvariant().Contains("the error locates at".ToUpperInvariant())));
                            throw new EAnielException(string.Join("\n", resposta));
                        }

                        string serialNormal = "";
                        string serialConvertido = "";

                        foreach (var serial in listaAuxiliar)
                        {
                            serialNormal = string.IsNullOrEmpty(serial.Item1) 
                                ? ""
                                : serial.Item1.ToUpperInvariant();

                            serialConvertido = string.IsNullOrEmpty(serial.Item2)
                            ? serialNormal
                            : serial.Item2.ToUpperInvariant();

                            if (resposta.Exists(p => p.ToUpperInvariant().Contains(serialNormal.ToUpperInvariant()))
                                || resposta.Exists(p => p.ToUpperInvariant().Contains(serialConvertido.ToUpperInvariant())))
                            {
                                var linha_serial = resposta.First(p => p.ToUpperInvariant().Contains(serialNormal.ToUpperInvariant())
                                || p.ToUpperInvariant().Contains(serialConvertido.ToUpperInvariant()));

                                LogIntegracaoAtivacao(linha_serial);

                                retorno.Add(new Models.StatusONU
                                {
                                    Serial = serialNormal,
                                    Status = linha_serial.ToUpperInvariant().Contains("online".ToUpperInvariant()) ? Sinapse.Model.AspNetCore.enumerados.StatusONU.Online : Sinapse.Model.AspNetCore.enumerados.StatusONU.Offline
                                });
                            }
                            else
                            {
                                retorno.Add(new Models.StatusONU
                                {
                                    Serial = serialNormal,
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
            catch (Exception ex)
            {
                LogIntegracaoAtivacao(ex);
                throw new EAnielException(ex.Message);
            }

            return retorno;
        }

        public async Task<DiagnosticoONU> DiagnosticoONU(ConfigurarONUModel onu)
        {
            var retorno = new DiagnosticoONU();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            #region SSH
            if (protocol == Protocol.Ssh)
            {
                using (RemoteConnection rc = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {
                    rc.Connect();

                    string numSlot = Convert.ToString(onu.IdSlot);
                    string numPON = Convert.ToString(onu.IdPon);

                    string ontId = Convert.ToString(onu.OntID);

                    rc.InteractiveExecute<ShellStream>((stream) =>
                    {
                        stream.WriteLine("scroll");
                        stream.WriteLine("\r");

                        stream.WriteLine("enable");
                        stream.WriteLine("config");

                        stream.WriteLine($"interface gpon 0/{numSlot}");

                        stream.WriteLine($"display ont optical-info {numPON} {ontId}");
                        stream.WriteLine("\r");

                        //-----------------------------------------------------------------------------
                        //ONU NNI port ID: 0
                        //Module type                            : GPON
                        //Module sub - type                        : CLASS B+
                        //Used type: ONU
                        //Encapsulation Type: BOSA ON BOARD
                        //Optical power precision(dBm)           : 3.0
                        //Vendor name                            : HUAWEI
                        //Vendor rev: -
                        //Vendor PN: HW - BOB - 0007
                        //Vendor SN                              : 1914Y5123916A
                        //Date Code: 19 - 09 - 19
                        //Rx optical power(dBm)                  : -27.96
                        //Rx power current warning threshold(dBm): [-, -]
                        //Rx power current alarm threshold(dBm)  : [-29.0, -7.0]
                        //Tx optical power(dBm)                  : 2.24
                        //Tx power current warning threshold(dBm): [-,-]
                        //Tx power current alarm threshold(dBm) : [0.0,5.0]
                        //Laser bias current(mA) : 5
                        //Tx bias current warning threshold(mA) : [-,-]
                        //Tx bias current alarm threshold(mA) : [2.000,100.000]
                        //Temperature(C) : 38
                        //Temperature warning threshold(C) : [-,-]


                        string currLine;
                        var dataStarted = false;

                        const string recordSeparator = " -----------------------------------------------------------------------------";
                        const string firstDataField = "ONU NNI port ID";


                        while ((currLine = stream.ReadLine()) != null)
                        {
                            if (currLine.Contains(firstDataField))
                                dataStarted = true;
                            else if (currLine.Contains(recordSeparator) && dataStarted)
                                break;

                            if (currLine.Contains("Rx optical power(dBm)") && !currLine.Contains("CATV"))
                            {
                                var lineContent = currLine
                                                      .Split(':')
                                                      .Select(val => val.Trim());

                                retorno.Rx = lineContent.Last();
                            }
                            else if (currLine.Contains("Tx optical power(dBm)"))
                            {
                                var lineContent = currLine
                                                      .Split(':')
                                                      .Select(val => val.Trim());

                                retorno.Tx = lineContent.Last();
                            }
                            else if (currLine.Contains("Temperature(C)"))
                            {
                                var lineContent = currLine
                                                      .Split(':')
                                                      .Select(val => val.Trim());

                                retorno.Temperatura = lineContent.Last();
                            }
                            else if (currLine.Contains("Voltage(V)"))
                            {
                                var lineContent = currLine
                                                      .Split(':')
                                                      .Select(val => val.Trim());

                                retorno.Voltagem = lineContent.Last();
                            }
                        }
                    });
                }
            }
            #endregion
            else
            {
                #region TL1
                if (!String.IsNullOrEmpty(this._olt.IpTL1))
                {
                    throw new NotImplementedException();
                }
                #endregion
                #region Telnet
                else
                {
                    using (Client client = new Client(this._olt.IP, this._olt.PortaTelnet.GetValueOrDefault(22), new CancellationToken()))
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
                #endregion
            }

            return retorno;
        }

        public async Task<InfoConfigONU> GetConfigEquipamento(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
        }

        public async Task<InfoHardwareONU> GetInfoHardware(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
        }

        private string SubstituirVariaveis(ConfigurarONUModel onu, string script)
        {
            return script.Replace("<GABINETE>", onu.Gabinete).
                          Replace("<GEM_PORT>", Convert.ToString(onu.GemPort)).

                          Replace("<IP_GERENCIA>", onu.IpGerencia).
                          Replace("<LOGIN_ONU_CLIENTE>", onu.Login).

                          Replace("<NOME>", onu.ClienteCpfCnpj).

                          Replace("<OLT_DOWNLOAD>", onu.VelocidadeDownload).
                          Replace("<OLT_UPLOAD>", onu.VelocidadeUpload).
                          Replace("<OLT_IP>", onu.IpOLT).
                          Replace("<OLT_ID>", onu.IdOLT.ToString()).

                          Replace("<ONU_MAC>", onu.Mac).
                          Replace("<ONU_TIPO>", onu.Descricao).

                          Replace("<PLANO_CLIENTE>", onu.DescricaoPlano).

                          Replace("<PON_ID>", Convert.ToString(onu.IdPon)).

                          Replace("<PORTA_TELNET_ONU>", Convert.ToString(onu.PortaTelnet)).

                          Replace("<PPPOE_LOGIN>", onu.Login).
                          Replace("<PPPOE_SENHA>", onu.SenhaPPPOE).

                          Replace("<ROUTER_LOGIN>", onu.UsuarioRoteador).
                          Replace("<ROUTER_SENHA>", onu.SenhaRoteador).
                          Replace("<ROUTER_SSID>", onu.SSID_WIFI).
                          //Replace("<ROUTER_SENHA_WIFI>", onu.Senha_WIFI).

                          Replace("<SENHA_ONU>", onu.Senha).
                          Replace("<SENOR_ID>", Convert.ToString(onu.SenorId)).
                          Replace("<SERVICE_PORT>", Convert.ToString(onu.ServicePort)).
                          Replace("<SLOT_ID>", Convert.ToString(onu.IdSlot)).
                          Replace("<VLAN_UPLINK>", Convert.ToString(onu.VLAN_Uplink)).
                          Replace("<VLAN_LOGIN>", Convert.ToString(onu.VLAN_PPPOE)).
                          Replace("<VLAN_PROFILE>", Convert.ToString(onu.VLAN_Profile)).

                          Replace("<LINE_PROFILE>", Convert.ToString(onu.LineProfile)).
                          Replace("<INTERFACE>", String.Format($"{onu.Subrack}/{onu.IdSlot}/{onu.IdPon}")).
                          Replace("<SUBRACK>", onu.Subrack).
                          Replace("<PORTA_HTTP_ONU>", Convert.ToString(onu.PortaHttp));
        }

        public Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU)
        {
            throw new NotImplementedException();
        }

        private void LogIntegracaoAtivacao(string msg)
        {
            LogManager.GetLogger("IntegracaoOLT").Info($"[HUAWEI] - {msg}");
        }

        private void LogIntegracaoAtivacao(Exception ex)
        {
            LogManager.GetLogger("IntegracaoOLT").Error($"[HUAWEI] - {ex.Message}", ex);
        }
    }
}
