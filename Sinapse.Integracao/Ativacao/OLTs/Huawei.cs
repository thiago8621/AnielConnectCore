using Aniel.Connect.Core;
using log4net;
using NHibernate;
using PrimS.Telnet;
using Renci.SshNet;
using Sinapse.Core;
using Sinapse.DAO.repository;
using Sinapse.Integracao.Ativacao.Interfaces;
using Sinapse.Integracao.Ativacao.Models;
using Sinapse.Model;
using Sinapse.Model.enumerados;
using Sinapse.Util.RemoteConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Sinapse.Integracao.Ativacao.OLTs
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

        public async Task<Response> DesautorizarONU(ConfigurarONUModel onu, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            List<int> listIdServicePort = new List<int>();
            var numOs = string.IsNullOrEmpty(Detalhe) ? $"OPERAÇÃO REALIZADA SEM O.S -> SERIAL: {onu.Serial}" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var response = new Response();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;
            
            try
            {
                using (RemoteConnection client = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {
                    
                   
                    await client.Connect();
                    if (client.IsConnected())
                    {
                        var idLog = enviarLog($"[DesautorizarONU] [HUAWEI] -> ONU Serial: {onu.Serial}, OLT: {this._olt.Descricao}", numUsuario, numOs, null);
                        var comandos = new List<string>(); //Utilizado para gravar os comandos nos logs
                        var respostaOLT = new List<string>(); //Utilizado para gravar os retornos da OLT nos logs

                        await client.ExecuteCommandAsync("enable");
                        await client.ExecuteCommandAsync("config");
                        await client.ExecuteCommandAsync("undo smart");
                        await Task.Delay(2000);

                        var confereRetornoTerminal = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        if (confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("percentage".ToUpperInvariant()))
                       || confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                        {

                            await TrataSistemaOcupado(client);


                        }

                        await client.ExecuteCommandAsync("scroll");
                        await client.ExecuteCommandAsync($"display service-port port 0/{onu.IdSlot}/{onu.IdPon} ont {onu.OntID}");
                        comandos.Add($"display service-port port 0/{onu.IdSlot}/{onu.IdPon} ont {onu.OntID}");


                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 2000;
                        //await client.ReadAsync(delayTime);



                        await Task.Delay(delayTime);
                        //Thread.Sleep(2000);
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

                        var respostaServicePort = (await client.ReadAsync()).Split('\r', '\n').ToList();
                        respostaOLT.Add(string.Join("\n", respostaServicePort));
                        LogIntegracaoAtivacao(string.Join("\n", respostaServicePort));

                        //TODO: Verificar se é necessário remover Service Ports
                        if (respostaServicePort.Exists(p => !p.ToUpperInvariant().Contains("error".ToUpperInvariant()) && !p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                        {
                            /*var linhasServicePort = respostaServicePort.FindAll(p => p.Contains("vlan") && p.Contains("up")).ToList();
                            foreach (var linha in linhasServicePort)
                            {
                                var campos = linha.Split(' ', ':', '\t');
                                var IdVlan = linha.ElementAtOrDefault(1);

                                await client.ExecuteCommandAsync($"undo service-port port 0/{onu.IdSlot}/{onu.IdPon} ont {onu.OntID}");

                                Thread.Sleep(1000);
                                respostaServicePort = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                LogIntegracaoAtivacao(string.Join("\n", respostaServicePort));

                                if (respostaServicePort.Exists(p => p.ToUpperInvariant().Contains("Are you sure to release service virtual port(s)?".ToUpperInvariant())))
                                {
                                    
                                }
                                await client.ExecuteCommandAsync("y");

                                Thread.Sleep(4000);
                            }*/

                            var linhasServicePort = respostaServicePort.FindAll(p => p.ToUpperInvariant().Contains("gpon 0/".ToUpperInvariant())).ToList();
                            foreach (var linha in linhasServicePort)
                            {
                                if (linha.Trim().Replace(" ", String.Empty).ToUpperInvariant().Contains($"gpon0/{onu.IdSlot}/{onu.IdPon}".ToUpperInvariant()))
                                {
                                    await client.ExecuteCommandAsync($"undo service-port port 0/{onu.IdSlot}/{onu.IdPon} ont {onu.OntID}");
                                    comandos.Add($"undo service-port port 0/{onu.IdSlot}/{onu.IdPon} ont {onu.OntID}");


                                    //Thread.Sleep(2000);
                                    await Task.Delay(delayTime);
                                    respostaServicePort = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    respostaOLT.Add(string.Join("\n", respostaServicePort));
                                    LogIntegracaoAtivacao(string.Join("\n", respostaServicePort));

                                    if (respostaServicePort.Exists(p => p.ToUpperInvariant().Contains("Are you sure to release service virtual port(s)?".ToUpperInvariant())))
                                    {
                                        LogIntegracaoAtivacao("Confirmacao para deletar service-port.");
                                        
                                    }
                                    await client.ExecuteCommandAsync("y");
                                    comandos.Add("y");

                            Thread.Sleep(4000);
                            break;
                            }
                            }
                        }

                        await client.ExecuteCommandAsync($"interface gpon 0/{onu.IdSlot}");
                        comandos.Add($"interface gpon 0/{onu.IdSlot}");

                        await client.ExecuteCommandAsync($"ont delete {onu.IdPon} {onu.OntID}");
                        comandos.Add($"ont delete {onu.IdPon} {onu.OntID}");

                        var resposta_unauthorization = (await client.ReadAsync()).Split('\r', '\n').ToList();
                        respostaOLT.Add(string.Join("\n", resposta_unauthorization));
                        LogIntegracaoAtivacao(string.Join("\n", resposta_unauthorization));

                        //Junta os valores que foram adicionados em comandos e respostaOLT
                        var comandosLog = $"Comandos executados:\n{string.Join("\n", comandos)}";
                        var respostasLog = $"Respostas:\n{string.Join("\n", respostaOLT)}";

                        //Realiza gravação de logs
                        await enviarLog($"{comandosLog}\n\n{respostasLog}", numUsuario, numOs, idLog.Result);

                        if (resposta_unauthorization.Exists(p => p.ToUpperInvariant().Contains("error".ToUpperInvariant()) || p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                        {
                            throw new Exception(string.Join("\n", resposta_unauthorization.ToArray()));
                        }
                        else 
                        {
                            LogIntegracaoAtivacao(string.Join("\n", resposta_unauthorization));
                        }

                        response.result = true;
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
                response.result = false;
                response.exception = $" {ex.Message}";
            }           
           
            return response;
        }

        public async Task<List<object>> ListaOnusConectadas()
        {
            throw new NotImplementedException();
        }

        public async Task<List<AutorizarONUListModel>> ONUsPendentesRegistro(string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var listaONU = new List<AutorizarONUListModel>();

            //using (ISession session = HibernateUtil.GetSession())
            //using (var repository = new Conexao<OLT>(session))            
            //{
            var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            #region SSH
           /* if (protocolo == Protocol.Ssh)
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
                                LogIntegracaoAtivacao(currLine);
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


                                    // Inicialzando um novo registro de ONU */

                                   /* currOnu = new AutorizarONUListModel
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
                                } */
                                //else if (currLine.Contains("Ont autofind time"))
                                //{
                                    // Se já houver um registro de ONU, vamos adicioná-lo à lista do retorno

                                    //if (currOnu != null)
                                    //{
                                        //CÓDIGO MOVIDO PARA APICONTROLLER QUE CHAMA ESTA ROTINA NO CONNECT
                                        /*var posicaoEstoque = apiCodCpl.PosicaoEstoque(currOnu.Mac_Serial).Result;

                                        currOnu.Posicao_Estoque = posicaoEstoque.Posicao;
                                        currOnu.Informacao = posicaoEstoque.InfoAdicional;*/

                                        //listaONU.Add(currOnu); 
                                    //}
                                //}
                               /* else if (currLine.Contains("The number of GPON autofind ONT is") || currLine.Contains("The automatically found ONTs do not exist"))
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
            } */
            #endregion
            //else
            //{
                #region TL1
                if (!String.IsNullOrEmpty(this._olt.IpTL1))
                {
                    throw new NotImplementedException();
                }
                #endregion
                #region Telnet && SSH
                else
                {                    
                    using (var client = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                    {
                        await client.Connect();
                        if (client.IsConnected())
                        {

                            await client.ExecuteCommandAsync("enable");
                            await client.ExecuteCommandAsync("config");
                            await client.ExecuteCommandAsync("undo smart");
                            await Task.Delay(2000);

                        var confereRetornoTerminal = (await client.ReadAsync()).Split('\r', '\n').ToList();


                        if (confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("percentage".ToUpperInvariant()))
                           || confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                            {

                               await TrataSistemaOcupado(client);


                            }
                            

                            await client.ExecuteCommandAsync("scroll");                                                                                    
                            



                        //await client.ExecuteCommandAsync("cls");
                        var idLog = enviarLog($"[ONUsPendentesRegistro] [HUAWEI] -> {this._olt.Descricao}", numUsuario, numOs, null);
                        await client.ExecuteCommandAsync("display ont autofind all");
                        //await client.WriteLineAsync("\r");
                            
                        AutorizarONUListModel currOnu = null;
                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 2000;
                        await Task.Delay(delayTime);
                        //Thread.Sleep(delayTime);
                         var response = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        LogIntegracaoAtivacao(string.Join("\n", response));
                        await enviarLog(string.Join("\n", response), numUsuario, numOs, idLog.Result);
                            //DAO.CONSTANTES.log4Net.Error(string.Join("\n", response));

                            if (response.Exists(p => p.ToUpperInvariant().Contains("F/S/P".ToUpperInvariant())))
                            {
                            //var listaHeaderFSP = response.FindAll(p => p.ToUpperInvariant().Contains("F/S/P".ToUpperInvariant()));

                            //Lógica para obter os índices de cada linha do terminal que contém F/S/P (Subrack/Slot/Pon)
                            var listaIndexHeaderFSP = new List<int>();
                            var indexResponse = 0;
                            foreach (var linha in response)
                            {
                                if (linha.ToUpperInvariant().Contains("F/S/P".ToUpperInvariant()))
                                    listaIndexHeaderFSP.Add(indexResponse);
                                indexResponse++;

                            }


                            foreach (var index_header in listaIndexHeaderFSP)
                            {
                                //int? index_header = null;
                                //index_header = response.IndexOf(linhaFSP);

                                var lista_formatada = response.Skip(index_header + 1).ToList();

                                int proximo_header_index = lista_formatada.FindIndex(p => p.ToUpperInvariant().Contains("F/S/P".ToUpperInvariant())
                                    || p.ToUpperInvariant().Contains("The number of GPON autofind ONT is".ToUpperInvariant()));

                                if (proximo_header_index > 0)
                                {
                                    lista_formatada = lista_formatada.GetRange(0, proximo_header_index).ToList();
                                }

                                var interfaceFSP = response.ElementAtOrDefault(index_header).Split(':').ElementAtOrDefault(1);
                                var camposFSP = interfaceFSP.Split('/');

                                var numSlot = camposFSP.ElementAtOrDefault(1);
                                    var numPon = camposFSP.ElementAtOrDefault(2);

                                    var onu_model = new AutorizarONUListModel
                                    {
                                        Id = interfaceFSP,
                                        Num_OLT = this._olt.NumOLT,
                                        Id_OLT = this._olt.Id,
                                        Slot = numSlot,
                                        PON = numPon,
                                        Desc_OLT = this._olt.Descricao,
                                    };

                                    if (lista_formatada.Exists(p => p.ToUpperInvariant().Contains("Ont SN".ToUpperInvariant())))
                                    {
                                        var linhaSerial = lista_formatada.First(p => p.ToUpperInvariant().Contains("Ont SN".ToUpperInvariant())).Split(':').ElementAtOrDefault(1);

                                        var camposSerial = linhaSerial.Split(' ').ToList();
                                        camposSerial.RemoveAll(s => s == " " || s == String.Empty);
                                        var serial = camposSerial.ElementAtOrDefault(0);
                                        onu_model.Mac_Serial = serial;
                                    }

                                    if (lista_formatada.Exists(p => p.ToUpperInvariant().Contains("Ont EquipmentID".ToUpperInvariant())))
                                    {
                                        var onuType = lista_formatada.First(p => p.ToUpperInvariant().Contains("Ont EquipmentID".ToUpperInvariant())).Split(':').ElementAtOrDefault(1);
                                        onu_model.Desc_ONU = onuType;
                                    }

                                    listaONU.Add(onu_model);
                                }
                            }
                            await client.ExecuteCommandAsync("quit");
                            await client.ExecuteCommandAsync("quit");
                            await client.ExecuteCommandAsync("y");
                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }
                #endregion
            //}
            //}

            return listaONU;
        }

        public async Task<GetDadosONUOLT> GetDadosONUnaOLT(string Serial_Convertido, string serial, string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var DadosONU = new GetDadosONUOLT();
            List<string> listaResposta = new List<string>();

            var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            #region TL1
            if (!String.IsNullOrEmpty(this._olt.IpTL1))
            {
                throw new NotImplementedException();
            }
            #endregion
            #region Telnet && SSH
            else
            {
                using (var client = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                {
                    await client.Connect();
                    if (client.IsConnected())
                    {

                        await client.ExecuteCommandAsync("enable");
                        await client.ExecuteCommandAsync("config");
                        await client.ExecuteCommandAsync("undo smart");
                        await Task.Delay(2000);

                        var confereRetornoTerminal = (await client.ReadAsync()).Split('\r', '\n').ToList();


                        if (confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("percentage".ToUpperInvariant()))
                           || confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                        {

                            await TrataSistemaOcupado(client);


                        }

                        await client.ExecuteCommandAsync("scroll");

                        var idLog = enviarLog($"[GetDadosONUnaOLT] [HUAWEI] -> {this._olt.Descricao} SERIAL {serial} - SERIAL CONVERTIDO {Serial_Convertido}", numUsuario, numOs, null);
                        await client.ExecuteCommandAsync($"display ont info by-sn {serial?.ToUpper()}");

                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 5000;
                        await Task.Delay(delayTime);
                        var response = (await client.ReadAsync()).Split('\r', '\n').ToList();
                        listaResposta.AddRange(response);
                        LogIntegracaoAtivacao(string.Join("\n", response));


                        if (response.Exists(p => p.ToUpperInvariant().Contains("F/S/P".ToUpperInvariant()))                            )
                        {


                            var fspLine = response.FirstOrDefault(l => l.Trim().StartsWith("F/S/P"));
                            if (fspLine != null)
                            {
                                var parts = fspLine.Split(':');
                                if (parts.Length > 1)
                                {
                                    var fspParts = parts[1].Trim().Split('/');
                                    if (fspParts.Length == 3)
                                    {
  
                                        DadosONU.Slot = fspParts[1].Trim();
                                        DadosONU.Pon = fspParts[2].Trim();
                                    }
                                }
                            }

                            var ontIdLine = response.FirstOrDefault(l => l.Trim().StartsWith("ONT-ID"));
                            if (ontIdLine != null)
                            {

                                var parts = ontIdLine.Split(':');
                                if (parts.Length > 1)
                                {
                                    DadosONU.Onu_Id = parts[1].Trim();
                                }
                            }

                            DadosONU.Onu_Encontrada = true;
                            DadosONU.Serial = serial;

                            listaResposta.AddRange(response);


                        }
                        else if (DadosONU.Onu_Encontrada == false && this._olt.ConverteOlt == Sinapse.Model.enumerados.Status_Default.Ativo &&
                                !string.Equals(serial, Serial_Convertido, StringComparison.OrdinalIgnoreCase))
                        {
                            await client.ExecuteCommandAsync($"display ont info by-sn {Serial_Convertido?.ToUpper()}");
                            await Task.Delay(delayTime);
                            response = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            LogIntegracaoAtivacao(string.Join("\n", response));
                           

                            if (response.Exists(p => p.ToUpperInvariant().Contains("F/S/P".ToUpperInvariant())))
                            {

                                var fspLine = response.FirstOrDefault(l => l.Trim().StartsWith("F/S/P"));
                                if (fspLine != null)
                                {
                                    var parts = fspLine.Split(':');
                                    if (parts.Length > 1)
                                    {
                                        var fspParts = parts[1].Trim().Split('/');
                                        if (fspParts.Length == 3)
                                        {

                                            DadosONU.Slot = fspParts[1].Trim();
                                            DadosONU.Pon = fspParts[2].Trim();
                                        }
                                    }
                                }

                                var ontIdLine = response.FirstOrDefault(l => l.Trim().StartsWith("ONT-ID"));
                                if (ontIdLine != null)
                                {

                                    var parts = ontIdLine.Split(':');
                                    if (parts.Length > 1)
                                    {
                                        DadosONU.Onu_Id = parts[1].Trim();
                                    }
                                }

                                DadosONU.Onu_Encontrada = true;
                                DadosONU.Serial = Serial_Convertido;
                            }
                            else
                            {
                                DadosONU = new GetDadosONUOLT
                                {
                                    Onu_Encontrada = false,
                                    Onu_Id = "",
                                    Slot = "",
                                    Pon = "",
                                    Serial = ""
                                };
                            }

                            listaResposta.AddRange(response);


                        }

                        LogIntegracaoAtivacao(string.Join("\n", listaResposta));
                        await enviarLog(string.Join("\n", listaResposta), numUsuario, numOs, idLog.Result);

                        await client.ExecuteCommandAsync("quit");
                        await client.ExecuteCommandAsync("quit");
                        await client.ExecuteCommandAsync("y");

                      
                    }
                    else
                    {
                        throw new EAnielException("Erro ao estabelecer conexão.");
                    }
                }
            }
            #endregion


            return DadosONU;
        }

        public async Task<Response> ReiniciarONU(ConfigurarONUModel onu)
        {
            var response = new Response();

            try
            {
                var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

                var retornoStatus = false;

                using (RemoteConnection client = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {

                    await client.Connect();
                    if (client.IsConnected())
                    {                        
                        await client.ExecuteCommandAsync("enable");
                        await client.ExecuteCommandAsync("config");
                        await client.ExecuteCommandAsync("undo smart");
                        await client.ExecuteCommandAsync("scroll");
                        await client.ExecuteCommandAsync($"interface gpon {onu.Subrack}/{onu.IdSlot}");
                        await client.ExecuteCommandAsync($"ont reset {onu.IdPon} {onu.OntID}");

                        var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        LogIntegracaoAtivacao(string.Join("\n", resposta));

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Resetting the ONT(s) will interrupt all services on the ONT(s)".ToUpperInvariant())))
                        {
                            await client.ExecuteCommandAsync("y");
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("the error locates at".ToUpperInvariant())))
                        {
                            response.result = false;
                            response.message = string.Join("\n", resposta);

                            return response;
                        }

                        var tentativas = 0;

                        //Rotina para consultar se a ONU voltou a ficar Online, caso não tenha sucesso em até 10 tentativas aborta
                        while (!retornoStatus && tentativas <= 10)
                        {
                            await client.ExecuteCommandAsync($"interface gpon {onu.Subrack}/{onu.IdSlot}");
                            await client.ExecuteCommandAsync($"display ont info {onu.IdPon} all");

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

                            const string endStr = "the total of ONTs are";
                            const string errStr = "the error locates at";

                            resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                            LogIntegracaoAtivacao(string.Join("\n", resposta));

                            if (resposta.Exists(p => p.ToUpperInvariant().Contains("the error locates at".ToUpperInvariant())))
                            {
                                response.result = false;
                                response.message = string.Join("\n", resposta);

                                break;
                            }

                            if (resposta.Exists(p => p.ToUpperInvariant().Contains(onu.Mac.ToUpperInvariant()) && p.ToUpperInvariant().Contains("online".ToUpperInvariant())))
                            {
                                retornoStatus = true;
                                break;
                            }

                            tentativas++;
                            if (tentativas == 1) //Primeira tentativa aguarda 30 segundos para tentar novamente, tentativas seguintes aguarda 10 segundos
                                Thread.Sleep(30000);
                            else
                                Thread.Sleep(10000);
                        }
                        response.result = retornoStatus;
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
                response.result = false;
                response.message = $"{ex.Message} - {ex.StackTrace}";
            }
            return response;
        }

        public async Task<Response> ResetarONU(ConfigurarONUModel onu)
        {
            var response = new Response();

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
                        await client.ExecuteCommandAsync($"interface gpon {onu.Subrack}/{onu.IdSlot}");
                        await client.ExecuteCommandAsync($"ont factory-setting-restore {onu.IdPon} {onu.OntID}");

                        var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains($"Are you sure to set the ONT(s) to factory configuration?".ToUpperInvariant())))
                        {
                            await client.ExecuteCommandAsync("y");
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("error".ToUpperInvariant())) || resposta.Exists(p => p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                        {
                            throw new EAnielException(string.Join("\n", resposta.ToArray()));
                        }

                        string scriptONU = null;
                        if (onu.ScriptONU != null)
                        {
                            var scriptONUBase64 = onu.ScriptONU;

                            scriptONU = Encoding.UTF8.GetString(Convert.FromBase64String(scriptONUBase64));
                            scriptONU = SubstituirVariaveis(onu, scriptONU);

                            client.ExecuteScript(scriptONU);
                        }

                        response.result = true;
                    }
                    else
                    {

                        throw new EAnielException("Erro ao estabelecer conexão.");
                    }
                }
            }
            catch (Exception ex)
            {
                response.result = false;
                response.message = $"{ex.Message} - {ex.StackTrace}";
            }

            //config
            //interface gpon 0/1
            //ont factory-setting-restore 0 4
            
            return response;
        }

        public async Task<List<Models.StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listSerial, List<(string, string)> listSerialConvertido = null, Func<string, string> consultarOntId = null)
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
                        await Task.Delay(2000);

                        var confereRetornoTerminal = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        if (confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("percentage".ToUpperInvariant()))
                       || confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                        {

                            await TrataSistemaOcupado(client);


                        }
                        await client.ExecuteCommandAsync("scroll");
                        await client.ExecuteCommandAsync($"interface gpon 0/{numSlot}");
                        await client.ExecuteCommandAsync($"display ont info {numPON} all");

                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 2000;
                        //await client.ReadAsync(delayTime);
                        await Task.Delay(delayTime);
                        //Thread.Sleep(delayTime);
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

                        foreach (var serial in listSerialConvertido)
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
                                    Status = linha_serial.ToUpperInvariant().Contains("online".ToUpperInvariant()) ? Sinapse.Model.enumerados.StatusONU.Online : Sinapse.Model.enumerados.StatusONU.Offline
                                });
                            }
                            else
                            {
                                retorno.Add(new Models.StatusONU
                                {
                                    Serial = serialNormal,
                                    Status = Sinapse.Model.enumerados.StatusONU.Indisponivel
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

            string numSlot = Convert.ToString(onu.IdSlot);
            string numPON = Convert.ToString(onu.IdPon);

            string ontId = Convert.ToString(onu.OntID);

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
                        await client.ExecuteCommandAsync($"display ont optical-info {numPON} {ontId}");

                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 2000;
                        await Task.Delay(delayTime);
                        //Thread.Sleep(2000);
                        /*BR-PA-AIU-ACN-OLT-01(config-if-gpon-0/14)#display ont optical-info 15 1
                            -----------------------------------------------------------------------------
                            ONU NNI port ID                        : -
                            Module type                            : -
                            Module sub-type                        : -
                            Used type                              : -
                            Encapsulation Type                     : -
                            Optical power precision(dBm)           : -
                            Vendor name                            : -
                            Vendor rev                             : -
                            Vendor PN                              : -
                            Vendor SN                              : -
                            Date Code                              : -
                            Rx optical power(dBm)                  : -20.60
                            Rx power current warning threshold(dBm): [-,-]
                            Rx power current alarm threshold(dBm)  : [-,-]
                            Tx optical power(dBm)                  : 1.61
                            Tx power current warning threshold(dBm): [-,-]
                            Tx power current alarm threshold(dBm)  : [-,-]
                            Laser bias current(mA)                 : 15
                            Tx bias current warning threshold(mA)  : [-,-]
                            Tx bias current alarm threshold(mA)    : [-,-]
                            Temperature(C)                         : 33
                            Temperature warning threshold(C)       : [-,-]
                            Temperature alarm threshold(C)         : [-,-]
                            Voltage(V)                             : 3.300
                            Supply voltage warning threshold(V)    : [-,-]
                            Supply voltage alarm threshold(V)      : [-,-]
                            OLT Rx ONT optical power(dBm)          : -22.84
                            CATV Rx optical power(dBm)             : -0.01
                            CATV Rx power alarm threshold(dBm)     : [-,-]
                             */

                        var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Rx optical power(dBm)".ToUpperInvariant()) && !p.ToUpperInvariant().Contains("CATV".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Rx optical power(dBm)".ToUpperInvariant()) && !p.ToUpperInvariant().Contains("CATV".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Rx = linha.LastOrDefault();
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Tx optical power(dBm)".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Tx optical power(dBm)".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Tx = linha.LastOrDefault();
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Temperature(C)".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Temperature(C)".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Temperatura = linha.LastOrDefault();
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Voltage(V)".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Voltage(V)".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Voltagem = linha.LastOrDefault();
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("OLT Rx ONT optical power(dBm)".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("OLT Rx ONT optical power(dBm)".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Rx_OLT = linha.LastOrDefault();
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Laser bias current(mA)".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Laser bias current(mA)".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Bias_Current = linha.LastOrDefault();
                        }

                        await client.ExecuteCommandAsync($"display ont info {numPON} {ontId}");

                        delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 2000;
                        await Task.Delay(delayTime);
                        //Thread.Sleep(2000);
                        resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                        

                        //ONT online duration     : 7 day(s), 20 hour(s), 48 minute(s), 33 second(s)
                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("ONT online duration".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("ONT online duration".ToUpperInvariant()))
                                                  .Split(':').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            string uptime_nao_formatado = linha.LastOrDefault();
                            retorno.Uptime = uptime_nao_formatado.Replace(" day(s)", "d").Replace(" hour(s)", "h").Replace(" minute(s)", "m").Replace(" second(s)", "s").Replace(",", string.Empty);
                        }

                        //ONT distance(m)         : 855
                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("ONT distance(m)".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("ONT distance(m)".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            double distancia = 0.0;
                            double.TryParse(linha.LastOrDefault(), out distancia);

                            if (distancia > 0)
                            {
                                if(distancia > 1000)
                                    retorno.Type = $"{distancia / 1000} Km";
                                else
                                    retorno.Type = $"{distancia} m";
                            }
                            else 
                            {
                                retorno.Type = "-";
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
                throw new EAnielException(ex.Message);
            }

            return retorno;
        }

        public async Task<InfoConfigONU> GetConfigEquipamento(ConfigurarONUModel onu)
        {
            InfoConfigONU retorno = new InfoConfigONU();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            string numSlot = Convert.ToString(onu.IdSlot);
            string numPON = Convert.ToString(onu.IdPon);

            string ontId = Convert.ToString(onu.OntID);
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
                        await client.ExecuteCommandAsync($"display ont info {numPON} {ontId}");

                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 3000;
                        await Task.Delay(delayTime);
                        //Thread.Sleep(3000);
                        #region Exemplo Retorno
                        /*BR-PA-AIU-ACN-OLT-01(config-if-gpon-0/14)#display ont info 15 3
                          -----------------------------------------------------------------------------
                          F/S/P                   : 0/14/15
                          ONT-ID                  : 3
                          Control flag            : active
                          Run state               : online
                          Config state            : normal
                          Match state             : match
                          DBA type                : SR
                          ONT distance(m)         : 855
                          ONT battery state       : -
                          Memory occupation       : -
                          CPU occupation          : -
                          Temperature             : -
                          Authentic type          : SN-auth
                          SN                      : 47573234ACA7FDF1 (GW24-ACA7FDF1)
                          Management mode         : OMCI
                          Software work mode      : normal
                          Isolation state         : normal
                          ONT IP 0 address/mask   : -
                          Description             : NELMA_SUELY_DA_CONCEICAO_MASCOTE
                                                    _zone_Zone_1_descr__authd_202304
                                                    20
                          Last down cause         : LOSi/LOBi
                          Last up time            : 2023-09-21 14:28:56-03:00
                          Last down time          : 2023-09-21 14:28:53-03:00
                          Last dying gasp time    : 2023-09-18 11:08:52-03:00
                          ONT online duration     : 7 day(s), 20 hour(s), 48 minute(s), 33 second(s)
                          Type C support          : Not support
                          Interoperability-mode   : ITU-T
                          -----------------------------------------------------------------------------
                          VoIP configure method   : Default
                          -----------------------------------------------------------------------------
                          Line profile ID      : 7
                          Line profile name    : Generic_1_V1415
                          -----------------------------------------------------------------------------
                          FEC upstream switch :Disable
                          OMCC encrypt switch :Off
                          Qos mode            :PQ
                          Mapping mode        :VLAN
                          TR069 management    :Disable
                          TR069 IP index      :0
                          -----------------------------------------------------------------------------
                          Notes: * indicates Discrete TCONT(TCONT Unbound)
                          -----------------------------------------------------------------------------
                          <T-CONT   0>          DBA Profile-ID:1
                          <T-CONT   1>          DBA Profile-ID:14
                           <Gem Index 1>
                           --------------------------------------------------------------------
                           |Serv-Type:ETH |Encrypt:off |Cascade:off |GEM-CAR:-            |
                           |Upstream-priority-queue:0  |Downstream-priority-queue:-       |
                           --------------------------------------------------------------------
                            Mapping VLAN  Priority Port    Port  Bundle  Flow  Transparent
                            index                  type    ID    ID      CAR
                           --------------------------------------------------------------------
                            1       1415  -        -       -     -       -     -
                           --------------------------------------------------------------------
                          -----------------------------------------------------------------------------
                          Notes: Run the display traffic table ip command to query
                                 traffic table configuration
                          -----------------------------------------------------------------------------
                          Service profile ID   : 9
                          Service profile name : Generic_1_V1415
                          -----------------------------------------------------------------------------
                          Port-type     Port-number
                          -----------------------------------------------------------------------------
                          POTS          adaptive
                          ETH           adaptive
                          VDSL          0
                          TDM           0
                          MOCA          0
                          CATV          adaptive
                          -----------------------------------------------------------------------------
                          TDM port type                     : E1
                          TDM service type                  : TDMoGem
                          MAC learning function switch      : Enable
                          ONT transparent function switch   : Disable
                          Ring check switch                 : Disable
                          Ring port auto-shutdown           : Enable
                          Ring detect frequency             : 8 (pps)
                          Ring resume interval              : 300 (s)
                          Multicast forward mode            : Unconcern
                          Multicast forward VLAN            : -
                          Multicast mode                    : Unconcern
                          Upstream IGMP packet forward mode : Unconcern
                          Upstream IGMP packet forward VLAN : -
                          Upstream IGMP packet priority     : -
                          Native VLAN option                : Concern
                          Upstream PQ color policy          : -
                          Downstream PQ color policy        : -
                          -----------------------------------------------------------------------------
                          Port-type Port-ID QinQmode  PriorityPolicy Inbound     Outbound
                          -----------------------------------------------------------------------------
                          ETH       1       unconcern unconcern      unconcern   unconcern
                          ETH       2       unconcern unconcern      unconcern   unconcern
                          ETH       3       unconcern unconcern      unconcern   unconcern
                          ETH       4       unconcern unconcern      unconcern   unconcern
                          ETH       5       unconcern unconcern      unconcern   unconcern
                          ETH       6       unconcern unconcern      unconcern   unconcern
                          ETH       7       unconcern unconcern      unconcern   unconcern
                          ETH       8       unconcern unconcern      unconcern   unconcern
                          -----------------------------------------------------------------------------
                          Notes: * indicates the discretely configured traffic profile,
                                 run the display traffic table ip command to query
                                 traffic table configuration.
                          -----------------------------------------------------------------------------
                          Port-type Port-ID  DownstreamMode  MismatchPolicy
                          -----------------------------------------------------------------------------
                          ETH             1  operation       discard
                          ETH             2  operation       discard
                          ETH             3  operation       discard
                          ETH             4  operation       discard
                          ETH             5  operation       discard
                          ETH             6  operation       discard
                          ETH             7  operation       discard
                          ETH             8  operation       discard
                          -----------------------------------------------------------------------------
                          Port-type Port-ID Dscp-mapping-table-index
                          -----------------------------------------------------------------------------
                          ETH       1       0
                          ETH       2       0
                          ETH       3       0
                          ETH       4       0
                          ETH       5       0
                          ETH       6       0
                          ETH       7       0
                          ETH       8       0
                          IPHOST    1       0
                          -----------------------------------------------------------------------------
                          Port   Port Service-type Index S-VLAN S-PRI C-VLAN C-PRI ENCAP      S-PRI
                          type   ID                                                           POLICY
                          -----------------------------------------------------------------------------
                          ETH    1    Translation  1     1415   -     1415   -     -          -
                          ETH    2    Translation  1     1415   -     1415   -     -          -
                          ETH    3    Translation  1     1415   -     1415   -     -          -
                          ETH    4    Translation  1     1415   -     1415   -     -          -
                          -----------------------------------------------------------------------------
                          Notes: * indicates transparent attribute of the vlan
                          -----------------------------------------------------------------------------
                          Port-type  Port-ID    IGMP-mode         IGMP-VLAN  IGMP-PRI  Max-MAC-Count
                          -----------------------------------------------------------------------------
                          ETH              1    -                         -         -      Unlimited
                          ETH              2    -                         -         -      Unlimited
                          ETH              3    -                         -         -      Unlimited
                          ETH              4    -                         -         -      Unlimited
                          ETH              5    -                         -         -      Unlimited
                          ETH              6    -                         -         -      Unlimited
                          ETH              7    -                         -         -      Unlimited
                          ETH              8    -                         -         -      Unlimited
                          -----------------------------------------------------------------------------
                          Alarm policy profile ID      : 0
                          Alarm policy profile name    : alarm-policy_0
                          -----------------------------------------------------------------------------
                         */
                        #endregion
                        var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                        LogIntegracaoAtivacao(string.Join("\n", resposta));

                        retorno.Id_Onu = onu.OntID.HasValue ? onu.OntID.Value : 0;
                        retorno.Porta_ONU = Convert.ToString(onu.OntID);
                        retorno.VLAN_ID = onu.VLAN_PPPOE.ToString();

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Service profile name".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Service profile name".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Nome_Servico = linha.LastOrDefault();
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Qos mode".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Qos mode".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.QoS = linha.LastOrDefault();
                        }
                        
                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("address/mask".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("address/mask".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.IP_Estatico = linha.LastOrDefault();
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

        public async Task<InfoHardwareONU> GetInfoHardware(ConfigurarONUModel onu)
        {                        
            InfoHardwareONU retorno = new InfoHardwareONU
                ();

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            string numSlot = Convert.ToString(onu.IdSlot);
            string numPON = Convert.ToString(onu.IdPon);

            string ontId = Convert.ToString(onu.OntID);
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
                        await client.ExecuteCommandAsync($"display ont version {numPON} {ontId}");

                        
                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 2000;
                        await Task.Delay(delayTime);
                        /*BR-PA-AIU-ACN-OLT-01(config-if-gpon-0/14)#display ont version 15 3
                          --------------------------------------------------------------------------
                          F/S/P                    : 0/14/15
                          ONT-ID                   : 3
                          Vendor-ID                : xPON
                          ONT Version              : Z710.1A
                          Product-ID               : 0
                          Equipment-ID             : ONU GW24AC
                          Main Software Version    : V1.0.9
                          Standby Software Version : V1.0.9
                          OntProductDescription    :
                          Support XML Version      :
                          --------------------------------------------------------------------------
                         */

                        var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        LogIntegracaoAtivacao(string.Join("\n", resposta));

                        retorno.Id_Onu = onu.OntID.HasValue ? onu.OntID.Value : 0;

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Equipment-ID".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Equipment-ID".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Modelo = linha.LastOrDefault();
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Main Software Version".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Main Software Version".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Versao_Software = linha.LastOrDefault();
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

        public async Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? $"OPERAÇÃO REALIZADA SEM O.S -> SERIAL: {onu.Serial}" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var response = new Response();
            bool verificaDisponibilidade = false;
            int numTentativa = 0;

            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            DAO.CONSTANTES.log4Net.Error("Criando RemoteConnection");
            #region ScriptOLT            
            using (RemoteConnection client = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
            {

                DAO.CONSTANTES.log4Net.Error("Tentando Conectar SSH");
                await client.Connect();
                if(client.IsConnected())
                {
                    var linhasERespostas = new List<(string Linha, List<string> Resposta)>();
                    DAO.CONSTANTES.log4Net.Error("Conectado ao SSH");
                    string[] scriptOltLines = scriptOLT.Split('\n');

                    int? ontID = null;

                    var testeLeitura = await client.ReadAsync();
                    DAO.CONSTANTES.log4Net.Error("Depois do login SSH:" + testeLeitura);

                    if (!scriptOLT.ToUpperInvariant().Contains("enable".ToUpperInvariant()))
                        await client.ExecuteCommandAsync("enable");

                    if (!scriptOLT.ToUpperInvariant().Contains("undo smart".ToUpperInvariant()))
                    {
                        await client.ExecuteCommandAsync("undo smart");
                        await Task.Delay(2000);

                        var confereRetornoTerminal = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        if (confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("percentage".ToUpperInvariant()))
                       || confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                        {

                            await TrataSistemaOcupado(client);


                        }
                    }
                        

                    testeLeitura = await client.ReadAsync();
                    DAO.CONSTANTES.log4Net.Error("Depois do undo smart:" + testeLeitura);
                    LogIntegracaoAtivacao(string.Join("\n", testeLeitura));
                    var idLog = enviarLog($"[AutorizarONUAsync] [HUAWEI] -> ONU Serial: {onu.Serial}, OLT: {this._olt.Descricao}", numUsuario, numOs, null);
                    foreach (var linha in scriptOltLines) 
                    {


                        string linha_formatada = null;
                        //Se a ONU está sem ID cadastrado ainda porém o 
                        //script possui variável para uso do ID, remove a variável da linha para deixa a OLT decidir qual ID atribuir.
                        if (linha.ToUpperInvariant().Contains("<ONT_ID>"))
                        {
                            if (!onu.OntID.HasValue && linha.ToUpperInvariant().Contains("ont add".ToUpperInvariant()))
                            {
                                linha_formatada = linha.Replace("<ONT_ID>", String.Empty);
                            }
                            else if (onu.OntID.HasValue)
                            {
                                linha_formatada = linha.Replace("<ONT_ID>", Convert.ToString(onu.OntID.Value));
                            }
                        }

                        if (!string.IsNullOrEmpty(linha_formatada))
                        {
                            await client.ExecuteCommandAsync(linha_formatada);
                            
                        }
                        else
                        {
                            if (linha.ToUpperInvariant().Contains("ont add".ToUpperInvariant()))
                            {
                                await client.ExecuteCommandAsync(linha);

                                await Task.Delay(10000);                               

                            }
                            else if (linha.ToUpperInvariant().Contains("undo smart".ToUpperInvariant()))
                            {
                                
                                await client.ExecuteCommandAsync(linha);
                                await Task.Delay(2000);

                                var confereRetornoTerminal = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("percentage".ToUpperInvariant()))
                               || confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                                {
                                    //Tratamento realizado para evitar que o comando "save"
                                    //realizado na OLT, afete o processo de provisionamento
                                    await TrataSistemaOcupado(client);


                                }
                            }
                            else if (linha.ToUpperInvariant().Contains("service-port vlan".ToUpperInvariant()))
                            {
                                await Task.Delay(15000);
                                await client.ExecuteCommandAsync(linha);
                                await Task.Delay(2000);
                            }
                            else
                            {


                                await client.ExecuteCommandAsync(linha);
                            }
                                                       
                        }
                                                      
                        var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                        LogIntegracaoAtivacao(string.Join("\n", resposta));
                        
                        DAO.CONSTANTES.log4Net.Error(string.Join("\n", resposta));

                       

                        //Se ONU está sem ID cadastrado, pega o ID atribuído pela própria OLT.
                        if (!onu.OntID.HasValue && resposta.Exists(p => p.ToUpperInvariant().Contains("ONTID".ToUpperInvariant())))
                        {
                            var linhaONTID = resposta.First(p => p.ToUpperInvariant().Contains("ONTID".ToUpperInvariant()));
                            int id_onu = 0;
                            
                            var existe_id_onu = Int32.TryParse(linhaONTID.Split(':').LastOrDefault(), out id_onu);

                            if(existe_id_onu)
                            {
                                
                                onu.OntID = id_onu;
                                LogIntegracaoAtivacao($" ID da ONU encontrado: { id_onu}");

                            }
                            
                        }

                       

                            linhasERespostas.Add((!string.IsNullOrEmpty(linha_formatada) ? linha_formatada : linha, resposta));
                       

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("the error locates at".ToUpperInvariant())))
                        {
                            LogIntegracaoAtivacao(string.Join("\n", resposta));
                            throw new Exception(string.Join("\n", resposta));                            
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("line profile does not exist".ToUpperInvariant())))
                        {
                            throw new EAnielException($"Line Profile cadastrado para o Perfil de Ativação {Convert.ToString(onu.DescricaoPerfilAtivacao)} não existe.");
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("SN already exists".ToUpperInvariant())))
                        {
                            throw new EAnielException($"O serial deste equipamento já se encontra autorizado em outro Slot/Pon. Serial: {onu.Mac}");
                        }
                    }
                    string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
   $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));

                    await enviarLog(fullLog, numUsuario, numOs, idLog.Result);
                }
                else
                {
                    DAO.CONSTANTES.log4Net.Error("Não Conectou no SSH");
                    throw new EAnielException("Erro ao estabelecer conexão.");
                }
            }
            
            #endregion            

            if (scriptONU != null)
            {
                try 
                {
                    using (RemoteConnection client = new RemoteConnection(onu.PerfilAtivacao_IpGerencia, onu.PerfilAtivacao_PortaTelnet.Value, onu.PerfilAtivacao_Login, onu.PerfilAtivacao_Senha, Protocol.Telnet))
                    {
                        client.Connect();
                        if (client.IsConnected())
                        {
                            client.ExecuteScript(scriptONU);
                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }
                catch(Exception ex)
                {
                    response.message = ex.Message;
                    response.stacktrace = ex.StackTrace;
                }
                
            }

            response.result = true;

            return response;
        }

        public async Task<bool> VerificaDisponibilidadeONU(RemoteConnection client, string serial, string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            string controlFlag = "";
            string runState = "";
            string configState = "";

            bool onuDisponivel = false;

            List<string> listaResposta = new List<string>();



                        var idLog = enviarLog($"[VerificaDisponibilidadeONU] [HUAWEI] -> {this._olt.Descricao} SERIAL {serial}", numUsuario, numOs, null);

                        await client.ExecuteCommandAsync($"display ont info by-sn {serial?.ToUpper()}");

                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 5000;
                        await Task.Delay(delayTime);
                        var response = (await client.ReadAsync()).Split('\r', '\n').ToList();
                        listaResposta.AddRange(response);
                        LogIntegracaoAtivacao(string.Join("\n", response));


                        if (response.Exists(p => p.ToUpperInvariant().Contains("F/S/P".ToUpperInvariant())))
                        {


                            // Control flag
                            var controlFlagLine = response.FirstOrDefault(l => l.Trim().StartsWith("Control flag"));
                            if (controlFlagLine != null)
                            {
                                var parts = controlFlagLine.Split(':');
                                if (parts.Length > 1)
                                {
                                    controlFlag = parts[1].Trim();
                                }
                            }

                            // Run state
                            var runStateLine = response.FirstOrDefault(l => l.Trim().StartsWith("Run state"));
                            if (runStateLine != null)
                            {
                                var parts = runStateLine.Split(':');
                                if (parts.Length > 1)
                                {
                                    runState = parts[1].Trim();
                                }
                            }

                            // Config state
                            var configStateLine = response.FirstOrDefault(l => l.Trim().StartsWith("Config state"));
                            if (configStateLine != null)
                            {
                                var parts = configStateLine.Split(':');
                                if (parts.Length > 1)
                                {
                                    configState = parts[1].Trim();
                                }
                            }

                            listaResposta.AddRange(response);


                        }
                       

                        LogIntegracaoAtivacao(string.Join("\n", listaResposta));
                        await enviarLog(string.Join("\n", listaResposta), numUsuario, numOs, idLog.Result);

            if (runState.Equals("online", StringComparison.OrdinalIgnoreCase) &&
                configState.Equals("normal", StringComparison.OrdinalIgnoreCase) &&
                controlFlag.Equals("active", StringComparison.OrdinalIgnoreCase)) 
            {
                onuDisponivel = true;
            }


            return onuDisponivel;
        }

        public async Task<Response> ConfigurarONUAsync(ConfigurarONUModel onu, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
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
        
        /*Se a OLT estiver ocupada devido ao comando "save",
           o método aguardará alguns segundos antes de tentar novamente.
           Serão realizadas 15 tentativas. Durante o processo, caso o sistema
           da OLT fique disponível, o loop while será finalizado e o próximo comando será executado.
        */
        private async Task TrataSistemaOcupado(RemoteConnection client)
        {
           
                int min = 0;
                int max = 15;
                List<string> confereNovoRetorno;

                while (min < max)
                {
                    await Task.Delay(10000);
                    LogIntegracaoAtivacao("Sistema da OLT está ocupado. Tentando novamente...");
                    await client.ExecuteCommandAsync("undo smart");
                    confereNovoRetorno = (await client.ReadAsync()).Split('\r', '\n').ToList();
                    LogIntegracaoAtivacao(string.Join("\n", confereNovoRetorno));
                    if (confereNovoRetorno.Exists(p => p.ToUpperInvariant().Contains("Interactive function is disabled".ToUpperInvariant())))
                    {
                        LogIntegracaoAtivacao("Sistema da OLT está disponível novamente.");
                        break;
                    }
                    min++;
                }
            }


        public async Task<TesteONUModels> GetDadosComplementaresONU(ConfigurarONUModel onu)
        {

            var retorno = new TesteONUModels();
            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

            string numSlot = Convert.ToString(onu.IdSlot);
            string numPON = Convert.ToString(onu.IdPon);

            string ontId = Convert.ToString(onu.OntID);
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
                        await client.ExecuteCommandAsync($"display ont version {numPON} {ontId}");


                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 2000;
                        await Task.Delay(delayTime);
                        /*BR-PA-AIU-ACN-OLT-01(config-if-gpon-0/14)#display ont version 15 3
                          --------------------------------------------------------------------------
                          F/S/P                    : 0/14/15
                          ONT-ID                   : 3
                          Vendor-ID                : xPON
                          ONT Version              : Z710.1A
                          Product-ID               : 0
                          Equipment-ID             : ONU GW24AC
                          Main Software Version    : V1.0.9
                          Standby Software Version : V1.0.9
                          OntProductDescription    :
                          Support XML Version      :
                          --------------------------------------------------------------------------
                         */

                        var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        LogIntegracaoAtivacao(string.Join("\n", resposta));

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Equipment-ID".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Equipment-ID".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Numero_Versao = linha.LastOrDefault();
                        }

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("Main Software Version".ToUpperInvariant())))
                        {
                            var linha = resposta.First(p => p.ToUpperInvariant().Contains("Main Software Version".ToUpperInvariant()))
                                                  .Split(' ', ':', '\t').ToList();
                            linha.RemoveAll(s => s == " " || s == String.Empty);

                            retorno.Versao_Firmware_Ativo = linha.LastOrDefault();
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

        public async Task<GetStatusADMeOperacionalONU> GetStatusADMeOperacionalONU(ConfigurarONUModel onu)
        {
            string controlFlag = "";
            string runState = "";
            string configState = "";
            var retorno = new GetStatusADMeOperacionalONU();

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
                        await Task.Delay(2000);

                        var confereRetornoTerminal = (await client.ReadAsync()).Split('\r', '\n').ToList();

                        if (confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("percentage".ToUpperInvariant()))
                       || confereRetornoTerminal.Exists(p => p.ToUpperInvariant().Contains("Failure".ToUpperInvariant())))
                        {

                            await TrataSistemaOcupado(client);


                        }
                        await client.ExecuteCommandAsync("scroll");
                        await client.ExecuteCommandAsync($"display ont info by-sn {onu.Serial?.ToUpper()}");

                        int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 5000;
                        await Task.Delay(delayTime);
                        var response = (await client.ReadAsync()).Split('\r', '\n').ToList();
                        LogIntegracaoAtivacao(string.Join("\n", response));


                        if (response.Exists(p => p.ToUpperInvariant().Contains("F/S/P".ToUpperInvariant())))
                        {


                            // Control flag
                            var controlFlagLine = response.FirstOrDefault(l => l.Trim().StartsWith("Control flag"));
                            if (controlFlagLine != null)
                            {
                                var parts = controlFlagLine.Split(':');
                                if (parts.Length > 1)
                                {
                                    controlFlag = parts[1].Trim();
                                }
                            }

                            // Run state
                            var runStateLine = response.FirstOrDefault(l => l.Trim().StartsWith("Run state"));
                            if (runStateLine != null)
                            {
                                var parts = runStateLine.Split(':');
                                if (parts.Length > 1)
                                {
                                    runState = parts[1].Trim();
                                }
                            }

                            // Config state
                            var configStateLine = response.FirstOrDefault(l => l.Trim().StartsWith("Config state"));
                            if (configStateLine != null)
                            {
                                var parts = configStateLine.Split(':');
                                if (parts.Length > 1)
                                {
                                    configState = parts[1].Trim();
                                }
                            }



                        }


                        LogIntegracaoAtivacao(string.Join("\n", response));

                        if (runState.Equals("online", StringComparison.OrdinalIgnoreCase))
                        {
                            retorno.Status_Ope = true;
                        }

                        if(controlFlag.Equals("active", StringComparison.OrdinalIgnoreCase))
                        {
                            retorno.Status_Adm = true;
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
                return new GetStatusADMeOperacionalONU
                {
                    Status_Adm = false,
                    Status_Ope = false,
                    Motivo_Erro = ex.Message,

                };
            }

            return retorno;
        }

    }
}