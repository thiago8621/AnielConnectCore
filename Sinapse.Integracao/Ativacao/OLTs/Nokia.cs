using Aniel.Connect.Core;
using Aniel.Connect.Model.nhibernate;
using log4net;
using NHibernate;
using NHibernate.Mapping;
using NHibernate.Type;
using PrimS.Telnet;
using Renci.SshNet;
using Renci.SshNet.Common;
using Sinapse.Core;
using Sinapse.DAO.repository;
using Sinapse.Integracao.Ativacao.Interfaces;
using Sinapse.Integracao.Ativacao.Models;
using Sinapse.Integracao.IntegracoesERP.Ixc.Models;
using Sinapse.Model;
using Sinapse.Model.enumerados;
using Sinapse.Util;
using Sinapse.Util.RemoteConnection;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services.Description;
using System.Web.UI.WebControls;




namespace Sinapse.Integracao.Ativacao.OLTs
{
    public class Nokia : IntegracaoAtivacao
    {
        private OLT _olt;

        public Nokia(OLT olt)
        {
            this._olt = olt;
        }

        public Response AutorizarONU(ConfigurarONUModel onu, string scriptOLT, string scriptONU)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? $"OPERAÇÃO REALIZADA SEM O.S -> SERIAL: {onu.Serial}" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var response = new Response();
            var result = false;


            try
            {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

                if (!String.IsNullOrEmpty(this._olt.IpTL1))
                {

                    //Coleta de ID para ser utilizado na ONU Nokia
                    //(conexão realizada via Telnet/SSH, com execução de comando em CLI).
                    onu.OntID = await GetProximoOnuIdDisp(onu.IdSlot.Value, onu.IdPon.Value);

                    if(onu.OntID.HasValue && onu.OntID.Value > 128)
                    {
                        throw new Exception($"Não há nenhum ID disponível para este equipamento no SLOT {onu.IdSlot.Value}, PON {onu.IdPon.Value}.");
                    }

                    using (RemoteConnection client = new RemoteConnection(this._olt.IpTL1, this._olt.PortaTL1_SSH, this._olt.LoginTL1, this._olt.SenhaTL1, Protocol.Ssh))
                    {
                        await client.Connect();
                        if (client.IsConnected())
                        {

                            await Task.Delay(15000);
                            var linhasScriptOLT = scriptOLT.Split('\n');
                            var idLog = enviarLog($"[AutorizarONUAsync] [NOKIA] -> ONU Serial: {onu.Serial}, OLT: {this._olt.Descricao}", numUsuario, numOs, null);
                            var linhasERespostas = new List<(string Linha, List<string> Resposta)>();
                            foreach (string linha in linhasScriptOLT)
                            {
                                string linha_formatada = null;
                                await Task.Delay(2000);
                                if (linha.ToUpperInvariant().Contains("<ONT_ID>".ToUpperInvariant()))
                                {
                                    /*await client.ExecuteCommandAsync(linha.Replace("<ONT_ID>", Convert.ToString(onu.OntID)));*/    
                                    
                                    if (onu.OntID.HasValue)
                                    {
                                        linha_formatada = linha.Replace("<ONT_ID>", Convert.ToString(onu.OntID.Value));
                                        await client.ExecuteCommandAsync(linha_formatada);
                                    }
                                    LogIntegracaoAtivacao(string.Join("\n", linha_formatada));
                                    
                                }
                                else
                                {
                                    await client.ExecuteCommandAsync(linha);
                                    LogIntegracaoAtivacao(string.Join("\n", linha));
                                }
                                   
                                


                           

                                var resposta = (await client.ReadAsync()).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                 linhasERespostas.Add((linha_formatada ?? linha, resposta.ToList()));


                                if (resposta.Any(line => line.ToUpperInvariant().Contains("The underlying facility has not been provisioned".ToUpperInvariant())))
                                {
                                        string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                             $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));


                                        await enviarLog(fullLogErro, numUsuario, numOs, idLog.Result);
                                        throw new Exception("A OLT retornou um erro durante o processo. Por favor, verifique script ou OLT e tente novamente.");
                             
                                }
                                else if (resposta.Any(line => line.ToUpperInvariant().Contains("DENY".ToUpperInvariant())))
                                {
                                        string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                         $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));
                                        await enviarLog(fullLogErro, numUsuario, numOs, idLog.Result);
                                        throw new Exception("A OLT retornou erro durante o processo. Por favor, verifique script e/ou OLT e tente novamente.");
                                }

                            }


                            if (!String.IsNullOrEmpty(scriptONU))
                            {
                                var configONU = await ConfigurarONUAsync(onu, scriptONU, numOs, numUsuario, enviarLog);

                                if (configONU.result == false)
                                {
                                    response.result = false;
                                    response.message = "A OLT retornou erro durante o processo. Por favor, verifique script e/ou OLT e tente novamente.";
                                    LogIntegracaoAtivacao(response.message);
                                }

                            }

                             string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
                                     $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));


                              await enviarLog(fullLog, numUsuario, numOs, idLog.Result);

                            response.result = true;
                                                   

                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }
                }

                #region Telnet && SSH
                else
                {
                    using (RemoteConnection client = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                    {
                        await client.Connect();
                        if (client.IsConnected())
                        {

                            onu.OntID = await GetProximoOnuIdDisp(client, onu.IdSlot.Value, onu.IdPon.Value);

                            if (onu.OntID.HasValue && onu.OntID.Value > 128)
                            {
                                throw new Exception($"Não há nenhum ID disponível para este equipamento no SLOT {onu.IdSlot.Value}, PON {onu.IdPon.Value}.");
                            }

                            var linhasScriptOLT = scriptOLT.Split('\n');
                            var idLog = enviarLog($"[AutorizarONUAsync] [NOKIA] -> ONU Serial: {onu.Serial}, OLT: {this._olt.Descricao}", numUsuario, numOs, null);
                            var linhasERespostas = new List<(string Linha, List<string> Resposta)>();
                            foreach (string linha in linhasScriptOLT)
                            {
                                await Task.Delay(800);
                                if (linha.ToUpperInvariant().Contains("<ONT_ID>".ToUpperInvariant()))
                                {
                                    await client.ExecuteCommandAsync(linha.Replace("<ONT_ID>", Convert.ToString(onu.OntID)));
                                    continue;
                                }
                                await client.ExecuteCommandAsync(linha);

                            

                            var resposta = await client.ReadAsync();
                            LogIntegracaoAtivacao(string.Join("\n", resposta));
                            linhasERespostas.Add((linha, resposta.Split('\r', '\n').ToList()));



                            if (resposta.ToUpperInvariant().Contains("error".ToUpperInvariant()))
                            {
                                    string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                                                 $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));

                                    await enviarLog(fullLogErro, numUsuario, numOs, idLog.Result);                                   
                                    throw new EAnielException("A OLT reportou um erro durante execução do script. Por favor, verifique o script de ativação.");

                            }
                            else if (resposta.ToUpperInvariant().Contains("Error : VLAN MGT".ToUpperInvariant()))
                            {
                                    string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                                                 $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));

                                    await enviarLog(fullLogErro, numUsuario, numOs, idLog.Result);
                                    throw new EAnielException("ONU provisionada porém VLAN está incorreta. Por favor, verifique o script de ativação ou o perfil da OLT.");
                            }
                            else if (resposta.ToUpperInvariant().Contains("invalid".ToUpperInvariant()))
                            {
                                    string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                                                 $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));

                                    await enviarLog(fullLogErro, numUsuario, numOs, idLog.Result);
                                    throw new EAnielException("Há um ou mais tokens inválidos no script. Por favor, verifique o script de ativação.");
                            }
                            else if (resposta.ToUpperInvariant().Contains("command is not complete".ToUpperInvariant()))
                            {
                                    string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                                                 $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));

                                    await enviarLog(fullLogErro, numUsuario, numOs, idLog.Result);
                                    throw new EAnielException("Há um ou mais comandos incompletos. Por favor, verifique o script de ativação.");
                            }

                                                          
                            }

                            if (scriptONU != null)
                            {
                                var configONU = await ConfigurarONUAsync(onu, scriptONU, numOs, numUsuario, enviarLog);

                                if (configONU.result == false)
                                {
                                    response = new Response
                                    {
                                        result = true,
                                        message = "Equipamento provisionado com sucesso, porém houve erro na configuração da ONU. Verifique o script de configuração."


                                    };
                                    //response.message = 
                                    //LogIntegracaoAtivacao(string.Join("\n", response.message));
                                }

                            }

                            response.result = true;
                            string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
                               $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));


                            await enviarLog(fullLog, numUsuario, numOs, idLog.Result);

                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }

                    }


                }
                #endregion

            }            
            catch (Exception ex)
            {
              
                LogIntegracaoAtivacao(ex);

                if (ex is SshOperationTimeoutException)
                {
                    throw new EAnielException("O limite de tempo de conexão com a OLT foi excedido.");
                }

                throw new EAnielException(ex.Message);
            }

            return response;


        }

        public async Task<Response> DesautorizarONU(ConfigurarONUModel onu, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? $"OPERAÇÃO REALIZADA SEM O.S -> SERIAL: {onu.Serial}" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var response = new Response();
            try
            {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

                #region Telnet && SSH

                using (RemoteConnection client = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
                {
                    await client.Connect();
                    if (client.IsConnected())
                    {
                        //Se o ID da ONU não estiver gravado na tabela
                        //(isso ocorre em casos onde a ONU foi importada da planilha)
                        //O método BuscaIdONU faz a busca do ID do equipamento e retorna este ID
                        //Sendo retornado o ID, irá atribuir o valor à onu.OntID
                        if(onu.OntID == 0 || onu.OntID == null)
                        {
                            var serial = "";
                            if(String.IsNullOrEmpty(onu.Serial))
                            {
                                serial = onu.Mac;
                            }
                            else
                            {
                                serial = onu.Serial;
                            }

                            var idONU = await BuscaIdONU(client, serial);

                            if(idONU != 0)
                            {
                                onu.OntID = idONU;
                            }
                        }

                        var idLog = enviarLog($"[DesautorizarONU] [NOKIA] -> ONU Serial: {onu.Serial}, OLT: {this._olt.Descricao}", numUsuario, numOs, null);
                        var comandos = new List<string>(); //Utilizado para gravar os comandos nos logs
                        var respostaOLT = new List<string>(); //Utilizado para gravar os retornos da OLT nos logs

                        await Task.Delay(500);

                        await client.ExecuteCommandAsync($"configure equipment ont interface 1/1/{onu.IdSlot}/{onu.IdPon}/{onu.OntID} admin-state down");
                        comandos.Add($"configure equipment ont interface 1/1/{onu.IdSlot}/{onu.IdPon}/{onu.OntID} admin-state down");
                        
                        await Task.Delay(1000);

                        await client.ExecuteCommandAsync($"configure equipment ont no interface 1/1/{onu.IdSlot}/{onu.IdPon}/{onu.OntID}");
                        comandos.Add($"configure equipment ont no interface 1/1/{onu.IdSlot}/{onu.IdPon}/{onu.OntID}");
                        await Task.Delay(1000);

                        var resposta = await client.ReadAsync();
                        respostaOLT.Add(string.Join("\n", resposta));
                        LogIntegracaoAtivacao(string.Join("\n", resposta));

                        //Junta os valores que foram adicionados em comandos e respostaOLT
                        var comandosLog = $"Comandos executados:\n{string.Join("\n", comandos)}";
                        var respostasLog = $"Respostas:\n{string.Join("\n", respostaOLT)}";

                        //Realiza gravação de logs
                        await enviarLog($"{comandosLog}\n\n{respostasLog}", numUsuario, numOs, idLog.Result);

                        if (resposta.ToUpperInvariant().Contains("Error : instance does not exist".ToUpperInvariant()))
                        {
                            throw new EAnielException("Equipamento não consta provisionado na OLT.");
                        }
                        else if (resposta.ToUpperInvariant().Contains("error".ToUpperInvariant()))
                        {
                            throw new EAnielException(resposta);
                        }
                        else if (resposta.ToUpperInvariant().Contains("invalid".ToUpperInvariant()))
                        {
                            throw new EAnielException("Há um ou mais tokens inválidos no comando. Não foi possível finalizar o processo.");
                        }
                        else if (resposta.ToUpperInvariant().Contains("command is not complete".ToUpperInvariant()))
                        {
                            throw new EAnielException("Há um ou mais comandos incompletos. Não foi possível finalizar o processo.");
                        }
                        else
                        {
                            response.result = true;
                        }
                    }
                    else
                    {
                        throw new EAnielException("Erro ao estabelecer conexão.");
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogIntegracaoAtivacao(ex);
                response.result = false;
                response.exception = $" {ex.Message}";
            }

            return response;
        }


public async Task<DiagnosticoONU> DiagnosticoONU(ConfigurarONUModel onu)
{
    var retorno = new DiagnosticoONU();

    var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
    var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

    string numSlot = Convert.ToString(onu.IdSlot);
    string numPON = Convert.ToString(onu.IdPon);
    string ontId = "";
       

    try
    {



        #region Telnet && SSH

        using (RemoteConnection client = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
        {
            await client.Connect();
            if (client.IsConnected())
            {

                        
                        //Se o ID da ONU não estiver gravado na tabela
                        //(isso ocorre em casos onde a ONU foi importada da planilha)
                        //O método BuscaIdONU faz a busca do ID do equipamento e retorna este ID
                        //Sendo retornado o ID, irá atribuir o valor à onu.OntID
                        if (onu.OntID == 0 || onu.OntID == null)
                        {
                            var serial = "";
                            if (String.IsNullOrEmpty(onu.Serial))
                            {
                                serial = onu.Mac;
                            }
                            else
                            {
                                serial = onu.Serial;
                            }

                            var idONU = await BuscaIdONU(client, serial);

                            if (idONU != 0)
                            {
                                onu.OntID = idONU;
                                ontId = Convert.ToString(onu.OntID);
                            }
                          
                                
                            
                        }
                        else
                        {
                            ontId = Convert.ToString(onu.OntID);
                        }



                        //show equipment ont optics
                        //OUTPUT:

                        /* ========================================================================================
                           optics table
                           ========================================================================================
                           |rx-signal  |tx-signal  |ont        |           |laser        |olt-rx     
                            ont-idx       |level      |level      |temperature|ont-voltage|bias-curr    |sig-level  
                           --------------+-----------+-----------+-----------+-----------+-------------+-----------
                           1/1/1/11/1     -16.198     2.408       51.301      3.24        13098.0       -12.3      
                           1/1/1/11/2     -18.268     2.342       46.102      3.24        7650.0        -22.8      
                           1/1/1/11/4     -18.240     2.256       48.102      3.24        12432.0       -21.0      
                           ----------------------------------------------------------------------------------------
                           optics count : 3
                           ========================================================================================
                                 */

                        await client.ExecuteCommandAsync($"show equipment ont optics");
                await Task.Delay(3000);

                var resposta = await client.ReadAsync();
                LogIntegracaoAtivacao(string.Join("\n", resposta));


                string[] lines = resposta.Split('\n');


                string requiredONTIndex = $"1/1/{numSlot}/{numPON}/{ontId}";
                string requiredLine = null;
                foreach (var line in lines)
                {
                    if (line.Contains(requiredONTIndex))
                    {
                        requiredLine = line;
                        break;
                    }
                }

                if (requiredLine != null)
                {

                    string[] data = requiredLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    string temperatura = data[3];
                    double temperaturaConversao = 0.0;
                    Double.TryParse(temperatura, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out temperaturaConversao);
                    double roundedTemperature = Math.Round(temperaturaConversao);

                    retorno.Rx = data[1];
                    retorno.Tx = data[2];
                    retorno.Temperatura = roundedTemperature.ToString();
                    retorno.Voltagem = data[4];
                    retorno.Bias_Current = data[5];
                    retorno.Rx_OLT = data[6];


                }



                //OUPUT
                /* typ:aniel># show equipment ont status pon ont 1/1/1/11/4
                   =============================================================================================================================================================================================
                   pon table
                   =============================================================================================================================================================================================
                             |              |             |admin   |oper    |olt-rx-sig |ont-olt      |                                 |                                 |
                   pon       |ont           |sernum       |status  |status  |level(dbm) |distance(km) |desc1                            |desc2                            |hostname
                   ----------+--------------+-------------+--------+--------+-----------+-------------+---------------------------------+---------------------------------+-------------------------------------
                   1/1/1/11   1/1/1/11/4     ALCL:FC4D0E8F up       up       -21.3       0.1           9                                 -                                 undefined
                   ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                   pon count : 1
                   =============================================================================================================================================================================================*/

                await client.ExecuteCommandAsync($"show equipment ont status pon ont 1/1/{numSlot}/{numPON}/{ontId}");
                await Task.Delay(10000);

                var resposta2 = (await client.ReadAsync()).Split('\r', '\n').ToList();
                var formattedResposta = string.Join("\n", resposta).Replace("-\u001b[1D\\", "")
                .Replace("\u001b[1D", "").Replace("|/|/|/- ", "").Replace("|/|/| ", "").Replace("|/|/|/ ", "")
                .Replace("|/|/ ", "").Replace("|/|/", "").Replace("|/", "").Replace("|/ ", "")
                .Replace("- ", "").Replace("| ", "");
                LogIntegracaoAtivacao(string.Join("\n", formattedResposta));


                string[] statusLines = formattedResposta.Split('\n');

                bool foundHeader = false;
                string requiredONTIndex2 = $"1/1/{numSlot}/{numPON}/{ontId}";
                string requiredLine2 = null;
                foreach (var line in statusLines)
                {

                    if (!foundHeader && line.Contains("pon table"))
                    {
                        foundHeader = true;
                        continue;
                    }

                    if (foundHeader)
                    {
                        if (line.Contains(requiredONTIndex2))
                        {
                            requiredLine2 = line;
                            break;
                        }
                    }
                }

                if (requiredLine2 != null)
                {

                    string[] statusData = requiredLine2.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);


                    string distanciaDado = statusData[6];
                    double distancia = 0.0;
                    string statusOnu = statusData[4];

                    Double.TryParse(distanciaDado, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out distancia);

                    if (distancia > 0)
                    {
                        if (distancia > 1.0)
                            retorno.Type = $"{distancia} Km";
                        else
                            retorno.Type = $"{distancia * 1000} m";
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

        #endregion
    }
    catch (Exception ex)
    {
        throw new EAnielException(ex.Message);
    }

    return retorno;
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
        var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
        var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;


        #region Telnet && SSH

        using (RemoteConnection client = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
        {
            await client.Connect();
            if (client.IsConnected())
            {

                //show equipment ont interface 1/1/1/11/4 detail
                //OUPUT:
                /* typ: aniel ># show equipment ont interface 1/1/1/11/4 detail
                 =============================================================================================================================================================================================
                 interface table (detailed)
                 =============================================================================================================================================================================================

                 ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                 interface
                 ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                ont-idx : 1/1/1/11/4                      eqpt-ver-num : 3FE49937ACAA01                    sw-ver-act : 3FE49568IJJL06                    sw-ver-psv : 3FE49568IJJJ31
                              vendor-id : ALCL                                equip-id : NOCLEICODEG1425GB           actual-num-slots : 2                             version-number : 3FE49937ACAA01
                             num-tconts : 32                             num-trf-sched : 32                           num-prio-queues : 256                      auto-sw-planned-ver : 3FE49568IJJL06
                   auto-sw-download-ver : DISABLED                              sernum : ALCL:FC4D0E8F
                           yp-serial-no : ALCLFC4D0E8F
                          oper-spec-ver : unknown                         act-ont-type : hgu                         act-txpower-ctrl : tx-rx                       sn-bundle-status : idle
                       cfgfile1-ver-act : PREALCL244                  cfgfile1-ver-psv :                             cfgfile2-ver-act : CFGALCL001                  cfgfile2-ver-psv :
                         actual-us-rate : 1.25g                          template-name : DEFAULT                     auto-prov-status : not-applicable
                 ============================================================================================================================================================================================= */

                        //Se o ID da ONU não estiver gravado na tabela
                        //(isso ocorre em casos onde a ONU foi importada da planilha)
                        //O método BuscaIdONU faz a busca do ID do equipamento e retorna este ID
                        //Sendo retornado o ID, irá atribuir o valor à onu.OntID
                        if(onu.OntID == 0 || onu.OntID == null)
                        {
                            var serial = "";
                            if(String.IsNullOrEmpty(onu.Serial))
                            {
                                serial = onu.Mac;
                            }
                            else
                            {
                                serial = onu.Serial;
                            }

                            var idONU = await BuscaIdONU(client, serial);

                            if(idONU != 0)
                            {
                                onu.OntID = idONU;
                            }
                        }

                await client.ExecuteCommandAsync($"show equipment ont interface 1/1/{onu.IdSlot}/{onu.IdPon}/{onu.OntID} detail");
                await Task.Delay(3000);
                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                LogIntegracaoAtivacao(string.Join("\n", resposta));
                string responseText = string.Join("\n", resposta);


                string swVerActPattern = @"sw-ver-act\s*:\s*(\S+)";
                string eqptVerNumPattern = @"eqpt-ver-num\s*:\s*(\S+)";

                var swVerActMatch = Regex.Match(responseText, swVerActPattern);
                string swVerAct = swVerActMatch.Success ? swVerActMatch.Groups[1].Value : "Not Found";

                var eqptVerNumMatch = Regex.Match(responseText, eqptVerNumPattern);
                string eqptVerNum = eqptVerNumMatch.Success ? eqptVerNumMatch.Groups[1].Value : "Not Found";

                retorno.Modelo = eqptVerNum;
                retorno.Versao_Software = swVerAct;


            }
            else
            {
                throw new EAnielException("Erro ao estabelecer conexão.");
            }
        }
    }

    #endregion

    catch (Exception ex)
    {
        LogIntegracaoAtivacao(ex);
        throw new EAnielException(ex.Message);
    }

    return retorno;

}

        public async Task<List<Models.StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listSerial, List<(string, string)> listSerialConvertido = null, Func<string, string> consultarOntId = null)
        {
            var retorno = new List<Models.StatusONU>();
            try
            {
                var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

                #region Telnet && SSH

                using (RemoteConnection client = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {
                    await client.Connect();
                    if (client.IsConnected())
                    {
                        await client.ExecuteCommandAsync($"show equipment ont status pon 1/1/{numSlot}/{numPON}");
                        await Task.Delay(800);

                        var resposta = (await client.ReadAsync()).Split('\r', '\n').Select(p => p.Trim()).ToList();
                        var formattedResposta = string.Join("\n", resposta).Replace("-\u001b[1D\\", "")
                         .Replace("\u001b[1D", "").Replace("|/|/|/- ", "").Replace("|/|/| ", "").Replace("|/|/|/ ", "")
                         .Replace("|/|/ ", "").Replace("|/|/", "").Replace("|/", "").Replace("|/ ", "")
                         .Replace("- ", "").Replace("| ", "");

                        LogIntegracaoAtivacao(formattedResposta);

                        foreach (var serial in listSerial)
                        {
                            var normalizedSerial = serial.ToUpperInvariant().Trim();

                            LogIntegracaoAtivacao($"Searching for serial: {normalizedSerial}");

                            



                            if (!String.IsNullOrEmpty(normalizedSerial))
                            {
                                if (formattedResposta.ToUpperInvariant().Contains("UP".ToUpperInvariant()))
                                {
                                    retorno.Add(new Models.StatusONU
                                    {
                                        Serial = serial,
                                        Status = Sinapse.Model.enumerados.StatusONU.Online
                                    });
                                    LogIntegracaoAtivacao(retorno.Last().ToString());
                                }
                                else
                                {
                                    retorno.Add(new Models.StatusONU
                                    {
                                        Serial = serial,
                                        Status = Sinapse.Model.enumerados.StatusONU.Offline
                                    });
                                    LogIntegracaoAtivacao(retorno.Last().ToString());
                                }
                            }
                            else
                            {
                                retorno.Add(new Models.StatusONU
                                {
                                    Serial = serial,
                                    Status = Sinapse.Model.enumerados.StatusONU.Indisponivel
                                });
                                LogIntegracaoAtivacao(retorno.Last().ToString());
                            }
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogIntegracaoAtivacao(ex);
                throw new EAnielException(ex.Message);
            }

            return retorno;
        }



        public Task<List<object>> ListaOnusConectadas()
{
    throw new NotImplementedException();
}

public async Task<List<AutorizarONUListModel>> ONUsPendentesRegistro(string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
{
    var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
    var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
    var listaONU = new List<AutorizarONUListModel>();


    var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
    var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;



    #region Telnet && SSH

    using (RemoteConnection client = new RemoteConnection(this._olt.IP, porta, this._olt.Login, this._olt.Senha, protocolo))
    {
        await client.Connect();
        if (client.IsConnected())
        {
            await client.ExecuteCommandAsync("show pon unprovision-onu");
                    // Formato ont info:
                    /*=============================================================================================================================================================================================
                      unprovision-onu table
                      =============================================================================================================================================================================================
                      alarm-idx |gpon-index        |sernum      |subscriber-locid                                                        |logical-authid          |actual-us-rate
                      ----------+------------------+------------+------------------------------------------------------------------------+------------------------+------------------------------------------------
                      47         1/1/1/7            ALCLB3D8A0B3 DEFAULT                                                                                           1.25g
                      48         1/1/1/7            ALCLB3D894F6 DEFAULT                                                                                           1.25g
                      49         1/1/1/7            ALCLB3D8A419 DEFAULT                                                                                           1.25g
                      50         1/1/1/7            ALCLB3D8ABB6 DEFAULT                                                                                           1.25g
                      51         1/1/1/7            ALCLB3D80812 DEFAULT                                                                                           1.25g
                      52         1/1/1/7            ALCLB3D891E2 DEFAULT                                                                                           1.25g
                      54         1/1/1/5            ALCLFC4C9122 DEFAULT                                                                                           1.25g
                      55         1/1/1/5            ALCLFC15E522 DEFAULT                                                                                           1.25g
                      56         1/1/1/5            ALCLFC527F64 DEFAULT                                                                                           1.25g
                      ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                      unprovision-onu count : 9
                      =============================================================================================================================================================================================


                     RETORNO (NENHUMA ONU):
                     =============================================================================================================================================================================================
                     unprovision-onu table
                     =============================================================================================================================================================================================
                     alarm-idx |gpon-index        |sernum      |subscriber-locid                                                        |logical-authid          |actual-us-rate
                     ----------+------------------+------------+------------------------------------------------------------------------+------------------------+------------------------------------------------
                     ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                     unprovision-onu count : 0
                     =============================================================================================================================================================================================

                     */

                    string formattedResposta = "";
                    List<string> resposta = null;
                    var idLog = enviarLog($"[ONUsPendentesRegistro] [NOKIA] -> {this._olt.Descricao}", numUsuario, numOs, null);
                    int numTentativas = 0;

                    while(numTentativas <= 10)
                    {
                        await Task.Delay(5000);
                        resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                        formattedResposta = string.Join("\n", resposta).Replace("-\u001b[1D\\", "")
                           .Replace("\u001b[1D", "").Replace("|/|/|/- ", "").Replace("|/|/| ", "").Replace("|/|/|/ ", "")
                           .Replace("|/|/ ", "").Replace("|/|/", "").Replace("|/", "").Replace("|/ ", "")
                           .Replace("- ", "").Replace("| ", "");

                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("unprovision-onu count".ToUpperInvariant())))
                        {
                            break;
                        }

                        numTentativas++;
                    }

 
           
           
           
                   
            LogIntegracaoAtivacao(formattedResposta);
            await enviarLog(formattedResposta, numUsuario, numOs, idLog.Result);
            var onuCountIndex = resposta.FindIndex(line => line.Contains("unprovision-onu count :"));
            var onuCountLine = resposta[onuCountIndex];
            var onuCount = int.Parse(onuCountLine.Split(':')[1].Trim());

            var lines = formattedResposta.Split('\n').ToList();
            int? index_header = resposta.FindIndex(linha => linha.Contains("alarm-idx") || linha.Contains("sernum"));

            if (index_header.HasValue)
            {
                var headerLine = formattedResposta.Split('\n')[index_header.Value];
                var headerIndex = formattedResposta.IndexOf(headerLine);

                if (onuCount > 0)
                {
                    var lista_onus = lines.SkipWhile(line => !line.Contains("---------")).Skip(1).Take(onuCount).ToList();
                    lista_onus.RemoveAll(s => string.IsNullOrWhiteSpace(s));


                    foreach (var onu in lista_onus)
                    {
                        var campos_onu = onu.Split(' ', '\t').ToList();
                        campos_onu.RemoveAll(s => s == " " || s == String.Empty);


                        var alarmIdx = campos_onu[0];
                        var gponIndex = campos_onu[1];
                        var sernum = campos_onu[2];

                        string formattedSerial = $"{sernum.Substring(0, 4)}:{sernum.Substring(4)}";

                        var campos = gponIndex.Split('/');
                        string numSlot = campos.ElementAtOrDefault(2);
                        string numPon = campos.ElementAtOrDefault(3);

                        var onu_model = new AutorizarONUListModel
                        {
                            Id = alarmIdx,
                            Num_OLT = this._olt.NumOLT,
                            Id_OLT = this._olt.Id,
                            Slot = numSlot,
                            PON = numPon,
                            Desc_OLT = this._olt.Descricao,
                            Mac_Serial = sernum
                        };

                        listaONU.Add(onu_model);

                    }
                }
            }
        }

        else
        {
            throw new EAnielException("Erro ao estabelecer conexão.");
        }
    }


    #endregion

    return listaONU;
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
                        //Se o ID da ONU não estiver gravado na tabela
                        //(isso ocorre em casos onde a ONU foi importada da planilha)
                        //O método BuscaIdONU faz a busca do ID do equipamento e retorna este ID
                        //Sendo retornado o ID, irá atribuir o valor à onu.OntID
                        if (onu.OntID == 0 || onu.OntID == null)
                        {
                            var serial = "";
                            if (String.IsNullOrEmpty(onu.Serial))
                            {
                                serial = onu.Mac;
                            }
                            else
                            {
                                serial = onu.Serial;
                            }

                            var idONU = await BuscaIdONU(client, serial);

                            if (idONU != 0)
                            {
                                onu.OntID = idONU;
                            }
                        }

                await client.ExecuteCommandAsync($"admin equipment ont interface 1/1/{onu.IdSlot}/{onu.IdPon}/{onu.OntID} reboot with-active-image");
                await Task.Delay(2000);
                await client.ExecuteCommandAsync("exit all");

                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                LogIntegracaoAtivacao(string.Join("\n", resposta));

                if (resposta.Exists(p => p.ToUpperInvariant().Contains("Error".ToUpperInvariant())))
                {
                    response.result = false;
                    response.message = string.Join("\n", resposta);

                    return response;
                }

                var tentativas = 0;
                //Rotina para consultar se a ONU voltou a ficar Online, caso não tenha sucesso em até 10 tentativas, aborta
                while (tentativas <= 10)
                {
                    await client.ExecuteCommandAsync($"show equipment ont status pon ont 1/1/{onu.IdSlot}/{onu.IdPon}/{onu.OntID}");
                    //await Task.Delay(10000);

                    //OUPUT
                    /* typ:aniel># show equipment ont status pon ont 1/1/1/11/4
                       =============================================================================================================================================================================================
                       pon table
                       =============================================================================================================================================================================================
                                 |              |             |admin   |oper    |olt-rx-sig |ont-olt      |                                 |                                 |
                       pon       |ont           |sernum       |status  |status  |level(dbm) |distance(km) |desc1                            |desc2                            |hostname
                       ----------+--------------+-------------+--------+--------+-----------+-------------+---------------------------------+---------------------------------+-------------------------------------
                       1/1/1/11   1/1/1/11/4     ALCL:FC4D0E8F up       up       -21.3       0.1           9                                 -                                 undefined
                       ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
                       pon count : 1
                       =============================================================================================================================================================================================*/


                    var resposta2 = (await client.ReadAsync(7000)).Split('\r', '\n').ToList();
                    var formattedResposta = string.Join("\n", resposta2).Replace("-\u001b[1D\\", "")
                        .Replace("\u001b[1D", "").Replace("|/|/|/- ", "").Replace("|/|/| ", "").Replace("|/|/|/ ", "")
                        .Replace("|/|/ ", "").Replace("|/|/", "").Replace("|/", "").Replace("|/ ", "")
                        .Replace("- ", "");
                    LogIntegracaoAtivacao(formattedResposta);


                    if (resposta2.Exists(p => p.ToUpperInvariant().Contains("Error".ToUpperInvariant())))
                    {
                        //response.result = false;
                        //response.message = string.Join("\n", resposta2);
                        throw new EAnielException("A OLT retornou um erro durante a operação.");

                        //break;
                    }

                    string[] statusLines = formattedResposta.Split('\n');

                    bool foundHeader = false;
                    string requiredONTIndex2 = $"1/1/{onu.IdSlot}/{onu.IdPon}/{onu.OntID}";
                    string requiredLine2 = null;
                    foreach (var line in statusLines)
                    {

                        if (!foundHeader && line.Contains("pon table"))
                        {
                            foundHeader = true;
                            continue;
                        }

                        if (foundHeader)
                        {
                            if (line.Contains(requiredONTIndex2))
                            {
                                requiredLine2 = line;
                                break;
                            }
                        }
                    }


                    if (requiredLine2 != null)
                    {

                        string[] statusData = requiredLine2.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);


                        string statusOnu = statusData[4];

                        if (statusOnu == "up")
                        {
                            //retornoStatus = true;
                            break;
                        }

                    }



                    tentativas++;
                    if (tentativas == 1) //Primeira tentativa aguarda 30 segundos para tentar novamente, tentativas seguintes aguarda 10 segundos
                        await Task.Delay(30000);
                    else
                        await Task.Delay(10000);

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
        response.message = ex.Message;

    }
    return response;
}


public Task<Response> ResetarONU(ConfigurarONUModel onu)
{
    throw new NotImplementedException();
}

private async Task<Int32> GetProximoOnuIdDisp(long numSlot, long numPon)
{
            Int32 IdOnu = 0;

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
                        await Task.Delay(2000);

                        await client.ExecuteCommandAsync($"show equipment ont status pon 1/1/{numSlot}/{numPon}");


                        string formattedResposta = "";
                        List<string> resposta = null;
                        int numTentativas = 0;

                        while (numTentativas <= 10)
                        {
                            await Task.Delay(5000);
                            resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            formattedResposta = string.Join("\n", resposta).Replace("-\u001b[1D\\", "")
                            .Replace("\u001b[1D", "").Replace("|/|/|/- ", "").Replace("|/|/| ", "").Replace("|/|/|/ ", "")
                            .Replace("|/|/ ", "").Replace("|/|/", "").Replace("|/", "").Replace("|/ ", "")
                            .Replace("- ", "").Replace("| ", "");

                            if (resposta.Exists(p => p.ToUpperInvariant().Contains("pon count".ToUpperInvariant())))
                            {
                                break;
                            }

                            numTentativas++;
                        }

                       

                        LogIntegracaoAtivacao(formattedResposta);


                        if (resposta.Exists(p => p.ToUpperInvariant().Contains("pon count : 0".ToUpperInvariant())))
                        {
                            return 1;
                        }


                        List<int> usedIds = new List<int>();


                        foreach (var line in resposta)
                        {
                            if (line.Contains($"1/1/{numSlot}/{numPon}/"))
                            {
                                int startIndex = line.LastIndexOf('/') + 1;
                                int endIndex = line.IndexOf(' ', startIndex);
                                if (endIndex == -1) endIndex = line.Length;

                                if (int.TryParse(line.Substring(startIndex, endIndex - startIndex), out int ontId))
                                {
                                    usedIds.Add(ontId);
                                }
                            }
                        }


                        usedIds.Sort();


                        int nextAvailableOntId = 1;
                        foreach (var id in usedIds)
                        {
                            if (id == nextAvailableOntId)
                            {
                                nextAvailableOntId++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        IdOnu = nextAvailableOntId;

                        LogIntegracaoAtivacao(IdOnu.ToString());

                    }
                }

            }
            catch(Exception ex)
            {
                LogIntegracaoAtivacao(ex);
                throw new Exception(ex.Message);
            }

            
            
            
            
            return IdOnu;
}

private async Task<Int32> GetProximoOnuIdDisp(RemoteConnection client, long numSlot, long numPon)
{
    Int32 IdOnu = 0;


    await client.ExecuteCommandAsync($"show equipment ont status pon 1/1/{numSlot}/{numPon}");

            string formattedResposta = "";
            List<string> resposta = null;
            int numTentativas = 0;

            while (numTentativas <= 10)
            {
                await Task.Delay(5000);
                resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                formattedResposta = string.Join("\n", resposta).Replace("-\u001b[1D\\", "")
                .Replace("\u001b[1D", "").Replace("|/|/|/- ", "").Replace("|/|/| ", "").Replace("|/|/|/ ", "")
                .Replace("|/|/ ", "").Replace("|/|/", "").Replace("|/", "").Replace("|/ ", "")
                .Replace("- ", "").Replace("| ", "");

                if (resposta.Exists(p => p.ToUpperInvariant().Contains("pon count".ToUpperInvariant())))
                {
                    break;
                }

                numTentativas++;
            }



            LogIntegracaoAtivacao(formattedResposta);


            if (resposta.Exists(p => p.ToUpperInvariant().Contains("pon count : 0".ToUpperInvariant())))
            {
                return 1;
            }


    List<int> usedIds = new List<int>();


    foreach (var line in resposta)
    {
            if (line.Contains($"1/1/{numSlot}/{numPon}/"))
            {
            int startIndex = line.LastIndexOf('/') + 1;
            int endIndex = line.IndexOf(' ', startIndex);
            if (endIndex == -1) endIndex = line.Length;

            if (int.TryParse(line.Substring(startIndex, endIndex - startIndex), out int ontId))
            {
                usedIds.Add(ontId);
            }
        }
    }


    usedIds.Sort();


    int nextAvailableOntId = 1;
    foreach (var id in usedIds)
    {
        if (id == nextAvailableOntId)
        {
            nextAvailableOntId++;
        }
        else
        {
            break;
        }
    }

    IdOnu = nextAvailableOntId;

    LogIntegracaoAtivacao(IdOnu.ToString());
    return IdOnu;
}

public async Task<Response> ConfigurarONUAsync(ConfigurarONUModel onu, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
{   
    var numOs = string.IsNullOrEmpty(Detalhe) ? $"OPERAÇÃO REALIZADA SEM O.S -> SERIAL: {onu.Serial}" : Detalhe;
    var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
    var response = new Response();

    try
    {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;

                if (onu.IpGerencia != null)
                {
                    using (Client client = new Client(onu.PerfilAtivacao_IpGerencia, onu.PerfilAtivacao_PortaTelnet.GetValueOrDefault(23), new CancellationToken()))
                    {


                        if (client.IsConnected)
                        {
                            var linhasScriptONU = scriptONU.Split('\n');
                            var linhasERespostas = new List<(string Linha, List<string> Resposta)>();
                            var idLog = enviarLog($"[ConfigurarONUAsync] [NOKIA] -> ONU Serial: {onu.Serial}, OLT: {this._olt.Descricao}", numUsuario, numOs, null);
                            foreach (string linha in linhasScriptONU)
                            {
                                await client.WriteLineAsync(linha);
                                await Task.Delay(2000);



                                var resposta = (await client.ReadAsync()).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                linhasERespostas.Add((linha, resposta.ToList()));
                                string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));


                                await enviarLog(fullLog, numUsuario, numOs, idLog.Result);

                                if (resposta.Any(line => line.ToUpperInvariant().Contains("DENY".ToUpperInvariant())))
                                {

                                    throw new Exception("A OLT retornou um erro durante o processo de configuração de ONU. Por favor, verifique script ou OLT e tente novamente.");

                                }

                            }

                            response.result = true;

                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                    }

                    }
                else
                {
                    using (RemoteConnection client = new RemoteConnection(this._olt.IpTL1, this._olt.PortaTL1_SSH, this._olt.LoginTL1, this._olt.SenhaTL1, Protocol.Ssh))
                    {
                        await client.Connect();
                        if (client.IsConnected())
                        {
                            var linhasScriptONU = scriptONU.Split('\n');
                            var linhasERespostas = new List<(string Linha, List<string> Resposta)>();
                            var idLog = enviarLog($"[ConfigurarONUAsync] [NOKIA] -> ONU Serial: {onu.Serial}, OLT: {this._olt.Descricao}", "", "", null);
                            foreach (string linha in linhasScriptONU)
                            {
                                await client.ExecuteCommandAsync(linha);
                                await Task.Delay(2000);




                                var resposta = (await client.ReadAsync()).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                linhasERespostas.Add((linha, resposta.ToList()));
                                string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));


                                await enviarLog(fullLog, "", "", idLog.Result);

                                if (resposta.Any(line => line.ToUpperInvariant().Contains("DENY".ToUpperInvariant())))
                                {

                                    throw new Exception("A OLT retornou um erro durante o processo de configuração de ONU. Por favor, verifique script ou OLT e tente novamente.");

                                }

                            }

                            response.result = true;

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
}

public async Task<Int32> BuscaIdONU(RemoteConnection client, string serial)
{
    Int32 idONU = 0;                          
    string prefix = serial.Substring(0, 4); // ALCL
    string suffix = serial.Substring(4);    // FCD75699
    
    
    string formattedSerial = $"{prefix}:{suffix}";

     int maxTentativa = 4;
     int numTentativa = 0;

    await client.ExecuteCommandAsync($"show equipment ont index sn:{formattedSerial}");

    while (numTentativa <= maxTentativa)
    {        
        await Task.Delay(4000);

        LogIntegracaoAtivacao($"Tentativa número {numTentativa} de ler o ID da ONU.");
        var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
        var formattedResposta = string.Join("\n", resposta).Replace("-\u001b[1D\\", "")
        .Replace("\u001b[1D", "").Replace("|/|/|/- ", "").Replace("|/|/| ", "").Replace("|/|/|/ ", "")
        .Replace("|/|/ ", "").Replace("|/|/", "").Replace("|/", "").Replace("|/ ", "")
        .Replace("- ", "").Replace("| ", "");

        LogIntegracaoAtivacao(formattedResposta);

         if(resposta.Any(line => line.ToUpperInvariant().Contains("DENY".ToUpperInvariant())))
         {  
           LogIntegracaoAtivacao("A OLT retornou um erro ao tentar encontrar o ID da ONU");
           throw new Exception("A OLT retornou um erro ao tentar encontrar o ID da ONU");       
         }

            
            var lines = formattedResposta.Split('\n');
                foreach (var line in resposta)
                {
                    // Verifica se a linha contém o prefixo do ont index ("1/1/")
                    if (line.Contains("1/1/"))
                    {
                        // Procura o index após o último /
                        int startIndex = line.LastIndexOf('/') + 1;

                        // Procura o próximo espaço vazio após a última barra, indicando o fim do index
                        int endIndex = line.IndexOf(' ', startIndex);

                        // Se nenhum espaço for encontrado, considera o final da linha
                        if (endIndex == -1) endIndex = line.Length;

                        // Extrai e executa um parse no id
                        if (int.TryParse(line.Substring(startIndex, endIndex - startIndex), out int ontId))
                        {
                            idONU = ontId;  
                            break;  
                        }
                    }
                }

                if (idONU != 0)
                {
                LogIntegracaoAtivacao($"ID da ONU encontrado! O ID é {idONU}");
                break;
                }
            else
            {

               if (numTentativa <= maxTentativa)
               {
                   await Task.Delay(2000);

               }
               numTentativa++;
            }

            

    }
    
    return idONU;

}

        public async Task<GetDadosONUOLT> GetDadosONUnaOLT(string Serial_Convertido, string serial, string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var DadosONU = new GetDadosONUOLT();
            string prefix = serial.Substring(0, 4); // ALCL
            string suffix = serial.Substring(4);
            string prefixConvertido = Serial_Convertido.Substring(0, 4); // ALCL
            string suffixConvertido = Serial_Convertido.Substring(4);  // FCD75699
            var protocol = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
            var port = protocol == Protocol.Ssh ? this._olt.PortaSSH.Value : this._olt.PortaTelnet.Value;


            string formattedSerial = $"{prefix}:{suffix}";
            string formattedSerialConvertido = $"{prefixConvertido}:{suffixConvertido}";

            try
            {
                using (RemoteConnection client = new RemoteConnection(this._olt.IP, port, this._olt.Login, this._olt.Senha, protocol))
                {
                    await client.Connect();
                    if (client.IsConnected())
                    {

                        var idLog = enviarLog($"[GetDadosONUnaOLT] [NOKIA] -> {this._olt.Descricao} SERIAL {serial} - SERIAL CONVERTIDO {Serial_Convertido}", numUsuario, numOs, null);
                        await client.ExecuteCommandAsync($"show equipment ont index sn:{formattedSerial?.ToUpper()}");

                        int maxTentativa = 4;
                        int numTentativa = 0;
                        var formattedResposta = "";

                        while (numTentativa <= maxTentativa)
                        {
                            await Task.Delay(4000);

                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            formattedResposta = string.Join("\n", resposta).Replace("-\u001b[1D\\", "")
                            .Replace("\u001b[1D", "").Replace("|/|/|/- ", "").Replace("|/|/| ", "").Replace("|/|/|/ ", "")
                            .Replace("|/|/ ", "").Replace("|/|/", "").Replace("|/", "").Replace("|/ ", "")
                            .Replace("- ", "").Replace("| ", "");

                            LogIntegracaoAtivacao(formattedResposta);

                            if (resposta.Exists(p => p.Contains("1/1/") && p.ToUpperInvariant().Contains($"sn:{formattedSerial}".ToUpperInvariant())))
                            {
                                var linha = resposta.First(p => p.Contains("1/1/") && p.ToUpperInvariant().Contains($"sn:{formattedSerial}".ToUpperInvariant()));

                                var parts = linha.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                var fspPart = parts.FirstOrDefault(p => p.Contains("1/1/"));

                                if (fspPart != null)
                                {
                                    var fspSplit = fspPart.Split('/');

                                    if (fspSplit.Length >= 5)
                                    {
                                        string slot = fspSplit[2];   // Slot = "2"
                                        string pon = fspSplit[3];    // Pon = "5"
                                        string ontId = fspSplit[4];  // ONT ID = "55"

                                        DadosONU.Slot = slot;
                                        DadosONU.Pon = pon;
                                        DadosONU.Onu_Id = ontId;
                                        DadosONU.Onu_Encontrada = true;
                                        DadosONU.Serial = serial;

                                    }
                                }

                                break;
                            }

                            numTentativa++;

                        }

                        if(DadosONU.Onu_Encontrada == false && !string.Equals(serial, Serial_Convertido, StringComparison.OrdinalIgnoreCase) 
                            && _olt.ConverteOlt == Status_Default.Ativo)
                        {
                            await client.ExecuteCommandAsync($"show equipment ont index sn:{formattedSerialConvertido?.ToUpper()}");

                            maxTentativa = 4;
                            numTentativa = 0;
                            formattedResposta = "";

                            while (numTentativa <= maxTentativa)
                            {
                                await Task.Delay(4000);

                                var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                formattedResposta = string.Join("\n", resposta).Replace("-\u001b[1D\\", "")
                                .Replace("\u001b[1D", "").Replace("|/|/|/- ", "").Replace("|/|/| ", "").Replace("|/|/|/ ", "")
                                .Replace("|/|/ ", "").Replace("|/|/", "").Replace("|/", "").Replace("|/ ", "")
                                .Replace("- ", "").Replace("| ", "");

                                LogIntegracaoAtivacao(formattedResposta);

                                if (resposta.Exists(p => p.Contains("1/1/") && p.ToUpperInvariant().Contains($"sn:{formattedSerialConvertido}".ToUpperInvariant())))
                                {
                                    var linha = resposta.First(p => p.Contains("1/1/") && p.ToUpperInvariant().Contains($"sn:{formattedSerialConvertido}".ToUpperInvariant()));

                                    var parts = linha.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                    var fspPart = parts.FirstOrDefault(p => p.Contains("1/1/"));

                                    if (fspPart != null)
                                    {
                                        var fspSplit = fspPart.Split('/');

                                        if (fspSplit.Length >= 5)
                                        {
                                            string slot = fspSplit[2];   // Slot = "2"
                                            string pon = fspSplit[3];    // Pon = "5"
                                            string ontId = fspSplit[4];  // ONT ID = "55"

                                            DadosONU.Slot = slot;
                                            DadosONU.Pon = pon;
                                            DadosONU.Onu_Id = ontId;
                                            DadosONU.Onu_Encontrada = true;
                                            DadosONU.Serial = serial;

                                        }
                                    }

                                    break;
                                }

                                numTentativa++;

                            }
                        }

                        LogIntegracaoAtivacao(string.Join("\n", formattedResposta));
                        await enviarLog(string.Join("\n", formattedResposta), numUsuario, numOs, idLog.Result);

                    }
                    else
                    {
                        throw new EAnielException("Erro ao estabelecer conexão.");
                    }

                }
            }
            catch(Exception ex)
            {
                LogIntegracaoAtivacao(ex);
                DadosONU = new GetDadosONUOLT
                {
                    Onu_Encontrada = false,
                    Onu_Id = "",
                    Slot = "",
                    Pon = "",
                    Serial = serial
                };
            }


            return DadosONU;
        }

        public async Task<TesteONUModels> GetDadosComplementaresONU(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
        }

        public async Task<GetStatusADMeOperacionalONU> GetStatusADMeOperacionalONU(ConfigurarONUModel onu)
        {
            throw new NotImplementedException();
        }


        private void LogIntegracaoAtivacao(string msg)
{
    LogManager.GetLogger("IntegracaoOLT").Info($"[NOKIA] - {msg}");
}

private void LogIntegracaoAtivacao(Exception ex)
{
    LogManager.GetLogger("IntegracaoOLT").Error($"[NOKIA] - {ex.Message}", ex);
}


}

}





