using Aniel.Connect.Core;
using Aniel.Connect.Model.nhibernate;
using log4net;
using NHibernate;
using NHibernate.SqlTypes;
using PrimS.Telnet;
using Renci.SshNet;
using Sinapse.Core;
using Sinapse.DAO;
using Sinapse.DAO.repository;
using Sinapse.Integracao.Ativacao.Interfaces;
using Sinapse.Integracao.Ativacao.Models;
using Sinapse.Model;
using Sinapse.Model.enumerados;
using Sinapse.Util.RemoteConnection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using static System.Collections.Specialized.BitVector32;

namespace Sinapse.Integracao.Ativacao.OLTs
{
    /// <summary>
    ///     Implementação para equipamentos com Firmware mais recente (Version A/1).
    /// </summary>
    public class Fiberhome_A1 : IntegracaoAtivacao
    {
        private OLT olt;

        public Fiberhome_A1(OLT olt)
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
                            var idLog = enviarLog($"[ONUsPendentesRegistro] [FIBERHOME A1] -> {this.olt.Descricao}", numUsuario, numOs, null);
                            //Thread.Sleep(1500);
                            int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 6000;
                            await Task.Delay(delayTime);

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
                    using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                    //using (var client = new Client(this.olt.IP, Convert.ToInt32(this.olt.PortaTelnet.GetValueOrDefault(23)), new CancellationToken()))
                    {
                        await client.Connect();
                        if (client.IsConnected())
                        {
                            bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                            bool terminal_configurado = await SetTerminalLength(client);


                                if(this.olt.Versao_OLT.HasValue && this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                                {
                                    await client.ExecuteCommandAsync("cd gpononu");
                                    await Task.Delay(1000);
                                    await client.ExecuteCommandAsync($"show discovery slot all link all");
                                    int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 8000;
                                    await Task.Delay(delayTime);

                                        /* -----  ONU Unauth Table ,SLOT = 1 PON = 1 ,ITEM = 0-----
                                   -----  ONU Unauth Table ,SLOT = 1 PON = 2 ,ITEM = 1-----
                                   NO       TYPE           PHY_ID        PWD           SN LOID, SN PWD
                                   --  --------------  ------------  ----------  --------------------------
                                   01  AN5506-04-B2    FHTT02cda6a0              ,
                                   -----  ONU Unauth Table ,SLOT = 1 PON = 3 ,ITEM = 0-----
                                   -----  ONU Unauth Table ,SLOT = 1 PON = 4 ,ITEM = 0-----
                                   -----  ONU Unauth Table ,SLOT = 1 PON = 5 ,ITEM = 0-----
                                   -----  ONU Unauth Table ,SLOT = 1 PON = 6 ,ITEM = 0-----
                                   -----  ONU Unauth Table ,SLOT = 1 PON = 7 ,ITEM = 0-----
                                   -----  ONU Unauth Table ,SLOT = 1 PON = 8 ,ITEM = 0-----                                 
                                  */

                                    var idLog = enviarLog($"[ONUsPendentesRegistro] [FIBERHOME A1] -> {this.olt.Descricao}", numUsuario, numOs, null);
                                    var resposta = (await client.ReadAsync()).Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToList();

                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    await enviarLog(string.Join("\n", resposta), numUsuario, numOs, idLog.Result);

                                    var lista_headers_slots_pons = resposta.FindAll(p => p.ToUpperInvariant().Contains("ONU Unauth Table".ToUpperInvariant()));

                                    if (lista_headers_slots_pons.Count > 0)
                                    {
                                        foreach (var header in lista_headers_slots_pons)
                                        {
                                            if (header.ToUpperInvariant().Contains("ITEM=0"))
                                                continue; // Skip empty ONU tables

                                            int index_header = resposta.IndexOf(header);

                                            // Slice only the part relevant to this header
                                            var remaining = resposta.Skip(index_header + 1).ToList();
                                            int end_index = remaining.FindIndex(p =>
                                                p.ToUpperInvariant().Contains("ONU Unauth Table".ToUpperInvariant()) ||
                                                p.ToUpperInvariant().Contains("Command execute success".ToUpperInvariant()));

                                            if (end_index >= 0)
                                                remaining = remaining.Take(end_index).ToList();

                                            // Skip header lines (like NO TYPE ...)
                                            var lista_formatada = remaining.Skip(2).ToList();

                                            var campos_header = header.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                                            foreach (var onu in lista_formatada)
                                            {
                                                var campos_onu = onu.Split(' ').ToList();
                                                campos_onu.RemoveAll(s => string.IsNullOrWhiteSpace(s));

                                                var slotEntry = campos_header.FirstOrDefault(p => p.ToUpperInvariant().Contains("SLOT="));
                                                var ponEntry = campos_header.FirstOrDefault(p => p.ToUpperInvariant().Contains("PON="));
                                                string numSlot = slotEntry?.Split('=')[1];
                                                string numPon = ponEntry?.Split('=')[1];
                                                var onu_type = campos_onu.ElementAtOrDefault(1);
                                                var serial = campos_onu.ElementAtOrDefault(2);
                                                var ont_posicao = campos_onu.ElementAtOrDefault(0);

                                                if (int.TryParse(ont_posicao, out int ont_posicao_convertido) && !string.IsNullOrEmpty(serial))
                                                {
                                                    listaOnus.Add(new AutorizarONUListModel
                                                    {
                                                        Id = $"{numSlot}/{numPon}/{ont_posicao_convertido}",
                                                        Desc_OLT = olt.Descricao,
                                                        Num_OLT = olt.NumOLT,
                                                        Id_OLT = olt.Id,
                                                        Slot = numSlot,
                                                        PON = numPon,
                                                        Mac_Serial = serial,
                                                        Desc_ONU = onu_type
                                                    });
                                                }
                                            }
                                            
                                        }

                                    }


                                 }
                                else 
                                {
                                    await client.ExecuteCommandAsync("cd onu");
                                    await Task.Delay(1000);
                                    await client.ExecuteCommandAsync($"show discovery slot all pon all");
                                    int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 8000;
                                    await Task.Delay(delayTime);

                                /* -----  ONU Unauth Table ,SLOT = 1 PON = 1 ,ITEM = 0-----
                             -----  ONU Unauth Table ,SLOT = 1 PON = 2 ,ITEM = 1-----
                             NO       TYPE           PHY_ID        PWD           SN LOID, SN PWD
                             --  --------------  ------------  ----------  --------------------------
                             01  AN5506-04-B2    FHTT02cda6a0              ,
                             -----  ONU Unauth Table ,SLOT = 1 PON = 3 ,ITEM = 0-----
                             -----  ONU Unauth Table ,SLOT = 1 PON = 4 ,ITEM = 0-----
                             -----  ONU Unauth Table ,SLOT = 1 PON = 5 ,ITEM = 0-----
                             -----  ONU Unauth Table ,SLOT = 1 PON = 6 ,ITEM = 0-----
                             -----  ONU Unauth Table ,SLOT = 1 PON = 7 ,ITEM = 0-----
                             -----  ONU Unauth Table ,SLOT = 1 PON = 8 ,ITEM = 0-----                                 
                          */

                                var idLog = enviarLog($"[ONUsPendentesRegistro] [FIBERHOME A1] -> {this.olt.Descricao}", numUsuario, numOs, null);
                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);


                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                await enviarLog(string.Join("\n", resposta), numUsuario, numOs, idLog.Result);

                                var lista_headers_slots_pons = resposta.FindAll(p => p.ToUpperInvariant().Contains("ONU Unauth Table".ToUpperInvariant()));


                                if (lista_headers_slots_pons.Count > 0)
                                {
                                    foreach (var header in lista_headers_slots_pons)
                                    {
                                        int? index_header = null;
                                        index_header = resposta.IndexOf(header);

                                        var lista_formatada = resposta.Skip(index_header.Value + 1).ToList();

                                        int proximo_header_index = lista_formatada.FindIndex(p => p.ToUpperInvariant().Contains("ONU Unauth Table".ToUpperInvariant())
                                         || p.ToUpperInvariant().Contains("Command execute success".ToUpperInvariant()));

                                        if (proximo_header_index > 0)
                                        {
                                            lista_formatada = lista_formatada.GetRange(0, proximo_header_index).Skip(2).ToList();


                                            var campos_header = header.Split(' ', ',', '-').ToList();


                                            foreach (var onu in lista_formatada)
                                            {
                                                var campos_onu = onu.Split(' ').ToList();
                                                campos_onu.RemoveAll(s => s == " " || s == String.Empty);

                                                var numSlot = campos_header[campos_header.FindIndex(p => p.ToUpperInvariant().Contains("Slot".ToUpperInvariant())) + 2];
                                                var numPon = campos_header[campos_header.FindIndex(p => p.ToUpperInvariant().Contains("Pon".ToUpperInvariant())) + 2];
                                                var onu_type = campos_onu.ElementAtOrDefault(1);
                                                var serial = campos_onu.ElementAtOrDefault(2);

                                                //Tratamento para checar se o serial foi obtido pois houveram casos onde ocorreu
                                                //quebra de linha no terminal e após tratar a linha a variável campos_onu estava vazia.
                                                if (!String.IsNullOrEmpty(serial))
                                                {
                                                    listaOnus.Add(new AutorizarONUListModel
                                                    {
                                                        Id = $"{numSlot}/{numPon}/{campos_onu.ElementAtOrDefault(0)}",
                                                        Desc_OLT = this.olt.Descricao,
                                                        Num_OLT = this.olt.NumOLT,
                                                        Id_OLT = this.olt.Id,
                                                        Slot = numSlot,
                                                        PON = numPon,
                                                        Mac_Serial = serial,
                                                        Desc_ONU = onu_type
                                                    });
                                                }
                                            }


                                        }
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

                                var logado = await LoginFiberhomeAsyncTL1(client, this.olt.LoginTL1, this.olt.SenhaTL1);
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
                        throw new NotImplementedException();
                        using (Client client = new Client(this.olt.IP, this.olt.PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                        {
                            if (client.IsConnected)
                            {

                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                client.MillisecondReadDelay = 500;
                                bool logado = await client.TryLoginAsync(this.olt.Login, this.olt.Senha, 1000) && await LoginFiberhomeAsyncTelnet(client, this.olt.Senha);

                                //NÃO ESTÁ SENDO USADO MAS SE NECESSÁRIO, SEGUE O COMANDO DE EXEMPLO.
                                await client.WriteLineAsync("cd onu");
                                await client.WriteLineAsync("show online slot all link all");

                                /*-----  ONU Auth Table, Total ITEM = 1 -----

                                A: Authorized  P: Preauthorized  R: System Reserved

                                -----  ONU Auth Table, SLOT = 16, PON = 1, ITEM = 1 -----
                                Slot Pon Onu OnuType        ST Lic OST PhyId        PhyPwd     LogicId                  LogicPwd
                                ---- --- --- -------------- -- --- --- ------------ ---------- ------------------------ ------------
                                16   1   1   HG6145E        A  1   up  ZTEGc4b4be9d
                                Command execute success.
                                */

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);
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

                                await Task.Delay(2000);
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
                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));


                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("DENY".ToUpperInvariant())))
                                {
                                    //response.result = true;
                                    await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                    throw new Exception($"Algo de errado ocorreu durante a reinicialização do equipamento. Por favor, tente novamente mais tarde.");
                                    //response.message = $"Status.: {resposta["STATUS"]} \n Desc.: {resposta["CODIGO"]} - {resposta["DESCRICAO"]}";
                                }
                                else
                                {
                                    /*response.result = false;
                                    response.message = $"Status.: {RetornoErroTratado(resposta)}"; */
                                    retornoResposta = true;
                                }

                                if (retornoResposta == true)
                                {
                                    var tentativas = 0;

                                    while (tentativas <= 10)
                                    {
                                        await client.ExecuteCommandAsync($"LST-ONUSTATE::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Serial}:CTAG::;");
                                        await Task.Delay(2500);

                                        var respostaStatus = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                        LogIntegracaoAtivacao(string.Join("\n", respostaStatus));

                                        if (respostaStatus.Exists(p => p.ToUpperInvariant().Contains("COMPLD".ToUpperInvariant())) &&
                                            respostaStatus.Exists(p => p.ToUpperInvariant().Contains("UP".ToUpperInvariant())))
                                        {
                                            LogIntegracaoAtivacao("ONU status is UP. Breaking the loop.");
                                            break;
                                        }
                                        else
                                        {
                                            tentativas++;
                                            LogIntegracaoAtivacao($"Tentativa {tentativas} de ler o retorno: {string.Join("\n", respostaStatus)}");

                                            if (tentativas == 1) //Primeira tentativa aguarda 15 segundos para tentar novamente, tentativas seguintes aguarda 10 segundos
                                                await Task.Delay(15000);
                                            else
                                                await Task.Delay(10000);
                                        }

                                        /* if (respostaStatus.ContainsKey("STATUS") && respostaStatus["STATUS"].Equals("COMPLD") &&
                                            respostaStatus.ContainsKey("STATUSONU") && respostaStatus["STATUSONU"].Equals("UP"))
                                        {
                                            LogIntegracaoAtivacao("ONU status is UP. Breaking the loop.");
                                            break; ;
                                        } */


                                        if (tentativas >= 10)
                                        {
                                            await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                            LogIntegracaoAtivacao("Número máximo de tentativas realizado. Status não identificado.");
                                            throw new Exception("Número máximo de tentativas realizado. Status não identificado.");

                                        }
                                    }
                                    response.result = true;
                                    //response.message = retornoStatus ? "ONU reiniciada e status verificado como UP." : "Status não identificado após várias tentativas.";
                                }

                                await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");

                            }
                            else
                            {
                                /*response.result = false;
                                response.message = "Erro ao estabelecer conexão."; */
                                throw new Exception("Erro ao estabelecer conexão.");
                            }
                        }
                    }
                    else
                    {
                        using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                        {
                            await client.Connect();
                            if (client.IsConnected())
                            {

                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(600);
                                bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                                bool terminal_configurado = await SetTerminalLength(client);


                                await client.ExecuteCommandAsync("cd maintenance");
                                await client.ExecuteCommandAsync($"reboot slot {onu.IdSlot} pon {onu.IdPon} onulist {onu.OntID}");

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("reset onu ok".ToUpperInvariant())))
                                    response.result = true;
                                //Retorno: reset onu ok! 

                                if (response.result)
                                {

                                    var tentativas = 0;

                                    await client.ExecuteCommandAsync($"cd ..");
                                    await client.ExecuteCommandAsync($"cd onu");

                                    while (!retornoStatus && tentativas <= 10)
                                    {
                                        await client.ExecuteCommandAsync($"show authorization slot {onu.IdSlot} pon {onu.IdPon}");
                                        await Task.Delay(2000);
                                        resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                        LogIntegracaoAtivacao(string.Join("\n", resposta));

                                        if (resposta.Exists(p => p.ToUpperInvariant().Contains(onu.Mac.ToUpperInvariant())) || resposta.Exists(p => p.ToUpperInvariant().Contains(onu.Serial.ToUpperInvariant())))
                                        {
                                            var linha = resposta.First(p => p.Contains(onu.Mac) || p.Contains(onu.Serial));
                                            var campos_onu = linha.Split(' ').ToList();
                                            campos_onu.RemoveAll(s => s == " " || s == String.Empty);

                                            if (campos_onu[6].ToUpperInvariant().Contains("up".ToUpperInvariant()) || campos_onu[6].Contains("1"))
                                            {
                                                retornoStatus = true;
                                                break;
                                            }
                                            else
                                            {
                                                tentativas++;
                                                if (tentativas == 1) //Primeira vez aguarda 15 segundos para tentar novamente, tentativas seguintes aguarda 10 segundos
                                                    Thread.Sleep(15000);
                                                else
                                                    Thread.Sleep(10000);
                                            }
                                        }
                                    }

                                    response.result = retornoStatus;
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
                else if (!String.IsNullOrEmpty(this.olt.IpTL1)) //Acesso TL1
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
                            var idLog = enviarLog($"[DesautorizarONU] [FIBERHOME A1] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);
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
                    using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                    {

                        await client.Connect();
                        if (client.IsConnected())
                        {
                            //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                            //client.MillisecondReadDelay = delayTime;
                            await Task.Delay(1500);
                            bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                            bool terminal_configurado = await SetTerminalLength(client);
                            var idLog = enviarLog($"[DesautorizarONU] [FIBERHOME A1] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);
                            var comandos = new List<string>(); //Utilizado para gravar os comandos nos logs
                            var respostaOLT = new List<string>(); //Utilizado para gravar os retornos da OLT nos logs

                            if(this.olt.Versao_OLT.HasValue)
                            {
                                if (this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                                {
                                    await client.ExecuteCommandAsync("cd gpononu");
                                    comandos.Add("cd gpononu");
                                    await client.ExecuteCommandAsync($"set whitelist phy_addr address {onu.Serial ?? onu.Mac} password null action delete slot null link null onu null type null");
                                    comandos.Add($"set whitelist phy_addr address {onu.Serial ?? onu.Mac} password null action delete slot null link null onu null type null");
                                }
                                else if (this.olt.Versao_OLT.Value == Versao_OLT.RP1200 || this.olt.Versao_OLT.Value == Versao_OLT.RP1800)
                                {
                                    await client.ExecuteCommandAsync("cd onu");
                                    comandos.Add("cd onu");
                                    await client.ExecuteCommandAsync($"set whitelist phy_addr address {onu.Serial ?? onu.Mac} password null action delete slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID} type {onu.Descricao}");
                                    comandos.Add($"set whitelist phy_addr address {onu.Serial ?? onu.Mac} password null action delete slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID} type {onu.Descricao}");
                                }

                            }
                            else
                            {
                                await client.ExecuteCommandAsync("cd onu");
                                comandos.Add("cd onu");
                                await client.ExecuteCommandAsync($"set whitelist phy_addr address {onu.Serial ?? onu.Mac} password null action delete slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID} type {onu.Descricao}");
                                comandos.Add($"set whitelist phy_addr address {onu.Serial ?? onu.Mac} password null action delete slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID} type {onu.Descricao}");
                            }
                          

                            var resposta_whitelist = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            respostaOLT.Add(string.Join("\n", resposta_whitelist));
                            LogIntegracaoAtivacao(string.Join("\n", resposta_whitelist));


                            if (resposta_whitelist.Exists(p => p.ToUpperInvariant().Contains("Command execute success".ToUpperInvariant()))
                                || resposta_whitelist.Exists(p => p.ToUpperInvariant().Contains("whitelist ok".ToUpperInvariant())))
                            {
                                if (this.olt.Versao_OLT.HasValue)
                                {
                                    if (this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                                    {
                                        //Junta os valores que foram adicionados em comandos e respostaOLT
                                        var comandosLogRP700 = $"Comandos executados:\n{string.Join("\n", comandos)}";
                                        var respostasLogRP700 = $"Respostas:\n{string.Join("\n", respostaOLT)}";

                                        //Realiza gravação de logs
                                        await enviarLog($"{comandosLogRP700}\n\n{respostasLogRP700}", numUsuario, numOs, idLog.Result);
                                        response.result = true;
                                        return response;
                                    }
                                    else if (this.olt.Versao_OLT.Value == Versao_OLT.RP1200 || this.olt.Versao_OLT.Value == Versao_OLT.RP1800)
                                    {
                                        await client.ExecuteCommandAsync($"set unauthorization slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID}");
                                        comandos.Add($"set unauthorization slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID}");
                                    }
                                }
                                else
                                {
                                    await client.ExecuteCommandAsync($"set unauthorization slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID}");
                                    comandos.Add($"set unauthorization slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID}");
                                }

                                var resposta_unauthorization = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                respostaOLT.Add(string.Join("\n", resposta_unauthorization));
                                LogIntegracaoAtivacao(string.Join("\n", resposta_unauthorization));

                                //Junta os valores que foram adicionados em comandos e respostaOLT
                                var comandosLog = $"Comandos executados:\n{string.Join("\n", comandos)}";
                                var respostasLog = $"Respostas:\n{string.Join("\n", respostaOLT)}";

                                //Realiza gravação de logs
                                await enviarLog($"{comandosLog}\n\n{respostasLog}", numUsuario, numOs, idLog.Result);

                                if (resposta_unauthorization.Exists(p => p.ToUpperInvariant().Contains("Command execute success".ToUpperInvariant()) &&
                                !p.ToUpperInvariant().Contains("error".ToUpperInvariant()) &&
                                !p.ToUpperInvariant().Contains("fail".ToUpperInvariant())))
                                {
                                    await client.ExecuteCommandAsync($"cd ..");
                                    await client.ExecuteCommandAsync($"save");
                                    await Task.Delay(1500);
                                    response.result = true;
                                }
                                else
                                {


                                    throw new EAnielException(string.Join("\n", resposta_unauthorization.ToArray()));
                                }
                            }
                            else
                            {
                                await enviarLog(string.Join("\n", resposta_whitelist), numUsuario, numOs, idLog.Result);
                                throw new EAnielException(string.Join("\n", resposta_whitelist.ToArray()));
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
            return response;
            //});
            //return task.Result;
            //return await task;
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
                        var logado = LoginFiberhomeAsyncTL1(client, this.olt.LoginTL1, this.olt.SenhaTL1).Result;
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
                        using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                        {
                            await client.Connect();
                            if (client.IsConnected())
                            {
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(600);
                                bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                                bool terminal_configurado = await SetTerminalLength(client);

                                if (this.olt.Versao_OLT.HasValue && this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                                {
                                    await client.ExecuteCommandAsync("cd gpononu");
                                    await client.ExecuteCommandAsync($"show online slot {numSlot} link {numPON}");

                                    /*ONLINE ONU TOTAL NUM = 1
                                    ONUID   ONUTYPE         SN              PASSWORD    LOGICAL SN
                                    -----   ------------    ------------    ----------  ---------------------
                                    01      AN5506-04-B2G   FHTT00030405 ,
                                    */

                                    var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    foreach (string serial in listSerial)
                                    {
                                        if (resposta.Exists(p => p.ToUpperInvariant().Contains(serial.ToUpperInvariant())))
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
                                    await client.ExecuteCommandAsync("cd onu");
                                    await client.ExecuteCommandAsync($"show authorization slot {numSlot} pon {numPON}");
                                    await Task.Delay(1500);
                                    /*
                                    CAMPO OST => up = Online / dn = Offline

                                    -----  ONU Auth Table, Total ITEM = 1 -----

                                    A: Authorized  P: Preauthorized  R: System Reserved

                                    -----  ONU Auth Table, SLOT = 16, PON = 1, ITEM = 1 -----
                                    Slot Pon Onu OnuType        ST Lic OST PhyId        PhyPwd     LogicId                  LogicPwd
                                    ---- --- --- -------------- -- --- --- ------------ ---------- ------------------------ ------------
                                    16   1   1   HG6145E        A  1   up  ZTEGc4b4be9d
                                    Command execute success.                                
                                    */

                                    var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);

                                    foreach (string serial in listSerial)
                                    {
                                        if (resposta.Exists(p => p.ToUpperInvariant().Contains(serial.ToUpperInvariant())))
                                        {
                                            var linha = resposta.First(p => p.Contains(serial));
                                            var campos_onu = linha.Split(' ').ToList();
                                            campos_onu.RemoveAll(s => s == " " || s == String.Empty);

                                            var status = Sinapse.Model.enumerados.StatusONU.Offline;

                                            if (campos_onu[6].ToUpperInvariant().Contains("up".ToUpperInvariant()) || campos_onu[6].Contains("1"))
                                            {
                                                status = Model.enumerados.StatusONU.Online;

                                            }

                                            retorno.Add(new Models.StatusONU
                                            {
                                                Serial = serial,
                                                Status = status
                                            });
                                        }
                                        else
                                        {
                                            retorno.Add(new Models.StatusONU
                                            {
                                                Serial = serial,
                                                Status = Sinapse.Model.enumerados.StatusONU.Indisponivel
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
                                //client.MillisecondReadDelay = 500;
                                await Task.Delay(2000);

                                //var logado = await LoginFiberhomeAsyncTL1(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                //if (logado["STATUS"].Equals("COMPLD"))

                                if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.MAC)
                                {
                                    await client.ExecuteCommandAsync($"LST-OMDDM::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::;");
                                }
                                else if (onu.Tipo_Identificacao == ONU_Tipo_Identificacao.LOID)
                                {
                                    await client.ExecuteCommandAsync($"LST-OMDDM::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::;");
                                }

                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 4000;
                                await Task.Delay(delayTime);

                                var resposta = await client.ReadAsync();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

                                /*-----------------------------------------------------------------
                                 ONUID AdminState OperStat AUTH AUTHINFO ONUIP LASTOFFTIME
                                 5       UP          UP    LOID aaa_bbb   --   0000-00-00 00:00:00
                                 -----------------------------------------------------------------*/

                                if (resposta.ToUpperInvariant().Contains("DENY".ToUpperInvariant()))
                                {
                                    await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                    throw new EAnielException("Falha ao executar o comando na OLT. Por favor, tente novamente mais tarde.");
                                }

                                try
                                {
                                    var m = TratarRetornoDiagnosticoONU(resposta);

                                    //retorno = m["ONU"] as DiagnosticoONU;


                                    if (m.ContainsKey("ONU") && m["ONU"] is List<Dictionary<string, object>> onuList && onuList.Count > 0)
                                    {

                                        var firstOnuDict = onuList[0];


                                        var diagnostico = new DiagnosticoONU
                                        {
                                            Rx = firstOnuDict["RxPower"].ToString(),
                                            Tx = firstOnuDict["TxPower"].ToString(),
                                            Temperatura = firstOnuDict["Temperature"].ToString(),
                                            Voltagem = firstOnuDict["Voltage"].ToString(),
                                            Bias_Current = firstOnuDict["CurrTxBias"].ToString()
                                        };

                                        retorno = diagnostico;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    //var m = TratarMensagemPadrao(resposta);
                                    //throw new EAnielException($"Status.: {RetornoErroTratado(m)}");

                                    LogIntegracaoAtivacao(ex);
                                    throw new EAnielException(ex.Message);
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
                        using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                        {
                            await client.Connect();
                            if (client.IsConnected())
                            {
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(1500);
                                bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                                bool terminal_configurado = await SetTerminalLength(client);

                                if (this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                                {
                                    await client.ExecuteCommandAsync("cd gpononu");
                                    await client.ExecuteCommandAsync($"show optic_module slot {onu.IdSlot} link {onu.IdPon} onu {onu.OntID}");
                                }
                                else
                                {
                                    await client.ExecuteCommandAsync("cd onu");
                                    await client.ExecuteCommandAsync($"show optic_module slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID}");
                                }
                                

                                //TODO: Ver qual comando está certo, o de cima ou esse:
                                //cd onu
                                //show optic_module slot 16 pon 1 onu 1                                                             

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

                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 3000;
                                await Task.Delay(delayTime);
                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("TYPE")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("TYPE"));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Type = $"{campos.ElementAtOrDefault(1)} {campos.ElementAtOrDefault(2).Replace("(", String.Empty).Replace(")", String.Empty)}";
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("TEMPERATURE")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("TEMPERATURE"));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Temperatura = campos.ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("VOLTAGE")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("VOLTAGE"));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Voltagem = campos.ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("BIAS CURRENT")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("BIAS CURRENT"));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Bias_Current = campos.ElementAtOrDefault(2);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("SEND POWER")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("SEND POWER"));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Tx = campos.ElementAtOrDefault(2);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("RECV POWER") && !p.ToUpperInvariant().Contains("OLT")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("RECV POWER"));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Rx = campos.ElementAtOrDefault(2);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("OLT RECV POWER")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("OLT RECV POWER"));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Rx_OLT = campos.ElementAtOrDefault(3);
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
                                await Task.Delay(2000);

                                //var logado = await LoginFiberhomeAsyncTL1(client, this.olt.LoginTL1, this.olt.SenhaTL1);
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
                                //Thread.Sleep(2500);
                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 6000;
                                await Task.Delay(delayTime);
                                var resposta = TratarMensagemPadrao(await client.ReadAsync());
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
                        using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                        {
                            await client.Connect();
                            if (client.IsConnected())
                            {

                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(600);

                                bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                                bool terminal_configurado = await SetTerminalLength(client);
                                await client.ExecuteCommandAsync("cd onu");
                                await client.ExecuteCommandAsync($"reset default_cfg slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID} default_cfg 1");
                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 5000;
                                await Task.Delay(delayTime);
                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Recover onu default cfg OK".ToUpperInvariant())))
                                    response.result = true;
                                else
                                    throw new EAnielException(string.Join("\n", resposta.ToArray()));
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
                            var idLog = enviarLog($"[AutorizarONUAsync] [FIBERHOME A1] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);
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
                                await GetIDONU(client, onu, numUsuario, numOs, enviarLog);

                            }


                            if (!String.IsNullOrEmpty(scriptONU))
                            {
                                var linhasScriptONU = scriptONU.Split(';');
                                LogIntegracaoAtivacao(string.Join("\n", linhasScriptONU));
                                var idLogConfig = enviarLog($"[AutorizarONUAsync - CONFIGURAÇÂO ONU] [FIBERHOME A1] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);
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
                else
                {
                    using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                    {
                        await client.Connect();
                        if (client.IsConnected())
                        {

                            //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                            //client.MillisecondReadDelay = delayTime;
                            await Task.Delay(500);
                            bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                            bool terminal_configurado = await SetTerminalLength(client);
                            var linhasERespostas = new List<(string Linha, string Resposta)>();
                            var idLog = enviarLog($"[AutorizarONUAsync] [FIBERHOME A1] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);


                           
                            /*if (!scriptOLT.ToUpperInvariant().Contains("cd onu".ToUpperInvariant()))
                            {
                                await client.ExecuteCommandAsync("cd onu");
                            }*/

                            var linhasScriptOLT = scriptOLT.Split('\n');
                            LogIntegracaoAtivacao(string.Join("\n", linhasScriptOLT));
                            foreach (string linha in linhasScriptOLT)
                            {

                                if (linha.ToUpperInvariant().Contains("ONU NULL"))
                                {
                                    if (!onu.OntID.HasValue)
                                    {
                                        //Pegar próximo Id para autorização
                                        var idOnu = await GetProximoOnuIdDisp(client, onu.IdSlot.Value, onu.IdPon.Value, this.olt.Versao_OLT);
                                        await Task.Delay(1000);
                                        LogIntegracaoAtivacao("ID da ONU encontrado! O ID é: " + idOnu);
                                        onu.OntID = idOnu;

                                    }
                                }

                                if (linha.ToUpperInvariant().Contains("<ONT_ID>"))
                                {
                                    if (!onu.OntID.HasValue)
                                    {
                                        //Pegar próximo Id para autorização
                                        var idOnu = await GetProximoOnuIdDisp(client, onu.IdSlot.Value, onu.IdPon.Value, this.olt.Versao_OLT);
                                        await Task.Delay(1000);
                                        LogIntegracaoAtivacao("ID da ONU encontrado! O ID é: " + idOnu);
                                        onu.OntID = idOnu;

                                    }

                                    await client.ExecuteCommandAsync(linha.Replace("<ONT_ID>", Convert.ToString(onu.OntID)));
                                    continue;
                                }
                                await client.ExecuteCommandAsync(linha);
                                LogIntegracaoAtivacao(linha);

                                var resposta = await client.ReadAsync();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

                                linhasERespostas.Add((linha, resposta));

                                if (resposta.ToUpperInvariant().Contains("Command executes failed".ToUpperInvariant()) ||
                              resposta.ToUpperInvariant().Contains("Unknown command".ToUpperInvariant()) ||
                              resposta.ToUpperInvariant().Contains("Error".ToUpperInvariant()))
                                {
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));
                                    await enviarLog(fullLogErro, numUsuario, numOs, idLog.Result);
                                    throw new EAnielException(resposta);

                                }
                                await Task.Delay(500);

                            }

                            response.result = true;

                            string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));
                            await enviarLog(fullLog, numUsuario, numOs, idLog.Result);

                        }
                        else
                        {
                            DAO.CONSTANTES.log4Net.Error("Falha na conexão.");
                            throw new EAnielException("Erro ao estabelecer conexão.");
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
                        using (var client = new RemoteConnection(this.olt.IpTL1, this.olt.PortaTL1.GetValueOrDefault(23), this.olt.LoginTL1, this.olt.SenhaTL1, Protocol.Telnet, true))
                        {
                            await client.ConnectTL1();
                            if (client.IsConnected())
                            {
                                await Task.Delay(1500);

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
                        using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                        {
                            await client.Connect();
                            if (client.IsConnected())
                            {
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(600);
                                bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                                bool terminal_configurado = await SetTerminalLength(client);


                                #region Informações da WAN
                                /*BR-PA-CAH-CCL-OLT-03-FIBERHOME\onu# show wan_info slot 16 pon 1 onu 2
                                    ----- HG WAN CONNECT INF STATIST 16.1.2 ,ITEM=1-----
                                    WAN CONNECT INDEX   : 1
                                    WPS CONNECT NAME    : INTERNET_R_VID_600
                                    VLAN ID             : 600
                                    VLAN COS            : 0
                                    GET IP MODE TYPE    : pppoe
                                    QoS Switch          : 0
                                    WPS CONNECT STATUS  : 1
                                    STATIC IPADDR       : 100.70.94.98
                                    MASK                : 32
                                    Default Gateway     : 10.88.88.1
                                    First DNS           : 170.82.180.18
                                    Backup DNS En       : 1.1.1.1
                                 */

                                await client.ExecuteCommandAsync("cd onu");
                                await client.ExecuteCommandAsync($"show wan_info slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID}");
                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 4000;
                                await Task.Delay(delayTime);

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("WPS CONNECT NAME")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("WPS CONNECT NAME".ToUpperInvariant()));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Nome_Servico = campos.ElementAtOrDefault(3);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("VLAN ID")))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("VLAN ID".ToUpperInvariant()));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.VLAN_ID = campos.ElementAtOrDefault(2);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("VLAN COS".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("VLAN COS".ToUpperInvariant()));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.VLAN_COS = campos.ElementAtOrDefault(2);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("GET IP MODE TYPE".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("GET IP MODE TYPE".ToUpperInvariant()));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.Modo_Conexao = campos.ElementAtOrDefault(4);
                                    retorno.Tipo_Conexao = campos.ElementAtOrDefault(4);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("STATIC IPADDR".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("STATIC IPADDR".ToUpperInvariant()));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.IP_Estatico = campos.ElementAtOrDefault(2);
                                    retorno.Tipo_IP = String.IsNullOrEmpty(campos.ElementAtOrDefault(2)) ? "Não informado" : retorno.IP_Estatico.Contains(":") ? "IPV6" : "IPV4";
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("MASK".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("MASK".ToUpperInvariant()));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.IP_Mascara = campos.ElementAtOrDefault(1);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Default Gateway".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Default Gateway".ToUpperInvariant()));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.IP_Gateway = campos.ElementAtOrDefault(2);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("First DNS".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("First DNS".ToUpperInvariant()));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.DNS_Master = campos.ElementAtOrDefault(2);
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Backup DNS En".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("Backup DNS En".ToUpperInvariant()));
                                    var campos = linha.Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.DNS_Slave = campos.ElementAtOrDefault(3);
                                }
                                #endregion

                                #region Informações de PPPOE
                                /*BR-PA-CAH-CCL-OLT-03-FIBERHOME\onu\lan# show onu_service slot 16 pon 1 onu 2

                                -----------------------------  WAN CONFIG  -----------------------------------
                                ID:1  NAME:1_INTERNET_R_VID_109  MODE:inter  TYPE:route  NAT:en  QOS:dis

                                WV_M:tran  WV:109 COS:0 TV_M:dis TV:null COS:null SV_M:dis TPID:33024 SV:null COS:null

                                ADDR_M:pppoe  PROXY_M:dis  DIAL_MODE:auto

                                PPPOE_NAME:yago.dias  PPPOE_PSW:******  PPPOE_SER_NAME:null

                                WAN_BAND_PORT:FE1 FE2 FE3 FE4 SD1
                                 */
                                await client.ExecuteCommandAsync("cd lan");
                                await client.ExecuteCommandAsync($"show onu_service slot {onu.IdSlot} pon {onu.IdPon} onu {onu.OntID}");
                                delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 4000;
                                await Task.Delay(delayTime);
                                resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("NAT".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("NAT".ToUpperInvariant())).Split(' ', '\t').ToList();
                                    var campos = linha.First(p => p.ToUpperInvariant().Contains("NAT".ToUpperInvariant())).Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.NAT = TratarCampoGetConfigEquipamento(campos.ElementAtOrDefault(1));
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("QOS".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("QOS".ToUpperInvariant())).Split(' ', '\t').ToList();
                                    var campos = linha.First(p => p.ToUpperInvariant().Contains("QOS".ToUpperInvariant())).Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.QoS = TratarCampoGetConfigEquipamento(campos.ElementAtOrDefault(1));
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("PPPOE_NAME".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("PPPOE_NAME".ToUpperInvariant())).Split(' ', '\t').ToList();
                                    var campos = linha.First(p => p.ToUpperInvariant().Contains("PPPOE_NAME".ToUpperInvariant())).Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.PPPOE_Usuario = TratarCampoGetConfigEquipamento(campos.ElementAtOrDefault(1));
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("PPPOE_SER_NAME".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("PPPOE_SER_NAME".ToUpperInvariant())).Split(' ', '\t').ToList();
                                    var campos = linha.First(p => p.ToUpperInvariant().Contains("PPPOE_SER_NAME".ToUpperInvariant())).Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.PPPOE_ServicoName = TratarCampoGetConfigEquipamento(campos.ElementAtOrDefault(1));
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("PROXY_M".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("PROXY_M".ToUpperInvariant())).Split(' ', '\t').ToList();
                                    var campos = linha.First(p => p.ToUpperInvariant().Contains("PROXY_M".ToUpperInvariant())).Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.PPPOE_Proxy = TratarCampoGetConfigEquipamento(campos.ElementAtOrDefault(1));
                                }

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("DIAL_MODE".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.ToUpperInvariant().Contains("DIAL_MODE".ToUpperInvariant())).Split(' ', '\t').ToList();
                                    var campos = linha.First(p => p.ToUpperInvariant().Contains("DIAL_MODE".ToUpperInvariant())).Split(' ', ':', '\t').ToList();
                                    campos.RemoveAll(s => s == " " || s == String.Empty);
                                    retorno.PPPOEMODE = TratarCampoGetConfigEquipamento(campos.ElementAtOrDefault(1));
                                }
                                #endregion

                                retorno.Porta_ONU = Convert.ToString(onu.OntID);
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
                                //client.MillisecondReadDelay = 500;
                                await Task.Delay(2000);

                                //var logado = await LoginFiberhomeAsyncTL1(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                //if (logado["STATUS"].Equals("COMPLD"))

                                if (onu.Tipo_Identificacao == Sinapse.Model.enumerados.ONU_Tipo_Identificacao.MAC)
                                {

                                    await client.ExecuteCommandAsync($"LST-DEVINFO::OLTID={olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Mac}:CTAG::;");
                                }
                                else
                                {
                                    await client.ExecuteCommandAsync($"LST-DEVINFO::OLTID={olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=LOID,ONUID={onu.Serial}:CTAG::;");
                                }
                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 6000;
                                await Task.Delay(delayTime);
                                var resposta = await client.ReadAsync();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

                                if (resposta.ToUpperInvariant().Contains("DENY".ToUpperInvariant()))
                                {
                                    await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                    throw new EAnielException("Falha ao executar o comando na OLT. Por favor, tente novamente mais tarde.");
                                }

                                try
                                {
                                    var m = TratarRetornoInformacaoHardware(resposta);
                                    if (m.ContainsKey("HARDWARE") && m["HARDWARE"] is List<Dictionary<string, string>> hardwareList && hardwareList.Count > 0)
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
                                catch (Exception ex)
                                {
                                    /*var m = TratarMensagemPadrao(resposta);                                        
                                    throw new EAnielException($"Status.: {RetornoErroTratado(m)}");*/
                                    LogIntegracaoAtivacao(ex);
                                    throw new EAnielException(ex.Message);
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
                        using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                        {
                            await client.Connect();
                            if (client.IsConnected())
                            {
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(600);
                                bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                                bool terminal_configurado = await SetTerminalLength(client);

                                /*BR-PA-CAH-CCL-OLT-03-FIBERHOME\onu# show onu_ver slot 16 pon 1
                                ----- ONU Version Info.  SLOT=16,PON=1,ITEM=3 -----
                                ONU CFG_TYPE         REAL_TYPE        SOFT_VER         HARD_VER         CFG_VER
                                ---------------------------------------------------------------------------------
                                1   5506-01-A1       5506-01-A1       V2.1.1           HZ660.1B
                                2   5506-04-FA       5506-04-FA       RP2815           WKE2.094.326A01
                                3   HG6145E          HG6145E          V1.1.20P3N6B     V1.1.01
                                 */

                                if (this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                                {
                                    await client.ExecuteCommandAsync("cd gpononu");
                                    await client.ExecuteCommandAsync($"show onu_ver slot {onu.IdSlot} link {onu.IdPon}");
                                    int delay = olt.Delay.HasValue ? (int)olt.Delay.Value : 5000;

                                    var respostaComando = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", respostaComando));

                                    if (respostaComando.Exists(p => p.ToUpperInvariant().Contains("not match!".ToUpperInvariant())))
                                    {
                                        await client.ExecuteCommandAsync("cd gpononu");
                                        await client.ExecuteCommandAsync($"show onu_ver slot {onu.IdSlot} link {onu.IdPon}");
                                    }
                                }
                                else
                                {
                                    await client.ExecuteCommandAsync("cd onu");
                                    await client.ExecuteCommandAsync($"show onu_ver slot {onu.IdSlot} pon {onu.IdPon}");
                                }
                                
                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 5000;
                                await Task.Delay(delayTime);
                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

                                int index = resposta.FindIndex(p => p.ToUpperInvariant().Contains("ONU Version Info".ToUpperInvariant()));

                                if (index > 0)
                                {
                                    resposta.RemoveRange(0, index);
                                    resposta.RemoveAll(p => p == "");
                                    var lista_formatada = resposta.Skip(3).ToList();

                                    if (lista_formatada.Count >= 1)
                                    {
                                        foreach (var linha in lista_formatada)
                                        {
                                            var campos = linha.Split(' ', '\t').ToList();
                                            campos.RemoveAll(s => s == " " || s == String.Empty);

                                            if (this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                                            {
                                                var idONU = "";

                                                if(onu.OntID.HasValue && onu.OntID.Value < 10)
                                                {
                                                    idONU = $"0{Convert.ToString(onu.OntID)}";
                                                }

                                                if (campos.ElementAtOrDefault(0).Equals(idONU))
                                                {
                                                    retorno.Modelo = campos.ElementAtOrDefault(2);
                                                    retorno.Versao_Software = campos.ElementAtOrDefault(3);
                                                    retorno.Id_Onu = onu.OntID.HasValue ? onu.OntID.Value : 0;
                                                    retorno.IP = "-";

                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (campos.ElementAtOrDefault(0).Equals(Convert.ToString(onu.OntID)))
                                                {
                                                    retorno.Modelo = campos.ElementAtOrDefault(2);
                                                    retorno.Versao_Software = campos.ElementAtOrDefault(3);
                                                    retorno.Id_Onu = onu.OntID.HasValue ? onu.OntID.Value : 0;
                                                    retorno.IP = "-";

                                                    break;
                                                }
                                            }
                                           
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
            }
            catch (Exception ex)
            {
                LogIntegracaoAtivacao(ex);
                throw new EAnielException(ex.Message);
            }

            return retorno;
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

                var idLog = enviarLog($"[GetIDONU] [FIBERHOME A1] -> ONU Serial: {onu.Serial}, OLT: {this.olt.Descricao}", numUsuario, numOs, null);
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

        public async Task<GetDadosONUOLT> GetDadosONUnaOLT(string Serial_Convertido, string serial, string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var DadosONU = new GetDadosONUOLT();
            var respostaOLT = new List<string>(); //Utilizado para gravar os retornos da OLT nos logs

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
                            
                            await Task.Delay(5000);
                            int numTentativa = 0;
                            List<string> resposta = null;

                            while (numTentativa <= 5)
                            {
                                resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

                                if (resposta.Any(p => p.Contains(";".ToUpperInvariant())))
                                {
                                    break;
                                }

                                numTentativa++;
                                await Task.Delay(10000);
                            }

                            //var resposta = await client.ReadAsync();
                           
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
                else
                {
                    using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                    {
                        await client.Connect();
                        if (client.IsConnected())
                        {
                            //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                            //client.MillisecondReadDelay = delayTime;
                            await Task.Delay(600);
                            bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                            bool terminal_configurado = await SetTerminalLength(client);

                            if (this.olt.Versao_OLT.HasValue && this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                            {
                                /*
                                SER-VAZ01-PO-AN551-003# cd gpononu
                                SER-VAZ01-PO-AN551-003\gpononu# show onu-authinfo phy-id ZTEGd14f7a43
                                ONU:               13-1-1
                                OnuType:           AN5506-SFU (857)
                                Physical ID:       ZTEGd14f7a43
                                Logical ID:
                                Logical password:
                                Physical password:
                                Auth-Status:       Authed
                                Auth-Type:         MAC
                                SER-VAZ01-PO-AN551-003\gpononu# show onu-authinfo phy-id ZTEGd14f7a45
                                [Error -506]: Onu is not authcated.
                                Command executes failed.
                                SER-VAZ01-PO-AN551-003\gpononu#    
                                                                */
                                string vendorSerial = !string.IsNullOrEmpty(serial) && serial.Length >= 4
                                      ? serial.Substring(0, 4)
                                      : string.Empty;

                                string deviceSerial = !string.IsNullOrEmpty(serial) && serial.Length > 4
                                    ? serial.Substring(4)
                                    : string.Empty;

                                string vendorSerialConvertido = !string.IsNullOrEmpty(Serial_Convertido) && Serial_Convertido.Length >= 4
                                    ? Serial_Convertido.Substring(0, 4)
                                    : string.Empty;

                                string deviceSerialConvertido = !string.IsNullOrEmpty(Serial_Convertido) && Serial_Convertido.Length > 4
                                    ? Serial_Convertido.Substring(4)
                                    : string.Empty;

                                string serialTratado = $"{vendorSerial?.ToUpper()}{deviceSerial?.ToLower()}";
                                string serialConvertidoTratado = $"{vendorSerialConvertido?.ToUpper()}{deviceSerialConvertido?.ToLower()}";



                                await client.ExecuteCommandAsync("cd gpononu");
                                await client.ExecuteCommandAsync($"show onu-authinfo phy-id {serialTratado}");
                                await Task.Delay(1500);

                                var idLog = enviarLog($"[GetDadosONUnaOLT] [FIBERHOME A1] -> {this.olt.Descricao} - SERIAL {serial} - SERIAL CONVERTIDO {Serial_Convertido}", numUsuario, numOs, null);
                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);
                                respostaOLT.Add(string.Join("\n", resposta));

                                if (resposta.Any(p => p.Contains("ONU:".ToUpperInvariant())) &&
                                   !resposta.Any(p => p.Contains("Error".ToUpperInvariant())))
                                
                                {
                                    var linhaOnu = resposta.FirstOrDefault(l => l.TrimStart().StartsWith("ONU:"));

                                    if (!string.IsNullOrEmpty(linhaOnu))
                                    {
                                        // Get only the numbers part after "ONU:"
                                        var parteNumerica = linhaOnu.Split(':')[1].Trim(); // "13-1-1"
                                        var partes = parteNumerica.Split('-');

                                        if (partes.Length == 3)
                                        {
                                            string slot = partes[0];
                                            string pon = partes[1];
                                            string ontId = partes[2];

                                            DadosONU.Slot = slot;
                                            DadosONU.Pon = pon;
                                            DadosONU.Onu_Id = ontId;
                                            DadosONU.Onu_Encontrada = true;
                                            DadosONU.Serial = serial;

                                        }
                                    }

                                }
                                else if (DadosONU.Onu_Encontrada == false && this.olt.ConverteOlt == Sinapse.Model.enumerados.Status_Default.Ativo &&
                                !string.Equals(serial, Serial_Convertido, StringComparison.OrdinalIgnoreCase))
                                {
                                    await client.ExecuteCommandAsync($"show onu-authinfo phy-id {serialConvertidoTratado}");
                                    await Task.Delay(1500);

                                    resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);
                                    respostaOLT.Add(string.Join("\n", resposta));

                                    if (resposta.Exists(p => p.ToUpperInvariant().Contains("OnuType".ToUpperInvariant()) ||
                                        p.ToUpperInvariant().Contains("Physical ID".ToUpperInvariant())))
                                    {
                                        var linhaOnu = resposta.FirstOrDefault(l => l.TrimStart().StartsWith("ONU:"));

                                        if (!string.IsNullOrEmpty(linhaOnu))
                                        {
                                            // Get only the numbers part after "ONU:"
                                            var parteNumerica = linhaOnu.Split(':')[1].Trim(); // "13-1-1"
                                            var partes = parteNumerica.Split('-');

                                            if (partes.Length == 3)
                                            {
                                                string slot = partes[0];
                                                string pon = partes[1];
                                                string ontId = partes[2];

                                                DadosONU.Slot = slot;
                                                DadosONU.Pon = pon;
                                                DadosONU.Onu_Id = ontId;
                                                DadosONU.Onu_Encontrada = true;
                                                DadosONU.Serial = Serial_Convertido;

                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                await client.ExecuteCommandAsync("cd onu");
                                await client.ExecuteCommandAsync($"show onu-info by {serial?.ToUpper()}");
                                await Task.Delay(1500);

                                /*
                                OLT-LSO-CEN-01# cd onu
                                OLT-LSO-CEN-01\onu# show onu-info by FHTT9ba02298
                                    ----- SLOT  PON  ONU  STATE -----
                                    -----  13    1   9     Auth -----
                                OLT-LSO-CEN-01\onu#    
                                                                */
                                var idLog = enviarLog($"[GetDadosONUnaOLT] [FIBERHOME A1] -> {this.olt.Descricao} - SERIAL {serial} - SERIAL CONVERTIDO {Serial_Convertido}", numUsuario, numOs, null);
                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);
                                respostaOLT.Add(string.Join("\n", resposta));

                                if (resposta.Exists(p => p.ToUpperInvariant().Contains("SLOT".ToUpperInvariant())))
                                {
                                    string linhaDados = resposta
                                    .SkipWhile(p => !p.ToUpperInvariant().Contains("SLOT".ToUpperInvariant()))
                                    .Skip(1)
                                    .FirstOrDefault(p =>
                                        p.Contains("-----") &&
                                        p.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length >= 4);

                                    if (linhaDados != null)
                                    {

                                        var parts = linhaDados
                                        .Replace("-", "") 
                                        .Trim()
                                        .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);


                                        if (parts.Length >= 4)
                                        {
                                            var slot = parts[0];
                                            var pon = parts[1];  
                                            var onu = parts[2];  


                                            DadosONU.Slot = slot;
                                            DadosONU.Pon = pon;
                                            DadosONU.Onu_Id = onu;
                                            DadosONU.Serial = serial;
                                            DadosONU.Onu_Encontrada = true;
                                        }       
                                    }
                                }
                                else if (DadosONU.Onu_Encontrada == false && this.olt.ConverteOlt == Sinapse.Model.enumerados.Status_Default.Ativo &&
                                !string.Equals(serial, Serial_Convertido, StringComparison.OrdinalIgnoreCase))
                                {
                                    await client.ExecuteCommandAsync($"show onu-info by {Serial_Convertido?.ToUpper()}");
                                    await Task.Delay(1500);
                                    resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);
                                    respostaOLT.Add(string.Join("\n", resposta));

                                    if (resposta.Exists(p => p.ToUpperInvariant().Contains("SLOT".ToUpperInvariant())))
                                    {
                                        string linhaDados = resposta
                                        .FirstOrDefault(p => p.Contains("-----") && p.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length >= 5);

                                        if (linhaDados != null)
                                        {

                                            var parts = linhaDados
                                            .Replace("-", "")
                                            .Trim()
                                            .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);


                                            if (parts.Length >= 4)
                                            {
                                                var slot = parts[0];
                                                var pon = parts[1];
                                                var onu = parts[2];


                                                DadosONU.Slot = slot;
                                                DadosONU.Pon = pon;
                                                DadosONU.Onu_Id = onu;
                                                DadosONU.Serial = serial;
                                                DadosONU.Onu_Encontrada = true;
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
                                            Pon = "",
                                            Serial = serial
                                        };
                                    }
                                }

                                var respostasLog = $"Respostas:\n{string.Join("\n", respostaOLT)}";
                                await enviarLog($"{respostasLog}", numUsuario, numOs, idLog.Result);



                            }


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

        public async Task<TesteONUModels> GetDadosComplementaresONU(ConfigurarONUModel onu)
        {
            var retornoDados = new TesteONUModels();
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
                                /* LST-ONUVERSION::OLTID=177.128.221.5,PONID=NA-NA-11-14,ONUIDTYPE=MAC,ONUID=FHTT11002430:CTAG::;


                                   FH_187.95.224.18 2025-10-13 16:48:43
                                M  CTAG COMPLD
                                   total_blocks=1
                                   block_number=1
                                   block_records=1

                                list of ONU version
                                --------------------------------------------------------------------------------
                                OLTID   PONID   ONUID   SWVER   HWVER
                                177.128.221.5   1-1-11-14       FHTT11002430    RP2520  ZS-2.134.321B7G
                                --------------------------------------------------------------------------------
                                                                                                                    */
                                await client.ExecuteCommandAsync($"LST-ONUVERSION::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon},ONUIDTYPE=MAC,ONUID={onu.Serial}:CTAG::;");

                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 4000;
                                await Task.Delay(delayTime);

                                var resposta = await client.ReadAsync();
                                LogIntegracaoAtivacao(string.Join("\n", resposta));

                                /*-----------------------------------------------------------------
                                 ONUID AdminState OperStat AUTH AUTHINFO ONUIP LASTOFFTIME
                                 5       UP          UP    LOID aaa_bbb   --   0000-00-00 00:00:00
                                 -----------------------------------------------------------------*/

                                if (resposta.ToUpperInvariant().Contains("DENY".ToUpperInvariant()))
                                {
                                    await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");
                                    throw new EAnielException("Falha ao executar o comando na OLT. Por favor, tente novamente mais tarde.");
                                }

                                try
                                {
                                    var m = TratarRetornoGetDadosComplementaresONU(resposta);

                                    //retorno = m["ONU"] as DiagnosticoONU;


                                    if (m.ContainsKey("ONU") && m["ONU"] is List<Dictionary<string, object>> onuList && onuList.Count > 0)
                                    {

                                        var firstOnuDict = onuList[0];


                                        var diagnostico = new TesteONUModels
                                        {
                                            Versao_Firmware_Ativo = firstOnuDict["SWVER"].ToString(),
                                            Numero_Versao = firstOnuDict["HWVER"].ToString(),
                                            MAC = firstOnuDict["ONUID"].ToString()
                                        };

                                        retornoDados = diagnostico;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    //var m = TratarMensagemPadrao(resposta);
                                    //throw new EAnielException($"Status.: {RetornoErroTratado(m)}");

                                    LogIntegracaoAtivacao(ex);
                                    throw new EAnielException(ex.Message);
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
                        using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                        {
                            await client.Connect();
                            if (client.IsConnected())
                            {
                                if (this.olt.Versao_OLT.HasValue && this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                                {
                                    throw new NotImplementedException();
                                }
                                else
                                {
                                    await Task.Delay(1500);
                                    bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                                    bool terminal_configurado = await SetTerminalLength(client);

                                    await client.ExecuteCommandAsync("cd onu");
                                    await client.ExecuteCommandAsync($"show onu_ver slot {onu.IdSlot} pon {onu.IdPon}");


                                    int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 2000;
                                    await Task.Delay(delayTime);
                                    var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));

                                    if(!onu.OntID.HasValue)
                                    {
                                        throw new Exception("ID da ONU (ONT ID) não consta nos dados do Connect.");
                                    }

                                    var line = resposta.FirstOrDefault(l => l.TrimStart().StartsWith($"{onu.OntID} "));

                                    if (line != null)
                                    {
                                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                        if (parts.Length <= 6)
                                        {

                                            var diagnostico = new TesteONUModels
                                            {
                                                Versao_Firmware_Ativo = parts[3],
                                                Numero_Versao = parts[4],
                                                MAC = onu.Serial
                                            };

                                            retornoDados = diagnostico;

                                            /*var info = new
                                            {
                                                OnuId = parts[0],
                                                CfgType = parts[1],
                                                RealType = parts[2],
                                                SoftVer = parts[3],
                                                HardVer = parts[4],
                                                CfgVer = parts[5]
                                            };*/


                                        }
                                        else
                                        {
                                            throw new Exception($"Linha encontrada, mas o formato é inesperado: {line}");
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception($"ONU de ID {onu.Id} não foi encontrada na listagem retornada pela OLT.");
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
                retornoDados.MotivoReprovaTesteDados = ex.Message;
            }          

                return retornoDados;
        }

        public async Task<GetStatusADMeOperacionalONU> GetStatusADMeOperacionalONU(ConfigurarONUModel onu)
        {
            var retorno = new GetStatusADMeOperacionalONU();

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

                                //var logado = await LoginFiberhomeAsyncTL1(client, this.olt.LoginTL1, this.olt.SenhaTL1);
                                //if (logado["STATUS"].Equals("COMPLD"))

                                    await client.ExecuteCommandAsync($"LST-ONUSTATE::OLTID={this.olt.IP},PONID=NA-NA-{onu.IdSlot}-{onu.IdPon}:CTAG::;");
                                                          
                                int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 4000;
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

                                 var linha = resposta.FirstOrDefault(p =>
                                         p.ToUpperInvariant().Contains(onu.Serial.ToUpperInvariant())
                                 );


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
                                        retorno.Status_Adm = true;
                                    }
                                    else
                                    {
                                        retorno.Status_Adm = false;
                                    }

                                    if (campos.ElementAtOrDefault(2).ToUpperInvariant().Contains("UP".ToUpperInvariant()))
                                    {
                                        retorno.Status_Ope = true;
                                    }
                                    else
                                    {
                                        retorno.Status_Ope = false;
                                    }

                                    retorno.Motivo_Erro = String.Empty;



                                    await client.ExecuteCommandAsync($"LOGOUT:::CTAG::;");

                                }
                                else
                                {
                                    throw new EAnielException("Erro ao estabelecer conexão.");
                                }


                            }
                        }
                    }
                    else
                    {
                        using (var client = new RemoteConnection(this.olt.IP, porta, this.olt.Login, this.olt.Senha, protocolo, true))
                        {
                            await client.Connect();
                            if (client.IsConnected())
                            {
                                //int delayTime = olt.Delay.HasValue ? (int)olt.Delay.Value : 500;
                                //client.MillisecondReadDelay = delayTime;
                                await Task.Delay(600);
                                bool logado = await LoginFiberhomeAsyncTelnetRemote(client, this.olt.Senha);
                                bool terminal_configurado = await SetTerminalLength(client);

                                if (this.olt.Versao_OLT.HasValue && this.olt.Versao_OLT.Value == Versao_OLT.RP700)
                                {
                                    await client.ExecuteCommandAsync("cd gpononu");
                                    await client.ExecuteCommandAsync($"show online slot {onu.IdSlot} link {onu.IdPon}");

                                    /*ONLINE ONU TOTAL NUM = 1
                                    ONUID   ONUTYPE         SN              PASSWORD    LOGICAL SN
                                    -----   ------------    ------------    ----------  ---------------------
                                    01      AN5506-04-B2G   FHTT00030405 ,
                                    */

                                    var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));

                                        if (resposta.Exists(p => p.ToUpperInvariant().Contains(onu.Serial.ToUpperInvariant())))
                                        {

                                            retorno.Status_Ope = true;
                                            retorno.Status_Adm = true;

                                        }
                                        else
                                        {
                                            retorno.Status_Ope = false;
                                            retorno.Status_Adm = false;
                                        }
                                    
                                }
                                else
                                {
                                    await client.ExecuteCommandAsync("cd onu");
                                    await client.ExecuteCommandAsync($"show authorization slot {onu.IdSlot} pon {onu.IdPon}");
                                    await Task.Delay(1500);
                                    /*
                                    CAMPO OST => up = Online / dn = Offline

                                    -----  ONU Auth Table, Total ITEM = 1 -----

                                    A: Authorized  P: Preauthorized  R: System Reserved

                                    -----  ONU Auth Table, SLOT = 16, PON = 1, ITEM = 1 -----
                                    Slot Pon Onu OnuType        ST Lic OST PhyId        PhyPwd     LogicId                  LogicPwd
                                    ---- --- --- -------------- -- --- --- ------------ ---------- ------------------------ ------------
                                    16   1   1   HG6145E        A  1   up  ZTEGc4b4be9d
                                    Command execute success.                                
                                    */

                                    var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);


                                        if (resposta.Exists(p => p.ToUpperInvariant().Contains(onu.Serial.ToUpperInvariant())))
                                        {
                                            var linha = resposta.First(p => p.ToUpperInvariant().Contains(onu.Serial.ToUpperInvariant()));
                                            var campos_onu = linha.Split(' ').ToList();
                                            campos_onu.RemoveAll(s => s == " " || s == String.Empty);


                                            if (campos_onu[6].ToUpperInvariant().Contains("up".ToUpperInvariant()) || campos_onu[6].Contains("1"))
                                            {
                                                retorno.Status_Ope = true;
                                                retorno.Status_Adm = true;

                                            }
                                            else
                                            {

                                                retorno.Status_Ope = false;
                                                retorno.Status_Adm = false;
                                            }

                                          
                                        }
                                        else
                                        {
                                            retorno.Status_Adm = false;
                                            retorno.Status_Ope = false;
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
                return new GetStatusADMeOperacionalONU
                {
                    Status_Adm = false,
                    Status_Ope = false,
                    Motivo_Erro = ex.Message,

                };
            }
            //});

            return retorno;
        }

        #region metodos-privados

        /// <summary>
        ///     Função que pega o proximo Id disponível para autorização (Só implementado para Telnet no momento).
        /// </summary>
        private async Task<Int32> GetProximoOnuIdDisp(RemoteConnection client, long numSlot, long numPon, Versao_OLT? versao)
        {
            Int32 IdOnu = 1;

            //Lista os ids disponíveis para alocação
            //BR-PA-CAH-CCL-OLT-03-FIBERHOME\onu# show authorization slot {numSlot} pon {numPon}

            //RETORNO SEM IDS:                                   
            //Command execute success.

            //RETORNO COM IDS:
            /*-----  ONU Auth Table, Total ITEM = 1 -----

            A: Authorized  P: Preauthorized  R: System Reserved

            -----  ONU Auth Table, SLOT = 16, PON = 1, ITEM = 1 -----
            Slot Pon Onu OnuType        ST Lic OST PhyId        PhyPwd     LogicId                  LogicPwd
            ---- --- --- -------------- -- --- --- ------------ ---------- ------------------------ ------------
            16   1   1   5506-04-FA     A  1   up  FHTT98b4bd38
            Command execute success.*/

            if(versao.HasValue)
            {
                if(versao.Value == Versao_OLT.RP700)
                {
                    await client.ExecuteCommandAsync($"show authorization slot {numSlot} link {numPon}");
                }
                else if(versao.Value == Versao_OLT.RP1200 || versao.Value == Versao_OLT.RP1800)
                {
                    await client.ExecuteCommandAsync($"show authorization slot {numSlot} pon {numPon}");
                }
                
            }
            else
            {
                await client.ExecuteCommandAsync($"show authorization slot {numSlot} pon {numPon}");
            }
           
            await Task.Delay(1500);
            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
            LogIntegracaoAtivacao(string.Join("\n", resposta));
            resposta.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);

            var lista_headers_slots_pons = resposta.FindAll(p => p.ToUpperInvariant().Contains("ONU Auth Table".ToUpperInvariant()));

            if (lista_headers_slots_pons.Count > 0)
            {
                foreach (var header in lista_headers_slots_pons)
                {
                    int? index_header = null;
                    index_header = resposta.IndexOf(header);

                    var lista_formatada = resposta.Skip(index_header.Value + 1).ToList();
                    int proximo_header_index = 0;

                    if(!versao.HasValue || versao.Value != Versao_OLT.RP700)
                    {
                        proximo_header_index = lista_formatada.FindIndex(p => p.ToUpperInvariant().Contains("ONU Auth Table".ToUpperInvariant())
                       || p.ToUpperInvariant().Contains("Command execute success".ToUpperInvariant()));
                    }
                    else if(versao.HasValue && versao.Value == Versao_OLT.RP700)
                    {
                        proximo_header_index = lista_formatada.FindIndex(p => p.ToUpperInvariant().Contains("ONU Auth Table".ToUpperInvariant())
                       || p.ToUpperInvariant().Contains("A: Authorized  P: Preauthorized  R: System Reserved".ToUpperInvariant()));
                    }


                    if (proximo_header_index > 0)
                    {
                        lista_formatada = lista_formatada.GetRange(0, proximo_header_index).Skip(2).ToList();

                        List<Int32> lista_ids_onu = new List<int>();

                        foreach (var onu in lista_formatada)
                        {
                            var campos_onu = onu.Split(' ').ToList();
                            campos_onu.RemoveAll(s => s == " " || s == String.Empty);

                            var id_onu = 0;
                            if (Int32.TryParse(campos_onu[2], out id_onu))
                                lista_ids_onu.Add(id_onu);
                        }

                        if (lista_ids_onu.Count > 0)
                        {
                            //Cada PON pode ter de 1-128 ONUs conectadas.
                            var lista_range = Enumerable.Range(1, 128).ToList();

                            //Pega o primeiro Id disponível a partir da exceção (Tem na lista_range mas não existe na lista_ids_onu).
                            IdOnu = lista_range.Except(lista_ids_onu).First();
                        }
                        else
                        {
                            //Se lista está vazia, não há nenhuma ONU conectada/autorizada na PON informada.
                            IdOnu = 1;
                        }
                    }
                    else
                    {
                        //Se lista de headers está vazia, não há nenhuma ONU conectada/autorizada na PON informada.
                        IdOnu = 1;
                    }
                }
            }

            return IdOnu;
        }

        /// <summary>
        ///     Configura o terminal da OLT para não haver quebra de linha ou necessidade de scroll manual.
        ///     Precisa ser utilizado antes de qualquer comando na OLT.
        /// </summary>
        private async Task<bool> SetTerminalLength(RemoteConnection client)
        {
            bool sucesso = true;
            await client.ExecuteCommandAsync("cd service");
            await client.ExecuteCommandAsync("terminal length 0");
            await client.ExecuteCommandAsync("cd ..");
            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

            if (resposta.Exists(p => p.ToUpperInvariant().Contains("error".ToUpperInvariant()) || p.ToUpperInvariant().Contains("Unknown command".ToUpperInvariant())))
                sucesso = false;

            return sucesso;
        }

        /// <summary>
        ///     Configura o terminal da OLT para não haver quebra de linha ou necessidade de scroll manual.
        ///     Precisa ser utilizado antes de qualquer comando na OLT.
        ///     Utilizar este método em conexões que utilizam Client.
        /// </summary>
        private async Task<bool> SetTerminalLengthClient(Client client)
        {
            bool sucesso = true;
            await client.WriteLineAsync("cd service");
            await client.WriteLineAsync("terminal length 0");
            await client.WriteLineAsync("cd ..");
            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

            if (resposta.Exists(p => p.ToUpperInvariant().Contains("error".ToUpperInvariant()) || p.ToUpperInvariant().Contains("Unknown command".ToUpperInvariant())))
                sucesso = false;

            return sucesso;
        }



        /* private Dictionary<string, string> LoginFiberhomeAsyncTL1(Client client, string login, string senha)
         {   

             client.WriteLineAsync(";");
             Task.Delay(2000).Wait();
             client.WriteLineAsync($"LOGIN:::CTAG::UN={login},PWD={senha};");
             String resultado = client.ReadAsync().Result;
             LogIntegracaoAtivacao(string.Join("\n", resultado));
             return TratarMensagemPadrao(resultado);

         } */

        private async Task<Response> LoginFiberhomeAsyncTL1(Client client, string login, string senha)
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

        private async Task<Response> LoginFiberhomeAsyncTL1Teste(RemoteConnection client, string login, string senha)
        {
            var response = new Response();
            await Task.Delay(2000);
            await client.ExecuteCommandAsync(";\n");
            await Task.Delay(2000);
            await client.ExecuteCommandAsync($"LOGIN:::CTAG::UN={login},PWD={senha};");
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

        private async Task<bool> LoginFiberhomeAsyncTelnet(Client client, string senha)
        {
            await client.WriteLineAsync("enable");
            await client.WriteLineAsync(senha);

            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
            LogIntegracaoAtivacao(string.Join("\n", resposta));

            return resposta.Exists(p => p.ToUpperInvariant().Contains("Bad Password".ToUpperInvariant()));
        }

        private async Task<bool> LoginFiberhomeAsyncTelnetRemote(RemoteConnection client, string senha)
        {
            await client.ExecuteCommandAsync("enable");
            await client.ExecuteCommandAsync(senha);

            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
            LogIntegracaoAtivacao(string.Join("\n", resposta));

            return resposta.Exists(p => p.ToUpperInvariant().Contains("Bad Password".ToUpperInvariant()));
        }

        private string TratarCampoGetConfigEquipamento(string campo)
        {
            if (string.IsNullOrEmpty(campo))
                return "Não informado";

            if (campo.ToUpperInvariant().Contains("dis".ToUpperInvariant()))
                return "Desabilitado";

            if (campo.ToUpperInvariant().Contains("en".ToUpperInvariant()))
                return "Habilitado";

            if (campo.ToUpperInvariant().Contains("null".ToUpperInvariant()))
                return "Não informado";

            return campo;
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
        private Dictionary<string, object> TratarRetornoInformacaoHardware(string resposta)
        {
            var lista = resposta.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var mensagem = new Dictionary<string, object>();
            /*mensagem.Add("LOG", lista[3].Trim());
            mensagem.Add("STATUS", lista[5].Split(' ', ' ').GetValue(lista[5].Split(' ', ' ').Length - 1));
            mensagem.Add("TOTAL BLOCKS", lista[7].Trim().Split('=')[1]);
            mensagem.Add("BLOCK NUMBER", lista[9].Trim().Split('=')[1]);
            mensagem.Add("BLOCK RECORDS", lista[11].Trim().Split('=')[1]);*/

            var hardware = new List<Dictionary<string, string>>();
            foreach (var line in lista.Skip(7)) // Pula as linhas onde há informações que não utilizamos.
            {
                if (line.Contains("-----")) // Finaliza o looping ao final da tabela retornada pela OLT
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(line)) // Pula linhas vazias.
                {
                    continue;
                }

                // Divide a linha em colunas.
                var columns = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();


                var dispositivo = new Dictionary<string, string>
                {
                    { "DEVNAME", columns.ElementAtOrDefault(0) ?? string.Empty },
                    { "DEVIP", columns.ElementAtOrDefault(1) ?? string.Empty },
                    { "DT", columns.ElementAtOrDefault(2) ?? string.Empty },
                    { "DEVER", columns.ElementAtOrDefault(3) ?? string.Empty },
                    { "MEM", columns.ElementAtOrDefault(4) ?? string.Empty },
                    { "CPU", columns.ElementAtOrDefault(5) ?? string.Empty },
                    { "TEMPERATURE", columns.ElementAtOrDefault(6) ?? string.Empty }
            };


                hardware.Add(dispositivo);
            }

            // Add the list to the main message dictionary
            mensagem.Add("HARDWARE", hardware);
            return mensagem;
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
            var lista = resposta.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var mensagem = new Dictionary<string, object>();
            var onu = new List<Dictionary<string, object>>();

            /* mensagem.Add("LOG", lista[3].Trim());
             mensagem.Add("STATUS", lista[5].Split(' ', ' ').GetValue(lista[5].Split(' ', ' ').Length - 1));
             mensagem.Add("TOTAL BLOCKS", lista[7].Trim().Split('=')[1]);
             mensagem.Add("BLOCK NUMBER", lista[9].Trim().Split('=')[1]);
             mensagem.Add("BLOCK RECORDS", lista[11].Trim().Split('=')[1]); */


            /*-----------------------------------------------------------
            ONUID RxPower RxPowerR TxPower TxPowerR CurrTxBias CurrTxBiasR Temperature TemperatureR Voltage VoltageR PTxPower PRxPower
            5       -9.76 Normal    1.76    Normal  15.80       Normal      62.37       Normal      3.40    Normal      --      --
            -----------------------------------------------------------*/
            var parsingTable = false;

            foreach (var line in lista.Skip(7))
            {
                if (line.Contains("ONUID")) // Começa a processar os dados da tabela retornada pelo terminal
                {
                    parsingTable = true;
                    continue;
                }

                if (line.Contains("-----") || line.Contains(";")) // Ao chegar no final da tabela, encerra o looping
                {
                    break;
                }

                if (!parsingTable || string.IsNullOrWhiteSpace(line)) // Pula linhas irrelevantes
                {
                    continue;
                }

                var columns = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (columns.Count < 13) // Valida se a linha possui pelo menos 13 colunas
                {
                    continue;
                }

                var dispositivo = new Dictionary<string, object>
                {
                    { "ONUID", columns.ElementAtOrDefault(0) },
                    { "RxPower", columns.ElementAtOrDefault(1) },
                    { "RxPowerR", columns.ElementAtOrDefault(2) },
                    { "TxPower", columns.ElementAtOrDefault(3) },
                    { "TxPowerR", columns.ElementAtOrDefault(4) },
                    { "CurrTxBias", columns.ElementAtOrDefault(5) },
                    { "CurrTxBiasR", columns.ElementAtOrDefault(6) },
                    { "Temperature", columns.ElementAtOrDefault(7) },
                    { "TemperatureR", columns.ElementAtOrDefault(8) },
                    { "Voltage", columns.ElementAtOrDefault(9) },
                    { "VoltageR", columns.ElementAtOrDefault(10) },
                    { "PTxPower", columns.ElementAtOrDefault(11) },
                    { "PRxPower", columns.ElementAtOrDefault(12) }
                };

                onu.Add(dispositivo);
            }

            mensagem.Add("ONU", onu);
            return mensagem;
        }

        private Dictionary<string, object> TratarRetornoGetDadosComplementaresONU(string resposta)
        {
            var lista = resposta.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var mensagem = new Dictionary<string, object>();
            var onu = new List<Dictionary<string, object>>();

            /*;LST-ONUVERSION::OLTID=177.128.221.5,PONID=NA-NA-11-14,ONUIDTYPE=MAC,ONUID=FHTT11002430:CTAG::;


                   FH_187.95.224.18 2025-10-13 16:48:43
                M  CTAG COMPLD
                   total_blocks=1
                   block_number=1
                   block_records=1

                list of ONU version
                --------------------------------------------------------------------------------
                OLTID   PONID   ONUID   SWVER   HWVER
                177.128.221.5   1-1-11-14       FHTT11002430    RP2520  ZS-2.134.321B7G
                --------------------------------------------------------------------------------
                */

            var parsingTable = false;

            foreach (var line in lista.Skip(7))
            {
                if (line.Contains("ONUID")) // Começa a processar os dados da tabela retornada pelo terminal
                {
                    parsingTable = true;
                    continue;
                }

                if (line.Contains("-----") || line.Contains(";")) // Ao chegar no final da tabela, encerra o looping
                {
                    break;
                }

                if (!parsingTable || string.IsNullOrWhiteSpace(line)) // Pula linhas irrelevantes
                {
                    continue;
                }

                var columns = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (columns.Count < 5) // Valida se a linha possui pelo menos 13 colunas
                {
                    continue;
                }

                var dispositivo = new Dictionary<string, object>
                {
                    { "ONUID", columns.ElementAtOrDefault(2) },
                    { "SWVER", columns.ElementAtOrDefault(3) },
                    { "HWVER", columns.ElementAtOrDefault(4) }
                };

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
            /* var lista = resposta.Split('\r', '\n');
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

        public async Task<string> VerificaIdONUBySerial(string serial)
        {
            string ontId = "";
            try
            {
                using (ISession session = HibernateUtil.GetSession())
                using (var onuRepo = new Conexao<ConfigurarONUModel>(session))
                {
                    var onu = await onuRepo.selectFirst(p => p.Serial == serial);
                    if (onu != null)
                    {
                        if (onu.OntID != null)
                        {
                            ontId = Convert.ToString(onu.OntID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return ontId;

        }

        public async Task<Response> ConfigurarONUAsync(ConfigurarONUModel onu, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
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