using Aniel.Connect.Core;
using NetTools;
using NHibernate;
using NHibernate.Hql.Ast;
using PrimS.Telnet;
using Renci.SshNet;
using Sinapse.DAO;
using Sinapse.DAO.MongoDB;
using Sinapse.DAO.repository;
using Sinapse.DAO.Repository;
using Sinapse.Integracao.Ativacao.Concentradoras;
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
using System.Web.UI.WebControls;
using static System.Collections.Specialized.BitVector32;

namespace Sinapse.Integracao.Ativacao.Radius
{
    public class AnielRadius : IntegracaoRadius
    {
        private Sinapse.Model.Radius radius;
        private ISession sessionRadius;

        public AnielRadius(Sinapse.Model.Radius radius,ISession _sessaoExterna = null)
        {
            this.radius = radius;

            if(radius == null)
            throw new Exception($"Radius não identificado");

            if (String.IsNullOrEmpty(this.radius.Dominio))
                throw new Exception($"Usuário do Radius \"{this.radius.Descricao}\" não configurado");

            if (String.IsNullOrEmpty(this.radius.Senha))
                throw new Exception($"Senha do Radius \"{this.radius.Descricao}\" não configurada");

            if (String.IsNullOrEmpty(this.radius.Ip))
                throw new Exception($"Endereço Ip do Radius \"{this.radius.Descricao}\" não configurado");

            //TODO: Necessário definir estrutura para salvar credenciais do banco para criar session dinâmica
            if (!String.IsNullOrEmpty(this.radius.Usuario_Banco) && !String.IsNullOrEmpty(this.radius.Senha_Banco))
            {
                sessionRadius = Sinapse.DAO.repository.HibernateUtil.GetSessionRadius(this.radius.Ip, this.radius.Usuario_Banco, this.radius.Senha_Banco);
            }

            if (_sessaoExterna != null)
                sessionRadius = _sessaoExterna;
        }

        //Não usar por enquanto
        public ISession GetSessionRadius()
        {
            return sessionRadius;
        }

        #region Concentradora

        public async Task CadastrarConcentradora(ISession session, Concentradora concentradora)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("INSERT INTO nas (nasname, shortname, type, ports, secret, server, community, Id_Concentradora) ");
                sql.AppendLine("VALUES (@NASNAME, @SHORTNAME, @TYPE, @PORTS, @SECRET, @SERVER, @COMMUNITY, @IdConcentradora) ");

                parametros.Add("@NASNAME", concentradora.Ip);
                parametros.Add("@SHORTNAME", concentradora.Nome);

                switch (concentradora.Tipo)
                {
                    default:
                        parametros.Add("@TYPE", "other");
                        break;
                }

                parametros.Add("@PORTS", null);
                parametros.Add("@SECRET", CONSTANTES.secretKeyRadius);
                parametros.Add("@SERVER", null);
                parametros.Add("@COMMUNITY", null);
                parametros.Add("@IdConcentradora", concentradora.Id);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                ReiniciarServicoRadius();
            }
        }

        public async Task EditarConcentradora(ISession session, Concentradora concentradora)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                sql.AppendLine("UPDATE nas SET ");
                sql.AppendLine("nasname = @NASNAME, ");
                sql.AppendLine("shortname = @SHORTNAME,");
                sql.AppendLine("type = @TYPE,ports = @PORTS, ");
                sql.AppendLine("server = @SERVER, community = @COMMUNITY ");
                sql.AppendLine("WHERE Id_Concentradora = @IdConcentradora ");

                parametros.Add("@NASNAME", concentradora.Ip);
                parametros.Add("@SHORTNAME", concentradora.Nome);

                switch (concentradora.Tipo)
                {
                    default:
                        parametros.Add("@TYPE", "other");
                        break;
                }

                parametros.Add("@PORTS", null);
                parametros.Add("@SERVER", null);
                parametros.Add("@COMMUNITY", null);
                parametros.Add("@IdConcentradora", concentradora.Id);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                ReiniciarServicoRadius();
            }
        }

        public async Task DeletarConcentradora(ISession session, long idConcentradora)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                sql.AppendLine("DELETE FROM nas WHERE Id_Concentradora = @IdConcentradora ");

                parametros.Add("@IdConcentradora", idConcentradora);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                ReiniciarServicoRadius();
            }
        }

        #endregion

        #region Usuário

        public async Task CadastrarUsuario(ISession session, LoginRadiusModel login)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                // TODO: Estudar a questão das conexões agarradas com o Simultaneous-Use = 1 antes de adicionar novamente

                sql.AppendLine("INSERT INTO radcheck (username, attribute, op, value, Id_Cliente) VALUES ");
                sql.AppendLine("                     (@USERNAME, 'Cleartext-Password', ':=', @SENHA, @IDCLIENTE), ");
                //sql.AppendLine("                     (@USERNAME, 'Simultaneous-Use' , ':=', '1', @IDCLIENTE), ");
                sql.AppendLine("                     (@USERNAME, 'Framed-Protocol', ':=', 'PPP', @IDCLIENTE), ");
                sql.AppendLine("                     (@USERNAME, 'NAS-IP-Address', '==', @NAS_IP, @IDCLIENTE) ");                

                /*if (login.MAC != null)
                {
                    sql.AppendLine("                   ,(@USERNAME, 'Calling-Station', ':=', @MAC, @IDCLIENTE) ");
                    parametros.Add("@MAC", login.MAC);
                } */

                parametros.Add("@USERNAME", login.Login);
                parametros.Add("@SENHA", login.Senha);
                parametros.Add("@NAS_IP", login.Concentradora.Ip);
                parametros.Add("@IDCLIENTE", login.IdCliente);
                if(login.Pool != null)
                {
                    parametros.Add("@POOL_NAME", login.Pool.Descricao);
                }
                
                
                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
            if(login.Pool != null)
            {
                switch (login.Concentradora.Tipo)
                {
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                        await new Mikrotik().ConfigurarPoolCliente(session, login);
                        break;
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Huawei:
                        await new Huawei().ConfigurarPoolCliente(session, login);
                        break;
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Juniper:
                        await new Juniper().ConfigurarPoolCliente(session, login);
                        break;
                }
            }
           
        }

        public async Task CadastrarUsuario(ISession session, string usuario, string senha, long idCliente)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine(" INSERT INTO radcheck (username, attribute, op, value, Id_Cliente) VALUES ");
                sql.AppendLine("                      (@USERNAME, 'Cleartext-Password', ':=', @SENHA, @IDCLIENTE) ");

                parametros.Add("@USERNAME", usuario);
                parametros.Add("@SENHA", senha);
                parametros.Add("@IDCLIENTE", idCliente);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task FinalizarCadastroUsuario(ISession session, LoginRadiusModel login)
        {
            using (var com = session.Connection.CreateCommand())
            {
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                StringBuilder sql = new StringBuilder();

                sql.AppendLine(" INSERT INTO radcheck (username, attribute, op, value, Id_Cliente) VALUES ");
                //sql.AppendLine("                      (@USERNAME, 'Simultaneous-Use', ':=', '1', @IDCLIENTE), ");
                sql.AppendLine("                      (@USERNAME, 'Framed-Protocol', ':=', 'PPP', @IDCLIENTE), ");
                sql.AppendLine("                      (@USERNAME, 'NAS-IP-Address', '==', @NAS_IP, @IDCLIENTE) ");
                

               /* if (login.MAC != null)
                {
                    sql.AppendLine(",(@USERNAME, 'Calling-Station', ':=', @MAC, @IDCLIENTE) ");
                    parametros.Add("@MAC", login.MAC);
                } */

                parametros.Add("@USERNAME", login.Login);
                parametros.Add("@IDCLIENTE", login.IdCliente);
                parametros.Add("@NAS_IP", login.Concentradora.Ip);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            if(login.Pool != null)
            {
                switch (login.Concentradora.Tipo)
                {
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                        await new Mikrotik().ConfigurarPoolCliente(session, login);
                        break;
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Huawei:
                        await new Huawei().ConfigurarPoolCliente(session, login);
                        break;
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Juniper:
                        await new Juniper().ConfigurarPoolCliente(session, login);
                        break;
                }
            }
     
        }


        public async Task EditarUsuario(ISession session, LoginRadiusModel login)
        {
            var cadastroSimplificado = false;            
            var temMacCadastrado = false;
            //var usernameAnterior = "";
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

               sql.AppendLine(" SELECT COUNT(id) as QTD_REGISTROS FROM radcheck WHERE Id_Cliente = @IDCLIENTE AND attribute LIKE 'Calling-Station' ");
                parametros.Add("@IDCLIENTE", login.IdCliente);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        temMacCadastrado = Convert.ToInt32(reader["QTD_REGISTROS"]) == 1;
                    }
                } 
            }           

            /*using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                //sql.AppendLine(" SELECT COUNT(id) as QTD_REGISTROS FROM radcheck WHERE Id_Cliente = @IDCLIENTE AND (username = @USERNAME OR username = @USERNAMEANTERIOR) ");
                sql.AppendLine(" SELECT COUNT(id) as QTD_REGISTROS FROM radcheck WHERE (username = @USERNAME OR username = @USERNAMEANTERIOR) ");
                parametros.Add("@IDCLIENTE", login.IdCliente);
                parametros.Add("@USERNAME", login.Login);
                parametros.Add("@USERNAMEANTERIOR", login.LoginAntigo);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        cadastroSimplificado = Convert.ToInt32(reader["QTD_REGISTROS"]) == 1;
                    }
                }
            }*/

            //Coleta username anterior para poder utilizar na condição do "Where" da edição"
            /*using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine(" SELECT username FROM radusergroup WHERE idCliente_PlanoProduto = @IDPLANOPRODUTO ");
                sql.AppendLine(" LIMIT 1 ");
                parametros.Add("@IDPLANOPRODUTO", login.Id_Cliente_Plano_Produto);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        usernameAnterior = reader["username"].ToString();
                    }
                }
            }*/

            if (cadastroSimplificado)
            {
                await FinalizarCadastroUsuario(session, login);
            }
            else
            {
                var listaAtributosExistentes = await GetAtributosExistentesByLogin(login.LoginAntigo ?? login.Login);

                using (var com = session.Connection.CreateCommand())
                { 
                    StringBuilder sql = new StringBuilder();
                    Dictionary<string, object> parametros = new Dictionary<string, object>();

                    /*sql.AppendLine("UPDATE radcheck set username = @USERNAME WHERE Id_Cliente = @IDCLIENTE AND username = @USERNAMEANTERIOR;");
                    sql.AppendLine("UPDATE radcheck set value = @SENHA WHERE attribute = 'Cleartext-Password' and Id_Cliente = @IDCLIENTE and username = @USERNAMEANTERIOR;");
                    sql.AppendLine("UPDATE radcheck set value = @NAS_IP WHERE attribute = 'NAS-IP-Address' and Id_Cliente = @IDCLIENTE and username = @USERNAMEANTERIOR;");

                    sql.AppendLine("UPDATE radreply set username = @USERNAME WHERE id_Cliente = @IDCLIENTE AND username = @USERNAMEANTERIOR;");

                    //Atualizar username na radusergroup também
                    sql.AppendLine("UPDATE radusergroup SET username = @USERNAME, idCliente_PlanoProduto = @IDPLANOPRODUTO WHERE username = @USERNAMEANTERIOR;");*/

                    //Atualiza login e Id Cliente em todos os registros existentes na radcheck
                    sql.AppendLine("UPDATE radcheck set username = @USERNAME, Id_Cliente = @IDCLIENTE WHERE username = @USERNAMEANTERIOR;");
                    //Atualiza login e Id Cliente em todos os registros existentes na radreply
                    sql.AppendLine("UPDATE radreply set username = @USERNAME, id_Cliente = @IDCLIENTE WHERE username = @USERNAMEANTERIOR;");
                    //Atualiza login e Id Cliente Plano Produto em todos os registros existentes na radusergroup
                    sql.AppendLine("UPDATE radusergroup SET username = @USERNAME, idCliente_PlanoProduto = @IDPLANOPRODUTO WHERE username = @USERNAMEANTERIOR;");

                    if (listaAtributosExistentes.Any(p => p == AtributosLoginRadius.Cleartext_Password))
                    {
                        sql.AppendLine("UPDATE radcheck set value = @SENHA WHERE attribute = 'Cleartext-Password' and username = @USERNAME;");
                    }
                    else
                    {
                        sql.AppendLine(" INSERT INTO radcheck (username, attribute, op, value, Id_Cliente) VALUES ");
                        sql.AppendLine("                      (@USERNAME, 'Cleartext-Password', ':=', @SENHA, @IDCLIENTE); ");
                    }

                    if (listaAtributosExistentes.Any(p => p == AtributosLoginRadius.NAS_IP_Address))
                    {
                        sql.AppendLine("UPDATE radcheck set value = @NAS_IP WHERE attribute = 'NAS-IP-Address' and username = @USERNAME;");
                    }
                    else
                    {
                        sql.AppendLine(" INSERT INTO radcheck (username, attribute, op, value, Id_Cliente) VALUES ");                        
                        sql.AppendLine("                      (@USERNAME, 'NAS-IP-Address', '==', @NAS_IP, @IDCLIENTE); ");
                    }                                        

                    if (!listaAtributosExistentes.Any(p => p == AtributosLoginRadius.Framed_Protocol))
                    {
                        sql.AppendLine(" INSERT INTO radcheck (username, attribute, op, value, Id_Cliente) VALUES ");                        
                        sql.AppendLine("                      (@USERNAME, 'Framed-Protocol', ':=', 'PPP', @IDCLIENTE); ");                        
                    }

                    /*if (login.MAC != null)
                    {
                        if (!temMacCadastrado)
                        {
                            sql.AppendLine("INSERT INTO radcheck (username, attribute, op, value, Id_Cliente) VALUES (@USERNAME, 'Calling-Station', ':=', @MAC, @IDCLIENTE);");
                        }
                        else
                        {
                            sql.AppendLine("UPDATE radcheck set value = @MAC WHERE attribute = 'Calling-Station' and Id_Cliente = @IDCLIENTE;");
                        }

                        parametros.Add("@MAC", login.MAC);
                    } */

                    parametros.Add("@USERNAME", login.Login);
                    parametros.Add("@SENHA", login.Senha);
                    parametros.Add("@NAS_IP", login.Concentradora.Ip);
                    parametros.Add("@IDCLIENTE", login.IdCliente);
                    parametros.Add("@USERNAMEANTERIOR", login.LoginAntigo ?? login.Login);
                    parametros.Add("@IDPLANOPRODUTO", login.Id_Cliente_Plano_Produto);


                    await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                }
            }
            
            if(login.Pool != null)
            {
                switch (login.Concentradora.Tipo)
                {
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                        await new Mikrotik().ConfigurarPoolCliente(session, login);
                        break;
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Huawei:
                        await new Huawei().ConfigurarPoolCliente(session, login);
                        break;
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Juniper:
                        await new Juniper().ConfigurarPoolCliente(session, login);
                        break;
                }
            }
           

            await DesconectarLogin(login, login.usaRadiusSemContainer);
        }

        public async Task EditarUsuario(ISession session, string Usuario, string Senha, long idCliente, long idClientePlanoProduto)
        {
            var usernameAnterior = "";
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            //Coleta username anterior para poder utilizar na condição do "Where" da edição"
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                

                sql.AppendLine(" SELECT username FROM radusergroup WHERE idCliente_PlanoProduto = @IDPLANOPRODUTO ");
                sql.AppendLine(" LIMIT 1 ");
                parametros.Add("@IDPLANOPRODUTO", idClientePlanoProduto);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        usernameAnterior = reader["username"].ToString();
                    }
                }
            }

            

            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("UPDATE radcheck set username = @USERNAME WHERE Id_Cliente = @IDCLIENTE AND username = @USERNAMEANTERIOR;");
                sql.AppendLine("UPDATE radcheck set value = @SENHA WHERE attribute = 'Cleartext-Password' and Id_Cliente = @IDCLIENTE and username = @USERNAMEANTERIOR;");

                parametros.Add("@USERNAME", Usuario);
                parametros.Add("@SENHA", Senha);
                parametros.Add("@USERNAMEANTERIOR", usernameAnterior);
                parametros.Add("@IDCLIENTE", idCliente);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task DeletarUsuario(ISession session, long idCliente)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                sql.AppendLine("DELETE FROM radcheck WHERE Id_Cliente = @IdCliente; ");
                sql.AppendLine("DELETE FROM radreply WHERE id_Cliente = @IdCliente;");
                parametros.Add("@IdCliente", idCliente);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }


        public async Task BloquearUsuario(ISession session, string Usuario, string IpConcentradora, string PortaIncoming, string Secret, bool UsaRadiusSemContainer)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("UPDATE radusergroup set groupname = 'Inativo' WHERE username = @USERNAME;");

                parametros.Add("@USERNAME", Usuario);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (String.IsNullOrEmpty(PortaIncoming))
                        PortaIncoming = "3799";

                    string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + Usuario + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}"
                        : "docker exec -i radius bash -c 'echo \"User-Name = " + Usuario + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}'";

                    rc.ExecuteCommand(comando);
                }
            }
        }

        public async Task DesbloquearUsuario(ISession session, string Usuario, string Plano)
        {
            try
            {
                using (var com = session.Connection.CreateCommand())
                {
                    StringBuilder sql = new StringBuilder();
                    Dictionary<string, object> parametros = new Dictionary<string, object>();

                    sql.AppendLine("UPDATE radusergroup set groupname = @PLANO WHERE username = @USERNAME;");

                    string nomePlanoTratado = Plano.Replace(",", " ");

                    parametros.Add("@PLANO", nomePlanoTratado);
                    parametros.Add("@USERNAME", Usuario);

                    await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            
            
            }
           
        }

        public async Task BloquearSinalPorPool(ISession session, string Usuario, long idCliente, string IpConcentradora, string PortaIncoming, string nomePoolBloqueio, bool UsaRadiusSemContainer)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("UPDATE radcheck set value = @POOLNAME WHERE Id_Cliente = @IDCLIENTE AND (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool') AND username = @USERNAME;");
                sql.AppendLine("UPDATE radreply set value = @POOLNAME WHERE id_Cliente = @IDCLIENTE AND (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool') AND username = @USERNAME;");

                parametros.Add("@IDCLIENTE", idCliente);
                parametros.Add("@POOLNAME", nomePoolBloqueio);
                parametros.Add("@USERNAME", Usuario);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (String.IsNullOrEmpty(PortaIncoming))
                        PortaIncoming = "3799";

                    string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + Usuario + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}"
                        : "docker exec -i radius bash -c 'echo \"User-Name = " + Usuario + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}'";

                    rc.ExecuteCommand(comando);
                }
            }
        }

        public async Task DesbloquearSinalPorPool(ISession session, LoginRadiusModel login)
        {
            try
            {
                switch (login.Concentradora.Tipo)
                {
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                        await new Mikrotik().ConfigurarPoolCliente(session, login);
                        break;
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Huawei:
                        await new Huawei().ConfigurarPoolCliente(session, login);
                        break;
                    case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Juniper:
                        await new Juniper().ConfigurarPoolCliente(session, login);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

              
        }

        //Apenas para HUAWEI
        public async Task BloquearSinalPorDomain(ISession session, String Login, String IpConcentradora, String PortaIncoming, String IdDomainName, long IdCliente, bool UsaRadiusSemContainer)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("INSERT INTO radreply (username, `attribute`, op, value, id_Cliente)");
                sql.AppendLine("VALUES");
                sql.Append($" (@USERNAME, 'Huawei-Domain-Name', '=', @DOMAIN_NAME, @ID_CLIENTE); ");

                parametros.Add("@USERNAME", Login);
                parametros.Add("@DOMAIN_NAME", IdDomainName);
                parametros.Add("@ID_CLIENTE", IdCliente);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (String.IsNullOrEmpty(PortaIncoming))
                        PortaIncoming = "3799";

                    string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + Login + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}"
                        : "docker exec -i radius bash -c 'echo \"User-Name = " + Login + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}'";

                    rc.ExecuteCommand(comando);
                }
            }
        }

        //Apenas para HUAWEI
        public async Task DesbloquearSinalPorDomain(ISession session, ISession sessionConnect, LoginRadiusModel login, bool UsaRadiusSemContainer)
        {
            try
            {
                using (var com = session.Connection.CreateCommand())
                {
                    StringBuilder sql = new StringBuilder();
                    Dictionary<string, object> parametros = new Dictionary<string, object>();

                    sql.AppendLine("DELETE FROM radreply ");
                    sql.AppendLine("WHERE `attribute` = 'Huawei-Domain-Name' ");
                    sql.AppendLine("  AND id_Cliente = @ID_CLIENTE");
                    sql.AppendLine("  AND username = @USERNAME");

                    parametros.Add("@USERNAME", login.Login);
                    parametros.Add("@ID_CLIENTE", login.IdCliente);

                    await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            try
            {
                using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
                {
                    rc.Connect();

                    if (rc.IsConnected())
                    {
                        if (String.IsNullOrEmpty(login.Concentradora.PortaIncoming))
                            login.Concentradora.PortaIncoming = "3799";

                        string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + login.Login + "\" | radclient " + login.Concentradora.Ip + ":" + login.Concentradora.PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}"
                            : "docker exec -i radius bash -c 'echo \"User-Name = " + login.Login + "\" | radclient " + login.Concentradora.Ip + ":" + login.Concentradora.PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}'";

                        rc.ExecuteCommand(comando);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            
        }

        public async Task AssociaUsuarioAoPlano(ISession session, string UsuarioPPPOE, string NomePlano, string Prioridade, long IdClientePlanoProduto)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                sql.AppendLine(" INSERT INTO radusergroup (username, groupname, priority, idCliente_PlanoProduto) VALUES ");
                sql.AppendLine("                          (@USERNAME, @GROUPNAME, @PRIORITY, @IDCLIENTE_PLANO_PRODUTO); ");

                string nomePlanoTratado = NomePlano.Replace(",", " ");

                parametros.Add("@USERNAME", UsuarioPPPOE);
                parametros.Add("@GROUPNAME", nomePlanoTratado);
                parametros.Add("@PRIORITY", Prioridade);
                parametros.Add("@IDCLIENTE_PLANO_PRODUTO", IdClientePlanoProduto);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task CadastrarPoolIp(ISession session, string NomePool, IPAddressRange RangeIP)
        {
            using (var com = session.Connection.CreateCommand())
            {
                int i = 1;
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                foreach (var ip in RangeIP)
                {
                    sql.AppendLine(" INSERT INTO radippool (pool_name, framedipaddress) VALUES ");
                    sql.AppendLine($" (@NOMEPOOL, @IP{i}); ");

                    parametros.Add($"@IP{i}", ip.ToString());

                    i++;
                }

                parametros.Add("@NOMEPOOL", NomePool);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task EditarPoolIp(ISession session, string NomePool, IPAddressRange Novo_RangeIP, string Novo_NomePool = null)
        {

            await DeletarPoolIp(session, NomePool);
            if (!String.IsNullOrEmpty(Novo_NomePool))
            {
                await CadastrarPoolIp(session, Novo_NomePool, Novo_RangeIP);
                //Editar na Radcheck caso o nome da pool tenha mudado
                await AtualizarPoolIpDosUsuarios(session, NomePool, Novo_NomePool);
            }
            else
                await CadastrarPoolIp(session, NomePool, Novo_RangeIP);
        }

        public async Task DeletarPoolIp(ISession session, string NomePool)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine(" DELETE FROM radippool ");
                sql.AppendLine(" WHERE pool_name = @POOLNAME ");

                parametros.Add("@POOLNAME", NomePool);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task DesvincularPoolIpDosUsuarios(ISession session, string NomePool)
        {
            // WHERE attribute = 'Pool-Name' and value = @POOL_NAME and Id_Cliente = @IDCLIENTE;"
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine(" DELETE FROM radcheck ");
                sql.AppendLine(" WHERE attribute = 'Pool-Name' and value = @POOL_NAME "/*and Id_Cliente = @IDCLIENTE*/ );

                parametros.Add("@POOL_NAME", NomePool);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task AtualizarPoolIpDosUsuarios(ISession session, string Antigo_PoolName, string Novo_NomePool)
        {
            // WHERE attribute = 'Pool-Name' and value = @POOL_NAME and Id_Cliente = @IDCLIENTE;"
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("UPDATE radcheck set value = @POOL_NAME_NOVO WHERE (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool') and value = @POOL_NAME_ANTIGO; " /*and Id_Cliente = @IDCLIENTE;"*/);
                sql.AppendLine("UPDATE radreply set value = @POOL_NAME_NOVO WHERE (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool') and value = @POOL_NAME_ANTIGO; ");

                parametros.Add("@POOL_NAME_ANTIGO", Antigo_PoolName);
                parametros.Add("@POOL_NAME_NOVO", Novo_NomePool);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }
        
        public async Task AtualizarPoolIpUsuario(ISession session, LoginRadiusModel login)
        {
            switch (login.Concentradora.Tipo)
            {
                case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                    await new Mikrotik().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Huawei:
                    await new Huawei().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.enumerados.Marca_Modelo_Concentradora.Juniper:
                    await new Juniper().ConfigurarPoolCliente(session, login);
                    break;
            }

            await DesconectarLogin(login, login.usaRadiusSemContainer, false);
        }

        public async Task AtualizarPoolIpUsuario(ISession session, long Id_Cliente, string Novo_NomePool, string userName)
        {
            // WHERE attribute = 'Pool-Name' and value = @POOL_NAME and Id_Cliente = @IDCLIENTE;"
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("UPDATE radcheck set value = @POOL_NAME_NOVO WHERE (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool') and Id_Cliente = @IDCLIENTE AND username = @USERNAME; ");
                sql.AppendLine("UPDATE radreply set value = @POOL_NAME_NOVO WHERE (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool') and id_Cliente = @IDCLIENTE AND username = @USERNAME; ");

                parametros.Add("@IDCLIENTE", Id_Cliente);
                parametros.Add("@POOL_NAME_NOVO", Novo_NomePool);
                parametros.Add("@USERNAME", userName);

                await Sinapse.DAO.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task<string> GetPoolBloqueioDisponivel(ISession session, List<String> Pools_Bloqueio)
        {
            List<String> listaRetorno = new List<String>();

            //SELECT pool_name FROM radippool WHERE(username IS NULL OR username LIKE '')
            //AND pool_name IN('Pool1', 'Pool2')
            //GROUP BY pool_name
            string listaPools = string.Join(", ", Pools_Bloqueio.Where(p => !string.IsNullOrEmpty(p)).Select(p => string.Format("'{0}'", p)));
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("SELECT pool_name FROM radippool WHERE(username IS NULL OR username LIKE '') ");
                sql.AppendLine(String.Format("AND pool_name IN({0})", Pools_Bloqueio));
                sql.AppendLine("GROUP BY pool_name");

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        listaRetorno.Add(Convert.ToString(reader["pool_name"]));
                    }
                }
            }

            return listaRetorno.First();
        }

        public async Task<string> GetPoolReducaoVelocidadeDisponivel(ISession session, List<String> Pools_Reducao)
        {
            List<String> listaRetorno = new List<String>();

            string listaPools = string.Join(", ", Pools_Reducao.Where(p => !string.IsNullOrEmpty(p)).Select(p => string.Format("'{0}'", p)));
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("SELECT pool_name FROM radippool WHERE(username IS NULL OR username LIKE '') ");
                sql.AppendLine(String.Format("AND pool_name IN({0})", Pools_Reducao));
                sql.AppendLine("GROUP BY pool_name");

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        listaRetorno.Add(Convert.ToString(reader["pool_name"]));
                    }
                }
            }

            return listaRetorno.First();
        }

        public bool TesteLogin(string ipConcentradora, string usuario, string senha, bool UsaRadiusSemContainer)
        {
            var retorno = false;
            var logSh = new List<String>();
            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {


                    rc.InteractiveExecute<ShellStream>((sh) =>
                    {
                        string comando = UsaRadiusSemContainer == true ? $"radtest {usuario} {senha} {this.radius.Ip} 0 {CONSTANTES.secretKeyRadius} 1 {ipConcentradora}"
                        : $"docker exec radius sh -c \"radtest {usuario} {senha} 127.0.0.1 0 {CONSTANTES.secretKeyRadius} 1 {ipConcentradora}\"";
                       
                        sh.WriteLine(comando);
                        string currLine;
                        Thread.Sleep(2000);

                        var precisaPermissao = false;
                        while ((currLine = sh.ReadLine(TimeSpan.FromSeconds(10))) != null)
                        {
                            logSh.Add(currLine);
                            Console.WriteLine(currLine);
                            if (currLine.Contains("Received Access-Accept"))
                            {
                                retorno = true;
                                break;
                            }
                            else if (currLine.Contains("Received Access-Reject"))
                            {
                                break;
                            }
                            else if (currLine.Contains("permission denied"))
                            {
                                precisaPermissao = true;
                                break;
                            }
                        }

                        if (precisaPermissao)
                        {
                            sh.WriteLine($"sudo {comando}");
                            Thread.Sleep(1500);

                            sh.WriteLine(this.radius.Senha);
                            while ((currLine = sh.ReadLine(TimeSpan.FromSeconds(10))) != null)
                            {
                                Console.WriteLine(currLine);
                                logSh.Add(currLine);
                                if (currLine.Contains("Received Access-Accept"))
                                {
                                    retorno = true;
                                    break;
                                }
                                else if (currLine.Contains("Received Access-Reject") || currLine.Contains("No reply from server"))
                                {
                                    break;
                                }
                            }
                        }

                    });
                }
            }

            return retorno;
        }

        #endregion

        public void ReiniciarServicoRadius()
        {
            using (SshClient client = new SshClient(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha))
            {
                client.Connect();
                var command = client.CreateCommand("");
                command.Execute("docker restart radius");
                client.Disconnect();

            }
        }

        public async Task<GetHistoricoConexaoRadius_Response> GetHistoricoConexao(ISession session, GetHistoricoConexaoRadius_DataTableRequest request)
        {
            GetHistoricoConexaoRadius_Response retorno = new GetHistoricoConexaoRadius_Response();
            Int32 recordsTotal = 0;
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            StringBuilder sqlFrom = new StringBuilder("FROM radacct A")
                .AppendLine(" WHERE A.username = @USERNAME ");

            //Filtrando
            if (!String.IsNullOrEmpty(request.search.value)) 
            {
                sqlFrom.AppendLine(" AND (A.framedipaddress LIKE @SEARCH OR A.framedipv6address LIKE @SEARCH) ");

                parametros.Add("@SEARCH", String.Format("%{0}%", request.search.value.ToUpper()));
            }

            if(!String.IsNullOrEmpty(request.Data_Inicial) && !String.IsNullOrEmpty(request.Data_Fim))
            {
                sqlFrom.AppendLine(" AND (COALESCE(A.acctstarttime, A.connectinfo_start) > @DATA_INICIO AND COALESCE(A.acctstarttime, A.connectinfo_start) < @DATA_FIM ");
                sqlFrom.AppendLine(" OR A.acctupdatetime > @DATA_INICIO AND  A.acctupdatetime < @DATA_FIM) ");

                parametros.Add("@DATA_INICIO", String.Format("{0}", DateTime.Parse(request.Data_Inicial).ToString("yyyy/MM/dd HH:mm:ss")));
                parametros.Add("@DATA_FIM", String.Format("{0}", DateTime.Parse(request.Data_Fim).AddHours(23).AddMinutes(59).AddSeconds(59).ToString("yyyy/MM/dd HH:mm:ss")));
            }

            parametros.Add("@USERNAME", request.Login_Radius);

            //Count
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder("SELECT COUNT(A.radacctid) CO ");
                sql.AppendLine(sqlFrom.ToString());

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        recordsTotal = Convert.ToInt32(reader["CO"]);
                    }
                }
            }

            List<RelatorioHistoricoConexaoModel> listaRetorno = new List<RelatorioHistoricoConexaoModel>();

            //Select
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("SELECT COALESCE(A.framedipaddress, '-') AS ENDERECO_IPV4, COALESCE(A.framedipv6address, '-') AS ENDERECO_IPV6, A.callingstationid AS MAC, COALESCE(A.acctstarttime, A.connectinfo_start) AS HORA_LOGIN, ");
                sql.AppendLine(" COALESCE(A.acctstoptime, A.connectinfo_stop) AS HORA_LOGOUT, A.acctinputoctets AS DOWNLOAD, A.acctoutputoctets AS UPLOAD, A.acctterminatecause AS MOTIVO_DESCONEXAO ");
                //sql.AppendLine(" FROM radacct A ");
                //sql.AppendLine(" WHERE A.username = @USERNAME");
                sql.AppendLine(sqlFrom.ToString());

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReaderFirst(com, sql, parametros, session.Connection, null, request.length, request.start))
                {
                    while (await reader.ReadAsync())
                    {
                        listaRetorno.Add(new RelatorioHistoricoConexaoModel
                        {
                            Endereco_Ipv4 = Convert.ToString(reader["ENDERECO_IPV4"]),
                            Endereco_Ipv6 = Convert.ToString(reader["ENDERECO_IPV6"]),
                            Mac = Convert.ToString(reader["MAC"]),
                            Hora_Login = FormatPadraoData(Convert.ToString(reader["HORA_LOGIN"])),
                            Hora_Logout = FormatPadraoData(Convert.ToString(reader["HORA_LOGOUT"])),
                            Tempo_Conectado = FormatTempoConectado(Convert.ToString(reader["HORA_LOGIN"]), Convert.ToString(reader["HORA_LOGOUT"])),
                            Upload = reader["UPLOAD"] != DBNull.Value ? OctetsToKbMbGb(Convert.ToInt64(reader["UPLOAD"])) : "-",
                            Download = reader["DOWNLOAD"] != DBNull.Value ? OctetsToKbMbGb(Convert.ToInt64(reader["DOWNLOAD"])) : "-",
                            Motivo_Desconexao = reader["MOTIVO_DESCONEXAO"] != DBNull.Value ? Convert.ToString(reader["MOTIVO_DESCONEXAO"]) : "-"
                        });
                    }
                }
            }

            retorno.lista = listaRetorno;
            retorno.recordsTotal = recordsTotal;

            return retorno;
        }

        public async Task<List<ConsumoPeriodoModel>> GetConsumoPeriodo(ISession session, String Login_Radius, HistoricoConsumoPeriodoEnum Periodo_Consumo, DateTime? Data_Inicio, DateTime? Data_Fim)
        {
            List<ConsumoPeriodoModel> retorno = new List<ConsumoPeriodoModel>();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            //BASEADO NO EXEMPLO
            /*
             --      SELECT
                --          DATE_FORMAT(period_start, '%Y-%M') AS month,
                --          SUM(acctinputoctets)/1000/1000/1000 AS GB_in,
                --          SUM(acctoutputoctets)/1000/1000/1000 AS GB_out
                --      FROM
                --          data_usage_by_period
                --      WHERE
                --          username='bob' AND
                --          period_end IS NOT NULL
                --      GROUP BY
                --          YEAR(period_start), MONTH(period_start);
                --
                --      +----------------+----------------+-----------------+
                --      | month          | GB_in          | GB_out          |
                --      +----------------+----------------+-----------------+
                --      | 2019-July      | 5.782279230000 | 50.545664820000 |
                --      | 2019-August    | 4.230543340000 | 48.523096420000 |
                --      | 2019-September | 4.847360590000 | 48.631835480000 |
                --      | 2019-October   | 6.456763250000 | 51.686231930000 |
                --      | 2019-November  | 6.362537730000 | 52.385710570000 |
                --      | 2019-December  | 4.301524440000 | 50.762240270000 |
                --      | 2020-January   | 5.436280540000 | 49.067775280000 |
                --      +----------------+----------------+-----------------+
             */

            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();

                string sqlDateFormat = String.Empty;
                StringBuilder sqlGroupBy = new StringBuilder();

                //Caso não seja consulta por periodo dinâmico, Data_Fim passa a ser a data do momento da consulta
                if (!Data_Fim.HasValue && Periodo_Consumo == HistoricoConsumoPeriodoEnum.Periodo)
                    Data_Fim = DateTime.Now;

                switch (Periodo_Consumo)
                {
                    case HistoricoConsumoPeriodoEnum.Dia:
                        sqlDateFormat = "A.period_start";
                        //sqlGroupBy.AppendLine(" GROUP BY HOUR(A.period_start), DAY(A.period_start);");
                        parametros.Add("@DATA_INICIO", String.Format("{0}", Data_Fim.Value.AddHours(-23).AddMinutes(-59).AddSeconds(-59).ToString("yyyy/MM/dd HH:mm:ss")));
                        parametros.Add("@DATA_FIM", String.Format("{0}", Data_Fim.Value.ToString("yyyy/MM/dd HH:mm:ss")));
                        break;
                    case HistoricoConsumoPeriodoEnum.Semana:
                        sqlDateFormat = "A.period_start";
                        //sqlDateFormat = "DATE_FORMAT(A.period_start, '%W')";
                        // sqlGroupBy.AppendLine(" GROUP BY DAY(A.period_start);");
                        parametros.Add("@DATA_INICIO", String.Format("{0}", Data_Fim.Value.AddDays(-6).AddHours(-23).AddMinutes(-59).AddSeconds(-59).ToString("yyyy/MM/dd HH:mm")));
                        parametros.Add("@DATA_FIM", String.Format("{0}", Data_Fim.Value.ToString("yyyy/MM/dd HH:mm:ss")));
                        break;
                    case HistoricoConsumoPeriodoEnum.Mes:
                        sqlDateFormat = "A.period_start";
                        //sqlDateFormat = "DATE_FORMAT(A.period_start, '%d/%m')";
                        //sqlGroupBy.AppendLine(" GROUP BY DAY(A.period_start);");
                        parametros.Add("@DATA_INICIO", String.Format("{0}", Data_Fim.Value.AddDays(-29).AddHours(-23).AddMinutes(-59).AddSeconds(-59).ToString("yyyy/MM/dd HH:mm")));
                        parametros.Add("@DATA_FIM", String.Format("{0}", Data_Fim.Value.ToString("yyyy/MM/dd HH:mm:ss")));
                        break;
                    case HistoricoConsumoPeriodoEnum.Periodo:
                        sqlDateFormat = "A.period_start";
                        //sqlDateFormat = "DATE_FORMAT(A.period_start, '%m/%Y')";
                        //sqlGroupBy.AppendLine(" GROUP BY DAY(A.period_start);");
                        parametros.Add("@DATA_INICIO", String.Format("{0}", Data_Inicio.Value.ToString("yyyy/MM/dd HH:mm:ss")));
                        parametros.Add("@DATA_FIM", String.Format("{0}", Data_Fim.Value.AddHours(23).AddMinutes(59).AddSeconds(59).ToString("yyyy/MM/dd HH:mm:ss")));
                        break;
                    default:
                        break;
                }


                sql.AppendLine("SET SESSION sql_mode=(SELECT REPLACE(@@sql_mode,'ONLY_FULL_GROUP_BY','')); ");
                sql.AppendLine("SET lc_time_names = 'pt_PT'; ");
                //if(Periodo_Consumo == HistoricoConsumoPeriodoEnum.Dia)
                sql.AppendLine($"SELECT {sqlDateFormat} AS PERIODO, A.acctinputoctets AS DOWNLOAD, A.acctoutputoctets AS UPLOAD ");
                //else
                //sql.AppendLine($"SELECT {sqlDateFormat} AS PERIODO, SUM(A.acctinputoctets) AS DOWNLOAD, SUM(A.acctoutputoctets) AS UPLOAD ");
                sql.AppendLine(" FROM data_usage_by_period A ");
                sql.AppendLine(" WHERE A.username=@USERNAME AND A.period_end IS NOT NULL AND (A.period_start > @DATA_INICIO AND A.period_start < @DATA_FIM) ");
                sql.AppendLine(sqlGroupBy.ToString());

                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    while (await reader.ReadAsync())
                    {
                        retorno.Add(new ConsumoPeriodoModel
                        {
                            DataHoraConsumoFormatada = Convert.ToDateTime(Convert.ToString(reader["PERIODO"])).ToString("yyyy-MM-dd HH:mm:ss"),
                            DataHoraConsumo = Convert.ToDateTime(Convert.ToString(reader["PERIODO"])),
                            UploadString = reader["UPLOAD"] != DBNull.Value ? OctetsToKbMbGb(Convert.ToDouble(reader["UPLOAD"])) : "-",
                            Upload = reader["UPLOAD"] != DBNull.Value ? Convert.ToDouble(reader["UPLOAD"]) : 0.0,
                            DownloadString = reader["DOWNLOAD"] != DBNull.Value ? OctetsToKbMbGb(Convert.ToInt64(reader["DOWNLOAD"])) : "-",
                            Download = reader["DOWNLOAD"] != DBNull.Value ? Convert.ToDouble(reader["DOWNLOAD"]) : 0.0
                        });
                    }
                }
            }

            return retorno;
        }

        public async Task<ConsumoPeriodoModel> GetConexaoTempoRealOld(ISession session, String Login_Radius, Medida_Velocidade_ConexaoEnum Unidade_Medida_Velocidade)
        {
            ConsumoPeriodoModel retorno = new ConsumoPeriodoModel();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            //CALL fr_new_data_usage_period_by_user(usuario);
            //SELECT A.period_start AS PERIODO, A.acctinputoctets AS DOWNLOAD, A.acctoutputoctets AS UPLOAD 
            //FROM data_usage_by_period A 
            //WHERE A.username = 'yago.dias' AND A.period_end IS NOT NULL            
            //ORDER BY A.period_end DESC LIMIT 1

            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("SET lc_time_names = 'pt_PT'; ");

#if DEBUG
                //sql.AppendLine(UpdateRegistroRadacct()); //TODO: REMOVER ESSA ROTINA POIS ELA GERA DADOS DE TESTE NO RADIUS
#endif

                sql.AppendLine(" CALL fr_new_data_usage_period_by_user(@USERNAME); ");
                sql.AppendLine($" SELECT A.period_start AS PERIODO, A.acctinputoctets AS DOWNLOAD, A.acctoutputoctets AS UPLOAD ");
                sql.AppendLine(" FROM data_usage_by_period A ");
                sql.AppendLine(" WHERE A.username=@USERNAME AND A.period_end IS NOT NULL ");
                sql.AppendLine(" ORDER BY A.period_end DESC LIMIT 1; ");

                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        retorno = new ConsumoPeriodoModel
                        {
                            DataHoraConsumoFormatada = Convert.ToDateTime(Convert.ToString(reader["PERIODO"])).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                            DataHoraConsumo = Convert.ToDateTime(Convert.ToString(reader["PERIODO"])).ToLocalTime(),
                            UploadString = reader["UPLOAD"] != DBNull.Value ? OctetsToKbMbGb(Convert.ToDouble(reader["UPLOAD"]), Unidade_Medida_Velocidade) : "-",
                            Upload = reader["UPLOAD"] != DBNull.Value ? Convert.ToDouble(reader["UPLOAD"]) : 0.0,
                            DownloadString = reader["DOWNLOAD"] != DBNull.Value ? OctetsToKbMbGb(Convert.ToInt64(reader["DOWNLOAD"]), Unidade_Medida_Velocidade) : "-",
                            Download = reader["DOWNLOAD"] != DBNull.Value ? Convert.ToDouble(reader["DOWNLOAD"]) : 0.0
                        };
                    }
                }
            }

            return retorno;
        }

        public async Task<ConsumoPeriodoModel> GetConexaoTempoReal(ISession session, String Login_Radius, Medida_Velocidade_ConexaoEnum Unidade_Medida_Velocidade, double LastTotalDownloadOctects, double LastTotalUploadOctects)
        {
            ConsumoPeriodoModel retorno = new ConsumoPeriodoModel();
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            using (var com = session.Connection.CreateCommand())
            {
                //SELECT acctstarttime, acctupdatetime, acctstoptime, acctinputoctets, acctoutputoctets  FROM radacct WHERE username = 'yago.dias' ORDER BY acctstarttime DESC LIMIT 1 
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("SET lc_time_names = 'pt_PT'; ");
                
                sql.AppendLine($" SELECT A.acctupdatetime AS PERIODO, A.acctinputoctets AS DOWNLOAD, A.acctoutputoctets AS UPLOAD ");
                sql.AppendLine(" FROM radacct A ");
                sql.AppendLine(" WHERE A.username=@USERNAME AND A.acctstoptime IS NULL ");
                sql.AppendLine(" ORDER BY A.acctstarttime DESC LIMIT 1; ");

                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        double octets_upload = reader["UPLOAD"] != DBNull.Value ? Convert.ToDouble(reader["UPLOAD"]) : 0.0;
                        double octets_download = reader["DOWNLOAD"] != DBNull.Value ? Convert.ToDouble(reader["DOWNLOAD"]) : 0.0;

                        //A tabela do Radius guarda apenas o consumo total de download e upload do login, que é atualizado periodicamente.
                        //É necessário subtrair para obter o valor consumido entre o intervalo da última consulta e a atual
                        double valor_upload = octets_upload != LastTotalUploadOctects ? octets_upload - LastTotalUploadOctects : LastTotalUploadOctects;
                        double valor_download = octets_download != LastTotalDownloadOctects ? octets_download - LastTotalDownloadOctects : LastTotalDownloadOctects;

                        retorno = new ConsumoPeriodoModel
                        {
                            DataHoraConsumoFormatada = Convert.ToDateTime(Convert.ToString(reader["PERIODO"]))/*.ToLocalTime()*/.ToString("yyyy-MM-dd HH:mm:ss"),
                            DataHoraConsumo = Convert.ToDateTime(Convert.ToString(reader["PERIODO"]))/*.ToLocalTime()*/,
                            UploadString = OctetsToKbMbGb(valor_upload, Unidade_Medida_Velocidade),
                            Upload = valor_upload,
                            DownloadString = OctetsToKbMbGb(valor_download, Unidade_Medida_Velocidade),
                            Download = valor_download,
                            LastTotalDownloadOctects = octets_download,
                            LastTotalUploadOctects = octets_upload
                        };
                    }
                }
            }

            return retorno;
        }

        public async Task<List<TempoOnlineOfflineModel>> GetTempoOnline(ISession session, String Login_Radius, DateTime Data_Inicio, DateTime Data_Fim)
        {
            List<TempoOnlineOfflineModel> retorno = new List<TempoOnlineOfflineModel>();
            Dictionary<string, object> parametros = new Dictionary<string, object>();


            List<dynamic> lista = new List<dynamic>();
            StringBuilder sql = new StringBuilder();
            /*SELECT connectinfo_start AS DATAHORA_INICIO_CONEXAO, connectinfo_stop AS DATAHORA_FIM_CONEXAO
            FROM radacct 
            WHERE username = 'yago.dias' 
            AND (
            (connectinfo_start > '2022/09/01 00:00:00' AND connectinfo_start < '2022/09/08 11:14:00')
            OR
            (connectinfo_stop > '2022/09/01 00:00:00' AND connectinfo_stop < '2022/09/08 11:14:00')
            )*/
            DateTime data_atual = DateTime.Now;
            using (var com = session.Connection.CreateCommand())
            {
                /*sql.AppendLine("SELECT connectinfo_start AS DATAHORA_INICIO_CONEXAO, connectinfo_stop AS DATAHORA_FIM_CONEXAO ");
                sql.AppendLine("FROM radacct ");
                sql.AppendLine("WHERE username = @USERNAME ");
                sql.AppendLine("AND ((connectinfo_start > @DATA_INICIO AND connectinfo_start < @DATA_FIM) ");
                sql.AppendLine("OR (connectinfo_stop > @DATA_INICIO AND connectinfo_stop < @DATA_FIM)) ");*/

                sql.AppendLine("SELECT COALESCE(acctstarttime, connectinfo_start) AS DATAHORA_INICIO_CONEXAO, COALESCE(acctstoptime  ,connectinfo_stop) AS DATAHORA_FIM_CONEXAO  ");
                sql.AppendLine("FROM radacct ");
                sql.AppendLine("WHERE username = @USERNAME ");
                sql.AppendLine("AND ((COALESCE(acctstarttime, connectinfo_start) > CAST(@DATA_INICIO AS DATETIME) AND COALESCE(acctstarttime, connectinfo_start) < CAST(@DATA_FIM AS DATETIME))  ");
                sql.AppendLine("OR (COALESCE(acctstoptime  ,connectinfo_stop) > CAST(@DATA_INICIO AS DATETIME) AND COALESCE(acctstoptime  ,connectinfo_stop) < CAST(@DATA_FIM AS DATETIME))) ");

                parametros.Add("@DATA_INICIO", String.Format("{0}", Data_Inicio.ToString("yyyy/MM/dd HH:mm")));
                parametros.Add("@DATA_FIM", String.Format("{0}", Data_Fim.ToString("yyyy/MM/dd HH:mm:ss")));
                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new
                        {
                            DataHora_Inicio_Conexao = Convert.ToDateTime(Convert.ToString(reader["DATAHORA_INICIO_CONEXAO"])).ToLocalTime(),
                            DataHora_Termino_Conexao = (reader["DATAHORA_FIM_CONEXAO"] != DBNull.Value && !String.IsNullOrEmpty(reader["DATAHORA_FIM_CONEXAO"].ToString())) ? Convert.ToDateTime(Convert.ToString(reader["DATAHORA_FIM_CONEXAO"])).ToLocalTime() : (DateTime?)null
                        });
                    }
                }
            }

            for (var i = (Data_Fim.Subtract(Data_Inicio).Days * -1); i < 0; i++)
            {
                retorno.Add(new TempoOnlineOfflineModel
                {
                    Data_Conexao = Data_Fim.AddDays(i).Date,
                    Data_Conexao_String = Data_Fim.AddDays(i).ToString("yyyy-MM-dd"),
                });
            }
            retorno.Add(new TempoOnlineOfflineModel
            {
                Data_Conexao = Data_Fim.Date,
                Data_Conexao_String = Data_Fim.ToString("yyyy-MM-dd"),
            });

            if (lista.Count > 0)
            {
                foreach (var item in lista)
                {

                    var dataHora_Inicio = ((DateTime)item.DataHora_Inicio_Conexao);
                    var dataHora_Fim = ((DateTime?)item.DataHora_Termino_Conexao);

                    var conexao_Aberta = false;

                    var model = retorno.FirstOrDefault(p => p.Data_Conexao.Value == dataHora_Inicio.Date || p.Data_Conexao.Value == dataHora_Fim?.Date);
                    if (model != null)
                    {
                        var horasOnline = model.Horas_Online_Offline;
                        if(!dataHora_Fim.HasValue)
                        {
                            conexao_Aberta = true;
                            if(dataHora_Inicio.Date == DateTime.Now.Date)
                            {
                                var subtracao = DateTime.Now.Subtract(dataHora_Inicio).Hours;
                                horasOnline += subtracao <= 24 ? subtracao : 24;
                            }
                            else
                            {
                                if (dataHora_Inicio.Date < DateTime.Now.Date)
                                    horasOnline = 24;
                                else
                                    horasOnline = 0;
                            }
                            
                        }
                        else if (dataHora_Inicio.Date == dataHora_Fim.Value.Date) //Login e Logoff no mesmo dia
                        {
                            horasOnline += dataHora_Fim.Value.Subtract(dataHora_Inicio).Hours;
                        }
                        else if (dataHora_Inicio.Date == model.Data_Conexao.Value.Date) //Login no dia mas Logoff não
                        {
                            horasOnline += model.Data_Conexao.Value.AddHours(23).AddMinutes(59).AddSeconds(59).Subtract(dataHora_Inicio).Hours;
                        }
                        else //Logoff no dia mas Login não
                        {
                            horasOnline += model.Data_Conexao.Value.AddHours(23).AddMinutes(59).AddSeconds(59).Subtract(dataHora_Fim.Value).Hours;
                        }

                        if (horasOnline > 24) horasOnline = 24;
                        retorno.First(p => p.Data_Conexao == model.Data_Conexao).Horas_Online_Offline = horasOnline;
                    }
                    else continue;
                }
            }

            return retorno;
        }

        public async Task<List<TempoOnlineOfflineModel>> GetTempoOffline(ISession session, String Login_Radius, DateTime Data_Inicio, DateTime Data_Fim)
        {
            List<TempoOnlineOfflineModel> retorno = new List<TempoOnlineOfflineModel>();
            Dictionary<string, object> parametros = new Dictionary<string, object>();


            List<dynamic> lista = new List<dynamic>();
            StringBuilder sql = new StringBuilder();
            /*SELECT connectinfo_start AS DATAHORA_INICIO_CONEXAO, connectinfo_stop AS DATAHORA_FIM_CONEXAO
            FROM radacct 
            WHERE username = 'yago.dias' 
            AND (
            (connectinfo_start > '2022/09/01 00:00:00' AND connectinfo_start < '2022/09/08 11:14:00')
            OR
            (connectinfo_stop > '2022/09/01 00:00:00' AND connectinfo_stop < '2022/09/08 11:14:00')
            )*/
            using (var com = session.Connection.CreateCommand())
            {
                /*sql.AppendLine("SELECT connectinfo_start AS DATAHORA_INICIO_CONEXAO, connectinfo_stop AS DATAHORA_FIM_CONEXAO ");
                sql.AppendLine("FROM radacct ");
                sql.AppendLine("WHERE username = @USERNAME ");
                sql.AppendLine("AND ((connectinfo_start > @DATA_INICIO AND connectinfo_start < @DATA_FIM) ");
                sql.AppendLine("OR (connectinfo_stop > @DATA_INICIO AND connectinfo_stop < @DATA_FIM)) ");*/

                sql.AppendLine("SELECT COALESCE(acctstarttime, connectinfo_start) AS DATAHORA_INICIO_CONEXAO, COALESCE(acctstoptime  ,connectinfo_stop) AS DATAHORA_FIM_CONEXAO ");
                sql.AppendLine("FROM radacct ");
                sql.AppendLine("WHERE username = @USERNAME ");
                sql.AppendLine("AND ((COALESCE(acctstarttime, connectinfo_start) > CAST(@DATA_INICIO AS DATETIME) AND COALESCE(acctstarttime, connectinfo_start) < CAST(@DATA_FIM AS DATETIME))  ");
                sql.AppendLine("OR (COALESCE(acctstoptime  ,connectinfo_stop) > CAST(@DATA_INICIO AS DATETIME) AND COALESCE(acctstoptime  ,connectinfo_stop) < CAST(@DATA_FIM AS DATETIME))) ");

                parametros.Add("@DATA_INICIO", String.Format("{0}", Data_Inicio.ToString("yyyy/MM/dd HH:mm")));
                parametros.Add("@DATA_FIM", String.Format("{0}", Data_Fim.ToString("yyyy/MM/dd HH:mm:ss")));
                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new
                        {
                            DataHora_Inicio_Conexao = Convert.ToDateTime(Convert.ToString(reader["DATAHORA_INICIO_CONEXAO"])).ToLocalTime(),
                            DataHora_Termino_Conexao = (reader["DATAHORA_FIM_CONEXAO"] != DBNull.Value && !String.IsNullOrEmpty(reader["DATAHORA_FIM_CONEXAO"].ToString())) ? Convert.ToDateTime(Convert.ToString(reader["DATAHORA_FIM_CONEXAO"])).ToLocalTime() : (DateTime?)null
                        });
                    }
                }
            }
            
            for (var i = (Data_Fim.Subtract(Data_Inicio).Days * -1); i < 0; i++)
            {
                retorno.Add(new TempoOnlineOfflineModel
                {
                    Data_Conexao = Data_Fim.AddDays(i).Date,
                    Data_Conexao_String = Data_Fim.AddDays(i).ToString("yyyy-MM-dd"),
                    Horas_Online_Offline = 24
                });
            }
            retorno.Add(new TempoOnlineOfflineModel
            {
                Data_Conexao = Data_Fim.Date,
                Data_Conexao_String = Data_Fim.ToString("yyyy-MM-dd"),
                Horas_Online_Offline = 24
            });

            if (lista.Count > 0)
            {
                foreach (var item in lista)
                {

                    var dataHora_Inicio = ((DateTime)item.DataHora_Inicio_Conexao);
                    var dataHora_Fim = ((DateTime?)item.DataHora_Termino_Conexao);

                    var conexao_Aberta = false;

                    var model = retorno.FirstOrDefault(p => p.Data_Conexao.Value == dataHora_Inicio.Date || p.Data_Conexao.Value == dataHora_Fim?.Date);
                    if (model != null)
                    {
                        var horasOffline = model.Horas_Online_Offline;

                        if(!dataHora_Fim.HasValue)
                        {
                            conexao_Aberta = true;
                            if(dataHora_Inicio.Date == DateTime.Now.Date)
                            {
                                var subtracao = DateTime.Now.Subtract(dataHora_Inicio).Hours;
                                horasOffline -= subtracao <= 24 ? subtracao : 24;
                            }
                            else
                            {
                                horasOffline = 0;
                            }                            
                        }
                        else if (dataHora_Inicio.Date == dataHora_Fim.Value.Date) //Login e Logoff no mesmo dia
                        {
                            horasOffline -= dataHora_Fim.Value.Subtract(dataHora_Inicio).Hours;
                        }
                        else if (dataHora_Inicio.Date == model.Data_Conexao.Value.Date) //Login no dia mas Logoff não
                        {
                            horasOffline -= model.Data_Conexao.Value.AddHours(23).AddMinutes(59).AddSeconds(59).Subtract(dataHora_Inicio).Hours;
                        }
                        else //Logoff no dia mas Login não
                        {
                            horasOffline -= model.Data_Conexao.Value.AddHours(23).AddMinutes(59).AddSeconds(59).Subtract(dataHora_Fim.Value).Hours;
                        }

                        if (horasOffline < 0) horasOffline = 0;
                        retorno.First(p => p.Data_Conexao == model.Data_Conexao).Horas_Online_Offline = horasOffline;
                    }
                    else continue;
                }
            }

            return retorno;
        }        

        public async Task<bool> DesconectarLogin(LoginRadiusModel login, bool UsaRadiusSemContainer, bool waitResponse = true)
        {
            var retorno = false;
            var getacctsessionid = await GetUltimoAccountingSessionId(login.Login);

            var acctsessionid = getacctsessionid.acctsessionid;

            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                await rc.Connect();
               
                if (rc.IsConnected())
                {
                    if (String.IsNullOrEmpty(login.Concentradora.PortaIncoming))
                        login.Concentradora.PortaIncoming = "3799";

                    string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + login.Login + "\nAcct-Session-Id = " + acctsessionid + "\" | radclient " + login.Concentradora.Ip + ":" + login.Concentradora.PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}"
                        : "docker exec -i radius bash -c 'echo \"User-Name = " + login.Login + "\nAcct-Session-Id = " + acctsessionid + "\" | radclient " + login.Concentradora.Ip + ":" + login.Concentradora.PortaIncoming + $" disconnect {CONSTANTES.secretKeyRadius}'"; 

                  

                    //rc.ExecuteCommand(comando);

                    /* rc.InteractiveExecute<ShellStream>((shellStream) =>
                     {                        
                         shellStream.WriteLine(comando);

                         if(waitResponse)
                         {
                             string readLine;
                             while ((readLine = shellStream.ReadLine(TimeSpan.FromSeconds(10))) != null)
                             {
                                 if (readLine.Contains("Disconnect-ACK"))
                                 {
                                     retorno = true;
                                     break;
                                 }
                             }
                         }                        
                     }); */
                    await Task.Delay(1500);
                    await rc.ExecuteCommandAsync(comando);
                    await Task.Delay(2500);
                    var resposta = (await rc.ReadAsync()).Split('\r', '\n').ToList();
                    if (resposta.Exists(p => p.ToUpperInvariant().Contains("Disconnect-ACK".ToUpperInvariant())))
                    {
                        retorno = true;
                    }

                    if (resposta.Exists(p => p.ToUpperInvariant().Contains("got Disconnect-NAK".ToUpperInvariant())))
                    {
                        retorno = false;
                    }

                }
            }
            return retorno;
        }

        public async Task<List<DadosConexoesAtivasModel>> GetDadosConexoesAtivas(ISession session, DateTime? Data_Hora_Corte)
        {
            List<DadosConexoesAtivasModel> retorno = new List<DadosConexoesAtivasModel>();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            StringBuilder sql = new StringBuilder();
            using (var com = session.Connection.CreateCommand())
            {
                /*
                SET lc_time_names = 'pt_PT';
                SELECT A.username, A.acctupdatetime, B.idCliente_PlanoProduto, A.framedipaddress, 
                A.framedipv6address, A.framedipv6prefix, A.callingstationid 
                FROM radacct A
                INNER JOIN radusergroup B
                ON(A.username = B.username)
                INNER JOIN radgroupcheck C
                ON(B.groupname = C.groupname)
                INNER JOIN radacct D
                ON(A.radacctid = D.radacctid AND A.radacctid = (SELECT radacctid FROM radacct WHERE username = A.username ORDER BY acctupdatetime DESC LIMIT 1 ))
                WHERE B.groupname <> 'Inativo' AND C.value = 'S' 
                AND A.acctstarttime IS NOT NULL 
                AND A.acctupdatetime IS NOT NULL 
                ORDER BY A.acctupdatetime DESC;
                 */

                sql.AppendLine("SET lc_time_names = 'pt_PT'; ");
                sql.AppendLine("SELECT A.username, A.acctupdatetime, B.idCliente_PlanoProduto, A.framedipaddress, ");
                sql.AppendLine("A.framedipv6address, A.framedipv6prefix, A.callingstationid ");
                sql.AppendLine("FROM radacct A ");
                sql.AppendLine("INNER JOIN radusergroup B ");
                sql.AppendLine("ON(A.username = B.username) ");
                sql.AppendLine("INNER JOIN radgroupcheck C ");
                sql.AppendLine("ON(B.groupname = C.groupname) ");
                sql.AppendLine("INNER JOIN radacct D ");
                sql.AppendLine("ON(A.radacctid = D.radacctid AND A.radacctid = (SELECT radacctid FROM radacct WHERE username = A.username ORDER BY acctupdatetime DESC LIMIT 1 )) ");
                sql.AppendLine("WHERE B.groupname <> 'Inativo' AND C.value = 'S' ");
                sql.AppendLine("AND A.acctstarttime IS NOT NULL ");
                sql.AppendLine("AND A.acctupdatetime IS NOT NULL ");

                if(Data_Hora_Corte.HasValue)
                {
                    sql.AppendLine("AND (A.acctstoptime IS NULL OR A.acctstoptime > @DATA_CORTE_CONEXOES_MORTAS)");
                    parametros.Add("@DATA_CORTE_CONEXOES_MORTAS", Data_Hora_Corte.Value.ToString("yyyy/MM/dd HH:mm"));
                }

                sql.AppendLine("ORDER BY A.acctupdatetime DESC;");                                               

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    while (await reader.ReadAsync())
                    {
                        retorno.Add(new DadosConexoesAtivasModel
                        {
                            DataHora_Atualizacao = Convert.ToDateTime(Convert.ToString(reader["acctupdatetime"])).ToLocalTime(),
                            Login_Radius = Convert.ToString(reader["username"]),
                            Id_Cliente_Plano_Produto = reader["idCliente_PlanoProduto"] != DBNull.Value ? Convert.ToInt64(reader["idCliente_PlanoProduto"]) : (long?)null,
                            Endereco_IPv4 = Convert.ToString(reader["framedipaddress"]),
                            Endereco_IPv6 = Convert.ToString(reader["framedipv6address"]),
                            Endereco_IPv6_Prefix = Convert.ToString(reader["framedipv6prefix"]),
                            MAC = Convert.ToString(reader["callingstationid"]),
                        });
                    }
                }
            }

            return retorno;
        }

        private String OctetsToKbMbGb(Double octets_velocidade, Medida_Velocidade_ConexaoEnum? unidade_Medida_Velocidade = null)
        {
            String velocidade_convertida = String.Empty;
            try
            {
                Double velocidade_Kb = (octets_velocidade / 1000);
                Double velocidade_Mb = velocidade_Kb / 1000;
                Double velocidade_Gb = velocidade_Mb / 1000;

                if (unidade_Medida_Velocidade.HasValue)
                {
                    switch (unidade_Medida_Velocidade)
                    {
                        case Medida_Velocidade_ConexaoEnum.Kbs:
                            velocidade_convertida = String.Format("{0} Kb", velocidade_Kb.ToString("0.##"));
                            break;
                        case Medida_Velocidade_ConexaoEnum.Mps:
                            velocidade_convertida = String.Format("{0} Mb", velocidade_Mb.ToString("0.##"));
                            break;
                        case Medida_Velocidade_ConexaoEnum.Gbs:
                            velocidade_convertida = String.Format("{0} Gb", velocidade_Gb.ToString("0.##"));
                            break;
                        default:
                            velocidade_convertida = String.Format("{0} Kb", velocidade_Kb.ToString("0.##"));
                            break;
                    }
                }
                else
                {
                    if (velocidade_Mb > 1024d) //Manter em Gb 
                    {
                        velocidade_convertida = String.Format("{0} Gb", velocidade_Gb.ToString("0.##"));
                    }
                    else if (velocidade_Kb > 1024d) //Manter em Mb
                    {
                        velocidade_convertida = String.Format("{0} Mb", velocidade_Mb.ToString("0.##"));
                    }
                    else //Manter em Kb
                    {
                        velocidade_convertida = String.Format("{0} Kb", velocidade_Kb.ToString("0.##"));
                    }
                }

                return velocidade_convertida;
            }
            catch (Exception ex)
            {
                return "-";
            }
        }

        private String FormatTempoConectado(String Hora_Login, String Hora_Logout)
        {
            String retorno = String.Empty;

            try
            {
                DateTime DataHora_Login = Convert.ToDateTime(Hora_Login);
                DateTime DataHora_Logout = Convert.ToDateTime(Hora_Logout);

                TimeSpan Tempo_Conectado = DataHora_Logout.Subtract(DataHora_Login);

                //retorno = String.Format("{0}d, {1}", Tempo_Conectado.Days, Tempo_Conectado.ToString(@"hh\:mm\:ss"));
                retorno = String.Format("{0}d, {1}", Tempo_Conectado.Days, Tempo_Conectado.ToString(@"hh\:mm\:ss"));

                return retorno;
            }
            catch (Exception ex)
            {
                return "-";
            }
        }

        /// <summary>
        /// Coleta último accounting session id para utilizar no comando de desconexão do PPPoE
        /// pois é necessário informar este id no comando para que a concentradora saiba a sessão de accounting e desconecte o PPPoE.
        /// </summary>
        /// <param name="Login_Radius"></param>
        /// <returns></returns>        
        public async Task<SessaoAccountingModel> GetUltimoAccountingSessionId(String Login_Radius)
        {
            SessaoAccountingModel retorno = new SessaoAccountingModel();
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            using (ISession sessionRadius = HibernateUtil.GetSessionRadius())
            using (var com = sessionRadius.Connection.CreateCommand())
            {
                sql.AppendLine("SELECT acctsessionid");
                sql.AppendLine("FROM radacct ");
                sql.AppendLine("WHERE username = @USERNAME ");
                sql.AppendLine("ORDER BY acctstarttime DESC ");
                sql.AppendLine("LIMIT 1");


                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, sessionRadius.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {

                        retorno.acctsessionid = reader["acctsessionid"].ToString();
                    }
                }
            }

            return retorno;
        }

        /// <summary>
        /// Retorna de enumerados contendo os atributos já cadastrados no Radius para o Login.
        /// </summary>
        /// <param name="Login_Radius"></param>
        /// <returns> </returns>
        public async Task<List<AtributosLoginRadius>> GetAtributosExistentesByLogin(String Login_Radius)
        {
            List<AtributosLoginRadius> retorno = new List<AtributosLoginRadius>();            

            using (ISession sessionRadius = HibernateUtil.GetSessionRadius())
            using (var com = sessionRadius.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("SELECT attribute ");
                sql.AppendLine("FROM radcheck ");
                sql.AppendLine("WHERE username = @USERNAME ");                

                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, sessionRadius.Connection, null))
                {
                    while (await reader.ReadAsync())
                    {
                        switch(reader["attribute"].ToString())
                        {
                            case "Cleartext-Password":
                                retorno.Add(AtributosLoginRadius.Cleartext_Password);
                                break;
                            case "NAS-IP-Address":
                                retorno.Add(AtributosLoginRadius.NAS_IP_Address);
                                break;
                            case "Framed-Protocol":
                                retorno.Add(AtributosLoginRadius.Framed_Protocol);
                                break;
                            case "Pool-Name":
                                retorno.Add(AtributosLoginRadius.Pool_Name);
                                break;
                            case "Calling-Station":
                                retorno.Add(AtributosLoginRadius.Calling_Station);
                                break;
                        }
                        
                    }
                }
            }

            using (ISession sessionRadius = HibernateUtil.GetSessionRadius())
            using (var com = sessionRadius.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("SELECT attribute ");
                sql.AppendLine("FROM radreply ");
                sql.AppendLine("WHERE username = @USERNAME ");

                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, sessionRadius.Connection, null))
                {
                    while (await reader.ReadAsync())
                    {
                        switch (reader["attribute"].ToString())
                        {
                            case "Framed-Route":
                                retorno.Add(AtributosLoginRadius.Framed_Route);
                                break;
                            case "Framed-Pool":
                                retorno.Add(AtributosLoginRadius.Framed_Pool);
                                break;
                            case "Framed-IP-Address":
                                retorno.Add(AtributosLoginRadius.Framed_IP_Address);
                                break;
                            case "Framed-IPv6-Address":
                                retorno.Add(AtributosLoginRadius.Framed_IPv6_Address);
                                break;
                            case "Mikrotik-Wireless-PSK":
                                retorno.Add(AtributosLoginRadius.Mikrotik_Wireless_PSK);
                                break;
                            case "Huawei-Framed-IPv6-Address":
                                retorno.Add(AtributosLoginRadius.Huawei_Framed_IPv6_Address);
                                break;                            
                        }
                    }
                }
            }

            return retorno;
        }

        /// <summary>
        /// Consulta os dados do Login Radius para exibição na Listagem do Painel de Diagnósticos.
        /// </summary>
        /// <param name="Login_Radius"></param>
        /// <returns></returns>
        public async Task<DadosLoginPainelDiagnostico> GetDadosLoginPainelDiagnostico(String Login_Radius)
        {
            DadosLoginPainelDiagnostico retorno = new DadosLoginPainelDiagnostico();
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            using (ISession sessionRadius = HibernateUtil.GetSessionRadius())
            using (var com = sessionRadius.Connection.CreateCommand())
            {
                parametros.Add("@USERNAME", Login_Radius);

                sql.AppendLine("SELECT A.username, A.acctupdatetime, A.acctstoptime, A.acctstarttime ");
                sql.AppendLine("FROM radacct A ");
                sql.AppendLine("WHERE A.username = @USERNAME ");
                sql.AppendLine("ORDER BY A.acctupdatetime DESC ");
                sql.AppendLine("LIMIT 1");
                
                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, sessionRadius.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        retorno.username = Login_Radius;
                        retorno.dataUpdateUltimoAcct = reader["acctupdatetime"].ToString();
                        retorno.dataInicioUltimoAcct = reader["acctstarttime"].ToString();
                        retorno.dataFimUltimoAcct = reader["acctstoptime"].ToString();
                    }
                }

                //Esses dados serão obtidos pelo Aniel Extrator OLT Service
                /*sql.Clear();
                sql.AppendLine("SELECT B.username,B.reply, B.authdate ");
                sql.AppendLine("FROM radpostauth B ");
                sql.AppendLine("WHERE B.username = @USERNAME ");
                sql.AppendLine("ORDER BY B.authdate ");
                sql.AppendLine("DESC LIMIT 1 ");

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, sessionRadius.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        retorno.username = Login_Radius;
                        retorno.replyAuth = reader["reply"].ToString();
                        retorno.dataUltimoAuth = reader["authdate"].ToString();
                    }
                }*/
            }

            return retorno;
        }        

        private string FormatPadraoData(String DataString)
        {
            String retorno = String.Empty;

            try
            {
                retorno = Convert.ToDateTime(DataString).ToString("dd/MM/yyyy HH:mm:ss");

                return retorno;
            }
            catch (Exception ex)
            {
                return DataString;
            }
        }

        /// <summary>
        /// Consulta se o IP inserido no parâmetro está disponível para uso. Retorna true se o IP estiver disponível.
        /// </summary>
        /// <param name="endereco_ip"></param>
        /// <returns></returns>
        public async Task<bool> VerificaIPDisponivel(String endereco_ip)
        {
            bool retorno = false;
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            using (ISession sessionRadius = HibernateUtil.GetSessionRadius())
            using (var com = sessionRadius.Connection.CreateCommand())
            {


                sql.AppendLine("SELECT radreply.username, radreply.value, radippool.username, radippool.framedipaddress   ");
                sql.AppendLine("FROM radreply ");
                sql.AppendLine("LEFT JOIN radippool ON (radreply.value = radippool.framedipaddress) ");
                sql.AppendLine("WHERE (radippool.framedipaddress = @IP AND radippool.username IS NOT NULL) ");
                sql.AppendLine("OR ");
                sql.AppendLine("  (radreply.attribute = 'Framed-IP-Address' AND radreply.value = @IP) ");
                sql.AppendLine("LIMIT 1");

                parametros.Add("@IP", endereco_ip);

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, sessionRadius.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        retorno = false;
                    }
                    else
                    {
                        retorno = true;
                    }
                }
            }

            return retorno;
        }


        /// <summary>
        /// Consulta status do login radius na tabela radacct, olhando a sessão gravada na tabela.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<StatusLoginRadius> VerificaStatusLogin(string login)
        {
            StatusLoginRadius response = StatusLoginRadius.Indisponivel;
            var acctstarttime = "";
            var acctstoptime = "";
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            try
            {
                using (ISession sessionRadius = HibernateUtil.GetSessionRadius())
                using (var com = sessionRadius.Connection.CreateCommand())
                {
                    sql.AppendLine("SELECT acctstarttime, acctstoptime");
                    sql.AppendLine("FROM radacct ");
                    sql.AppendLine("WHERE username = @USERNAME ");
                    sql.AppendLine("ORDER BY acctstarttime DESC");
                    sql.AppendLine("LIMIT 1");


                    parametros.Add("@USERNAME", login);

                    using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, sessionRadius.Connection, null))
                    {
                        if (await reader.ReadAsync())
                        {

                            acctstarttime = reader["acctstarttime"]?.ToString();
                            acctstoptime = reader["acctstoptime"]?.ToString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(acctstarttime) && string.IsNullOrEmpty(acctstoptime))
                {
                    response = StatusLoginRadius.Online;
                }
                else if (!string.IsNullOrEmpty(acctstoptime))
                {
                    response = StatusLoginRadius.Offline;
                }
                else
                {
                    response = StatusLoginRadius.Indisponivel;
                }
            }
            catch (Exception ex)
            {
                response = StatusLoginRadius.Erro;
            }
      
            return response;
        }


        /// <summary>
        /// Consulta IP atribuído ao login radius na tabela radacct, olhando a sessão gravada na tabela.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<string> GetIPLoginSessao(string login)
        {
            string response = "";
  
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            try
            {
                using (ISession sessionRadius = HibernateUtil.GetSessionRadius())
                using (var com = sessionRadius.Connection.CreateCommand())
                {
                    sql.AppendLine("SELECT acctstarttime, acctstoptime, framedipaddress");
                    sql.AppendLine("FROM radacct ");
                    sql.AppendLine("WHERE username = @USERNAME ");
                    sql.AppendLine("ORDER BY acctstarttime DESC");
                    sql.AppendLine("LIMIT 1");


                    parametros.Add("@USERNAME", login);

                    using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReader(com, sql, parametros, sessionRadius.Connection, null))
                    {
                        if (await reader.ReadAsync())
                        {
                            response = reader["framedipaddress"]?.ToString();
                        }
                    }
                }

               
            }
            catch (Exception ex)
            {
                response = "";
            }

            return response;
        }

        /* /// <summary>
         /// Consulta os dados da tabela radpostauth, no radius.
         /// </summary>
         /// <param name="SearchLogin"></param>
         /// <param name="Login_Radius"></param>
         /// <param name="Data_Autenticacao"></param>
         /// <param name="Desc_Concentradora"></param>
         /// <param name="Status_Autenticacao"></param>
         /// <param name="Status_Autenticacao"></param>
         /// <param name="length"></param>
         /// <param name="start"></param>
         /// <param name="Lista_Nomes_PoolIP"></param>
         /// <returns></returns>
         public async Task<List<RelatorioPainelHistoricoAutenticacao>> GetDadosPainelHistoricoAutenticacoes(string SearchLogin, string Login_Radius, 
             string Data_Autenticacao, string Desc_Concentradora, string Status_Autenticacao, int length, int start, List<Pool_Ip> Lista_Nomes_PoolIP)
         {

             List<RelatorioPainelHistoricoAutenticacao> retorno = new List<RelatorioPainelHistoricoAutenticacao>();
             StringBuilder sql = new StringBuilder();
             Dictionary<string, object> parametros = new Dictionary<string, object>();

             using (ISession sessionRadius = HibernateUtil.GetSessionRadius())
             using (var com = sessionRadius.Connection.CreateCommand())
             {

                 //parametros.Add("@REPLY", Status_Autenticacao);
                 sql.AppendLine("SELECT ");
                 sql.AppendLine("    A.username, ");
                 sql.AppendLine("    A.pass, ");
                 sql.AppendLine("    A.reply, ");
                 sql.AppendLine("    A.authdate, ");
                 sql.AppendLine("    C.shortname, ");
                 sql.AppendLine("    E.framedipaddress, ");
                 sql.AppendLine("    COALESCE(G.value, H.pool_name) AS pool_name ");
                 sql.AppendLine("FROM ");
                 sql.AppendLine("    radpostauth A ");
                 sql.AppendLine("LEFT JOIN (SELECT DISTINCT rc.username, rc.value FROM radcheck rc WHERE rc.attribute = 'NAS-IP-Address') B ON (A.username = B.username) ");
                 sql.AppendLine("LEFT JOIN nas C ON (B.value = C.nasname) ");
                 sql.AppendLine("LEFT JOIN ( ");
                 sql.AppendLine("    SELECT ");
                 sql.AppendLine("        ra.username, ");
                 sql.AppendLine("        ra.framedipaddress, ");
                 sql.AppendLine("        ra.acctstarttime ");
                 sql.AppendLine("    FROM ");
                 sql.AppendLine("        radacct ra ");
                 sql.AppendLine("    INNER JOIN ( ");
                 sql.AppendLine("        SELECT ");
                 sql.AppendLine("            username, ");
                 sql.AppendLine("            MAX(acctstarttime) AS latest_acctstarttime ");
                 sql.AppendLine("        FROM ");
                 sql.AppendLine("            radacct ");
                 sql.AppendLine("        GROUP BY ");
                 sql.AppendLine("            username ");
                 sql.AppendLine("    ) latest ON ra.username = latest.username AND ra.acctstarttime = latest.latest_acctstarttime ");
                 sql.AppendLine(") E ON (A.username = E.username) ");
                 sql.AppendLine("LEFT JOIN radreply G ON (A.username = G.username AND G.attribute = 'Framed-Pool') ");
                 sql.AppendLine("LEFT JOIN radippool H ON (E.framedipaddress = H.framedipaddress) ");
                 sql.AppendLine("WHERE ");
                 sql.AppendLine("    A.reply IN ('Access-Reject', 'Access-Accept') ");
                 //sql.AppendLine("ORDER BY ");
                 //sql.AppendLine("    A.authdate ASC;");



                 //Filtros
                 if (!String.IsNullOrEmpty(Login_Radius) && String.IsNullOrEmpty(SearchLogin) && Login_Radius != "Todos")
                 {
                     parametros.Add("@USERNAME", Login_Radius);                   
                     sql.AppendLine(" AND UPPER(A.USERNAME) = UPPER(@USERNAME) ");
                 }

                 if (!String.IsNullOrEmpty(SearchLogin) && String.IsNullOrEmpty(Login_Radius))
                 {
                     parametros.Add("@SEARCH", String.Format("%{0}%", SearchLogin.ToUpper()));
                     sql.AppendLine(" AND UPPER(A.USERNAME) LIKE @SEARCH ");
                 }

                 if (!String.IsNullOrEmpty(Data_Autenticacao))
                 {
                     parametros.Add("@AUTHDATE", String.Format("%{0}%", Data_Autenticacao));
                     sql.AppendLine(" AND A.AUTHDATE LIKE @AUTHDATE ");
                 }

                 if (!String.IsNullOrEmpty(Desc_Concentradora) && Desc_Concentradora != "Todos")
                 {
                     parametros.Add("@SHORTNAME", Desc_Concentradora);
                     sql.AppendLine(" AND UPPER(C.shortname) = UPPER(@SHORTNAME) ");
                 }

                 if (!String.IsNullOrEmpty(Status_Autenticacao) && Status_Autenticacao != "Todos" && Status_Autenticacao == "1")
                 {
                     sql.AppendLine(" AND UPPER(A.REPLY) = UPPER('Access-Accept') ");
                 }
                 else if (!String.IsNullOrEmpty(Status_Autenticacao) && Status_Autenticacao != "Todos" && Status_Autenticacao == "0")
                 {
                     sql.AppendLine(" AND UPPER(A.REPLY) = UPPER('Access-Reject') ");
                 }


                 sql.AppendLine("ORDER BY A.authdate ASC ");

                 //Count
                 StringBuilder sqlCount = new StringBuilder();
                 sqlCount.AppendLine("SELECT COUNT(*) FROM (");
                 sqlCount.AppendLine(sql.ToString());
                 sqlCount.AppendLine(") AS totalQuery");

                 // Execute the count query
                 com.CommandText = sqlCount.ToString();
                 foreach (var param in parametros)
                 {
                     var dbParam = com.CreateParameter();
                     dbParam.ParameterName = param.Key;
                     dbParam.Value = param.Value;
                     com.Parameters.Add(dbParam);
                 }

                 var totalRegistros = Convert.ToInt32(await com.ExecuteScalarAsync());


                 var atribuirNomePool = "";
                 var nomePool = "";

                 using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReaderFirst(com, sql, parametros, sessionRadius.Connection, null, length, start))
                 {
                     while (await reader.ReadAsync())
                     {
                         if(Lista_Nomes_PoolIP != null)
                         {
                             nomePool = Convert.ToString(reader["pool_name"]);

                             //reseta variável
                             atribuirNomePool = "";

                             foreach (var pool in Lista_Nomes_PoolIP)
                             {
                                 if(!String.IsNullOrEmpty(nomePool))
                                 {
                                     if(pool.Nome_Pool_Concentradora != null && nomePool == pool.Nome_Pool_Concentradora )
                                     {
                                         atribuirNomePool = pool.Descricao;
                                     }
                                     else if(pool.Descricao != null && nomePool == pool.Descricao)
                                     {
                                         atribuirNomePool = pool.Descricao;
                                     }
                                 }
                             }
                         }

                         retorno.Add(new RelatorioPainelHistoricoAutenticacao
                         {

                             Login_Radius = Convert.ToString(reader["username"]),
                             Senha = Convert.ToString(reader["pass"]),
                             Status_Autenticacao = Convert.ToString(reader["reply"]) == "Access-Accept" ? "Autenticado" : "Rejeitado",
                             Data_Autenticacao = Convert.ToString(reader["authdate"]),
                             Desc_Concentradora = Convert.ToString(reader["shortname"]),
                             Desc_Pool_Ip = atribuirNomePool ?? nomePool,
                             Endereco_Ip = Convert.ToString(reader["framedipaddress"])
                         });
                     }
                 }




             }

             return retorno;
         } */

        /// <summary>
        /// Consulta os dados da tabela radpostauth, no radius.
        /// </summary>
        /// <param name=request></param>
        /// <param name="Lista_Nomes_PoolIP"></param>
        /// <returns></returns>
        public async Task<DataTableResponse<RelatorioPainelHistoricoAutenticacao>> GetDadosPainelHistoricoAutenticacoes(RelatorioPainelHistoricoAutenticacao_DataTableRequest request, List<Sinapse.Integracao.Ativacao.Models.Pool_IpModel> Lista_Nomes_PoolIP)
        {
            int totalRegistros = 0;
            IOrderedEnumerable<RelatorioPainelHistoricoAutenticacao> listaOrdenada = null;
            List<RelatorioPainelHistoricoAutenticacao> retorno = new List<RelatorioPainelHistoricoAutenticacao>();
            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

            using (ISession sessionRadius = HibernateUtil.GetSessionRadius())
            using (var com = sessionRadius.Connection.CreateCommand())
            {

                //parametros.Add("@REPLY", Status_Autenticacao);
                sql.AppendLine("SELECT ");
                sql.AppendLine("    A.username, ");
                sql.AppendLine("    A.pass, ");
                sql.AppendLine("    A.reply, ");
                sql.AppendLine("    A.authdate, ");
                sql.AppendLine("    C.shortname, ");
                sql.AppendLine("    E.framedipaddress, ");
                sql.AppendLine("    COALESCE(G.value, H.pool_name) AS pool_name ");
                sql.AppendLine("FROM ");
                sql.AppendLine("    radpostauth A ");
                sql.AppendLine("LEFT JOIN (SELECT DISTINCT rc.username, rc.value FROM radcheck rc WHERE rc.attribute = 'NAS-IP-Address') B ON (A.username = B.username) ");
                sql.AppendLine("LEFT JOIN nas C ON (B.value = C.nasname) ");
                sql.AppendLine("LEFT JOIN ( ");
                sql.AppendLine("    SELECT ");
                sql.AppendLine("        ra.username, ");
                sql.AppendLine("        ra.framedipaddress, ");
                sql.AppendLine("        ra.acctstarttime ");
                sql.AppendLine("    FROM ");
                sql.AppendLine("        radacct ra ");
                sql.AppendLine("    INNER JOIN ( ");
                sql.AppendLine("        SELECT ");
                sql.AppendLine("            username, ");
                sql.AppendLine("            MAX(acctstarttime) AS latest_acctstarttime ");
                sql.AppendLine("        FROM ");
                sql.AppendLine("            radacct ");
                sql.AppendLine("        GROUP BY ");
                sql.AppendLine("            username ");
                sql.AppendLine("    ) latest ON ra.username = latest.username AND ra.acctstarttime = latest.latest_acctstarttime ");
                sql.AppendLine(") E ON (A.username = E.username) ");
                sql.AppendLine("LEFT JOIN radreply G ON (A.username = G.username AND G.attribute = 'Framed-Pool') ");
                sql.AppendLine("LEFT JOIN radippool H ON (E.framedipaddress = H.framedipaddress) ");
                sql.AppendLine("WHERE ");
                sql.AppendLine("    A.reply IN ('Access-Reject', 'Access-Accept') ");
                //sql.AppendLine("ORDER BY ");
                //sql.AppendLine("    A.authdate ASC;");



                //Filtros
                if (!String.IsNullOrEmpty(request.Login_Radius) && String.IsNullOrEmpty(request.search.value) && request.Login_Radius != "Todos")
                {
                    parametros.Add("@USERNAME", request.Login_Radius);
                    sql.AppendLine(" AND UPPER(A.USERNAME) = UPPER(@USERNAME) ");
                }

                if (!String.IsNullOrEmpty(request.search.value) && String.IsNullOrEmpty(request.Login_Radius))
                {
                    parametros.Add("@SEARCH", String.Format("%{0}%", request.search.value.ToUpper()));
                    sql.AppendLine(" AND UPPER(A.USERNAME) LIKE @SEARCH ");
                }

                if (!String.IsNullOrEmpty(request.Data_Autenticacao))
                {
                    string inputFormat = "dd/MM/yyyy";
                    string outputFormat = "yyyy-MM-dd";
                    DateTime parsedDate = DateTime.ParseExact(request.Data_Autenticacao, inputFormat, null);
                    request.Data_Autenticacao = parsedDate.ToString(outputFormat);
                    parametros.Add("@AUTHDATE", String.Format("%{0}%", request.Data_Autenticacao));
                    sql.AppendLine(" AND A.AUTHDATE LIKE @AUTHDATE ");
                }

                if (!String.IsNullOrEmpty(request.Desc_Concentradora) && request.Desc_Concentradora != "Todos")
                {
                    parametros.Add("@SHORTNAME", request.Desc_Concentradora);
                    sql.AppendLine(" AND UPPER(C.shortname) = UPPER(@SHORTNAME) ");
                }

                if (!String.IsNullOrEmpty(request.Status_Autenticacao) && request.Status_Autenticacao != "Todos" && request.Status_Autenticacao == "1")
                {
                    sql.AppendLine(" AND UPPER(A.REPLY) = UPPER('Access-Accept') ");
                }
                else if (!String.IsNullOrEmpty(request.Status_Autenticacao) && request.Status_Autenticacao != "Todos" && request.Status_Autenticacao == "0")
                {
                    sql.AppendLine(" AND UPPER(A.REPLY) = UPPER('Access-Reject') ");
                }


                sql.AppendLine("ORDER BY A.authdate ASC ");

                //Count
                StringBuilder sqlCount = new StringBuilder();
                sqlCount.AppendLine("SELECT COUNT(*) FROM (");
                sqlCount.AppendLine(sql.ToString());
                sqlCount.AppendLine(") AS totalQuery");

                // Execute the count query
                com.CommandText = sqlCount.ToString();
                foreach (var param in parametros)
                {
                    var dbParam = com.CreateParameter();
                    dbParam.ParameterName = param.Key;
                    dbParam.Value = param.Value;
                    com.Parameters.Add(dbParam);
                }

                totalRegistros = Convert.ToInt32(await com.ExecuteScalarAsync());


                var atribuirNomePool = "";
                var nomePool = "";

                using (var reader = await Sinapse.DAO.repository.CommandHelper.ExecuteReaderFirst(com, sql, parametros, sessionRadius.Connection, null, request.length, request.start))
                {
                    while (await reader.ReadAsync())
                    {
                        if (Lista_Nomes_PoolIP != null)
                        {
                            nomePool = Convert.ToString(reader["pool_name"]);

                            //reseta variável
                            atribuirNomePool = "";

                            foreach (var pool in Lista_Nomes_PoolIP)
                            {
                                if (!String.IsNullOrEmpty(nomePool))
                                {
                                    if (pool.NomePoolConcentradora != null && nomePool == pool.NomePoolConcentradora)
                                    {
                                        atribuirNomePool = pool.Descricao;
                                    }
                                    else if (pool.Descricao != null && nomePool == pool.Descricao)
                                    {
                                        atribuirNomePool = pool.Descricao;
                                    }
                                }
                            }
                        }

                        retorno.Add(new RelatorioPainelHistoricoAutenticacao
                        {

                            Login_Radius = Convert.ToString(reader["username"]),
                            Senha = Convert.ToString(reader["pass"]),
                            Status_Autenticacao = Convert.ToString(reader["reply"]) == "Access-Accept" ? "Autenticado" : "Rejeitado",
                            Data_Autenticacao = Convert.ToString(reader["authdate"]),
                            Desc_Concentradora = Convert.ToString(reader["shortname"]),
                            Desc_Pool_Ip = atribuirNomePool ?? nomePool,
                            Endereco_Ip = Convert.ToString(reader["framedipaddress"])
                        });
                    }
                }





            }

            listaOrdenada = retorno
              .OrderBy(x => DateTime.TryParse(x.Data_Autenticacao, out var date) ? date : DateTime.MinValue);

            return new DataTableResponse<RelatorioPainelHistoricoAutenticacao>
            {
                draw = request.draw,
                //recordsTotal = recordsTotal,
                recordsFiltered = totalRegistros,
                data = retorno,
                error = ""
            };
        }


        #region DEBUG
        private String UpdateRegistroRadacct()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("UPDATE radacct SET ");
            sql.AppendLine($"acctinputoctets = acctinputoctets * 1.1, ");
            sql.AppendLine($"acctoutputoctets = acctoutputoctets * 1.1 ");
            sql.AppendLine(" WHERE acctstoptime IS NULL AND username = @USERNAME; ");

            return sql.ToString();
        }      
        #endregion
    }
}