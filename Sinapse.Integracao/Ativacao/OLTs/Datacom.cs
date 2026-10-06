using Aniel.Connect.Core;
using log4net;
using NHibernate;
using PrimS.Telnet;
using Renci.SshNet;
using Sinapse.Core;
using Sinapse.DAO.repository;
using Sinapse.Integracao.Ativacao.Interfaces;
using Sinapse.Integracao.Ativacao.Models;
using Sinapse.Integracao.IntegracoesERP.Ixc.Models;
using Sinapse.Integracao.Rastreamento.Cobli.Models;
using Sinapse.Model;
using Sinapse.Model.enumerados;
using Sinapse.Util.RemoteConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Sinapse.Integracao.Ativacao.OLTs
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

        public async Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? $"OPERAÇÃO REALIZADA SEM O.S -> SERIAL: {onu.Serial}" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var response = new Response();
            var result = false;

            using (ISession session = HibernateUtil.GetSession())
            {
                var protocolo = this._olt.PortaSSH.HasValue ? Protocol.Ssh : Protocol.Telnet;
                var porta = protocolo == Protocol.Ssh ? this._olt.PortaSSH.GetValueOrDefault(22) : this._olt.PortaTelnet.GetValueOrDefault(23);

                /* try
                 { */
                /*if ((scriptOLT.Contains("service-port") || (!String.IsNullOrEmpty(scriptONU) && scriptONU.Contains("service-port"))) && onu.ServicePort.HasValue == false)
                {
                    throw new EAnielException($"O script utiliza service-port porém esta informação não está cadastrada para o Perfil de Ativação: {onu?.DescricaoPerfilAtivacao}");
                }*/

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
                            LogIntegracaoAtivacao(string.Join("\n", linhasScriptOLT));
                            var linhasERespostas = new List<(string Linha, List<string> Resposta)>();
                            var idLog = enviarLog($"[AutorizarONUAsync] [DATACOM] -> ONU Serial: {onu.Serial}, OLT: {this._olt.Descricao}", numUsuario, numOs, null);
                            foreach (string linha in linhasScriptOLT)
                            {
                                //await Task.Delay(700);
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
                                        await Task.Delay(1500);
                                        var resposta_onuID = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                        LogIntegracaoAtivacao(string.Join("\n", resposta_onuID));
                                        linhasERespostas.Add(("onu ?", resposta_onuID));

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
                                            LogIntegracaoAtivacao(string.Join("\n", lista_ids_string));
                                            lista_ids_string.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);

                                            var lista_ids = new List<Int32>();
                                            foreach (var item in lista_ids_string)
                                            {
                                                var lista_formatada = item.Replace("<cr>", String.Empty).Split('\t', '\r', '\n', ' ').ToList();
                                                lista_formatada.RemoveAll(s => s == "\n" || s == "\r" || s == String.Empty);
                                                foreach (var id in lista_formatada)
                                                {
                                                    if (!String.IsNullOrEmpty(id) && !r.IsMatch(id))
                                                        lista_ids.Add(Convert.ToInt32(id));
                                                }
                                            };

                                            for (var i = 0; i <= 127; i++)
                                            {
                                                if (!lista_ids.Contains(i))
                                                {
                                                    onu.OntID = i;
                                                    break;
                                                }
                                            }

                                            if(!onu.OntID.HasValue)
                                            {
                                                throw new EAnielException($"Não há nenhum ID disponível para uso na interface 1/{onu.IdSlot}/{onu.IdPon}. Libere um ID de alguma ONU existente na OLT e tente novamente.");
                                            }
                                        }
                                    }

                                    await client.WriteLineAsync(linha.Replace("<ONT_ID>", Convert.ToString(onu.OntID)));
                                    continue;
                                }



                                //int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 4000;
                                    await client.WriteLineAsync(linha);

                                //Caso a linha do script contenha service-port new
                                //A aplicação fará 10 tentativas para ler o retorno após esse comando
                                //A cada tentativa, um delay de 1000 ms será acionado para 
                                //permitir que a OLT tenha tempo de retornar algo.
                                //A OLT retornando algo, o loop while se encerra.
                                if (linha.ToUpperInvariant().Contains("service-port new".ToUpperInvariant()))
                                {
                                    int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 5000;
                                    await Task.Delay(delayTime);
                                    var resposta = await client.ReadAsync();
                                     int attempts = 0;
                                     int maxAttempts = 10; 

                                     while (string.IsNullOrWhiteSpace(resposta) && attempts < maxAttempts)
                                     {
                                         await Task.Delay(1000);  
                                         resposta = await client.ReadAsync();  
                                         attempts++;
                                     }

                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    linhasERespostas.Add((linha, resposta.Split('\r', '\n').ToList()));

                                }




                                if (linha.ToUpperInvariant().Contains("commit".ToUpperInvariant()))
                                {
                                    int delayTime2 = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 5000;
                                    await Task.Delay(delayTime2);
                                    var resposta = await client.ReadAsync();
                                    int attempts = 0;
                                    int maxAttempts = 5;


                                    while (attempts < maxAttempts)
                                    {
                                        if(string.IsNullOrWhiteSpace(resposta) 
                                            || !resposta.ToUpperInvariant().Contains("commit complete".ToUpperInvariant())
                                            && !resposta.ToUpperInvariant().Contains("ABORT".ToUpperInvariant()) 
                                            && !resposta.ToUpperInvariant().Contains("syntax error".ToUpperInvariant()))
                                        {
                                            await Task.Delay(30000);
                                            resposta = await client.ReadAsync();
                                            attempts+=1;
                                            LogIntegracaoAtivacao($"Tentativa {attempts} de ler commit: {string.Join("\n", resposta)}");
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    LogIntegracaoAtivacao(string.Join("\n", resposta));
                                    linhasERespostas.Add((linha, resposta.Split('\r', '\n').ToList()));


                                    if (resposta.ToUpperInvariant().Contains("commit complete".ToUpperInvariant()))
                                    {
                                       result = true;
                                       break;
                                    }
                                    if (!resposta.ToUpperInvariant().Contains("commit".ToUpperInvariant()) && !resposta.ToUpperInvariant().Contains("commit complete".ToUpperInvariant()))
                                    {
                                      string mensagemCommit = "Algo deu errado. Não foi possível finalizar a operação.";
                                      LogIntegracaoAtivacao(string.Join("\n", mensagemCommit));
                                        string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));


                                        await enviarLog(fullLogErro, numUsuario, numOs, idLog.Result);
                                        throw new EAnielException(mensagemCommit);
                                    }
                                    if (resposta.ToUpperInvariant().Contains("ABORT".ToUpperInvariant()) || resposta.ToUpperInvariant().Contains("syntax error".ToUpperInvariant()))
                                    {
                                      var mensagem_erro = resposta.ToUpperInvariant().Contains("ABORT".ToUpperInvariant()) || resposta.ToUpperInvariant().Contains("syntax error".ToUpperInvariant());
                                      LogIntegracaoAtivacao(string.Join("\n", resposta));
                                        string fullLogErro = string.Join("\n\n", linhasERespostas.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));


                                        await enviarLog(fullLogErro, numUsuario, numOs, idLog.Result);
                                        throw new EAnielException(resposta);


                                    }

                                }

                                //await Task.Delay(delayTime);



                            }

                            string fullLog = string.Join("\n\n", linhasERespostas.Select(item =>
                                    $"Linha Executada:\n{item.Linha}\nResposta:\n{string.Join("\n", item.Resposta)}"));


                            await enviarLog(fullLog, numUsuario, numOs, idLog.Result);

                        }
                        else
                        {
                            throw new EAnielException("Erro ao estabelecer conexão.");
                        }
                        //int delayTime2 = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 4000;
                        //double readResponseTime = delayTime2 / 1000;
                    }
                }
                /* }

                 catch (Exception ex)
                 {
                     LogIntegracaoAtivacao(ex);
                     response.result = false;
                     response.exception = $" {ex.Message}";

                 } */



                return new Response
                {
                    result = result
                };
                //throw new NotImplementedException();
            }
        }

        public async Task<Response> DesautorizarONU(ConfigurarONUModel onu, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
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
            var numOs = string.IsNullOrEmpty(Detalhe) ? $"OPERAÇÃO REALIZADA SEM O.S -> SERIAL: {onu.Serial}" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
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
                            var idLog = enviarLog($"[DesautorizarONU] [DATACOM] -> ONU Serial: {onu.Serial}, OLT: {this._olt.Descricao}", numUsuario, numOs, null);
                            var comandos = new List<string>(); //Utilizado para gravar os comandos nos logs
                            var respostaOLT = new List<string>(); //Utilizado para gravar os retornos da OLT nos logs

                            await client.WriteLineAsync("config");
                            comandos.Add("config");
                            await client.WriteLineAsync($"do show running-config service-port | context-match \"gpon 1/{onu.IdSlot}/{onu.IdPon} onu {onu.OntID}\"");
                            comandos.Add($"do show running-config service-port | context-match \"gpon 1/{onu.IdSlot}/{onu.IdPon} onu {onu.OntID}\"");
                            await Task.Delay(45000);
                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            respostaOLT.Add(string.Join("\n", resposta));
                            LogIntegracaoAtivacao(string.Join("\n", resposta));

                            string service_port = null;
                            if (!resposta.Exists(p => p.ToUpperInvariant().Contains("No entries found".ToUpperInvariant())))
                            {
                                var linhas_service_port = resposta.FirstOrDefault(p => p.TrimStart().ToUpperInvariant().StartsWith("service-port".ToUpperInvariant())).Split(' ', '\t').ToList();

                                    linhas_service_port.RemoveAll(s => s == " " || s == String.Empty);
                                    service_port = linhas_service_port.ElementAtOrDefault(1);

                                    string msg = $"Leitura de service-port da onu {onu.OntID} na interface 1/{onu.IdSlot}/{onu.IdPon}";
                                    LogIntegracaoAtivacao(string.Join("\n", msg, resposta, $"Service-Port encontrada: {service_port}"));

                                    if (String.IsNullOrEmpty(service_port))
                                    {
                                        if (onu.ServicePort.HasValue)
                                        {
                                            service_port = Convert.ToString(onu.ServicePort);
                                        }
                                        else
                                        {
                                            retorno.message = $"Não foi possível localizar o Service-Port {service_port} da ONU.";
                                            return retorno;
                                        }
                                    }                                
                                
                                await client.WriteLineAsync($"interface gpon 1/{onu.IdSlot}/{onu.IdPon}");
                                comandos.Add($"interface gpon 1/{onu.IdSlot}/{onu.IdPon}");
                                await client.WriteLineAsync($"no service-port {service_port}");
                                comandos.Add($"no service-port {service_port}");
                                await Task.Delay(5000);
                                await client.WriteLineAsync("commit");
                                comandos.Add("commit");
                               
                                var aguardaRetorno = await AguardaRetornoTerminal(async () => await client.ReadAsync());

                                await client.WriteLineAsync("exit");

                                respostaOLT.Add(string.Join("\n", aguardaRetorno));
                                LogIntegracaoAtivacao(string.Join("\n", aguardaRetorno));

                                if (aguardaRetorno.ToUpperInvariant().Contains("COMMIT COMPLETE".ToUpperInvariant()) || aguardaRetorno.ToUpperInvariant().Contains("NO MODIFICATIONS".ToUpperInvariant()))
                                {
                                    LogIntegracaoAtivacao($"Service-port {service_port} removida da interface 1/{onu.IdSlot}/{onu.IdPon} Onu {onu.OntID}");
                                    //retorno.result = true;
                                    //return retorno;
                                }
                                else
                                {
                                    retorno.message = $"Não foi possível desvincular o Service-Port {service_port} da ONU.";
                                    //Junta os valores que foram adicionados em comandos e respostaOLT
                                    var comandosLog = $"Comandos executados:\n{string.Join("\n", comandos)}";
                                    var respostasLog = $"Respostas:\n{string.Join("\n", respostaOLT)}";

                                    //Realiza gravação de logs
                                    await enviarLog($"{comandosLog}\n\n{respostasLog}", numUsuario, numOs, idLog.Result);
                                    return retorno;
                                }
                            }else if(resposta.Exists(p => p.ToUpperInvariant().Contains("No entries found".ToUpperInvariant())))
                            {
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                LogIntegracaoAtivacao("Nenhum service-port encontrado. Prosseguindo com nova tentativa.");

                            }else if(resposta.Exists(p => p.ToUpperInvariant().Contains("ABORT".ToUpperInvariant())) 
                                || resposta.Exists(p => p.ToUpperInvariant().Contains("".ToUpperInvariant())))
                            {
                                LogIntegracaoAtivacao(string.Join("\n", resposta));
                                LogIntegracaoAtivacao("Ocorreu um erro ao buscar o service-port. Prosseguindo com nova tentativa.");
                            }

                                //await client.WriteLineAsync("config");
                            await client.WriteLineAsync($"interface gpon 1/{onu.IdSlot}/{onu.IdPon}");
                            await client.WriteLineAsync($"no onu {onu.OntID}");
                            comandos.Add($"no onu {onu.OntID}");
                            await client.WriteLineAsync("commit");
                            comandos.Add("commit");

                            var aguardaRetorno2 = await AguardaRetornoTerminal(async () => await client.ReadAsync());
                            respostaOLT.Add(string.Join("\n", aguardaRetorno2));
                            LogIntegracaoAtivacao(string.Join("\n", aguardaRetorno2));

                            if (aguardaRetorno2.ToUpperInvariant().Contains("COMMIT COMPLETE".ToUpperInvariant()) || aguardaRetorno2.ToUpperInvariant().Contains("NO MODIFICATIONS".ToUpperInvariant()))
                            {
                                LogIntegracaoAtivacao($"Onu {onu.OntID} removida da interface 1/{onu.IdSlot}/{onu.IdPon}");
                                //Junta os valores que foram adicionados em comandos e respostaOLT
                                var comandosLog = $"Comandos executados:\n{string.Join("\n", comandos)}";
                                var respostasLog = $"Respostas:\n{string.Join("\n", respostaOLT)}";

                                //Realiza gravação de logs
                                await enviarLog($"{comandosLog}\n\n{respostasLog}", numUsuario, numOs, idLog.Result);
                                retorno.result = true;
                                return retorno;
                            }

                            if (aguardaRetorno2.ToUpperInvariant().Contains("ABORT".ToUpperInvariant()) || aguardaRetorno2.ToUpperInvariant().Contains("SERVICE-PORT".ToUpperInvariant()) && aguardaRetorno2.ToUpperInvariant().Contains("CONFIGURED".ToUpperInvariant()))
                            {
                                LogIntegracaoAtivacao($"Erro ao tentar remover Onu {onu.OntID} da interface 1/{onu.IdSlot}/{onu.IdPon}. Iniciar tentativa de remover service-port.");
                                comandos.Add($"Erro ao tentar remover Onu {onu.OntID} da interface 1/{onu.IdSlot}/{onu.IdPon}. Iniciar tentativa de remover service-port.");
                                //await client.WriteLineAsync("top");
                                await client.WriteLineAsync($"no service-port {onu.ServicePort}");
                                comandos.Add($"no service-port {onu.ServicePort}");
                                
                                await client.WriteLineAsync("commit");
                                comandos.Add("commit");
                               
                                var aguardaRetorno3 = await AguardaRetornoTerminal(async () => await client.ReadAsync());
                                respostaOLT.Add(string.Join("\n", aguardaRetorno3));
                                
                                await client.WriteLineAsync($"no onu {onu.OntID}");
                                comandos.Add($"no onu {onu.OntID}");

                                await client.WriteLineAsync("commit");
                                comandos.Add("commit");

                                aguardaRetorno3 = await AguardaRetornoTerminal(async () => await client.ReadAsync());
                                respostaOLT.Add(string.Join("\n", aguardaRetorno3));

                                //Junta os valores que foram adicionados em comandos e respostaOLT
                                var comandosLog = $"Comandos executados:\n{string.Join("\n", comandos)}";
                                var respostasLog = $"Respostas:\n{string.Join("\n", respostaOLT)}";

                                //Realiza gravação de logs
                                await enviarLog($"{comandosLog}\n\n{respostasLog}", numUsuario, numOs, idLog.Result);

                                if (aguardaRetorno3.ToUpperInvariant().Contains("COMMIT COMPLETE".ToUpperInvariant()) || aguardaRetorno3.ToUpperInvariant().Contains("NO MODIFICATIONS".ToUpperInvariant()))
                                {
                                    LogIntegracaoAtivacao(string.Join("\n", aguardaRetorno3));
                                    retorno.result = true;
                                    return retorno;
                                }
                                else
                                {
                                    var mensagem_erro = aguardaRetorno3.ToUpperInvariant().Contains("ABORT".ToUpperInvariant());
                                    LogIntegracaoAtivacao(string.Join("\n", aguardaRetorno2));
                                    retorno.message = aguardaRetorno2;
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
                                LogIntegracaoAtivacao(string.Join("\n", resposta));


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
                LogIntegracaoAtivacao(ex);
                throw new EAnielException(ex.Message);
            }


            return retorno;
        }

        public async Task<List<Models.StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listSerial, List<(string, string)> listSerialConvertido = null, Func<string, string> consultarOntId = null)
        {
            List<Models.StatusONU> retorno = new List<Models.StatusONU>();

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
                            //client.MillisecondReadDelay = 500;
                            await Task.Delay(500);

                            bool logado = await client.TryLoginAsync(this._olt.Login, this._olt.Senha, 2000);

                            await client.WriteLineAsync("config");
                            await Task.Delay(500);
                            await client.WriteLineAsync($"do show interface gpon 1/{numSlot}/{numPON} onu | nomore");
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

                                    Sinapse.Model.enumerados.StatusONU status = Sinapse.Model.enumerados.StatusONU.Indisponivel;

                                    switch (campos_onu[3].ToUpperInvariant())
                                    {
                                        case "UP":
                                            status = Sinapse.Model.enumerados.StatusONU.Online;
                                            break;
                                        case "DOWN":
                                            status = Sinapse.Model.enumerados.StatusONU.Offline;
                                            break;
                                        default:
                                            status = Sinapse.Model.enumerados.StatusONU.Indisponivel;
                                            break;
                                    }

                                    retorno.Add(new Models.StatusONU
                                    {
                                        Serial = campos_onu[2],
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

        public async Task<List<AutorizarONUListModel>> ONUsPendentesRegistro(string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
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

            var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
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
                            await Task.Delay(500);
                            //await client.TerminatedReadAsync(new Regex("config"), TimeSpan.FromMilliseconds(1000));
                            await client.WriteLineAsync($"do show interface gpon discovered-onus");
                            var idLog = enviarLog($"[ONUsPendentesRegistro] [DATACOM] -> {this._olt.Descricao}", numUsuario, numOs, null);
                            await Task.Delay(4000);

                            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                            LogIntegracaoAtivacao(string.Join("\n", resposta));
                            await enviarLog(string.Join("\n", resposta), numUsuario, numOs, idLog.Result);

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
                                LogIntegracaoAtivacao(string.Join("\n", resposta_pergunta));

                                if (resposta_pergunta.Exists(p => p.ToUpperInvariant().Contains("ARE YOU SURE YOU WANT TO RESET THIS ONU")))
                                {
                                    await client.WriteLineAsync("yes");

                                    var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
                                    LogIntegracaoAtivacao(string.Join("\n", resposta));

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
                                    LogIntegracaoAtivacao(string.Join("\n", mensagem));
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
                LogIntegracaoAtivacao(ex);
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

        public async Task<Response> ConfigurarONUAsync(ConfigurarONUModel onu, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            throw new NotImplementedException();
        }


         public async Task<string> AguardaRetornoTerminal(Func<Task<string>> readAsyncFunc)
        {
   
            int attempts = 0;
            int maxAttempts = 6;
            string resposta = string.Empty;

            while (attempts < maxAttempts)
            {
                if (string.IsNullOrWhiteSpace(resposta)
                    || !resposta.ToUpperInvariant().Contains("commit complete".ToUpperInvariant())
                    && !resposta.ToUpperInvariant().Contains("ABORT".ToUpperInvariant())
                    && !resposta.ToUpperInvariant().Contains("syntax error".ToUpperInvariant()))
                {
                    await Task.Delay(20000);
                    resposta = await readAsyncFunc();
                    attempts += 1;
                    LogIntegracaoAtivacao($"Tentativa {attempts} de ler commit: {string.Join("\n", resposta)}");
                }
                else if(resposta.ToUpperInvariant().Contains("no modifications".ToUpperInvariant()))
                {
                    break;
                }
                else
                {
                    break;
                }
            }
            return resposta;

        }

        public async Task<GetDadosONUOLT> GetDadosONUnaOLT(string Serial_Convertido, string serial, string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            var numOs = string.IsNullOrEmpty(Detalhe) ? "OPERAÇÃO REALIZADA SEM O.S" : Detalhe;
            var numUsuario = string.IsNullOrEmpty(Usuario) ? "" : Usuario;
            var DadosONU = new GetDadosONUOLT();

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
                        if(client.IsConnected)
{
                            client.MillisecondReadDelay = 500;
                            bool logado = await client.TryLoginAsync(this._olt.Login, this._olt.Senha, 1000);                           

                            string commandString = "show interface gpon onu";

                            bool encontrado = await TryGetOnuData(client, serial, Serial_Convertido, commandString, 6, DadosONU, numOs, numUsuario, enviarLog);
                       
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

        private async Task<bool> TryGetOnuData(Client client, string searchSerial, string serialConvertido, string commandString, int tentativaMax, 
            GetDadosONUOLT DadosONU, string numOs, string numUsuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null)
        {
            int numTentativa = 0;
            bool found = false;

            if(String.IsNullOrEmpty(searchSerial) && String.IsNullOrEmpty(serialConvertido))
            {
                return false;
            }

            string vendorSerial = !string.IsNullOrEmpty(searchSerial) && searchSerial.Length >= 4
                  ? searchSerial.Substring(0, 4)
                  : string.Empty;

            string deviceSerial = !string.IsNullOrEmpty(searchSerial) && searchSerial.Length > 4
                ? searchSerial.Substring(4)
                : string.Empty;

            string vendorSerialConvertido = !string.IsNullOrEmpty(serialConvertido) && serialConvertido.Length >= 4
                ? serialConvertido.Substring(0, 4)
                : string.Empty;

            string deviceSerialConvertido = !string.IsNullOrEmpty(serialConvertido) && serialConvertido.Length > 4
                ? serialConvertido.Substring(4)
                : string.Empty;

            string serialLower = $"{vendorSerial?.ToUpper()}{deviceSerial?.ToLower()}";
            string serialUpper = $"{vendorSerial?.ToUpper()}{deviceSerial?.ToUpper()}";
            string serialConverLower = $"{vendorSerialConvertido?.ToUpper()}{deviceSerialConvertido?.ToLower()}";
            string serialConverUpper = $"{vendorSerialConvertido?.ToUpper()}{deviceSerialConvertido?.ToUpper()}";

            var idLog = enviarLog($"[GetDadosONUnaOLT] [DATACOM] -> {this._olt.Descricao} SERIAL {searchSerial} - SERIAL CONVERTIDO {serialConvertido}", numUsuario, numOs, null);

            await client.WriteLineAsync($"show interface gpon onu | include \"{serialLower} | {serialUpper} | {serialConverLower} | {serialConverUpper}\"");
            await Task.Delay(5000);

            var resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();
            LogIntegracaoAtivacao(string.Join("\n", resposta));

            if (!resposta.Exists(p =>
             (
                 p.IndexOf(serialLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 p.IndexOf(serialUpper, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 p.IndexOf(serialConverLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 p.IndexOf(serialConverUpper, StringComparison.OrdinalIgnoreCase) >= 0
             )
             && p.IndexOf(commandString, StringComparison.OrdinalIgnoreCase) < 0))
            {
                int delayTime = _olt.Delay.HasValue ? (int)_olt.Delay.Value : 10000;
                while (numTentativa <= tentativaMax)
                {
                    
                    await Task.Delay(delayTime);
                    resposta = (await client.ReadAsync()).Split('\r', '\n').ToList();

                    if (resposta.Exists(p =>
                     (
                         p.IndexOf(serialLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                         p.IndexOf(serialUpper, StringComparison.OrdinalIgnoreCase) >= 0 ||
                         p.IndexOf(serialConverLower, StringComparison.OrdinalIgnoreCase) >= 0 ||
                         p.IndexOf(serialConverUpper, StringComparison.OrdinalIgnoreCase) >= 0
                     )
                     && p.IndexOf(commandString, StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        found = ParseOnuData(resposta, serialUpper, serialConverUpper, serialLower, serialConverLower, commandString, DadosONU);
                        break;
                    }

                    numTentativa++;
                }
            }
            else
            {
                found = ParseOnuData(resposta, serialUpper, serialConverUpper, serialLower, serialConverLower, commandString, DadosONU);
            }

            LogIntegracaoAtivacao(string.Join("\n", resposta));
            await enviarLog(string.Join("\n", resposta), numUsuario, numOs, idLog.Result);


            return found;
        }

        private bool ParseOnuData(List<string> resposta, string serial, string serialConvertido, string serialLower, string serialLowerConvertido, 
            string commandString, GetDadosONUOLT DadosONU)
        {
            var lista_onus_terminal = resposta
                .Where(p =>
                    p.IndexOf(serial, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    p.IndexOf(commandString, StringComparison.OrdinalIgnoreCase) < 0)
                .ToList();

            if (lista_onus_terminal.Count > 0)
            {
                foreach (var linha in lista_onus_terminal)
                {
                    var parts = linha.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 2)
                    {
                        var fsp = parts[0]; // "1/1/11"
                        var fspSplit = fsp.Split('/');

                        if (fspSplit.Length == 3)
                        {
                            DadosONU.Slot = fspSplit[1];
                            DadosONU.Pon = fspSplit[2];
                            DadosONU.Onu_Id = parts[1];
                            DadosONU.Serial = parts[2];
                            DadosONU.Onu_Encontrada = true;
                            return true;
                        }
                    }
                }
            }

            return false;
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
            LogManager.GetLogger("IntegracaoOLT").Info($"[DATACOM] - {msg}");
        }

        private void LogIntegracaoAtivacao(Exception ex)
        {
            LogManager.GetLogger("IntegracaoOLT").Error($"[DATACOM] - {ex.Message}", ex);
        }
    }
}