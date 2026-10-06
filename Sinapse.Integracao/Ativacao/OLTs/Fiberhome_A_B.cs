using Aniel.Connect.Core;
using log4net;
using NHibernate;
using NHibernate.Hql.Ast;
using PrimS.Telnet;
using Renci.SshNet;
using Sinapse.Core;
using Sinapse.DAO;
using Sinapse.Integracao.Ativacao.Interfaces;
using Sinapse.Integracao.Ativacao.Models;
using Sinapse.Model;
using Sinapse.Model.enumerados;
using Sinapse.Util.RemoteConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Sinapse.Integracao.Ativacao.OLTs
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

        public async Task<List<AutorizarONUListModel>> ONUsPendentesRegistro(string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
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
                    using (var client = new RemoteConnection(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), this.olt.LoginTL1, this.olt.SenhaTL1, Protocol.Telnet, true))
                    {
                        await client.ConnectTL1();
                        if (client.IsConnected())
                        {


                            // -=-=-=-=-=-=- Comando Para Listar ONUs -=-=-=-=-=-=-=-//
                            await client.ExecuteCommandAsync($"LST-UNREGONU::OLTID={this.olt.IP}:CTAG::;");
                            var idLog = enviarLog($"[ONUsPendentesRegistro] [FIBERHOME AB] -> {this.olt.Descricao}", numUsuario, numOs, null);
                            //Thread.Sleep(1500);
                            int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 6000;
                            await Task.Delay(delayTime);

                            //var resposta = await client.ReadAsync();
                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            LogIntegracaoAtivacao(string.Join("\n", resposta));
                            await enviarLog(string.Join("\n", resposta), numUsuario, numOs, idLog.Result);

                            bool prosseguir = false;

                            foreach (var linha in resposta)
                            {
                                //Encontra a linha com a lista das onus disponíveis para provisionamento
                                if (linha.ToUpperInvariant().Contains("List of unreg onu information".ToUpperInvariant()))
                                {
                                    prosseguir = true;
                                    continue;
                                }

                                //Pula o cabeçalho que contém o tracejado -----
                                if (prosseguir && !string.IsNullOrWhiteSpace(linha) && !linha.Contains("----"))
                                {
                                    //Pula o nome das colunas
                                    if (!linha.ToUpperInvariant().Contains("SLOTNO".ToUpperInvariant()) && !linha.ToUpperInvariant().Contains("PONNO".ToUpperInvariant()))
                                    {
                                        //Formata as colunas e coleta os dados das ONUs
                                        var colunas = linha.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                                        var onuMac = colunas.ElementAtOrDefault(2);
                                        string onuTipo = colunas.ElementAtOrDefault(8);

                                        if (!String.IsNullOrEmpty(onuMac) && onuMac.ToUpperInvariant().Contains("ALCL".ToUpperInvariant()))
                                        {
                                            if (colunas.ElementAtOrDefault(9) != null && colunas.ElementAtOrDefault(10) == null)
                                            {
                                                onuTipo = $"{colunas.ElementAtOrDefault(8)} {colunas.ElementAtOrDefault(9)}";
                                            }
                                            else if (colunas.ElementAtOrDefault(9) != null && colunas.ElementAtOrDefault(10) != null)
                                            {
                                                onuTipo = $"{colunas.ElementAtOrDefault(8)} {colunas.ElementAtOrDefault(9)} {colunas.ElementAtOrDefault(10)}";
                                            }
                                        }

                                        if (colunas.Count >= 8)
                                        {
                                            listaOnus.Add(new AutorizarONUListModel
                                            {
                                                Slot = colunas.ElementAtOrDefault(0),
                                                PON = colunas.ElementAtOrDefault(1),
                                                Mac_Serial = colunas.ElementAtOrDefault(2),
                                                //Desc_ONU = colunas.ElementAtOrDefault(8),
                                                Desc_ONU = onuTipo,
                                                Num_OLT = olt.NumOLT,
                                                Id_OLT = olt.Id,
                                                Desc_OLT = olt.Descricao
                                            });
                                        }
                                    }

                                }
                            }

                            /* try
                             {
                                 var mensagem = TratarRetornoListaONUsPendentesRegistro(resposta);
                                 if (mensagem.ContainsKey("STATUS") && mensagem["STATUS"].Equals("COMPLD"))
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
                             } */

                            await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");

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
                            //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                            //client.MillisecondReadDelay = delayTime;

                            client.MillisecondReadDelay = 500;
                            bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 1000);

                            await client.WriteLineAsync("cd gpononu;");
                            await Task.Delay(1000);
                            await client.WriteLineAsync($"show discovery slot all link all");
                            int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 8000;
                            await Task.Delay(delayTime);

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
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                client.MillisecondReadDelay = 500;

                                var logado = await LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                //if (logado["STATUS"].Equals("COMPLD"))
                                if (logado.result == true)
                                {
                                    await client.WriteLineAsync($"LST-ONU::OLTID={this.olt.IP}:CTAG::;");
                                    var resposta = client.ReadAsync().Result;
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
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

                                    //throw new EAnielException($"Status.: {RetornoErroTratado(logado)}");
                                    throw new EAnielException($"Status.: Usuário ou senha inválidos.");
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

                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
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
                LogIntegracaoAtivacao(ex);
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
                var retornoStatus = false;
                var retornoResposta = false;
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
                                await Task.Delay(2000);

                                //var logado = await LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                //if (logado["STATUS"].Equals("COMPLD"))

                                if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.MAC)
                                {
                                    await client.ExecuteCommandAsync($"RESET-ONU::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::RESETTYPE=0;");
                                }
                                else if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.LOID)
                                {
                                    await client.ExecuteCommandAsync($"RESET-ONU::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::RESETTYPE=0;");
                                }
                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 6000;
                                await Task.Delay(delayTime);
                                var resposta = TratarMensagemPadrao(await client.ReadAsync());
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                if (resposta["STATUS"].Equals("COMPLD"))
                                {
                                    //response.result = true;
                                    retornoResposta = true;
                                    //response.message = $"Status.: {resposta["STATUS"]} \n Desc.: {resposta["CODIGO"]} - {resposta["DESCRICAO"]}";
                                }
                                else
                                {
                                    //response.result = false;
                                    //response.message = $"Status.: {RetornoErroTratado(resposta)}";
                                    await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                    throw new Exception($"Status.: {RetornoErroTratado(resposta)}");
                                }

                                if (retornoResposta == true)
                                {
                                    var tentativas = 0;

                                    while (tentativas <= 10)
                                    {
                                        await client.ExecuteCommandAsync($"LST-ONUSTATE::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Serial}:CTAG::;");

                                        await Task.Delay(2500);
                                        var respostaStatus = TratarRetornoGetStatusONU(await client.ReadAsync());
                                        LogIntegracaoAtivacao(string.Join("\n", respostaStatus));

                                        if (respostaStatus.ContainsKey("STATUS") && respostaStatus["STATUS"].Equals("COMPLD") &&
                                             respostaStatus.ContainsKey("STATUSONU") && respostaStatus["STATUSONU"].Equals("UP"))
                                        {
                                            LogIntegracaoAtivacao("ONU status is UP. Breaking the loop.");
                                            break;
                                        }
                                        else
                                        {
                                            tentativas++;
                                            LogIntegracaoAtivacao($"Tentativa {tentativas} de ler o retorno: {string.Join("\n", respostaStatus)}");
                                            if (tentativas == 1) //Primeira tentativa aguarda 15 segundos para tentar novamente, tentativas seguintes aguarda 10 segundos
                                                Thread.Sleep(15000);
                                            else
                                                Thread.Sleep(10000);
                                        }

                                        if (tentativas >= 10)
                                        {
                                            await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                            LogIntegracaoAtivacao("Número máximo de tentativas realizado. Status não identificado.");
                                            throw new Exception("Número máximo de tentativas realizado. Status não identificado.");
                                        }

                                    }

                                    response.result = true;
                                }

                                await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");


                            }
                            else
                            {
                                //response.result = false;
                                //response.message = "Erro ao estabelecer conexão.";
                                throw new Exception("Erro ao estabelecer conexão.");
                            }

                        }
                    }
                    else
                    {
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;

                                client.MillisecondReadDelay = 500;
                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 1000);

                                await client.WriteLineAsync("cd gpononu;");
                                await client.WriteLineAsync($"reset slot {onu.IdSlot} link {onu.IdPon} onulist {onu.OntID};");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
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
                LogIntegracaoAtivacao(ex);
                response.result = false;
                response.exception = $" {ex.Message}";
                response.message = ex.Message;
            }
            return response;
            //});

            //return task.Result;
        }

        public async Task<Response> DesautorizarONU(ConfigurarONUModel onu, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? $"OPERAÇÃO REALIZADA SEM O.S -> SERIAL: {onu.Serial}" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
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
                        using (var client = new RemoteConnection(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), this.olt.LoginTL1, this.olt.SenhaTL1, Protocol.Telnet, true))
                        {
                            await client.ConnectTL1();
                            if (client.IsConnected())
                            {

                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                //client.MillisecondReadDelay = 500;
                                await Task.Delay(2000);

                                //Console.WriteLine("Cliente Conectado");
                                //var logado = await LoginFiberhomeAsyncTL1(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                //if (logado["STATUS"].Equals("COMPLD"))

                                // -=-=-=-=-=-=-= Comando para Deletar ONUs =-=-=-=-=-=-
                                /*
                                 * ONUIDTYPE TEM QUE SER IGUAL AO QUE FOI ADICIONADO A ONU O TIPO DE AUTENTICAÇÃO
                                 * */
                                var idLog = enviarLog($"[DesautorizarONU] [FIBERHOME AB] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);
                                var comandos = new List<string>(); //Utilizado para gravar os comandos nos logs
                                var respostaOLT = new List<string>(); //Utilizado para gravar os retornos da OLT nos logs
                                if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.MAC)
                                {
                                    await client.ExecuteCommandAsync($"DEL-ONU::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon}:CTAG::ONUIDTYPE=MAC,ONUID={onu.Mac};");
                                    comandos.Add($"DEL-ONU::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon}:CTAG::ONUIDTYPE=MAC,ONUID={onu.Mac};");
                                }
                                else if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.LOID)
                                {
                                    await client.ExecuteCommandAsync($"DEL-ONU::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon}:CTAG::ONUIDTYPE=LOID,ONUID={onu.Serial};");
                                    comandos.Add($"DEL-ONU::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon}:CTAG::ONUIDTYPE=LOID,ONUID={onu.Serial};");
                                }
                                //Thread.Sleep(1500);
                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 8000;
                                await Task.Delay(delayTime);
                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                //Junta os valores que foram adicionados em comandos e respostaOLT
                                var comandosLog = $"Comandos executados:\n{string.Join("\n", comandos)}";
                                var respostasLog = $"Respostas:\n{string.Join("\n", resposta)}";

                                //Realiza gravação de logs
                                await enviarLog($"{comandosLog}\n\n{respostasLog}", numUsuario, numOs, idLog.Result);
                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("DENY".ToUpperInvariant())))
                                {
                                    //response.result = false;
                                    //response.message = $"Status.: Falha ao provisionar. O script contém caracteres inválidos.";
                                    await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                    LogIntegracaoAtivacao("Status.: A OLT retornou um erro ao tentar desprovisionar este equipamento.");
                                    throw new EAnielException("Status.: A OLT retornou um erro ao tentar desprovisionar este equipamento.");
                                }

                                response.result = true;

                                await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");

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
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
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
                LogIntegracaoAtivacao(ex);
                response.result = false;
                response.exception = $" {ex.Message}";
            }
            return response;


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

                        //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                        //client.MillisecondReadDelay = delayTime;
                        client.MillisecondReadDelay = 500;

                        Console.WriteLine("Cliente Conectado");
                        var logado = LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1).Result;
                        //if (logado["STATUS"].Equals("COMPLD"))
                        if (logado.result == true)
                        {
                            var linhasScriptOLT = scriptOLT.Split('\n');
                            foreach (string linha in linhasScriptOLT)
                            {
                                client.WriteLineAsync(linha);
                                Thread.Sleep(1500);

                                var resposta = client.ReadAsync().Result;
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
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
                                client.WriteLineAsync($"LST-ONUSTATE::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Serial}:CTAG::;");

                                Thread.Sleep(2500);
                                var respostaIdOnu = TratarRetornoGetIdONU(client.ReadAsync().Result);
                                LogIntegracaoAtivacao(string.Join("\n", respostaIdOnu));

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
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
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
                            //response.message = $"Status.: {RetornoErroTratado(logado)}";
                            response.message = $"Status.: Usuário ou senha inválidos.";

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
                LogIntegracaoAtivacao(ex);
                response.result = false;
                response.exception = $" {ex.Message}";
            }
            return response;
            //});
            //return task.Result;
        }

        public async Task<List<Models.StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listSerial, List<(string, string)> listSerialConvertido = null, Func<string, string> consultarOntId = null)
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
                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 5000;
                                await Task.Delay(delayTime);

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
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
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
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                foreach (string serial in listSerial)
                                {
                                    if (resposta.Exists(p => p.ToUpperInvariant().Contains("serial")))
                                    {
                                        retorno.Add(new Models.StatusONU
                                        {
                                            Serial = serial,
                                            Status = Sinapse.Model.enumerados.StatusONU.Online
                                        });
                                    }
                                    else
                                    {
                                        retorno.Add(new Models.StatusONU
                                        {
                                            Serial = serial,
                                            Status = Sinapse.Model.enumerados.StatusONU.Offline
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
                LogIntegracaoAtivacao(ex);
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
                        using (var client = new RemoteConnection(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), this.olt.LoginTL1, this.olt.SenhaTL1, Protocol.Telnet, true))
                        {
                            await client.ConnectTL1();
                            if (client.IsConnected())
                            {

                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(2000);

                                //var logado = await LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                //if (logado["STATUS"].Equals("COMPLD"))

                                if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.MAC)
                                {
                                    await client.ExecuteCommandAsync($"LST-OMDDM::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::;");
                                }
                                else if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.LOID)
                                {
                                    await client.ExecuteCommandAsync($"LST-OMDDM::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::;");
                                }

                                await Task.Delay(4000);
                                var resposta = await client.ReadAsync();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                try
                                {
                                    var m = TratarRetornoDiagnosticoONU(resposta);
                                    if (m["STATUS"].Equals("COMPLD"))
                                    {
                                        //retorno = m["ONU"] as DiagnosticoONU;

                                        if (m.ContainsKey("ONU") && m["ONU"] is List<Dictionary<string, object>> onuList && onuList.Count > 0)
                                        {

                                            var firstOnuDict = onuList[0];


                                            var diagnostico = new DiagnosticoONU
                                            {
                                                Rx = firstOnuDict["RxPower"].ToString(),
                                                Tx = firstOnuDict["TxPower"].ToString(),
                                                Temperatura = firstOnuDict["Temperature"].ToString(),
                                                Voltagem = firstOnuDict["Voltage"].ToString()
                                            };

                                            retorno = diagnostico;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    var m = TratarMensagemPadrao(resposta);
                                    throw new EAnielException($"Status.: {RetornoErroTratado(m)}");
                                }

                                await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");

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
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
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
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

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
                LogIntegracaoAtivacao(ex);
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
                        using (var client = new RemoteConnection(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), this.olt.LoginTL1, this.olt.SenhaTL1, Protocol.Telnet, true))
                        {
                            await client.ConnectTL1();
                            if (client.IsConnected())
                            {

                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(1500);

                                //var logado = await LoginFiberhomeAsync(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                //if (logado["STATUS"].Equals("COMPLD"))

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
                                    await client.ExecuteCommandAsync($"RESTORE-DEFAULTCFG::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::;");
                                }
                                else if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.LOID)
                                {
                                    await client.ExecuteCommandAsync($"RESTORE-DEFAULTCFG::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::;");
                                }
                                Thread.Sleep(2500);
                                var resposta = TratarMensagemPadrao(client.ReadAsync().Result);
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
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
                                await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");

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
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
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
                LogIntegracaoAtivacao(ex);
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
                        using (var client = new RemoteConnection(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), this.olt.LoginTL1, this.olt.SenhaTL1, Protocol.Telnet, true))
                        {
                            await client.ConnectTL1();
                            if (client.IsConnected())
                            {

                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(2000);

                                //var logado = await LoginFiberhomeAsync(client, olt.Login, olt.Senha);
                                //if (logado["STATUS"].Equals("COMPLD"))

                                if (onu.Tipo_Identificacao == Sinapse.Model.enumerados.ONU_Tipo_Identificacao.MAC)
                                {

                                    await client.ExecuteCommandAsync($"LST-DEVINFO::OLTID={olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::;");
                                }
                                else
                                {
                                    await client.ExecuteCommandAsync($"LST-DEVINFO::OLTID={olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::;");
                                }
                                await Task.Delay(2000);
                                var resposta = await client.ReadAsync();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
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
                                await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");

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
                LogIntegracaoAtivacao(ex);
                throw new EAnielException(ex.Message);
            }

            return retorno;
        }

        public async Task<GetDadosONUOLT> GetDadosONUnaOLT(string Serial_Convertido, string serial, string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var DadosONU = new GetDadosONUOLT();

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


                            // -=-=-=-=-=-=- Comando Para Listar ONUs -=-=-=-=-=-=-=-//
                            await client.ExecuteCommandAsync($"LST-ONU::OLTID={this.olt.IP}:CTAG::;");
                            var idLog = enviarLog($"[GetDadosONUnaOLT] [FIBERHOME A1] -> {this.olt.Descricao} SERIAL {serial} - SERIAL CONVERTIDO {Serial_Convertido}", numUsuario, numOs, null);
                            //Thread.Sleep(1500);
                            int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 6000;
                            await Task.Delay(delayTime);

                            //var resposta = await client.ReadAsync();
                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            LogIntegracaoAtivacao(string.Join("\n", resposta));
                            await enviarLog(string.Join("\n", resposta), numUsuario, numOs, idLog.Result);

                            var lista_onus_terminal = resposta.Where(p => p.ToUpperInvariant().Contains(serial.ToUpperInvariant()) || p.ToUpperInvariant().Contains(Serial_Convertido.ToUpperInvariant())).ToList();

                            if (lista_onus_terminal.Count > 0)
                            {
                                foreach (var linha in lista_onus_terminal)
                                {
                                    if (linha.IndexOf(serial, StringComparison.OrdinalIgnoreCase) >= 0 || (linha.IndexOf(Serial_Convertido, StringComparison.OrdinalIgnoreCase) >= 0))
                                    {
                                        var parts = linha.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                        if (parts.Length >= 3)
                                        {
                                            var fsp = parts[1];
                                            var fspSplit = fsp.Split('-');
                                            if (fspSplit.Length == 4)
                                            {
                                                DadosONU.Slot = fspSplit[2];
                                                DadosONU.Pon = fspSplit[3];
                                            }

                                            DadosONU.Onu_Id = parts[2];
                                            DadosONU.Onu_Encontrada = true;
                                            DadosONU.Serial = serial;
                                        }

                                        break;
                                    }
                                }

                            }
                            else
                            {

                                DadosONU = new GetDadosONUOLT
                                {
                                    Onu_Encontrada = false,
                                    Onu_Id = "",
                                    Slot = "",
                                    Pon = ""

                                };


                            }


                            await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");

                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }

                    }
                }


            }

            return DadosONU;
        }

        public async Task<GetStatusADMeOperacionalONU> GetStatusADMeOperacionalONU(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
        }



        private void LogIntegracaoAtivacao(string msg)
        {
            LogManager.GetLogger("IntegracaoOLT").Info($"[FIBERHOME_A_B] - {msg}");
        }

        private void LogIntegracaoAtivacao(Exception ex)
        {
            LogManager.GetLogger("IntegracaoOLT").Error($"[FIBERHOME_A_B] - {ex.Message}", ex);
        }

        #region metodos-privados

        /* private Dictionary<string, string> LoginFiberhomeAsync(Client client, string login, string senha)
         {
             //client.WriteLine(";\r");
             client.WriteLineAsync($"LOGIN:::CTAG::UN={login},PWD={senha};");
             String resultado = client.ReadAsync().Result;
             return TratarMensagemPadrao(resultado);

         } */


        private async Task<Response> LoginFiberhomeAsync(Client client, string login, string senha)
        {
            var response = new Response();
            await Task.Delay(2000);
            await client.WriteLineAsync(";\n");
            await Task.Delay(2000);
            await client.WriteLineAsync($"LOGIN:::CTAG::UN={login},PWD={senha};");
            await Task.Delay(5000);
            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

            LogIntegracaoAtivacao(string.Join("\n", resposta));

            // Check if any line contains "COMPLD"
            if (resposta.Exists(p => p.ToUpperInvariant().Contains("COMPLD".ToUpperInvariant())))
            {
                response.result = true;
            }
            else
            {
                response.result = false;
            }

            return response;
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
                LogIntegracaoAtivacao(ex);
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

        private Dictionary<string, object> TratarRetornoGetStatusONUsByPON(List<string> resposta, List<string> listSerial, Func<string, string> consultarOntId = null)
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
                var linha = resposta.FirstOrDefault(p => p.ToUpperInvariant().Contains(serial.ToUpperInvariant()));

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
                            Status = Model.enumerados.StatusONU.Online
                        };

                        onu.Add(novoDispositivo);
                    }
                    else
                    {
                        var novoDispositivo = new Models.StatusONU
                        {
                            Serial = serial,
                            Status = Model.enumerados.StatusONU.Offline
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
                    linha = resposta.FirstOrDefault(p => p.ToUpperInvariant().Contains("PASSWORD".ToUpperInvariant()));

                    if (!String.IsNullOrEmpty(linha))
                    {
                        var onuID = consultarOntId(serial);
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
                                    Status = Model.enumerados.StatusONU.Online
                                };

                                onu.Add(novoDispositivo);
                            }
                            else
                            {
                                var novoDispositivo = new Models.StatusONU
                                {
                                    Serial = serial,
                                    Status = Model.enumerados.StatusONU.Offline
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
                            Status = Model.enumerados.StatusONU.Indisponivel
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
            /*var lista = resposta.Split('\r', '\n');
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
             ----------------------------------------------------------------- 

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
            return mensagem; */

            var lista = resposta.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lista.Length; i++)
            {
                Console.WriteLine("Index " + i + ": " + lista[i]);
            }

            var mensagem = new Dictionary<string, object>();

            foreach (var line in lista)
            {
                if (line.ToUpperInvariant().Contains("FH_".ToUpperInvariant()))
                {
                    mensagem["LOG"] = line.Trim();
                }
                else if (line.ToUpperInvariant().Contains("total_blocks=".ToUpperInvariant()))
                {
                    mensagem["TOTAL BLOCKS"] = line.Split('=')[1].Trim();
                }
                else if (line.ToUpperInvariant().Contains("block_number=".ToUpperInvariant()))
                {
                    mensagem["BLOCK NUMBER"] = line.Split('=')[1].Trim();
                }
                else if (line.ToUpperInvariant().Contains("block_records=".ToUpperInvariant()))
                {
                    mensagem["BLOCK RECORDS"] = line.Split('=')[1].Trim();
                }
                else if (line.ToUpperInvariant().Contains("COMPLD".ToUpperInvariant()))
                {

                    mensagem["STATUS"] = "COMPLD";
                }
                else if (line.ToUpperInvariant().Contains("DENY".ToUpperInvariant()))
                {

                    mensagem["STATUS"] = "DENY";
                }


            }

            Int32? IdOnu = null;

            bool foundOnuStateHeader = false;
            foreach (var line in lista)
            {
                if (line.ToUpperInvariant().Contains("ONUID".ToUpperInvariant()))
                {
                    foundOnuStateHeader = true;
                    continue;
                }

                if (foundOnuStateHeader && !string.IsNullOrWhiteSpace(line) && !line.Contains("-----"))
                {
                    var onuLineParts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (onuLineParts.Length > 0)
                    {
                        int parsedIdOnu;
                        if (int.TryParse(onuLineParts[0], out parsedIdOnu))
                        {
                            IdOnu = parsedIdOnu;
                        }
                    }
                    break;
                }
            }

            mensagem["ONUID"] = IdOnu;

            return mensagem;

        }


        public bool ColetaIDONU(string respostaIdOnu, ConfigurarONUModel onu)
        {
            bool result = false;

            if (respostaIdOnu.ToUpperInvariant().Contains("COMPLD".ToUpperInvariant()))
            {
                Int32? idOnu = null;


                var lines = respostaIdOnu.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                bool foundHeader = false;

                foreach (var line in lines)
                {
                    if (foundHeader)
                    {
                        if (!line.Contains("-----"))
                        {
                            var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 0)
                            {
                                if (Int32.TryParse(parts[0], out int onuIdValue))
                                {
                                    idOnu = onuIdValue;
                                    LogIntegracaoAtivacao("Encontrou o ID! O ID da ONU é: " + idOnu);
                                }
                            }
                            break;
                        }
                    }
                    else if (line.Contains("ONUID") && line.Contains("AdminState"))
                    {
                        foundHeader = true;
                    }
                }

                if (idOnu != null)
                {

                    onu.OntID = idOnu;
                    result = true;
                }
            }
            else if (respostaIdOnu.ToUpperInvariant().Contains("DENY".ToUpperInvariant()))
            {
                result = false;

            }

            return result;
        }


        public async Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? $"OPERAÇÃO REALIZADA SEM O.S -> SERIAL: {onu.Serial}" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
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
                else if (protocolo == Protocol.Telnet)
                {
                    throw new NotImplementedException();
                }
                else if (!String.IsNullOrEmpty(this.olt.IpTL1))
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

                            //var logado = await LoginFiberhomeAsyncTL1(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                            //if (logado["STATUS"].Equals("COMPLD"))

                            var linhasScriptOLT = scriptOLT.Split('\n');
                            LogIntegracaoAtivacao(string.Join("\n", linhasScriptOLT));
                            var idLog = enviarLog($"[AutorizarONUAsync] [FIBERHOME AB] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);
                            var linhasERespostas = new List<(string Linha, List<string> Resposta)>();

                            foreach (string linha in linhasScriptOLT)
                            {
                                if (!string.IsNullOrWhiteSpace(linha))
                                {
                                    await client.ExecuteCommandAsync(linha);
                                    //Thread.Sleep(1500);
                                    LogIntegracaoAtivacao(linha);

                                    await Task.Delay(3000);
                                    var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    //var mensagem = TratarMensagemPadrao(resposta);

                                    /* if(mensagem["STATUS"].Equals("COMPLD"))
                                     {
                                         response.result = true;
                                         response.message = $"Status.: {mensagem["STATUS"]} \n Desc.: {mensagem["CODIGO"]} - {mensagem["DESCRICAO"]} \n";
                                         continue;
                                     }*/
                                    linhasERespostas.Add((linha, resposta));

                                    if (resposta.Exists(p => p.ToUpperInvariant().Contains("DENY".ToUpperInvariant())))
                                    {
                                        //response.result = false;
                                        //response.message = $"Status.: Falha ao provisionar. O script contém caracteres inválidos.";

                                        string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));
                                        await enviarLog(string.Join("\n", fullLogErro), numUsuario, numOs, idLog.Result);
                                        await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                        LogIntegracaoAtivacao("Status.: Falha ao provisionar. O script contém caracteres inválidos.");
                                        throw new EAnielException("Status.: Falha ao provisionar. O script contém caracteres inválidos.");
                                    }


                                    response.result = true;

                                }

                            }
                            string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));
                            await enviarLog(string.Join("\n", fullLog), numUsuario, numOs, idLog.Result);

                            if (response.result)
                            {
                                await GetIDONU(client, onu, numOs, numUsuario, enviarLog);

                            }


                            if (!String.IsNullOrEmpty(scriptONU))
                            {
                                var linhasScriptONU = scriptONU.Split(';');
                                LogIntegracaoAtivacao(string.Join("\n", linhasScriptONU));
                                var idLogConfig = enviarLog($"[AutorizarONUAsync - CONFIGURAÇÂO ONU] [FIBERHOME AB] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);
                                var linhasERespostasConfig = new List<(string Linha, List<string> Resposta)>();
                                foreach (string linha in linhasScriptONU)
                                {
                                    await client.ExecuteCommandAsync(linha);
                                    LogIntegracaoAtivacao(linha);
                                    await Task.Delay(3000);
                                    var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    //var mensagem = TratarMensagemPadrao(resposta);
                                    linhasERespostasConfig.Add((linha, resposta));


                                    if (resposta.Exists(p => p.ToUpperInvariant().Contains("DENY".ToUpperInvariant())))
                                    {
                                        //response.result = false;
                                        //response.message = $"Status.: Falha ao provisionar. O script contém caracteres inválidos.";
                                        string fullLogErro = string.Join("\n\n", linhasERespostasConfig.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));
                                        await enviarLog(string.Join("\n", fullLogErro), numUsuario, numOs, idLogConfig.Result);
                                        await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                        LogIntegracaoAtivacao("Status.: Falha ao configurar o equipamento. O script contém caracteres inválidos.");
                                        throw new EAnielException("Status.: Falha ao configurar o equipamento. O script contém caracteres inválidos.");
                                    }
                                }
                                string fullLogConfig = string.Join("\n\n", linhasERespostasConfig.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));
                                await enviarLog(string.Join("\n", fullLogConfig), numUsuario, numOs, idLogConfig.Result);

                                response.result = true;
                            }

                            await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");


                        }
                        else
                        {
                            response.result = false;
                            response.message = "Erro ao estabelecer conexão.";
                        }


                    }
                }

            }
            catch (Exception ex)
            {
                LogIntegracaoAtivacao(ex);
                //response.result = false;
                //response.exception = $" {ex.Message}";
                throw new EAnielException(ex.Message);
            }
            return response;
            //});
            //return task.Result;      
        }

        public async Task<Response> ConfigurarONUAsync(ConfigurarONUModel onu, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> GetIDONU(RemoteConnection client, ConfigurarONUModel onu, string numUsuario, string numOs, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            Response response = new Response();
            var linhasERespostas = new List<(string Linha, string Resposta)>();
            try
            {

                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                //client.MillisecondReadDelay = delayTime;
                //client.MillisecondReadDelay = 500;

                await Task.Delay(2000);
                //if (logado["STATUS"].Equals("COMPLD"))

                //Tentará por 2 vezes realizar a leitura do retorno da OLT.
                //Assim que conseguir coletar o ID da ONU, o loop será finalizado.

                var idLog = enviarLog($"[GetIDONU] [FIBERHOME AB] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);
                int tentativa = 0;
                int maxTentativa = 1;
                bool coleta = false;
                string comando = "";
                while (tentativa <= maxTentativa)
                {
                    await client.ExecuteCommandAsync($"LST-ONUSTATE::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Serial}:CTAG::;");
                    comando = $"LST-ONUSTATE::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Serial}:CTAG::;";
                    await Task.Delay(4000);
                    var respostaIdOnu = await client.ReadAsync();
                    linhasERespostas.Add((comando, respostaIdOnu));
                    LogIntegracaoAtivacao(string.Join("\n", respostaIdOnu));
                    LogIntegracaoAtivacao("Tentativa de leitura do ID da ONU número " + tentativa);
                    coleta = ColetaIDONU(respostaIdOnu, onu);
                    if (coleta == true)
                    {
                        string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
                                   $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));
                        await enviarLog(string.Join("\n", fullLog), numUsuario, numOs, idLog.Result);
                        break;
                    }
                    else
                    {

                        if (tentativa <= maxTentativa)
                        {
                            await Task.Delay(30000);

                        }
                        tentativa++;
                    }
                }
                if (coleta == false)
                {
                    string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
                                   $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));
                    await enviarLog(string.Join("\n", fullLog), numUsuario, numOs, idLog.Result);
                    await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                    LogIntegracaoAtivacao("Falha ao coletar ID do equipamento durante o processo.");
                    throw new Exception("Falha ao coletar ID do equipamento durante o processo.");
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

        public async Task<TesteONUModels> GetDadosComplementaresONU(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}