using NHibernate;
using Sinapse.Core;
using Sinapse.DAO;
using Sinapse.DAO.repository;
using Sinapse.Integracao.Ativacao.Interfaces;
using Sinapse.Integracao.Ativacao.Models;
using Sinapse.Util.RemoteConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sinapse.Integracao.Ativacao.Concentradoras
{
    public class Huawei : IntegracaoConcentradora
    {
        #region Configuração do Plano

        public async Task ConfiguracaoPlanoCliente(ISession session, LoginRadiusModel login)
        {
            var planoCadastrado = false;

            string nomePlanoTratado = login.DescricaoPlano.Replace(",", " ");
           

            try
            {
                using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> param = new Dictionary<string, object>();

                sql.AppendLine("select count(id) as qtd");
                sql.AppendLine("from radgroupcheck");
                sql.AppendLine("where groupname = @PLANO");


                param.Add("@PLANO", nomePlanoTratado);

                using (var reader = await CommandHelper.ExecuteReader(com, sql, param, session.Connection, null))
                {
                    if (reader.Read())
                        planoCadastrado = Convert.ToInt32(reader["qtd"]) != 0;
                }
            }

            if (!planoCadastrado)
            {
                using (var com = session.Connection.CreateCommand())
                {
                    StringBuilder sql = new StringBuilder();
                    Dictionary<String, Object> parametros = new Dictionary<string, object>();

                    parametros.Add("@ID_PLANO", login.IdPlano);
                    parametros.Add("@DESCRICAO_PLANO", nomePlanoTratado);
                    parametros.Add("@IDCLIENTE", login.IdCliente);


                        sql.Append("INSERT INTO radgroupcheck (groupname, `attribute`, op, value, Id_Plano)");
                        sql.Append("                   VALUES (@DESCRICAO_PLANO, 'Plano-Ativo', '=', 'S', @ID_PLANO); ");

                        ConfigurarPlano(login, sql, parametros);                    

                    if (login.Senha_WIFI != null)
                    {
                        //InserirSenhaWifi(sql, parametros, login.Senha_WIFI, login.MAC);
                    }

                    await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                }
            }
            if(login.Pool != null)
            {
              await ConfigurarPoolCliente(session, login);
            }
            

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error in ConfiguracaoPlanoCliente: {ex.Message}");
                throw; 
            }
        }

        private static void ConfigurarPlano(LoginRadiusModel login, StringBuilder sql, Dictionary<string, object> parametros)
        {
            if (login.UtilizaQoSProfile)
            {
                sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
                sql.Append("VALUES ");
                sql.Append($" (@DESCRICAO_PLANO, 'Huawei-Qos-Profile-Name', '=', @QOS_PROFILE, @ID_PLANO); "); //QoS Profile cadastrado na Concentradora contendo todos as velocidades definidas
                parametros.Add("@QOS_PROFILE", login.IdQoSProfile);
            }
            else
            {
                // Insert Huawei-Input-Average-Rate
                sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
                sql.Append("VALUES ");
                InserirVelocidade(Convert.ToDouble(login.VelocidadeUploadPlano), sql, parametros, "Huawei-Input-Average-Rate"); // velocidade mínima de upload a fornecer

                // Insert Huawei-Input-Peak-Rate
                sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
                sql.Append("VALUES ");
                InserirVelocidade(Convert.ToDouble(login.VelocidadeUploadPlano), sql, parametros, "Huawei-Input-Peak-Rate"); // velocidade máxima de upload

                // Insert Huawei-Output-Average-Rate
                sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
                sql.Append("VALUES ");
                InserirVelocidade(Convert.ToDouble(login.VelocidadeDownloadPlano), sql, parametros, "Huawei-Output-Average-Rate"); // velocidade mínima de download a fornecer

                // Insert Huawei-Output-Peak-Rate
                sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
                sql.Append("VALUES ");
                InserirVelocidade(Convert.ToDouble(login.VelocidadeDownloadPlano), sql, parametros, "Huawei-Output-Peak-Rate"); // velocidade máxima de download (nominal)
            }

        }

        public async Task ReduzirVelocidade(ISession session, ISession sessionConnect, LoginRadiusModel login)
        {
            try
            {
                bool velocidadeReduzida = false;

                if (login.UtilizaQoSProfile)
                {
                    using (var checkCom = session.Connection.CreateCommand())
                    {
                        StringBuilder sql = new StringBuilder();
                        Dictionary<string, object> parametros = new Dictionary<string, object>();


                        sql.AppendLine("SELECT COUNT(id) AS qtd FROM radreply");
                        sql.AppendLine("WHERE id_Cliente = @ID_CLIENTE");
                        sql.AppendLine("  AND username = @USERNAME");
                        sql.AppendLine("AND `attribute` = 'Huawei-Qos-Profile-Name' ");

                        parametros.Add("@ID_CLIENTE", login.IdCliente);
                        parametros.Add("@USERNAME", login.Login);

                        using (var reader = await CommandHelper.ExecuteReader(checkCom, sql, parametros, session.Connection, null))
                        {
                            if (reader.Read())
                                velocidadeReduzida = Convert.ToInt32(reader["qtd"]) != 0;
                        }
                    }

                    if (!velocidadeReduzida)
                    {
                        using (var com = session.Connection.CreateCommand())                        
                        {
                            StringBuilder sql = new StringBuilder();
                            Dictionary<string, object> parametros = new Dictionary<string, object>();

                            sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                            sql.AppendLine("VALUES");
                            sql.Append($" (@USERNAME, 'Huawei-Qos-Profile-Name', '=', @QOS_PROFILE, @ID_CLIENTE); ");

                            parametros.Add("@QOS_PROFILE", login.IdQoSProfileReduzido);
                            parametros.Add("@ID_CLIENTE", login.IdCliente);
                            parametros.Add("@USERNAME", login.Login);

                            await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);                            

                            // Comando para atualizar a velocidade do usuário na concentradora                            
                            AplicarModificacaoVelocidadeQoSProfile(login.Concentradora.Radius, login.Login, login.Concentradora.Ip, login.IdQoSProfileReduzido, login.usaRadiusSemContainer);
                        }
                    }
                }
                else
                {
                    using (var checkCom = session.Connection.CreateCommand())
                    {
                        StringBuilder sql = new StringBuilder();
                        Dictionary<string, object> parametros = new Dictionary<string, object>();


                        sql.AppendLine("SELECT COUNT(id) AS qtd FROM radreply");
                        sql.AppendLine("WHERE id_Cliente = @ID_CLIENTE");
                        sql.AppendLine("  AND username = @USERNAME");
                        //sql.AppendLine("AND (`attribute` = 'HW-Input-Committed-Burst-Size' ");
                        sql.AppendLine("AND (`attribute` = 'Huawei-Input-Average-Rate' ");
                        sql.AppendLine("OR `attribute` = 'Huawei-Input-Peak-Rate' ");
                        //sql.AppendLine("OR `attribute` = 'HW-Output-Committed-Burst-Size' ");
                        sql.AppendLine("OR `attribute` = 'Huawei-Output-Average-Rate' ");
                        sql.AppendLine("OR `attribute` = 'Huawei-Output-Peak-Rate') ");

                        parametros.Add("@ID_CLIENTE", login.IdCliente);
                        parametros.Add("@USERNAME", login.Login);

                        using (var reader = await CommandHelper.ExecuteReader(checkCom, sql, parametros, session.Connection, null))
                        {
                            if (reader.Read())
                                velocidadeReduzida = Convert.ToInt32(reader["qtd"]) != 0;
                        }
                    }

                    if (!velocidadeReduzida)
                    {
                        using (var com = session.Connection.CreateCommand())                        
                        {
                            StringBuilder sql = new StringBuilder();
                            Dictionary<string, object> parametros = new Dictionary<string, object>();

                            sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                            sql.AppendLine("VALUES");
                            InserirVelocidadeReduzida(Convert.ToDouble(login.VelocidadeUploadReduzidaPlano), sql, parametros, "Huawei-Input-Average-Rate");

                            sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                            sql.AppendLine("VALUES");
                            InserirVelocidadeReduzida(Convert.ToDouble(login.VelocidadeUploadReduzidaPlano), sql, parametros, "Huawei-Input-Peak-Rate");

                            sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                            sql.AppendLine("VALUES");
                            InserirVelocidadeReduzida(Convert.ToDouble(login.VelocidadeDownloadReduzidaPlano), sql, parametros, "Huawei-Output-Average-Rate");

                            sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                            sql.AppendLine("VALUES");
                            InserirVelocidadeReduzida(Convert.ToDouble(login.VelocidadeDownloadReduzidaPlano), sql, parametros, "Huawei-Output-Peak-Rate");

                            parametros.Add("@ID_CLIENTE", login.IdCliente);
                            parametros.Add("@USERNAME", login.Login);

                            await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);                           

                            // Comando para atualizar a velocidade do usuário na concentradora
                            var strVelocidade = GetVelocidadeReduzidaHuawei(login);
                            AplicarModificacaoVelocidade(login.Concentradora.Radius, login.Login, login.Concentradora.Ip, strVelocidade, login.usaRadiusSemContainer);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task NormalizarVelocidade(ISession session, ISession sessionConnect, LoginRadiusModel login)
        {
            using (var com = session.Connection.CreateCommand())           
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("DELETE FROM radreply");
                sql.AppendLine("WHERE (`attribute` = 'Huawei-Input-Burst-Size' ");
                sql.AppendLine("OR `attribute` = 'Huawei-Input-Average-Rate' ");
                sql.AppendLine("OR `attribute` = 'Huawei-Input-Peak-Rate' ");
                sql.AppendLine("OR `attribute` = 'Huawei-Output-Burst-Size' ");
                sql.AppendLine("OR `attribute` = 'Huawei-Output-Average-Rate' ");
                sql.AppendLine("OR `attribute` = 'Huawei-Qos-Profile-Name' ");
                sql.AppendLine("OR `attribute` = 'Huawei-Output-Peak-Rate') ");
                sql.AppendLine("  AND id_Cliente = @ID_CLIENTE");
                sql.AppendLine("  AND username = @USERNAME");

                parametros.Add("@ID_CLIENTE", login.IdCliente);
                parametros.Add("@USERNAME", login.Login);

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);                

                // Comando para atualizar a velocidade do usuário na concentradora
                if (login.UtilizaQoSProfile)
                {
                    AplicarModificacaoVelocidadeQoSProfile(login.Concentradora.Radius, login.Login, login.Concentradora.Ip, login.IdQoSProfile, login.usaRadiusSemContainer);
                }
                else
                {
                    var strVelocidade = GetVelocidadeHuawei(login);
                    AplicarModificacaoVelocidade(login.Concentradora.Radius, login.Login, login.Concentradora.Ip, strVelocidade, login.usaRadiusSemContainer);
                }

            }
        }

        #endregion

        #region Comandos Radius

        public void AplicarModificacaoVelocidade(Sinapse.Model.Radius servidor, string usuario, string ipConcetradora, string velocidade, bool usaRadiusSemContainer)
        {
            using (var rc = new RemoteConnection(servidor.Ip, Convert.ToInt32(servidor.Porta_SSH), servidor.Dominio, servidor.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    string[] Velocidades_Huawei = velocidade.Split('|');
                    String Download_Cir = Velocidades_Huawei[0];
                    String Download_Pir = Velocidades_Huawei[1];
                    String Upload_Cir = Velocidades_Huawei[2];
                    String Upload_Pir = Velocidades_Huawei[3];

                    if (usaRadiusSemContainer)
                    {
                        string comando = $"echo \"User-Name = {usuario}, Huawei-Input-Average-Rate = \"{Download_Cir}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}";
                        rc.ExecuteCommand(comando);

                        comando = $"echo \"User-Name = {usuario}, Huawei-Input-Peak-Rate = \"{Download_Pir}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}";
                        rc.ExecuteCommand(comando);

                        comando = $"echo \"User-Name = {usuario}, Huawei-Output-Average-Rate = \"{Upload_Cir}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}";
                        rc.ExecuteCommand(comando);

                        comando = $"echo \"User-Name = {usuario}, Huawei-Output-Peak-Rate = \"{Upload_Pir}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}";
                        rc.ExecuteCommand(comando);
                    }
                    else
                    {
                        string comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, Huawei-Input-Average-Rate = \"{Download_Cir}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}'";
                        rc.ExecuteCommand(comando);

                        comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, Huawei-Input-Peak-Rate = \"{Download_Pir}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}'";
                        rc.ExecuteCommand(comando);

                        comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, Huawei-Output-Average-Rate = \"{Upload_Cir}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}'";
                        rc.ExecuteCommand(comando);

                        comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, Huawei-Output-Peak-Rate = \"{Upload_Pir}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}'";
                        rc.ExecuteCommand(comando);
                    }

                }
            }
        }

        public void AplicarModificacaoVelocidadeQoSProfile(Sinapse.Model.Radius servidor, string usuario, string ipConcetradora, string idQoSProfile, bool usaRadiusSemContainer)
        {
            using (var rc = new RemoteConnection(servidor.Ip, Convert.ToInt32(servidor.Porta_SSH), servidor.Dominio, servidor.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (usaRadiusSemContainer)
                    {
                        string comando = $"echo \"User-Name = {usuario}, Huawei-Qos-Profile-Name = \"{idQoSProfile}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}";
                        rc.ExecuteCommand(comando);
                    }
                    else
                    {
                        string comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, Huawei-Qos-Profile-Name = \"{idQoSProfile}\"\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}'";
                        rc.ExecuteCommand(comando);
                    }

                }
            }
        }

        #endregion

        #region Query de inserção do plano

        //Pesquisar um atributo no Radius para poder utilizar PSK com a Huawei
        private void InserirSenhaWifi(StringBuilder sql, Dictionary<string, object> parametros, string senha_WIFI, string mac)
        {
            sql.Append("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente) values (@MAC, 'Mikrotik-Wireless-PSK', ':=', @SENHA_WIFI, @IDCLIENTE);");
            parametros.Add("@MAC", mac);
            parametros.Add("@SENHA_WIFI", senha_WIFI);
        }

        private void InserirIPV6Cliente(StringBuilder sql, Dictionary<string, object> parametros, string login, string enderecoIPV6)
        {
            sql.Append("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente) ");
            sql.Append("VALUES (@USERNAME, 'Huawei-Framed-IPv6-Address', '=', @ENDERECO_IPV6, @IDCLIENTE);");

            parametros.Add("@ENDERECO_IPV6", enderecoIPV6);

            if (!parametros.ContainsKey("@USERNAME"))
                parametros.Add("@USERNAME", login);
        }

        /* private void InserirIPV4Cliente(StringBuilder sql, Dictionary<string, object> parametros, string login, string enderecoIPV4)
         {
             //sql.Append("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-IP-Address', '=', @ENDERECO_IPV4, @IDCLIENTE);");

             sql.Append("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente) ");
             sql.Append("VALUES (@USERNAME, 'Framed-IP-Address', '=', @ENDERECO_IPV4, @IDCLIENTE);");

             parametros.Add("@ENDERECO_IPV4", enderecoIPV4);

             if (!parametros.ContainsKey("@USERNAME"))
                 parametros.Add("@USERNAME", login);
         } */

        private void InserirIPV4Cliente(StringBuilder sql, Dictionary<string, object> parametros, string login, string enderecoIPV4)
        {
            sql.Append("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente) ");
            sql.Append("VALUES (@USERNAME, 'Framed-IP-Address', '=', @ENDERECO_IPV4, @IDCLIENTE);");

            // Verifica se a chave @ENDERECO_IPV4 já existe
            if (!parametros.ContainsKey("@ENDERECO_IPV4"))
            {
                parametros.Add("@ENDERECO_IPV4", enderecoIPV4);
            }
            else
            {
                parametros["@ENDERECO_IPV4"] = enderecoIPV4;
            }

            // Verifica se a chave @USERNAME já existe
            if (!parametros.ContainsKey("@USERNAME"))
            {
                parametros.Add("@USERNAME", login);
            }
            else
            {
                parametros["@USERNAME"] = login;
            }
        }


        private static void InserirVelocidade(Double velocidade, StringBuilder sql, Dictionary<string, object> parametros, String campoSql, bool Converter_Velocidade_Real = true)
        {
            /*
            sql.Append($" (@DESCRICAO_PLANO, '{campoSql}', '=', @VELOCIDADE_PLANO, @ID_PLANO); ");
            parametros.Add("@VELOCIDADE_PLANO", ConvertMbpsToKbps(velocidade, Converter_Velocidade_Real)); */
          
                sql.Append($" (@DESCRICAO_PLANO, '{campoSql}', '=', @VELOCIDADE_PLANO, @ID_PLANO); ");

                if (!parametros.ContainsKey("@VELOCIDADE_PLANO"))
                {
                    parametros.Add("@VELOCIDADE_PLANO", ConvertMbpsToKbps(velocidade, Converter_Velocidade_Real));
                }
                else
                {
                    parametros["@VELOCIDADE_PLANO"] = ConvertMbpsToKbps(velocidade, Converter_Velocidade_Real);
                }            
        }

        private static void InserirVelocidadeReduzida(Double velocidade, StringBuilder sql, Dictionary<string, object> parametros, String campoSql, bool Converter_Velocidade_Real = true)
        {
            sql.Append($" (@USERNAME, '{campoSql}', '=', @VELOCIDADE_PLANO, @ID_CLIENTE); ");
            parametros.Add("@VELOCIDADE_PLANO", ConvertMbpsToKbps(velocidade, Converter_Velocidade_Real));
        }

        #endregion

        #region Velocidade Huawei

        private static string GetVelocidadeHuawei(LoginRadiusModel model)
        {
            String Download_Cir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeDownloadPlano));
            String Download_Pir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeDownloadPlano));
            String Upload_Cir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeUploadPlano));
            String Upload_Pir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeUploadPlano));


            return $"{Download_Cir}|{Download_Pir}|{Upload_Cir}|{Upload_Pir}";
        }

        private static string GetVelocidadeReduzidaHuawei(LoginRadiusModel model)
        {
            String Download_Cir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeDownloadReduzidaPlano));
            String Download_Pir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeDownloadReduzidaPlano));
            String Upload_Cir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeUploadReduzidaPlano));
            String Upload_Pir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeUploadReduzidaPlano));


            return $"{Download_Cir}|{Download_Pir}|{Upload_Cir}|{Upload_Pir}";
        }

        private static String ConvertMbpsToKbps(Double Velocidade_Mb, bool Converter_Velocidade_Real = true)
        {
            //var Velocidade_Kbps = Velocidade_Mb * 1000;

            //Cálculo será feito para pegar a velocidade real em Kbps (em múltiplos de 8 bits). Ex: 2Mbps = 2048Kbps     
            //Para facilitar o cálculo está sendo multiplicado em Kbits (1 Kbit é equivalente a 1024 bits) 
            //if (Converter_Velocidade_Real)
            //{
                var Velocidade_Kbps_Real = Velocidade_Mb * 1024;
               var Velocidade_Kbps = Velocidade_Kbps_Real * 1000;

            //return Convert.ToString(Velocidade_Kbps_Real);
            //}
            return Convert.ToString(Velocidade_Kbps);
        }
        #endregion

        public async Task EditarPlanoCliente(ISession session, LoginRadiusModel model)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();


                sql.AppendLine(" UPDATE radgroupreply SET ");
                sql.AppendLine(" groupname = @GROUPNAME, ");
                sql.AppendLine(" value = @VALUE1 ");
                sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                sql.AppendLine(" AND `attribute` = 'Huawei-Input-Average-Rate'; ");

                sql.AppendLine(" UPDATE radgroupreply SET ");
                sql.AppendLine(" groupname = @GROUPNAME, ");
                sql.AppendLine(" value = @VALUE2 ");
                sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                sql.AppendLine(" AND `attribute` = 'Huawei-Input-Peak-Rate'; ");

                sql.AppendLine(" UPDATE radgroupreply SET ");
                sql.AppendLine(" groupname = @GROUPNAME, ");
                sql.AppendLine(" value = @VALUE3 ");
                sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                sql.AppendLine(" AND `attribute` = 'Huawei-Output-Average-Rate'; ");

                sql.AppendLine(" UPDATE radgroupreply SET ");
                sql.AppendLine(" groupname = @GROUPNAME, ");
                sql.AppendLine(" value = @VALUE4 ");
                sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                sql.AppendLine(" AND `attribute` = 'Huawei-Output-Peak-Rate'; ");

                #region Dados Plano              

                String Download_Cir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeDownloadPlano));
                String Download_Pir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeDownloadPlano));
                String Upload_Cir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeUploadPlano));
                String Upload_Pir = ConvertMbpsToKbps(Convert.ToDouble(model.VelocidadeUploadPlano));

                #endregion

                string nomePlanoTratado = model.DescricaoPlano.Replace(",", " ");

                parametros.Add("@GROUPNAME", nomePlanoTratado);
                parametros.Add("@VALUE1", $"{Upload_Cir}");
                parametros.Add("@VALUE2", $"{Upload_Pir}");
                parametros.Add("@VALUE3", $"{Download_Cir}");
                parametros.Add("@VALUE4", $"{Download_Pir}");
                parametros.Add("@ID_PLANO", model.IdPlano);

                sql.AppendLine(" UPDATE radgroupcheck SET ");
                sql.AppendLine(" groupname = @GROUPNAME ");
                sql.AppendLine(" value = @STATUS_PLANO ");
                sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                sql.AppendLine("   AND `attribute` = 'Plano-Ativo'; ");

                parametros.Add("@STATUS_PLANO", model.SituacaoPlanoAtivo ? "S" : "N");

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task DesbloqueioConfianca(ISession session, LoginRadiusModel model, String usuario)
        {           
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("UPDATE radgroupcheck SET");
                sql.AppendLine("groupname = @GROUPNAME,");
                sql.AppendLine("value = 'S'");
                sql.AppendLine("WHERE Id_Plano = @ID_PLANO");
                sql.AppendLine("  AND `attribute` = 'Plano-Ativo'");

                string nomePlanoTratado = model.DescricaoPlano.Replace(",", " ");

                parametros.Add("@ID_PLANO", model.IdPlano);
                parametros.Add("@GROUPNAME", nomePlanoTratado);

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task DeletarPlano(ISession session, Int64 IdClientePlanoProduto)
        {
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            using (var com = session.Connection.CreateCommand())
            {
                sql.AppendLine(" SELECT COUNT(*) as Usuarios_Ativos ");
                sql.AppendLine(" FROM radusergroup radusr ");

                sql.AppendLine(" WHERE idCliente_PlanoProduto = @ID_CLIENTE_PLANO_PRODUTO ");
                sql.AppendLine("   AND(SELECT rdc.value ");
                sql.AppendLine("       FROM radcheck rdc ");
                sql.AppendLine("       WHERE rdc.username = radusr.username ");
                sql.AppendLine("         AND rdc.`attribute` = 'Usuario-Ativo') = 'S' ");

                parametros.Add("@ID_CLIENTE_PLANO_PRODUTO", IdClientePlanoProduto);

                using (var reader = CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    while (await reader.Result.ReadAsync())
                    {
                        if (Convert.ToInt32(reader.Result["Usuarios_Ativos"]) > 0)
                        {
                            throw new EAnielException("Existem Clientes associados a esse plano");
                        }
                    }
                }
            }

            using (var deleteCom = session.Connection.CreateCommand())
            {
                sql.Clear();
                sql.Append("DELETE FROM radusergroup WHERE idCliente_PlanoProduto = @ID_CLIENTE_PLANO_PRODUTO");

                await CommandHelper.ExecuteNonQuery(deleteCom, sql, parametros, session.Connection, null);
            }
        }

        public async Task ConfigurarPoolCliente(ISession session, LoginRadiusModel login)
        {
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            await LimparPoolCliente(session, login.Login, login.IdCliente, login.Id_Cliente_Plano_Produto);

            using (var com = session.Connection.CreateCommand())
            {
                if (login.Pool.IsPoolIpFixo)
                {
                    if (!String.IsNullOrEmpty(login.EnderecoIPV4))
                    {
                        InserirIPV4Cliente(sql, parametros, login.Login, login.EnderecoIPV4);
                    }

                    if (!String.IsNullOrEmpty(login.EnderecoIPV6))
                    {
                        InserirIPV6Cliente(sql, parametros, login.Login, login.EnderecoIPV6);
                    }
                }

                if (login.Pool.IsPoolConcentradora) //Pool controlada por Concentradora
                {
                    sql.AppendLine(" INSERT INTO radreply (username, attribute, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-Pool', ':=', @POOL_NAME, @IDCLIENTE);");
                }
                 
                if (login.Pool.IsDistribuicaoBlocoIp) //Pool oferecida como bloco de IPs fixos
                {
                    sql.AppendLine(" INSERT INTO radreply (username, attribute, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-Route', ':=', @POOL_NAME, @IDCLIENTE);");
                }

                if (!login.Pool.IsPoolConcentradora) //Pool controlada pelo Radius
                {
                    sql.AppendLine(" INSERT INTO radcheck (username, attribute, op, value, Id_Cliente) VALUES (@USERNAME, 'Pool-Name', ':=', @POOL_NAME, @IDCLIENTE);");
                }

                if (!parametros.ContainsKey("@USERNAME"))
                {
                    parametros.Add("@USERNAME", login.Login);
                }

                if (!parametros.ContainsKey("@POOL_NAME"))
                {
                    parametros.Add("@POOL_NAME", login.Pool.IsPoolConcentradora ? login.Pool.NomePoolConcentradora : login.Pool.Descricao);
                }

                if (!parametros.ContainsKey("@IDCLIENTE"))
                {
                    parametros.Add("@IDCLIENTE", login.IdCliente);
                }

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task LimparPoolCliente(ISession session, string usuario, long idCliente, long? idClientePlanoProduto)
        {
            
            Dictionary<string, object> parametros = new Dictionary<string, object>();           

            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("DELETE FROM radreply ");
                sql.Append("WHERE username = @USERNAME ");
                sql.Append("AND (`attribute` = 'Framed-IP-Address' OR `attribute` = 'Huawei-Framed-IPv6-Address' OR `attribute` = 'Framed-Pool' OR `attribute` = 'Framed-Route'); ");

                sql.Append("DELETE FROM radcheck ");
                sql.Append("WHERE username = @USERNAME ");
                sql.Append("AND `attribute` = 'Pool-Name'; ");


                parametros.Add("@IdCliente", idCliente);
                parametros.Add("@USERNAME", usuario);

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }
    }
}