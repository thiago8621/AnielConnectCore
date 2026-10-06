using NHibernate;
using Sinapse.Core.AspNetCore;
using Sinapse.DAO.AspNetCore.repository;
using Sinapse.Integracao.AspNetCore.Ativacao.Interfaces;
using Sinapse.Integracao.AspNetCore.Ativacao.Models;
using Sinapse.Util.AspNetCore.RemoteConnection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Integracao.AspNetCore.Ativacao.Concentradoras
{
    public class Juniper : IntegracaoConcentradora
    {

        public async Task ConfiguracaoPlanoCliente(ISession session, LoginRadiusModel login)
        {
            var planoCadastrado = false;

            string nomePlanoTratado = login.DescricaoPlano.Replace(",", " ");

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
                    sql.Append("                   VALUES (@DESCRICAO_PLANO, 'Plano-Ativo', '=', 'S', @ID_PLANO);");

                    ConfigurarPlano(login, sql, parametros);

                    if (login.Senha_WIFI != null)
                    {
                        //InserirSenhaWifi(sql, parametros, login.Senha_WIFI, login.MAC);
                    }

                    await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                }
            }

            await ConfigurarPoolCliente(session, login);
        }

        private static void ConfigurarPlano(LoginRadiusModel login, StringBuilder sql, Dictionary<string, object> parametros)
        {
            if (login.UtilizaPolicyNameIPv6)
            {
                sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
                sql.Append("VALUES ");
                sql.Append($" (@DESCRICAO_PLANO, 'ERX-IPv6-Ingress-Policy-Name', '=', @POLICY_NAME_UPLOAD, @ID_PLANO); "); //Politica de Upload IPv6 cadastrada na Concentradora contendo todas as velocidades definidas
                parametros.Add("@POLICY_NAME_UPLOAD", login.IdPolicyNameUpload);

                sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
                sql.Append("VALUES ");
                sql.Append($" (@DESCRICAO_PLANO, 'ERX-IPv6-Egress-Policy-Name', '=', @POLICY_NAME_DOWNLOAD, @ID_PLANO); "); //Politica de Download IPv6 cadastrada na Concentradora contendo todas as velocidades definidas
                parametros.Add("@POLICY_NAME_DOWNLOAD", login.IdPolicyNameDownload);
            }
            else
            {
                sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
                sql.Append("VALUES ");
                sql.Append($" (@DESCRICAO_PLANO, 'ERX-Ingress-Policy-Name', '=', @POLICY_NAME_UPLOAD, @ID_PLANO); "); //Politica de Upload cadastrada IPv4 na Concentradora contendo todas as velocidades definidas
                parametros.Add("@POLICY_NAME_UPLOAD", login.IdPolicyNameUpload);

                sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
                sql.Append("VALUES ");
                sql.Append($" (@DESCRICAO_PLANO, 'ERX-Egress-Policy-Name', '=', @POLICY_NAME_DOWNLOAD, @ID_PLANO); "); //Politica de Download cadastrada IPv4 na Concentradora contendo todas as velocidades definidas
                parametros.Add("@POLICY_NAME_DOWNLOAD", login.IdPolicyNameDownload);
            }



        }

        public async Task DeletarPlano(ISession session, long IdClientePlanoProduto)
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

        public async Task DesbloqueioConfianca(ISession session, LoginRadiusModel model, string usuario)
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

        public async Task EditarPlanoCliente(ISession session, LoginRadiusModel model)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                if (!model.UtilizaPolicyNameIPv6)
                {
                    sql.AppendLine(" UPDATE radgroupreply SET ");
                    sql.AppendLine(" groupname = @GROUPNAME, ");
                    sql.AppendLine(" value = @VALUE1 ");
                    sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                    sql.AppendLine(" AND `attribute` = 'ERX-Ingress-Policy-Name'; ");

                    sql.AppendLine(" UPDATE radgroupreply SET ");
                    sql.AppendLine(" groupname = @GROUPNAME, ");
                    sql.AppendLine(" value = @VALUE2 ");
                    sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                    sql.AppendLine(" AND `attribute` = 'ERX-Egress-Policy-Name'; ");
                }
                else
                {
                    sql.AppendLine(" UPDATE radgroupreply SET ");
                    sql.AppendLine(" groupname = @GROUPNAME, ");
                    sql.AppendLine(" value = @VALUE1 ");
                    sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                    sql.AppendLine(" AND `attribute` = 'ERX-IPv6-Ingress-Policy-Name'; ");

                    sql.AppendLine(" UPDATE radgroupreply SET ");
                    sql.AppendLine(" groupname = @GROUPNAME, ");
                    sql.AppendLine(" value = @VALUE2 ");
                    sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                    sql.AppendLine(" AND `attribute` = 'ERX-IPv6-Egress-Policy-Name'; ");
                }


                string nomePlanoTratado = model.DescricaoPlano.Replace(",", " ");

                parametros.Add("@GROUPNAME", nomePlanoTratado);
                parametros.Add("@VALUE1", $"{model.IdPolicyNameUpload}");
                parametros.Add("@VALUE2", $"{model.IdPolicyNameDownload}");
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

        public async Task NormalizarVelocidade(ISession session, ISession sessionConnect, LoginRadiusModel login)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("DELETE FROM radreply");
                sql.AppendLine("WHERE (`attribute` = 'ERX-Ingress-Policy-Name' ");
                sql.AppendLine("OR `attribute` = 'ERX-Egress-Policy-Name' ");
                sql.AppendLine("OR `attribute` = 'ERX-IPv6-Ingress-Policy-Name' ");
                sql.AppendLine("OR `attribute` = 'ERX-IPv6-Egress-Policy-Name') ");
                sql.AppendLine("  AND id_Cliente = @ID_CLIENTE");
                sql.AppendLine("  AND username = @USERNAME");

                parametros.Add("@ID_CLIENTE", login.IdCliente);
                parametros.Add("@USERNAME", login.Login);

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);

                // Comando para atualizar a velocidade do usuário na concentradora

                AplicarModificacaoVelocidadePolicyName(login.Concentradora.Radius, login.Login, login.Concentradora.Ip, login.IdPolicyNameUpload, login.IdPolicyNameDownload, login.usaRadiusSemContainer);
            }
        }

        public async Task ReduzirVelocidade(ISession session, ISession sessionConnect, LoginRadiusModel login)
        {
            try
            {
                bool velocidadeReduzida = false;

                using (var checkCom = session.Connection.CreateCommand())
                {
                    StringBuilder sql = new StringBuilder();
                    Dictionary<string, object> parametros = new Dictionary<string, object>();


                    sql.AppendLine("SELECT COUNT(id) AS qtd FROM radreply");
                    sql.AppendLine("WHERE id_Cliente = @ID_CLIENTE");
                    sql.AppendLine("  AND username = @USERNAME");
                    sql.AppendLine("AND (`attribute` = 'ERX-Ingress-Policy-Name' OR `attribute` = 'ERX-Egress-Policy-Name' OR `attribute` = 'ERX-IPv6-Ingress-Policy-Name' OR `attribute` = 'ERX-IPv6-Egress-Policy-Name') ");

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

                        if (login.UtilizaPolicyNameIPv6)
                        {
                            sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                            sql.AppendLine("VALUES");
                            sql.Append($" (@USERNAME, 'ERX-IPv6-Ingress-Policy-Name', '=', @POLICY_NAME_UPLOAD, @ID_CLIENTE); ");

                            sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                            sql.AppendLine("VALUES");
                            sql.Append($" (@USERNAME, 'ERX-IPv6-Egress-Policy-Name', '=', @POLICY_NAME_DOWNLOAD, @ID_CLIENTE); ");
                        }
                        else
                        {
                            sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                            sql.AppendLine("VALUES");
                            sql.Append($" (@USERNAME, 'ERX-Ingress-Policy-Name', '=', @POLICY_NAME_UPLOAD, @ID_CLIENTE); ");

                            sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                            sql.AppendLine("VALUES");
                            sql.Append($" (@USERNAME, 'ERX-Egress-Policy-Name', '=', @POLICY_NAME_DOWNLOAD, @ID_CLIENTE); ");
                        }


                        parametros.Add("@POLICY_NAME_UPLOAD", login.IdPolicyNameUploadReduzido);
                        parametros.Add("@POLICY_NAME_DOWNLOAD", login.IdPolicyNameDownloadReduzido);
                        parametros.Add("@ID_CLIENTE", login.IdCliente);
                        parametros.Add("@USERNAME", login.Login);

                        await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);

                        // Comando para atualizar a velocidade do usuário na concentradora                            
                        AplicarModificacaoVelocidadePolicyName(login.Concentradora.Radius, login.Login, login.Concentradora.Ip, login.IdPolicyNameUploadReduzido, login.IdPolicyNameDownloadReduzido, login.usaRadiusSemContainer);
                    }
                }




            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Query de inserção do plano

        //TODO pesquisar um atributo no Radius para poder utilizar PSK com a Juniper
        private void InserirSenhaWifi(StringBuilder sql, Dictionary<string, object> parametros, string senha_WIFI, string mac)
        {
            sql.Append("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente) values (@MAC, 'Mikrotik-Wireless-PSK', ':=', @SENHA_WIFI, @IDCLIENTE);");
            parametros.Add("@MAC", mac);
            parametros.Add("@SENHA_WIFI", senha_WIFI);
        }

        private void InserirIPV6Cliente(StringBuilder sql, Dictionary<string, object> parametros, string login, string enderecoIPV6)
        {
            sql.Append("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-IPv6-Address', '=', @ENDERECO_IPV6, @IDCLIENTE);");

            parametros.Add("@ENDERECO_IPV6", enderecoIPV6);

            if (!parametros.ContainsKey("@USERNAME"))
                parametros.Add("@USERNAME", login);
        }

        private void InserirIPV4Cliente(StringBuilder sql, Dictionary<string, object> parametros, string login, string enderecoIPV4)
        {
            sql.Append("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-IP-Address', '=', @ENDERECO_IPV4, @IDCLIENTE);");

            parametros.Add("@ENDERECO_IPV4", enderecoIPV4);

            if (!parametros.ContainsKey("@USERNAME"))
                parametros.Add("@USERNAME", login);
        }
        #endregion

        #region Comandos Radius

        public void AplicarModificacaoVelocidade(Sinapse.Model.AspNetCore.Radius servidor, string usuario, string ipConcetradora, string velocidade, bool usaRadiusSemContainer)
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
                        string comando = $"echo \"User-Name = {usuario}, Huawei-Input-Average-Rate = \"{Download_Cir}\"\" | radclient " + ipConcetradora + " coa testing123";
                        rc.ExecuteCommand(comando);

                        comando = $"echo \"User-Name = {usuario}, Huawei-Input-Peak-Rate = \"{Download_Pir}\"\" | radclient " + ipConcetradora + " coa testing123";
                        rc.ExecuteCommand(comando);

                        comando = $"echo \"User-Name = {usuario}, Huawei-Output-Average-Rate = \"{Upload_Cir}\"\" | radclient " + ipConcetradora + " coa testing123";
                        rc.ExecuteCommand(comando);

                        comando = $"echo \"User-Name = {usuario}, Huawei-Output-Peak-Rate = \"{Upload_Pir}\"\" | radclient " + ipConcetradora + " coa testing123";
                        rc.ExecuteCommand(comando);
                    }
                    else
                    {
                        string comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, Huawei-Input-Average-Rate = \"{Download_Cir}\"\" | radclient " + ipConcetradora + " coa testing123'";
                        rc.ExecuteCommand(comando);

                        comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, Huawei-Input-Peak-Rate = \"{Download_Pir}\"\" | radclient " + ipConcetradora + " coa testing123'";
                        rc.ExecuteCommand(comando);

                        comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, Huawei-Output-Average-Rate = \"{Upload_Cir}\"\" | radclient " + ipConcetradora + " coa testing123'";
                        rc.ExecuteCommand(comando);

                        comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, Huawei-Output-Peak-Rate = \"{Upload_Pir}\"\" | radclient " + ipConcetradora + " coa testing123'";
                        rc.ExecuteCommand(comando);
                    }

                }
            }
        }

        public void AplicarModificacaoVelocidadePolicyName(Sinapse.Model.AspNetCore.Radius servidor, string usuario, string ipConcetradora, string idUploadPolicy, string idDownloadPolicy, bool usaRadiusSemContainer)
        {
            using (var rc = new RemoteConnection(servidor.Ip, Convert.ToInt32(servidor.Porta_SSH), servidor.Dominio, servidor.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (usaRadiusSemContainer)
                    {
                        string comando = $"echo \"User-Name = {usuario}, ERX-Ingress-Policy-Name = \"{idUploadPolicy}\"\" | radclient " + ipConcetradora + " coa testing123";
                        rc.ExecuteCommand(comando);

                        string comando2 = $"echo \"User-Name = {usuario}, ERX-Egress-Policy-Name = \"{idDownloadPolicy}\"\" | radclient " + ipConcetradora + " coa testing123";
                        rc.ExecuteCommand(comando);
                    }
                    else
                    {
                        string comando = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, ERX-Ingress-Policy-Name = \"{idUploadPolicy}\"\" | radclient " + ipConcetradora + " coa testing123'";
                        rc.ExecuteCommand(comando);

                        string comando2 = $"docker exec -i radius bash -c 'echo \"User-Name = {usuario}, ERX-Egress-Policy-Name = \"{idDownloadPolicy}\"\" | radclient " + ipConcetradora + " coa testing123'";
                        rc.ExecuteCommand(comando);
                    }

                }
            }
        }
        #endregion

        public async Task ConfigurarPoolCliente(ISession session, LoginRadiusModel login)
        {
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            await LimparPoolCliente(session, login.Login, login.IdCliente);

            using (var com = session.Connection.CreateCommand())
            {
                if (login.Pool.IsPoolIpFixo)
                {
                    if (login.EnderecoIPV4 != null)
                    {
                        InserirIPV4Cliente(sql, parametros, login.Login, login.EnderecoIPV4);
                    }

                    if (login.EnderecoIPV6 != null)
                    {
                        InserirIPV6Cliente(sql, parametros, login.Login, login.EnderecoIPV6);
                    }
                }
                else if (login.Pool.IsPoolConcentradora) //Pool controlada por Concentradora
                {
                    sql.AppendLine(" INSERT INTO radreply (username, attribute, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-Pool', ':=', @POOL_NAME, @IDCLIENTE);");
                }
                else if (login.Pool.IsDistribuicaoBlocoIp) //Pool oferecida como bloco de IPs fixos
                {
                    sql.AppendLine(" INSERT INTO radreply (username, attribute, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-Route', ':=', @POOL_NAME, @IDCLIENTE);");
                }
                else if (!login.Pool.IsPoolConcentradora) //Pool controlada pelo Radius
                {
                    sql.AppendLine(" INSERT INTO radcheck (username, attribute, op, value, Id_Cliente) VALUES (@USERNAME, 'Pool-Name', ':=', @POOL_NAME, @IDCLIENTE);");
                }

                parametros.Add("@USERNAME", login.Login);
                parametros.Add("@POOL_NAME", login.Pool.IsPoolConcentradora ? login.Pool.NomePoolConcentradora : login.Pool.Descricao);
                parametros.Add("@IDCLIENTE", login.IdCliente);

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task LimparPoolCliente(ISession session, string usuario, long idCliente)
        {
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            using (var com = session.Connection.CreateCommand())
            {
                sql.Append("DELETE FROM radreply ");
                sql.Append("WHERE id_Cliente = @IdCliente ");
                sql.Append("AND (`attribute` = 'Framed-IP-Address' OR `attribute` = 'Framed-IPv6-Address' OR `attribute` = 'Framed-Pool' OR `attribute` = 'Framed-Route'); ");

                sql.Append("DELETE FROM radcheck ");
                sql.Append("WHERE id_Cliente = @IdCliente ");
                sql.Append("AND `attribute` = 'Pool-Name'); ");

                parametros.Add("@IdCliente", idCliente);

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }
    }
}
