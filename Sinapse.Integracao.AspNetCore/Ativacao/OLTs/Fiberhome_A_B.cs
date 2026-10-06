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
    /// <summary>
    ///     Implementação original (Firmware Version A e B).
    /// </summary>
    public class Fiberhome_A_B : IntegracaoAtivacao
    {
        private OLT olt;

        public Fiberhome_A_B(OLT olt)
        {
            this.olt = olt;
        }

        public async Task<List<AutorizarONUListModel>> ONUsPendentesRegistro()
        {
            var listaOnus = new List<AutorizarONUListModel>();
            //var retornoTask = Task.Run(async () =>
            //{
            //try
            //{
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
                    using (var client = new Client(this.olt.IpTL1, Convert.ToInt32(this.olt.PortaTL1.GetValueOrDefault(23)), new CancellationToken()))
                    {

                        if (client.IsConnected)
                        {
                            client.MillisecondReadDelay = 500;

                            Console.WriteLine("Cliente Conectado");
                            var logado = LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);

                            if (logado["STATUS"].Equals("COMPLD"))
                            {
                                // -=-=-=-=-=-=- Comando Para Listar ONUs -=-=-=-=-=-=-=-//
                                await client.WriteLineAsync($"LST-UNREGONU::OLTID={this.olt.IP}:CTAG::;");
                                //Thread.Sleep(1500);

                                var resposta = await client.ReadAsync();
                                try
                                {
                                    var mensagem = TratarRetornoListaONUsPendentesRegistro(resposta);
                                    if (mensagem["STATUS"].Equals("COMPLD"))
                                    {
                                        Console.WriteLine(mensagem["BLOCK RECORDS"]);
                                        foreach (var onu in mensagem["ONUs"] as List<Dictionary<string, string>>)
                                        {
                                            listaOnus.Add(new AutorizarONUListModel
                                            {
                                                Slot = onu["SLOTNO"],
                                                PON = onu["PONNO"],
                                                Mac_Serial = onu["MAC"],
                                                Num_OLT = olt.NumOLT,
                                                Id_OLT = olt.Id,
                                                Desc_OLT = olt.Descricao,
                                                Desc_ONU = onu["DT"]
                                                //Nro_Slot = onu["SLOTNO"],
                                                //Nro_Pon = onu["PONNO"],
                                                //Onu_Mac = onu["MAC"],
                                                //Onu_Loid = onu["LOID"],
                                                //Onu_Pwd = onu["PWD"],
                                                //Onu_Error = onu["ERROR"],
                                                //Onu_Authime = onu["AUTHTIME"],
                                                //Onu_Modelo = onu["DT"],
                                            });
                                        }
                                    }
                                }
                                catch
                                {
                                    var m = TratarMensagemPadrao(resposta);
                                    throw new EAnielException($"Status.: {RetornoErroTratado(m)}");
                                }

                                DeslogarServidor(client);
                            }
                            else
                            {
                                throw new EAnielException($"Status.: {RetornoErroTratado(logado)}");
                            }
                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }
                else
                {
                    using (var client = new Client(this.olt.IP, Convert.ToInt32(this.olt.PortaTelnet.GetValueOrDefault(23)), new CancellationToken()))
                    {

                        if (client.IsConnected)
                        {
                            client.MillisecondReadDelay = 500;
                            bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 1000);

                            await client.WriteLineAsync("cd gpononu;");
                            await client.WriteLineAsync($"show discovery slot all link all");

                            /*----- ONU Unauth Table ,SLOT=11 PON=2 ,ITEM=1-----
                            NO   TYPE       PHY_ID              PWD     SN LOID,    SN PWD
                            --   --------   ----------------    ------  ----------  -----
                            01      HG266     FHTT00266b15              hg26677715,
                             */

                            //TODO: Testar se é o de cima ou esses:
                            //show unauth list
                            //OU
                            //show unauth_discovery
                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }

            }



            return listaOnus;

            //});

            //return retornoTask.Result;
        }


        public async Task<List<object>> ListaOnusConectadas()
        {
            //var task = Task.Run(async () =>
            //{
            //var response = new Response<List<object>>();
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

                                var logado = LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                if (logado["STATUS"].Equals("COMPLD"))
                                {
                                    await client.WriteLineAsync($"LST-ONU::OLTID={this.olt.IP}:CTAG::;");
                                    var resposta = client.ReadAsync().Result;
                                    try
                                    {
                                        var m = TratarRetornoInformacaoHardwareONU(resposta);
                                        if (m["STATUS"].Equals("COMPLD"))
                                        {

                                            foreach (var onu in m["ONUs"] as List<Dictionary<string, string>>)
                                            {
                                                lista.Add(new
                                                {
                                                    OLT_ID = onu["OLTID"],
                                                    PON_ID = onu["PONID"],
                                                    ONU_NO = onu["ONUNO"],
                                                    ONU_NOME = onu["NAME"],
                                                    ONU_DESC = onu["DESC"],
                                                    ONU_ONUTYPE = onu["ONUTYPE"],
                                                    ONU_IP = onu["IP"],
                                                    ONU_AUTH = onu["AUTH"],
                                                    ONU_MAC = onu["MAC"],
                                                    ONU_LOID = onu["LOID"],
                                                    ONU_PWD = onu["PWD"],
                                                    ONU_VersaoSoftware = onu["SWVER"]

                                                });
                                            }

                                        }
                                    }
                                    catch (Exception)
                                    {
                                        var m = TratarMensagemPadrao(resposta);
                                        throw new EAnielException($"Status.: {RetornoErroTratado(m)}");
                                    }
                                    DeslogarServidor(client);
                                    //return lista;
                                }
                                else
                                {

                                    throw new EAnielException($"Status.: {RetornoErroTratado(logado)}");
                                }
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

                                await client.WriteLineAsync("cd gpononu;");
                                await client.WriteLineAsync("show online slot all link all");

                                /*ONLINE ONU TOTAL NUM = 1
                                ONUID   ONUTYPE         SN              PASSWORD    LOGICAL SN
                                -----   ------------    ------------    ----------  ---------------------
                                01      AN5506-04-B2G   FHTT00030405 ,
                                */
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
            //});

            //return task.Result;
            //return await task;
        }


        public async Task<Response> ReiniciarONU(ConfigurarONUModel onu)
        {
            //var task = Task.Run(async () =>
            //{
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

                                var logado = LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                if (logado["STATUS"].Equals("COMPLD"))
                                {
                                    if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.MAC)
                                    {
                                        await client.WriteLineAsync($"RESET-ONU::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::RESETTYPE=0;");
                                    }
                                    else if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.LOID)
                                    {
                                        await client.WriteLineAsync($"RESET-ONU::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::RESETTYPE=0;");
                                    }
                                    Thread.Sleep(3500);
                                    var resposta = TratarMensagemPadrao(client.ReadAsync().Result);
                                    if (resposta["STATUS"].Equals("COMPLD"))
                                    {
                                        response.result = true;
                                        response.message = $"Status.: {resposta["STATUS"]} \n Desc.: {resposta["CODIGO"]} - {resposta["DESCRICAO"]}";
                                    }
                                    else
                                    {
                                        response.result = false;
                                        response.message = $"Status.: {RetornoErroTratado(resposta)}";
                                    }

                                    if (response.result)
                                    {
                                        var retornoStatus = false;
                                        var tentativas = 0;

                                        while (!retornoStatus && tentativas <= 10)
                                        {
                                            await client.WriteLineAsync($"LST-ONUSTATE::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=MAC,ONUID={onu.Serial}:CTAG::;");

                                            Thread.Sleep(2500);
                                            var respostaStatus = TratarRetornoGetStatusONU(client.ReadAsync().Result);

                                            if (respostaStatus["STATUS"].Equals("COMPLD") && respostaStatus["STATUSONU"].Equals("UP"))
                                            {
                                                retornoStatus = true;
                                                break;
                                            }
                                            else
                                            {
                                                tentativas++;
                                                if (tentativas == 1) //Primeira tentativa aguarda 15 segundos para tentar novamente, tentativas seguintes aguarda 10 segundos
                                                    Thread.Sleep(15000);
                                                else
                                                    Thread.Sleep(10000);
                                            }

                                        }

                                        response.result = retornoStatus;
                                    }

                                    DeslogarServidor(client);
                                }
                                else
                                {
                                    response.result = false;
                                    response.message = $"Status.: {RetornoErroTratado(logado)}";
                                }
                            }
                            else
                            {
                                response.result = false;
                                response.message = "Erro ao estabelecer conexão.";
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

                                await client.WriteLineAsync("cd gpononu;");
                                await client.WriteLineAsync($"reset slot {onu.IdSlot} link {onu.IdPon} onulist {onu.OntID};");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("RESET ONU OK")))
                                    response.result = true;
                                //Retorno: reset onu ok! 

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
            //});

            //return task.Result;
        }

        public async Task<Response> DesautorizarONU(ConfigurarONUModel onu)
        {
            var task = Task.Run(async () =>
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
                        if (!String.IsNullOrEmpty(this.olt.IpTL1)) //Acesso TL1
                        {
                            using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                            {
                                if (client.IsConnected)
                                {
                                    client.MillisecondReadDelay = 500;

                                    Console.WriteLine("Cliente Conectado");
                                    var logado = LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                    if (logado["STATUS"].Equals("COMPLD"))
                                    {
                                        // -=-=-=-=-=-=-= Comando para Deletar ONUs =-=-=-=-=-=-
                                        /*
                                         * ONUIDTYPE TEM QUE SER IGUAL AO QUE FOI ADICIONADO A ONU O TIPO DE AUTENTICAÇÃO
                                         * */
                                        if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.MAC)
                                        {
                                            await client.WriteLineAsync($"DEL-ONU::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome}:CTAG::ONUIDTYPE=MAC,ONUID={onu.Mac};");
                                        }
                                        else if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.LOID)
                                        {
                                            await client.WriteLineAsync($"DEL-ONU::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome}:CTAG::ONUIDTYPE=LOID,ONUID={onu.Serial};");
                                        }
                                        //Thread.Sleep(1500);
                                        var resposta = client.ReadAsync().Result;
                                        var mensagem = TratarMensagemPadrao(resposta);

                                        if (mensagem["STATUS"].Equals("COMPLD"))
                                        {
                                            response.result = true;
                                            response.message = $"Status.: {mensagem["STATUS"]} \n Desc.: {mensagem["CODIGO"]} - {mensagem["DESCRICAO"]} \n";
                                        }
                                        else
                                        {
                                            response.result = false;
                                            response.message = $"Status.: {RetornoErroTratado(mensagem)}";
                                        }

                                    }
                                    else
                                    {
                                        response.result = false;
                                        response.message = $"Status.: {RetornoErroTratado(logado)}";

                                    }
                                }
                                else
                                {
                                    response.result = false;
                                    response.message = "Erro ao estabelecer conexão.";
                                }
                            }
                        }
                        else //Acesso CLI via Telnet
                        {
                            using (Client client = new Client(this.olt.IP, porta, new CancellationToken()))
                            {

                                if (client.IsConnected)
                                {
                                    client.MillisecondReadDelay = 500;
                                    bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 1000);

                                    await client.WriteLineAsync("cd epononu;");
                                    await client.WriteLineAsync($"set epon slot {onu.IdSlot} pon {onu.IdPon} sequence {onu.OntID} unauthorized;");

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
            });
            //return task.Result;
            return await task;
        }


        public Response AutorizarONU(ConfigurarONUModel onu, string scriptOLT, string scriptONU)
        {
            //var task = Task.Run(async () =>
            //{
            Response response = new Response();
            try
            {
                using (Client client = new Client(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), new CancellationToken()))
                {

                    if (client.IsConnected)
                    {
                        client.MillisecondReadDelay = 500;

                        Console.WriteLine("Cliente Conectado");
                        var logado = LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                        if (logado["STATUS"].Equals("COMPLD"))
                        {
                            var linhasScriptOLT = scriptOLT.Split('\n');
                            foreach (string linha in linhasScriptOLT)
                            {
                                client.WriteLineAsync(linha);
                                Thread.Sleep(1500);

                                var resposta = client.ReadAsync().Result;
                                var mensagem = TratarMensagemPadrao(resposta);

                                if (mensagem["STATUS"].Equals("COMPLD"))
                                {
                                    response.result = true;
                                    response.message = $"Status.: {mensagem["STATUS"]} \n Desc.: {mensagem["CODIGO"]} - {mensagem["DESCRICAO"]} \n";
                                    continue;
                                }
                                else
                                {
                                    response.result = false;
                                    response.message = $"Status.: {RetornoErroTratado(mensagem)}";
                                    break;
                                }
                            }

                            if (response.result)
                            {
                                client.WriteLineAsync($"LST-ONUSTATE::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=MAC,ONUID={onu.Serial}:CTAG::;");

                                Thread.Sleep(2500);
                                var respostaIdOnu = TratarRetornoGetIdONU(client.ReadAsync().Result);

                                if (respostaIdOnu["STATUS"].Equals("COMPLD"))
                                {
                                    Int32? idOnu = Convert.ToInt32(respostaIdOnu["ONUID"]);
                                    if (idOnu != null)
                                        onu.OntID = idOnu;
                                }
                            }


                            if (!String.IsNullOrEmpty(scriptONU))
                            {
                                var linhasScriptONU = scriptONU.Split(';');
                                foreach (string linha in linhasScriptONU)
                                {
                                    client.WriteLineAsync(linha);

                                    var resposta = client.ReadAsync().Result;
                                    var mensagem = TratarMensagemPadrao(resposta);

                                    if (mensagem["STATUS"].Equals("COMPLD"))
                                    {
                                        response.result = true;
                                        response.message = $"Status.: {mensagem["STATUS"]} \n Desc.: {mensagem["CODIGO"]} - {mensagem["DESCRICAO"]} \n";
                                        continue;
                                    }
                                    else
                                    {
                                        response.result = false;
                                        response.message = $"Status.: {RetornoErroTratado(mensagem)}";
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            response.result = false;
                            response.message = $"Status.: {RetornoErroTratado(logado)}";

                        }
                    }
                    else
                    {
                        response.result = false;
                        response.message = "Erro ao estabelecer conexão.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.result = false;
                response.exception = $" {ex.Message}";
            }
            return response;
            //});
            //return task.Result;
        }

        public async Task<List<Models.StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listSerial, Func<string, Task<string>> consultarOntId = null, List<(string, string)> listaAuxiliar = null)
        {
            /*LST-ONUSTATE::OLTID=olt-name,PONID=ponport_location[,ONUIDTYPE=id-type,
            ONUID=onu-index]:CTAG::;
            
             Example 1, query the status of the ONU with ONUID being aaa_bbb. The ONU is
            connected to the slot 3 - PON 1 port of the OLT whose IP address is 10.250.18.100.
            *Command
            LST-ONUSTATE::OLTID=10.250.18.100,PONID=NA-NA-3-1,ONUIDTYPE=LOID,
            ONUID=aaa_bbb:CTAG::;

            *Response Message
            FH_10.250.18.133 2010-10-27 11:03:48
            M CTAG COMPLD
            total_blocks=1
            block_number=1
            block_records=1
            list of ONU state
            -----------------------------------------------------------------
            ONUID AdminState OperStat AUTH AUTHINFO ONUIP LASTOFFTIME
            5       UP          UP    LOID aaa_bbb   --   0000-00-00 00:00:00
            -----------------------------------------------------------------*/
            List<Models.StatusONU> retorno = new List<Models.StatusONU>();
            //var task = Task.Run(async () =>
            //{
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
                        using (var client = new RemoteConnection(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), this.olt.LoginTL1, this.olt.SenhaTL1, Protocol.Telnet, true))
                        {
                            await client.ConnectTL1();
                            if (client.IsConnected())
                            {
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                //client.MillisecondReadDelay = 500;
                                await Task.Delay(2000);

                                //var logado = await LoginFiberhomeAsyncTL1Teste(client, this.olt.LoginTL1, this.olt.SenhaTL1);

                                //if (logado["STATUS"].Equals("COMPLD"))
                                //if (logado.result == true)
                                //{
                                /*LST-ONUSTATE::OLTID=olt-name,PONID=ponport_location[,ONUIDTYPE=id-type,
                                ONUID=onu-index]:CTAG::;

                                 Example 1, query the status of the ONU with ONUID being aaa_bbb. The ONU is
                                connected to the slot 3 - PON 1 port of the OLT whose IP address is 10.250.18.100.
                                *Command
                                LST-ONUSTATE::OLTID=10.250.18.100,PONID=NA-NA-3-1,ONUIDTYPE=LOID,
                                ONUID=aaa_bbb:CTAG::;

                                *Response Message
                                FH_10.250.18.133 2010-10-27 11:03:48
                                M CTAG COMPLD
                                total_blocks=1
                                block_number=1
                                block_records=1
                                list of ONU state
                                -----------------------------------------------------------------
                                ONUID AdminState OperStat AUTH AUTHINFO ONUIP LASTOFFTIME
                                5       UP          UP    LOID aaa_bbb   --   0000-00-00 00:00:00
                                -----------------------------------------------------------------*/

                                await client.ExecuteCommandAsync($"LST-ONUSTATE::OLTID={this.olt.IP},PONID=NA-NA-{numSlot}-{numPON}:CTAG::;");
                                //Thread.Sleep(3500);

                                await Task.Delay(4000);

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

                                /*-----------------------------------------------------------------
                                 ONUID AdminState OperStat AUTH AUTHINFO ONUIP LASTOFFTIME
                                 5       UP          UP    LOID aaa_bbb   --   0000-00-00 00:00:00
                                 -----------------------------------------------------------------*/

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("DENY".ToUpperInvariant())))
                                {
                                    await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                    throw new EAnielException("Falha ao executar o comando na OLT. Por favor, tente novamente mais tarde.");
                                }

                                try
                                {
                                    var m = TratarRetornoGetStatusONUsByPON(resposta, listSerial, consultarOntId);
                                    if (m.ContainsKey("ONUs") && m["ONUs"] is List<Models.StatusONU> onuList && onuList.Count > 0)
                                    {
                                        retorno = m["ONUs"] as List<Models.StatusONU>;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    /*var m = TratarMensagemPadrao(resposta);
                                    throw new EAnielException($"Status.: {RetornoErroTratado(m)}"); */
                                    LogIntegracaoAtivacao(ex);
                                    throw new EAnielException(ex.Message);
                                }

                                await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");


                                //DeslogarServidor(client);
                                /* }
                                 else
                                 {
                                     throw new EAnielException($"Status.: Usuário ou senha inválidos.");
                                 }*/
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

                                await client.WriteLineAsync("cd gpononu;");
                                await client.WriteLineAsync($"show online slot {numSlot} link {numPON}");

                                /*ONLINE ONU TOTAL NUM = 1
                                ONUID   ONUTYPE         SN              PASSWORD    LOGICAL SN
                                -----   ------------    ------------    ----------  ---------------------
                                01      AN5506-04-B2G   FHTT00030405 ,
                                */

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                foreach (string serial in listSerial)
                                {
                                    if (resposta.Exists(p => p.ToUpperInvariant().Contains("serial")))
                                    {
                                        retorno.Add(new Models.StatusONU
                                        {
                                            Serial = serial,
                                            Status = Sinapse.Model.AspNetCore.enumerados.StatusONU.Online
                                        });
                                    }
                                    else
                                    {
                                        retorno.Add(new Models.StatusONU
                                        {
                                            Serial = serial,
                                            Status = Sinapse.Model.AspNetCore.enumerados.StatusONU.Offline
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
            }
            catch (Exception ex)
            {
                throw new EAnielException(ex.Message);
            }
            //});
            return retorno;
        }

        public async Task<DiagnosticoONU> DiagnosticoONU(ConfigurarONUModel onu)
        {
            /*LST-OMDDM::ONUIP=onu-name|OLTID=olt-name[,PONID=ponport_location][,
            ONUIDTYPE=id-type,ONUID=onu-index][,PORTID=lanport_index][,
            PEERFLAG=flag]:CTAG::;
            
             Example 1, query the optical module information of the ONU (having no
            management IP address) with ONUID being aa_bbb_ccc_111_222. The ONU is
            connected to the slot 3 - PON 1 port of the OLT whose IP address is 10.250.18.100.
            *Command
            LST-OMDDM::OLTID=10.250.18.100,PONID=NA-NA-3-1,ONUIDTYPE=LOID,
            ONUID=aaa_bbb_ccc_111_222:CTAG::;

             *Response Message
             FH_10.250.18.133 2010-10-27 11:05:05
            M CTAG COMPLD
            total_blocks=1
            block_number=1
            block_records=1
            List of Optical Power Info
            -----------------------------------------------------------
            ONUID RxPower RxPowerR TxPower TxPowerR CurrTxBias CurrTxBiasR Temperature TemperatureR Voltage VoltageR PTxPower PRxPower
            5       -9.76 Normal    1.76    Normal  15.80       Normal      62.37       Normal      3.40    Normal      --      --
            -----------------------------------------------------------*/
            DiagnosticoONU retorno = new DiagnosticoONU();
            //var task = Task.Run(async () =>
            //{
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

                                var logado = LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                if (logado["STATUS"].Equals("COMPLD"))
                                {
                                    if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.MAC)
                                    {
                                        await client.WriteLineAsync($"LST-OMDDM::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::;");
                                    }
                                    else if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.LOID)
                                    {
                                        await client.WriteLineAsync($"LST-OMDDM::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::;");
                                    }

                                    var resposta = client.ReadAsync().Result;
                                    try
                                    {
                                        var m = TratarRetornoDiagnosticoONU(client.ReadAsync().Result);
                                        if (m["STATUS"].Equals("COMPLD"))
                                        {
                                            retorno = m["ONU"] as DiagnosticoONU;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        var m = TratarMensagemPadrao(resposta);
                                        throw new EAnielException($"Status.: {RetornoErroTratado(m)}");
                                    }

                                    DeslogarServidor(client);
                                }
                                else
                                {
                                    throw new EAnielException($"Status.: {RetornoErroTratado(logado)}");
                                }
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

                                await client.WriteLineAsync("cd gpononu;");
                                await client.WriteLineAsync($"show optic_module slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID}");

                                //TODO: Ver qual comando está certo, o de cima ou esse:
                                //cd epononu
                                //"show epon slot {onu.Slot.NumSlot} pon {onu.PON.Num_PON} onu {onu.OntID} optInfo"                                                               

                                /*----- ONU OPTIC MODULE PAR INFO 11.2.16-----
                                NAME VALUE UNIT
                                ---------------------------------------
                                TYPE : 20 (KM)
                                TEMPERATURE : 33.07 ('C)
                                VOLTAGE : 3.28 (V)
                                BIAS CURRENT : 7.50 (mA)
                                SEND POWER : -0.10 (Dbm)
                                RECV POWER : -13.07 (Dbm)
                                OLT RECV POWER : -16.17 (Dbm)
                                 */

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("TEMPERATURE")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("TEMPERATURE"));
                                    var campos = linha.Split(' ', ':').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Temperatura = String.Format("{0} °C", campos[2]);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("VOLTAGE")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("VOLTAGE"));
                                    var campos = linha.Split(' ', ':').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Voltagem = String.Format("{0} V", campos[2]);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("SEND POWER")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("SEND POWER"));
                                    var campos = linha.Split(' ', ':').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Tx = String.Format("{0} Tx", campos[3]);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("RECV POWER")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("RECV POWER"));
                                    var campos = linha.Split(' ', ':').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Rx = String.Format("{0} Rx", campos[3]);
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
            //});

            return retorno;
        }

        public async Task<Response> ResetarONU(ConfigurarONUModel onu)
        {
            //var task = Task.Run(async () =>
            // {
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

                                var logado = LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                if (logado["STATUS"].Equals("COMPLD"))
                                {
                                    /*RESTORE-DEFAULTCFG::ONUIP=onu_name|OLTID=olt_name[,
                                    PONID=ponport_location,ONUIDTYPE=id-type,ONUID=onu_index]:CTAG::;

                                    Example, restore the ONU (having no management IP address) with MAC
                                    address being FHTT01e821a0 to factory default settings. The ONU is connected to
                                    the slot 18 - PON 4 port of the OLT whose IP address is 10.78.191.100.
                                    *Command
                                    RESTORE-DEFAULTCFG::OLTID=10.78.191.100,PONID=NA-NA-18-4,ONUIDTYPE=MAC,
                                    ONUID=FHTT01e821a0:CTAG::;
                                    *Response Message
                                    FH_10.82.25.73 2013-02-22 5:16:32 PM M CTAG COMPLD EN=0 ENDESC=No error;*/

                                    if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.MAC)
                                    {
                                        client.WriteLineAsync($"RESTORE-DEFAULTCFG::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::;");
                                    }
                                    else if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.LOID)
                                    {
                                        client.WriteLineAsync($"RESTORE-DEFAULTCFG::OLTID={this.olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::;");
                                    }
                                    Thread.Sleep(2500);
                                    var resposta = TratarMensagemPadrao(client.ReadAsync().Result);
                                    if (resposta["STATUS"].Equals("COMPLD"))
                                    {
                                        response.result = true;
                                        response.message = $"Status.: {resposta["STATUS"]} \n Desc.: {resposta["CODIGO"]} - {resposta["DESCRICAO"]}";
                                    }
                                    else
                                    {
                                        response.result = false;
                                        response.message = $"Status.: {RetornoErroTratado(resposta)}";
                                    }
                                    DeslogarServidor(client);
                                }
                                else
                                {
                                    response.result = false;
                                    response.message = $"Status.: {RetornoErroTratado(logado)}";
                                }
                            }
                            else
                            {
                                response.result = false;
                                response.message = "Erro ao estabelecer conexão.";
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
                                // bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 1000);


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
            //  });


        }

        public async Task<InfoConfigONU> GetConfigEquipamento(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
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

                                var logado = LoginFiberhomeAsync(client, olt.Login, olt.Senha);
                                if (logado["STATUS"].Equals("COMPLD"))
                                {
                                    if (onu.Tipo_Identificacao == Sinapse.Model.AspNetCore.enumerados.ONU_Tipo_Identificacao.MAC)
                                    {

                                        await client.WriteLineAsync($"LST-DEVINFO::OLTID={olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::;");
                                    }
                                    else
                                    {
                                        await client.WriteLineAsync($"LST-DEVINFO::OLTID={olt.IP},PONID={onu.PonIDFiberhome},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::;");
                                    }
                                    var resposta = client.ReadAsync().Result;
                                    try
                                    {
                                        var m = TratarRetornoInformacaoHardware(resposta);
                                        if (m["STATUS"].Equals("COMPLD"))
                                        {
                                            foreach (var hardware in m["HARDWARE"] as List<Dictionary<string, string>>)
                                            {
                                                retorno = new InfoHardwareONU
                                                {
                                                    Id_Onu = onu.Id,
                                                    Nome = hardware["DEVNAME"],
                                                    IP = hardware["DEVIP"],
                                                    Modelo = hardware["DT"],
                                                    Versao_Software = hardware["DEVER"],
                                                    Memoria = hardware["MEM"],
                                                    CPU = hardware["CPU"],
                                                    Temperatura = hardware["TEMPERATURE"]

                                                };
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        var m = TratarMensagemPadrao(resposta);
                                        throw new EAnielException($"Status.: {RetornoErroTratado(m)}");
                                    }
                                    DeslogarServidor(client);
                                }
                                else
                                {
                                    throw new EAnielException($"Status.: {RetornoErroTratado(logado)}");
                                }
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

            return retorno;
        }

        #region metodos-privados

        private Dictionary<string, string> LoginFiberhomeAsync(Client client, string login, string senha)
        {
            //client.WriteLine(";\r");
            client.WriteLineAsync($"LOGIN:::CTAG::UN={login},PWD={senha};");
            String resultado = client.ReadAsync().Result;
            return TratarMensagemPadrao(resultado);

        }

        private Dictionary<string, object> TratarRetornoInformacaoHardware(string resposta)
        {
            var lista = resposta.Split('\r', '\n');
            var mensagem = new Dictionary<string, object>();
            mensagem.Add("LOG", lista[3].Trim());
            mensagem.Add("STATUS", lista[5].Split(' ', ' ').GetValue(lista[5].Split(' ', ' ').Length - 1));
            mensagem.Add("TOTAL BLOCKS", lista[7].Trim().Split('=')[1]);
            mensagem.Add("BLOCK NUMBER", lista[9].Trim().Split('=')[1]);
            mensagem.Add("BLOCK RECORDS", lista[11].Trim().Split('=')[1]);

            var hardware = new List<Dictionary<string, string>>();

            for (int i = 20; i < lista.Length; i++)
            {
                var dispositivo = new Dictionary<string, string>();
                if (lista[i].Contains("-----"))
                {
                    break;
                }

                if (String.IsNullOrEmpty(lista[i]))
                {
                    continue;
                }

                dispositivo.Add("DEVNAME", lista[i].Split('\t')[0]);
                dispositivo.Add("DEVIP", lista[i].Split('\t')[1]);
                dispositivo.Add("DT", lista[i].Split('\t')[2]);
                dispositivo.Add("DEVER", lista[i].Split('\t')[3]);
                dispositivo.Add("MEM", lista[i].Split('\t')[4]);
                dispositivo.Add("CPU", lista[i].Split('\t')[5]);
                dispositivo.Add("TEMPERATURE", lista[i].Split('\t')[6]);


                hardware.Add(dispositivo);
            }

            mensagem.Add("HARDWARE", hardware);
            return mensagem;
        }
        private Dictionary<string, string> TratarMensagemPadrao(string resultado)
        {
            var lista = resultado.Split('\r', '\n');
            var mensagem = new Dictionary<string, string>();
            mensagem.Add("LOG", lista[3].Trim());
            mensagem.Add("STATUS", lista[5].Split(' ', ' ')[3]);
            mensagem.Add("CODIGO", lista[7].Trim().Split(' ')[0].Split('=')[1]);
            mensagem.Add("DESCRICAO", lista[7].Trim().Replace(lista[7].Trim().Split(' ', ' ')[0], "").Trim().Contains("  ") ? lista[7].Trim().Replace(lista[7].Trim().Split(' ', ' ')[0], "").Trim().Substring(0, lista[7].Trim().Replace(lista[7].Trim().Split(' ', ' ')[0], "").Trim().IndexOf("  ")).Split('=')[1] : lista[7].Trim().Replace(lista[7].Trim().Split(' ', ' ')[0], "").Trim().Split('=')[1]);
            mensagem.Add("ARGUMENTO", lista[7].Trim().Replace(lista[7].Trim().Split(' ', ' ')[0], "").Trim().Contains("  ") ? lista[7].Trim().Replace(lista[7].Trim().Split(' ', ' ')[0], "").Trim().Replace(lista[7].Trim().Replace(lista[7].Trim().Split(' ', ' ')[0], "").Trim().Substring(0, lista[7].Trim().Replace(lista[7].Trim().Split(' ', ' ')[0], "").Trim().IndexOf("  ")), "").Trim() : " ");


            return mensagem;
        }

        private Dictionary<string, object> TratarRetornoListaONUsPendentesRegistro(string resposta)
        {
            var lista = resposta.Split('\r', '\n');
            var mensagem = new Dictionary<string, object>();
            mensagem.Add("LOG", lista[3].Trim());
            mensagem.Add("STATUS", lista[5].Split(' ', ' ').GetValue(lista[5].Split(' ', ' ').Length - 1));
            mensagem.Add("TOTAL BLOCKS", lista[7].Trim().Split('=')[1]);
            mensagem.Add("BLOCK NUMBER", lista[9].Trim().Split('=')[1]);
            mensagem.Add("BLOCK RECORDS", lista[11].Trim().Split('=')[1]);

            var listaOnus = new List<Dictionary<string, string>>();

            for (int i = 20; i < lista.Length; i++)
            {
                var onu = new Dictionary<string, string>();
                if (lista[i].Contains("-----"))
                {
                    break;
                }

                if (String.IsNullOrEmpty(lista[i]))
                {
                    continue;
                }

                onu.Add("SLOTNO", lista[i].Split('\t')[0]);
                onu.Add("PONNO", lista[i].Split('\t')[1]);
                onu.Add("MAC", lista[i].Split('\t')[2]);
                onu.Add("LOID", lista[i].Split('\t')[3]);
                onu.Add("PWD", lista[i].Split('\t')[4]);
                onu.Add("ERROR", lista[i].Split('\t')[5]);
                onu.Add("AUTHTIME", lista[i].Split('\t')[6]);
                onu.Add("DT", lista[i].Split('\t')[7]);

                listaOnus.Add(onu);
                Console.WriteLine(lista[i]);
            }

            mensagem.Add("ONUs", listaOnus);

            return mensagem;
        }

        private String RetornoErroTratado(Dictionary<string, string> mensagem)
        {
            String retorno;
            if (mensagem["CODIGO"].ToString().Equals("IRNE"))
            {
                retorno = "O recurso não existe. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("IANE"))
            {
                retorno = "O alarme não existe. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("IMP"))
            {
                retorno = "Parâmetro ausente. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("IIPF"))
            {
                retorno = "Formato de parâmetro inválido. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("IIPE"))
            {
                retorno = "Erro de parâmetro de entrada. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("DDNS"))
            {
                retorno = "O dispositivo pode não suportar esta operação";
            }
            else if (mensagem["CODIGO"].ToString().Equals("DDOF"))
            {
                retorno = "Operação do dispositivo falhou. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("DDB"))
            {
                retorno = "Dispositivo está ocupado. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("SENS"))
            {
                retorno = "O EMS pode não suportar esta operação. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("SEOF"))
            {
                retorno = "Falha na operação do EMS. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("EEEH"))
            {
                retorno = "A exceção do EMS. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("TUB"))
            {
                retorno = "Usuário está ocupado. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("TUT"))
            {
                retorno = "Usuário está testando. ";
            }
            else if (mensagem["CODIGO"].ToString().Equals("TTMB"))
            {
                retorno = "O Módulo de teste está ocupado. ";
            }
            else
            {
                retorno = $"{mensagem["STATUS"]} \n Desc.: {mensagem["CODIGO"]} - {mensagem["DESCRICAO"]} \n Motivo.: {mensagem["ARGUMENTO"]}";
            }

            return retorno;

        }

        private void DeslogarServidor(Client client)
        {
            try
            {
                client.TerminatedReadAsync("LOGOUT:::CTAG::;");

            }
            catch (Exception ex)
            {
                //CONSTANTES.log4Net.Error(ex);
            }
        }


        private Dictionary<string, object> TratarRetornoInformacaoHardwareONU(string resposta)
        {
            var lista = resposta.Split('\r', '\n');
            var mensagem = new Dictionary<string, object>();
            mensagem.Add("LOG", lista[3].Trim());
            mensagem.Add("STATUS", lista[5].Split(' ', ' ').GetValue(lista[5].Split(' ', ' ').Length - 1));
            mensagem.Add("TOTAL BLOCKS", lista[7].Trim().Split('=')[1]);
            mensagem.Add("BLOCK NUMBER", lista[9].Trim().Split('=')[1]);
            mensagem.Add("BLOCK RECORDS", lista[11].Trim().Split('=')[1]);

            var onu = new List<Dictionary<string, object>>();

            for (int i = 20; i < lista.Length; i++)
            {
                var dispositivo = new Dictionary<string, object>();
                if (lista[i].Contains("-----"))
                {
                    break;
                }

                if (String.IsNullOrEmpty(lista[i]))
                {
                    continue;
                }

                dispositivo.Add("OLTID", lista[i].Split('\t')[0]);
                dispositivo.Add("PONID", lista[i].Split('\t')[1]);
                dispositivo.Add("ONUNO", lista[i].Split('\t')[2]);
                dispositivo.Add("NAME", lista[i].Split('\t')[3]);
                dispositivo.Add("DESC", lista[i].Split('\t')[4]);
                dispositivo.Add("ONUTYPE", lista[i].Split('\t')[5]);
                dispositivo.Add("IP", lista[i].Split('\t')[6]);
                dispositivo.Add("AUTH", lista[i].Split('\t')[7]);
                dispositivo.Add("MAC", lista[i].Split('\t')[8]);
                dispositivo.Add("LOID", lista[i].Split('\t')[9]);
                dispositivo.Add("PWD", lista[i].Split('\t')[10]);
                dispositivo.Add("SWVER", lista[i].Split('\t')[11]);


                onu.Add(dispositivo);
            }

            mensagem.Add("ONUs", onu);
            return mensagem;
        }

        private Dictionary<string, object> TratarRetornoDiagnosticoONU(string resposta)
        {
            var lista = resposta.Split('\r', '\n');
            var mensagem = new Dictionary<string, object>();
            mensagem.Add("LOG", lista[3].Trim());
            mensagem.Add("STATUS", lista[5].Split(' ', ' ').GetValue(lista[5].Split(' ', ' ').Length - 1));
            mensagem.Add("TOTAL BLOCKS", lista[7].Trim().Split('=')[1]);
            mensagem.Add("BLOCK NUMBER", lista[9].Trim().Split('=')[1]);
            mensagem.Add("BLOCK RECORDS", lista[11].Trim().Split('=')[1]);

            var onu = new List<Dictionary<string, object>>();

            /*-----------------------------------------------------------
            ONUID RxPower RxPowerR TxPower TxPowerR CurrTxBias CurrTxBiasR Temperature TemperatureR Voltage VoltageR PTxPower PRxPower
            5       -9.76 Normal    1.76    Normal  15.80       Normal      62.37       Normal      3.40    Normal      --      --
            -----------------------------------------------------------*/

            for (int i = 20; i < lista.Length; i++)
            {
                var dispositivo = new Dictionary<string, object>();
                if (lista[i].Contains("-----"))
                {
                    break;
                }

                if (String.IsNullOrEmpty(lista[i]))
                {
                    continue;
                }
                var diagnostico = new DiagnosticoONU
                {
                    Rx = lista[i].Split('\t')[1],
                    Tx = lista[i].Split('\t')[3],
                    Temperatura = lista[i].Split('\t')[7],
                    Voltagem = lista[i].Split('\t')[9]
                };
                dispositivo.Add("ONUID", lista[i].Split('\t')[0]);
                dispositivo.Add("RxPower", lista[i].Split('\t')[1]);
                dispositivo.Add("RxPowerR", lista[i].Split('\t')[2]);
                dispositivo.Add("TxPower", lista[i].Split('\t')[3]);
                dispositivo.Add("TxPowerR", lista[i].Split('\t')[4]);
                dispositivo.Add("CurrTxBias", lista[i].Split('\t')[5]);
                dispositivo.Add("CurrTxBiasR", lista[i].Split('\t')[6]);
                dispositivo.Add("Temperature", lista[i].Split('\t')[7]);
                dispositivo.Add("TemperatureR", lista[i].Split('\t')[8]);
                dispositivo.Add("Voltage", lista[i].Split('\t')[9]);
                dispositivo.Add("VoltageR", lista[i].Split('\t')[10]);
                dispositivo.Add("PTxPower", lista[i].Split('\t')[11]);
                dispositivo.Add("PRxPower", lista[i].Split('\t')[12]);


                onu.Add(dispositivo);
            }

            mensagem.Add("ONU", onu);
            return mensagem;
        }

        private Dictionary<string, object> TratarRetornoGetStatusONUsByPON(List<string> resposta, List<string> listSerial, Func<string, Task<string>> consultarOntId = null)
        {
            var mensagem = new Dictionary<string, object>();
            var onu = new List<Models.StatusONU>();
            /*
             - Exemplo de retorno da OLT:

              FH_187.95.224.18 2025-03-12 09:16:37

                    M  CTAG COMPLD

                       total_blocks=1

                       block_number=1

                       block_records=9



                    list of ONU state

                    --------------------------------------------------------------------------------

                    ONUID	AdminState	OperState	AUTH	AUTHINFO	ONUIP	LASTOFFTIME

                    1	UP	UP	MAC	FHTT9b9f5160	--	2025-03-06 18:54:23

                    3	UP	UP	MAC	ALCLfceb2029	--	2024-12-24 11:23:28

                    4	UP	UP	MAC	FHTT99663a00	--	2025-01-30 17:14:58

                    5	UP	UP	MAC	FHTT98d49c70	--	2024-07-21 11:06:44

                    6	UP	UP	MAC	HWTC8f7389a7	--	2025-03-11 21:30:22

                    7	UP	UP	MAC	FHTT920ae880	--	2025-01-27 17:41:34

                    8	UP	UP	MAC	FHTT9e510388	--	2025-02-23 21:55:59

                    9	UP	UP	MAC	FHTT9aa79aa0	--	2025-02-06 12:31:51

                    10	UP	UP	MAC	FHTT9c0aaf20	--	2025-03-04 15:36:50

                    --------------------------------------------------------------------------------
             */

            foreach (var serial in listSerial)
            {
                //Procura se o serial consta em alguma linha da resposta da OLT
                var linha = resposta.FirstOrDefault(p => p.Contains(serial.ToUpperInvariant(), StringComparison.InvariantCultureIgnoreCase));

                if (!String.IsNullOrEmpty(linha))
                {
                    /*Se o serial constar na linha, os campos da linha 
                    serão divididos para podermos coletar e checar os
                    valores
                    */
                    var campos = linha.Split(' ', '\t').ToList();

                    //Verifica status do equipamento (AdminState)
                    if (campos.ElementAtOrDefault(1).ToUpperInvariant().Contains("UP".ToUpperInvariant()))
                    {
                        var novoDispositivo = new Models.StatusONU
                        {
                            Serial = serial,
                            Status = Model.AspNetCore.enumerados.StatusONU.Online
                        };

                        onu.Add(novoDispositivo);
                    }
                    else
                    {
                        var novoDispositivo = new Models.StatusONU
                        {
                            Serial = serial,
                            Status = Model.AspNetCore.enumerados.StatusONU.Offline
                        };

                        onu.Add(novoDispositivo);
                    }
                }
                else
                {
                    /*
                     Há casos onde o campo AUTH possui o valor PASSWORD
                     ñão exibindo o MAC da ONU, apenas alguma palavra 
                     descritiva (como DEFAULT ou Fiberhome).
                     Nesses casos, procuraremos a ONU pelo ID
                     */
                    linha = resposta.FirstOrDefault(p => p.Contains("PASSWORD".ToUpperInvariant(), StringComparison.InvariantCultureIgnoreCase));

                    if (!String.IsNullOrEmpty(linha))
                    {
                        var onuID = consultarOntId(serial).Result.ToString();
                        var campos = linha.Split(' ', '\t').ToList();

                        //Se o ONT_ID na TB_ONU não estiver vazio e constar na linha
                        //Verificaremos o status da ONU.
                        //Se não constar, significa que a ONU está indisponível.
                        if (!string.IsNullOrEmpty(onuID) &&
                           (campos.ElementAtOrDefault(0).ToUpperInvariant().Contains(onuID.ToUpperInvariant())))
                        {
                            if (campos.ElementAtOrDefault(1).ToUpperInvariant().Contains("UP".ToUpperInvariant()))
                            {
                                var novoDispositivo = new Models.StatusONU
                                {
                                    Serial = serial,
                                    Status = Model.AspNetCore.enumerados.StatusONU.Online
                                };

                                onu.Add(novoDispositivo);
                            }
                            else
                            {
                                var novoDispositivo = new Models.StatusONU
                                {
                                    Serial = serial,
                                    Status = Model.AspNetCore.enumerados.StatusONU.Offline
                                };

                                onu.Add(novoDispositivo);
                            }

                        }
                    }
                    else
                    {
                        var novoDispositivo = new Models.StatusONU
                        {
                            Serial = serial,
                            Status = Model.AspNetCore.enumerados.StatusONU.Indisponivel
                        };

                        onu.Add(novoDispositivo);
                    }

                }

            }


            mensagem.Add("ONUs", onu);
            return mensagem;
        }

        private Dictionary<string, object> TratarRetornoGetStatusONU(string resposta)
        {
            var lista = resposta.Split('\r', '\n');
            var mensagem = new Dictionary<string, object>();
            mensagem.Add("LOG", lista[3].Trim());
            mensagem.Add("STATUS", lista[5].Split(' ', ' ').GetValue(lista[5].Split(' ', ' ').Length - 1));
            mensagem.Add("TOTAL BLOCKS", lista[7].Trim().Split('=')[1]);
            mensagem.Add("BLOCK NUMBER", lista[9].Trim().Split('=')[1]);
            mensagem.Add("BLOCK RECORDS", lista[11].Trim().Split('=')[1]);

            var StatusOnu = String.Empty;
            /*-----------------------------------------------------------------
             ONUID AdminState OperStat AUTH AUTHINFO ONUIP LASTOFFTIME
             5       UP          UP    LOID aaa_bbb   --   0000-00-00 00:00:00
             -----------------------------------------------------------------*/

            for (int i = 20; i < lista.Length; i++)
            {
                if (lista[i].Contains("-----"))
                {
                    break;
                }

                if (String.IsNullOrEmpty(lista[i]))
                {
                    continue;
                }
                StatusOnu = lista[i].Split('\t')[2]; //OperStat = STATUS                                  
            }

            mensagem.Add("STATUSONU", StatusOnu);
            return mensagem;
        }

        private Dictionary<string, object> TratarRetornoGetIdONU(string resposta)
        {
            var lista = resposta.Split('\r', '\n');
            var mensagem = new Dictionary<string, object>();
            mensagem.Add("LOG", lista[3].Trim());
            mensagem.Add("STATUS", lista[5].Split(' ', ' ').GetValue(lista[5].Split(' ', ' ').Length - 1));
            mensagem.Add("TOTAL BLOCKS", lista[7].Trim().Split('=')[1]);
            mensagem.Add("BLOCK NUMBER", lista[9].Trim().Split('=')[1]);
            mensagem.Add("BLOCK RECORDS", lista[11].Trim().Split('=')[1]);

            Int32? IdOnu = null;
            /*-----------------------------------------------------------------
             ONUID AdminState OperStat AUTH AUTHINFO ONUIP LASTOFFTIME
             5       UP          UP    LOID aaa_bbb   --   0000-00-00 00:00:00
             -----------------------------------------------------------------*/

            for (int i = 20; i < lista.Length; i++)
            {
                if (lista[i].Contains("-----"))
                {
                    break;
                }

                if (String.IsNullOrEmpty(lista[i]))
                {
                    continue;
                }
                IdOnu = Convert.ToInt32(lista[i].Split('\t')[0]); //ONUID                                  
            }

            mensagem.Add("ONUID", IdOnu);
            return mensagem;
        }

        public Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU)
        {
            throw new NotImplementedException();
        }

        private void LogIntegracaoAtivacao(string msg)
        {
            LogManager.GetLogger("IntegracaoOLT").Info($"[FIBERHOME_A1] - {msg}");
        }

        private void LogIntegracaoAtivacao(Exception ex)
        {
            LogManager.GetLogger("IntegracaoOLT").Error($"[FIBERHOME_A1] - {ex.Message}", ex);
        }
        #endregion
    }
}
