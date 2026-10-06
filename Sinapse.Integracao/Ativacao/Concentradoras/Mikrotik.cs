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
using System.Web.UI.WebControls;

namespace Sinapse.Integracao.Ativacao.Concentradoras
{
    public class Mikrotik : IntegracaoConcentradora
    {
        #region Configuração do Plano

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
                        InserirSenhaWifi(sql, parametros, login.Senha_WIFI, login.MAC);
                    }

                    await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                }
            }

            if(login.Pool != null)
            {
                await ConfigurarPoolCliente(session, login);
            }
            
        }

        private static void ConfigurarPlano(LoginRadiusModel login, StringBuilder sql, Dictionary<string, object> parametros)
        {
            sql.Append("INSERT INTO radgroupreply (groupname, `attribute`, op, value, Id_Plano) ");
            sql.Append("VALUES ");
            InserirVelocidade(login, sql, parametros);
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
                    sql.AppendLine("  AND `attribute` = 'Mikrotik-Rate-Limit'");

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

                        // O Mikrotik interpreta o primeiro registro como limitação do upload e o segundo como limitação do download

                        sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                        sql.AppendLine("VALUES");
                        InserirVelocidadeReduzida(login, sql, parametros);

                        parametros.Add("@ID_CLIENTE", login.IdCliente);
                        parametros.Add("@USERNAME", login.Login);

                        await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);                        

                        // Comando para atualizar a velocidade do usuário na concentradora
                        var strVelocidade = GetVelocidadeReduzidaMikrotik(login);
                        AplicarModificacaoVelocidade(login.Concentradora.Radius, login.Login, login.Concentradora.Ip, strVelocidade, login.usaRadiusSemContainer);
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
                sql.AppendLine("WHERE `attribute` = 'Mikrotik-Rate-Limit'");
                sql.AppendLine("  AND id_Cliente = @ID_CLIENTE");
                sql.AppendLine("  AND username = @USERNAME");

                parametros.Add("@ID_CLIENTE", login.IdCliente);
                parametros.Add("@USERNAME", login.Login);

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);                

                // Comando para atualizar a velocidade do usuário na concentradora
                var strVelocidade = GetVelocidadeMikrotik(login);
                AplicarModificacaoVelocidade(login.Concentradora.Radius, login.Login, login.Concentradora.Ip, strVelocidade, login.usaRadiusSemContainer);
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
                    if (usaRadiusSemContainer) 
                    {
                        velocidade = "\\\"" + velocidade + "\\\"";
                        string comando = "echo \"User-Name = " + usuario + ", Mikrotik-Rate-Limit = " + velocidade + "\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}";
                        rc.ExecuteCommand(comando);
                    }
                    else
                    {
                        velocidade = "\\\"" + velocidade + "\\\"";
                        string comando = "docker exec -i radius bash -c 'echo \"User-Name = " + usuario + ", Mikrotik-Rate-Limit = " + velocidade + "\" | radclient " + ipConcetradora + $" coa {CONSTANTES.secretKeyRadius}'";
                        rc.ExecuteCommand(comando);
                    }
                    

                    
                }
            }
        }

        #endregion

        #region Query de inserção do plano

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
        }

        private void InserirIPV4Cliente(StringBuilder sql, Dictionary<string, object> parametros, string login, string enderecoIPV4)
        {
            sql.Append("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-IP-Address', '=', @ENDERECO_IPV4, @IDCLIENTE);");

            parametros.Add("@ENDERECO_IPV4", enderecoIPV4);            
        }       

        private static void InserirVelocidade(LoginRadiusModel model, StringBuilder sql, Dictionary<string, object> parametros)
        {

            sql.Append($" (@DESCRICAO_PLANO, 'Mikrotik-Rate-Limit', '=', @VELOCIDADE_PLANO, @ID_PLANO); ");
            parametros.Add("@VELOCIDADE_PLANO", GetVelocidadeMikrotik(model));
        }

        private static void InserirVelocidadeReduzida(LoginRadiusModel model, StringBuilder sql, Dictionary<string, object> parametros)
        {
            sql.Append($" (@USERNAME, 'Mikrotik-Rate-Limit', '=', @VELOCIDADE_PLANO, @ID_CLIENTE); ");
            parametros.Add("@VELOCIDADE_PLANO", GetVelocidadeReduzidaMikrotik(model));
        }

        #endregion

        #region Velocidade Miktrotik

        private static string GetVelocidadeMikrotik(LoginRadiusModel model)
        {
            double upload = Convert.ToDouble(model.VelocidadeUploadPlano) * 1000;
            double download = Convert.ToDouble(model.VelocidadeDownloadPlano) * 1000;

            double limiteMaximoUpload = 0;
            double limiteMaximoDownload = 0;

            double uploadReduzido = Convert.ToDouble(model.VelocidadeUploadReduzidaPlano) * 1000;
            double downloadReduzido = Convert.ToDouble(model.VelocidadeDownloadReduzidaPlano) * 1000;

            Int64 tempoUpload = 0;
            Int64 tempoDownload = 0;

            double garantiaUpload = (Convert.ToDouble(model.VelocidadeUploadPlano) * 0.4) * 1000;
            double garantiaDownload = (Convert.ToDouble(model.VelocidadeDownloadPlano) * 0.4) * 1000;

            int prioridade = 8;

            return $"{(upload > 0 ? upload.ToString() + "k" : "0")}/{(download > 0 ? download.ToString() + "k" : "0")} {(limiteMaximoUpload > 0 ? limiteMaximoUpload.ToString() + "k" : "0")}/{(limiteMaximoDownload > 0 ? limiteMaximoDownload.ToString() + "k" : "0")} {(uploadReduzido > 0 ? uploadReduzido.ToString() + "k" : "0")}/{(downloadReduzido > 0 ? downloadReduzido.ToString() + "k" : "0")} {tempoUpload}/{tempoDownload} {prioridade} {(garantiaUpload > 0 ? garantiaUpload.ToString() + "k" : "0")}/{(garantiaDownload > 0 ? garantiaDownload.ToString() + "k" : "0")} ";
        }

        private static string GetVelocidadeReduzidaMikrotik(LoginRadiusModel model)
        {
            double upload = Convert.ToDouble(model.VelocidadeUploadReduzidaPlano) * 1000;
            double download = Convert.ToDouble(model.VelocidadeDownloadReduzidaPlano) * 1000;

            double limiteMaximoUpload = 0;
            double limiteMaximoDownload = 0;

            double uploadReduzido = upload;
            double downloadReduzido = download;

            Int64 tempoUpload = 0;
            Int64 tempoDownload = 0;

            double garantiaUpload = (Convert.ToDouble(model.VelocidadeUploadReduzidaPlano) * 0.4) * 1000;
            double garantiaDownload = (Convert.ToDouble(model.VelocidadeDownloadReduzidaPlano) * 0.4) * 1000;

            // Prioridade mais alta para se sobrepor à velocidade padrão do plano
            int prioridade = 1;

            return $"{(upload > 0 ? upload.ToString() + "k" : "0")}/{(download > 0 ? download.ToString() + "k" : "0")} {(limiteMaximoUpload > 0 ? limiteMaximoUpload.ToString() + "k" : "0")}/{(limiteMaximoDownload > 0 ? limiteMaximoDownload.ToString() + "k" : "0")} {(uploadReduzido > 0 ? uploadReduzido.ToString() + "k" : "0")}/{(downloadReduzido > 0 ? downloadReduzido.ToString() + "k" : "0")} {tempoUpload}/{tempoDownload} {prioridade} {(garantiaUpload > 0 ? garantiaUpload.ToString() + "k" : "0")}/{(garantiaDownload > 0 ? garantiaDownload.ToString() + "k" : "0")} ";
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
                sql.AppendLine(" value = @VALUE ");
                sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                sql.AppendLine("   AND `attribute` = 'Mikrotik-Rate-Limit'; ");

                #region Dados Plano

                var upload = Convert.ToDouble(model.VelocidadeUploadPlano) * 1000;
                var download = Convert.ToDouble(model.VelocidadeDownloadPlano) * 1000;

                var limiteMaxUpload = 0;
                var limiteMaxDownload = 0;

                var uploadReduzido = Convert.ToDouble(model.VelocidadeUploadReduzidaPlano) * 1000;
                var downloadReduzido = Convert.ToDouble(model.VelocidadeDownloadReduzidaPlano) * 1000;

                var tempoDownload = 0;
                var tempoUpload = 0;

                var garantiaUpload = (Convert.ToDouble(model.VelocidadeUploadPlano) * 0.4) * 1000;
                var garantiaDownload = (Convert.ToDouble(model.VelocidadeDownloadPlano) * 0.4) * 1000;

                int prioridade = 8;

                var uploadStr = upload > 0 ? upload.ToString() + "k" : "0";
                var downloadStr = download > 0 ? download.ToString() + "k" : "0";

                var limiteMaxUploadStr = limiteMaxUpload > 0 ? limiteMaxUpload.ToString() + "k" : "0";
                var limiteMaxDownloadStr = limiteMaxDownload > 0 ? limiteMaxDownload.ToString() + "k" : "0";

                var uploadReduzidoStr = uploadReduzido > 0 ? uploadReduzido.ToString() + "k" : "0";
                var downloadReduzidoStr = downloadReduzido > 0 ? downloadReduzido.ToString() + "k" : "0";

                var garantiaUploadStr = garantiaUpload > 0 ? garantiaUpload.ToString() + "k" : "0";
                var garantiaDownloadStr = garantiaDownload > 0 ? garantiaDownload.ToString() + "k" : "0";

                #endregion

                string nomePlanoTratado = model.DescricaoPlano.Replace(",", " ");

                parametros.Add("@GROUPNAME", nomePlanoTratado);
                parametros.Add("@VALUE", $"{uploadStr}/{downloadStr} {limiteMaxUploadStr}/{limiteMaxDownloadStr} {uploadReduzidoStr}/{downloadReduzidoStr} {tempoUpload}/{tempoDownload} {prioridade} {garantiaUploadStr}/{garantiaDownloadStr}");
                parametros.Add("@ID_PLANO", model.IdPlano);

                sql.AppendLine(" UPDATE radgroupcheck SET ");
                sql.AppendLine(" groupname = @GROUPNAME ");
                sql.AppendLine(" value = @STATUS_PLANO ");
                sql.AppendLine(" WHERE Id_Plano = @ID_PLANO ");
                sql.AppendLine("   AND `attribute` = 'Plano-Ativo' ");

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
            
            try
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                //Deleta todos os atributos relacionados a IP e Pool de IP
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
                    else if (login.Pool.IsDistribuicaoBlocoIp) //Pool oferecida como bloco de IPs fixos
                    {
                        sql.AppendLine(" INSERT INTO radreply (username, attribute, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-Route', ':=', @POOL_NAME, @IDCLIENTE);");
                    }
                    else if (login.Pool.IsPoolConcentradora) //Pool controlada por Concentradora
                    {
                        sql.AppendLine(" INSERT INTO radreply (username, attribute, op, value, id_Cliente) VALUES (@USERNAME, 'Framed-Pool', ':=', @POOL_NAME, @IDCLIENTE);");
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
            catch (Exception ex)
            {

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
                sql.Append("AND (`attribute` = 'Framed-IP-Address' OR `attribute` = 'Framed-IPv6-Address' OR `attribute` = 'Framed-Pool' OR `attribute` = 'Framed-Route'); ");

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