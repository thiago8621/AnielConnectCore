using DocumentFormat.OpenXml.Drawing;
using NetTools;
using NHibernate;
using Renci.SshNet;
using Sinapse.DAO.AspNetCore.repository;
using Sinapse.Integracao.AspNetCore.Ativacao.Concentradoras;
using Sinapse.Integracao.AspNetCore.Ativacao.Interfaces;
using Sinapse.Integracao.AspNetCore.Ativacao.Models;
using Sinapse.Integracao.AspNetCore.Ativacao.Models.enumerados;
using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Util.AspNetCore.RemoteConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sinapse.Integracao.AspNetCore.Ativacao.Radius
{
    public class AnielRadius : IntegracaoRadius
    {
        private Sinapse.Model.AspNetCore.Radius radius;
        private ISession sessionRadius;

        public AnielRadius(Sinapse.Model.AspNetCore.Radius radius)
        {
            this.radius = radius;
            if (String.IsNullOrEmpty(this.radius.Dominio))
                throw new Exception($"Usuário do Radius \"{this.radius.Descricao}\" não configurado");

            if (String.IsNullOrEmpty(this.radius.Senha))
                throw new Exception($"Senha do Radius \"{this.radius.Descricao}\" não configurada");

            if (String.IsNullOrEmpty(this.radius.Ip))
                throw new Exception($"Endereço Ip do Radius \"{this.radius.Descricao}\" não configurado");

            //TODO: Necessário definir estrutura para salvar credenciais do banco para criar session dinâmica
            if (!String.IsNullOrEmpty(this.radius.Usuario_Banco) && !String.IsNullOrEmpty(this.radius.Senha_Banco))
            {
                sessionRadius = Sinapse.DAO.AspNetCore.repository.HibernateUtil.GetSessionRadius(this.radius.Ip, this.radius.Usuario_Banco, this.radius.Senha_Banco);
            }
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
                parametros.Add("@SECRET", "testing123"); // SECRET DE TESTE
                parametros.Add("@SERVER", null);
                parametros.Add("@COMMUNITY", null);
                parametros.Add("@IdConcentradora", concentradora.Id);

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                if (login.MAC != null)
                {
                    sql.AppendLine("                   ,(@USERNAME, 'Calling-Station', ':=', @MAC, @IDCLIENTE) ");
                    parametros.Add("@MAC", login.MAC);
                }

                parametros.Add("@USERNAME", login.Login);
                parametros.Add("@SENHA", login.Senha);
                parametros.Add("@NAS_IP", login.Concentradora.Ip);
                parametros.Add("@IDCLIENTE", login.IdCliente);
                parametros.Add("@POOL_NAME", login.Pool.Descricao);

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            switch (login.Concentradora.Tipo)
            {
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                    await new Mikrotik().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Huawei:
                    await new Huawei().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Juniper:
                    await new Juniper().ConfigurarPoolCliente(session, login);
                    break;
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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


                if (login.MAC != null)
                {
                    sql.AppendLine(",(@USERNAME, 'Calling-Station', ':=', @MAC, @IDCLIENTE) ");
                    parametros.Add("@MAC", login.MAC);
                }

                parametros.Add("@USERNAME", login.Login);
                parametros.Add("@IDCLIENTE", login.IdCliente);
                parametros.Add("@NAS_IP", login.Concentradora.Ip);

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            switch (login.Concentradora.Tipo)
            {
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                    await new Mikrotik().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Huawei:
                    await new Huawei().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Juniper:
                    await new Juniper().ConfigurarPoolCliente(session, login);
                    break;
            }
        }


        public async Task EditarUsuario(ISession session, LoginRadiusModel login)
        {
            var cadastroSimplificado = false;
            var temMacCadastrado = false;
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine(" SELECT COUNT(id) as QTD_REGISTROS FROM radcheck WHERE Id_Cliente = @IDCLIENTE AND attribute LIKE 'Calling-Station' ");
                parametros.Add("@IDCLIENTE", login.IdCliente);

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        temMacCadastrado = Convert.ToInt32(reader["QTD_REGISTROS"]) == 1;
                    }
                }
            }

            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine(" SELECT COUNT(id) as QTD_REGISTROS FROM radcheck WHERE Id_Cliente = @IDCLIENTE ");
                parametros.Add("@IDCLIENTE", login.IdCliente);

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    if (await reader.ReadAsync())
                    {
                        cadastroSimplificado = Convert.ToInt32(reader["QTD_REGISTROS"]) == 1;
                    }
                }
            }

            if (cadastroSimplificado)
            {
                await FinalizarCadastroUsuario(session, login);
            }
            else
            {
                using (var com = session.Connection.CreateCommand())
                {
                    StringBuilder sql = new StringBuilder();
                    Dictionary<string, object> parametros = new Dictionary<string, object>();

                    sql.AppendLine("UPDATE radcheck set username = @USERNAME WHERE Id_Cliente = @IDCLIENTE;");
                    sql.AppendLine("UPDATE radcheck set value = @SENHA WHERE attribute = 'Cleartext-Password' and Id_Cliente = @IDCLIENTE;");
                    sql.AppendLine("UPDATE radcheck set value = @NAS_IP WHERE attribute = 'NAS-IP-Address' and Id_Cliente = @IDCLIENTE;");

                    if (login.MAC != null)
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
                    }

                    parametros.Add("@USERNAME", login.Login);
                    parametros.Add("@SENHA", login.Senha);
                    parametros.Add("@NAS_IP", login.Concentradora.Ip);
                    parametros.Add("@IDCLIENTE", login.IdCliente);

                    await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
                }
            }

            switch (login.Concentradora.Tipo)
            {
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                    await new Mikrotik().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Huawei:
                    await new Huawei().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Juniper:
                    await new Juniper().ConfigurarPoolCliente(session, login);
                    break;
            }

            DesconectarLogin(login, login.usaRadiusSemContainer);
        }

        public async Task EditarUsuario(ISession session, string Usuario, string Senha, long idCliente)
        {
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();

                sql.AppendLine("UPDATE radcheck set username = @USERNAME WHERE Id_Cliente = @IDCLIENTE;");
                sql.AppendLine("UPDATE radcheck set value = @SENHA WHERE attribute = 'Cleartext-Password' and Id_Cliente = @IDCLIENTE;");

                parametros.Add("@USERNAME", Usuario);
                parametros.Add("@SENHA", Senha);

                parametros.Add("@IDCLIENTE", idCliente);

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (String.IsNullOrEmpty(PortaIncoming))
                        PortaIncoming = "3799";

                    string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + Usuario + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + " disconnect testing123"
                        : "docker exec -i radius bash -c 'echo \"User-Name = " + Usuario + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + " disconnect testing123'";

                    rc.ExecuteCommand(comando);
                }
            }
        }

        public async Task DesbloquearUsuario(ISession session, string Usuario, string Plano)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("UPDATE radusergroup set groupname = @PLANO WHERE username = @USERNAME;");

                string nomePlanoTratado = Plano.Replace(",", " ");

                parametros.Add("@PLANO", nomePlanoTratado);
                parametros.Add("@USERNAME", Usuario);

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task BloquearSinalPorPool(ISession session, string Usuario, long idCliente, string IpConcentradora, string PortaIncoming, string nomePoolBloqueio, bool UsaRadiusSemContainer)
        {
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("UPDATE radcheck set value = @POOLNAME WHERE Id_Cliente = @IDCLIENTE AND (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool');");
                sql.AppendLine("UPDATE radreply set value = @POOLNAME WHERE id_Cliente = @IDCLIENTE AND (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool');");

                parametros.Add("@IDCLIENTE", idCliente);
                parametros.Add("@POOLNAME", nomePoolBloqueio);

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (String.IsNullOrEmpty(PortaIncoming))
                        PortaIncoming = "3799";

                    string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + Usuario + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + " disconnect testing123"
                        : "docker exec -i radius bash -c 'echo \"User-Name = " + Usuario + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + " disconnect testing123'";

                    rc.ExecuteCommand(comando);
                }
            }
        }

        public async Task DesbloquearSinalPorPool(ISession session, LoginRadiusModel login)
        {
            switch (login.Concentradora.Tipo)
            {
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                    await new Mikrotik().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Huawei:
                    await new Huawei().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Juniper:
                    await new Juniper().ConfigurarPoolCliente(session, login);
                    break;
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (String.IsNullOrEmpty(PortaIncoming))
                        PortaIncoming = "3799";

                    string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + Login + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + " disconnect testing123"
                        : "docker exec -i radius bash -c 'echo \"User-Name = " + Login + "\" | radclient " + IpConcentradora + ":" + PortaIncoming + " disconnect testing123'";

                    rc.ExecuteCommand(comando);
                }
            }
        }

        //Apenas para HUAWEI
        public async Task DesbloquearSinalPorDomain(ISession session, ISession sessionConnect, LoginRadiusModel login, bool UsaRadiusSemContainer)
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }

            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (String.IsNullOrEmpty(login.Concentradora.PortaIncoming))
                        login.Concentradora.PortaIncoming = "3799";

                    string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + login.Login + "\" | radclient " + login.Concentradora.Ip + ":" + login.Concentradora.PortaIncoming + " disconnect testing123"
                        : "docker exec -i radius bash -c 'echo \"User-Name = " + login.Login + "\" | radclient " + login.Concentradora.Ip + ":" + login.Concentradora.PortaIncoming + " disconnect testing123'";

                    rc.ExecuteCommand(comando);
                }
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public async Task AtualizarPoolIpUsuario(ISession session, LoginRadiusModel login)
        {
            switch (login.Concentradora.Tipo)
            {
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Mikrotik:
                    await new Mikrotik().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Huawei:
                    await new Huawei().ConfigurarPoolCliente(session, login);
                    break;
                case Sinapse.Model.AspNetCore.enumerados.Marca_Modelo_Concentradora.Juniper:
                    await new Juniper().ConfigurarPoolCliente(session, login);
                    break;
            }

            DesconectarLogin(login, login.usaRadiusSemContainer, false);
        }

        public async Task AtualizarPoolIpUsuario(ISession session, long Id_Cliente, string Novo_NomePool)
        {
            // WHERE attribute = 'Pool-Name' and value = @POOL_NAME and Id_Cliente = @IDCLIENTE;"
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder();
                Dictionary<string, object> parametros = new Dictionary<string, object>();

                sql.AppendLine("UPDATE radcheck set value = @POOL_NAME_NOVO WHERE (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool') and Id_Cliente = @IDCLIENTE; ");
                sql.AppendLine("UPDATE radreply set value = @POOL_NAME_NOVO WHERE (attribute = 'Pool-Name' OR attribute = 'Framed-Route' OR attribute = 'Framed-Pool') and id_Cliente = @IDCLIENTE; ");

                parametros.Add("@IDCLIENTE", Id_Cliente);
                parametros.Add("@POOL_NAME_NOVO", Novo_NomePool);

                await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
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

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
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

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
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
                        string comando = UsaRadiusSemContainer == true ? $"radtest {usuario} {senha} {this.radius.Ip} 0 testing123 1 {ipConcentradora}"
                        : $"docker exec radius sh -c \"radtest {usuario} {senha} 127.0.0.1 0 testing123 1 {ipConcentradora}\"";

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

            parametros.Add("@USERNAME", request.Login_Radius);

            //Count
            using (var com = session.Connection.CreateCommand())
            {
                StringBuilder sql = new StringBuilder("SELECT COUNT(A.radacctid) CO ");
                sql.AppendLine(sqlFrom.ToString());

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
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

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReaderFirst(com, sql, parametros, session.Connection, null, request.length, request.start))
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

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
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

        public async Task<ConsumoPeriodoModel> GetConexaoTempoRealOld(ISession session, String Login_Radius, Sinapse.Model.AspNetCore.enumerados.Medida_Velocidade_ConexaoEnum Unidade_Medida_Velocidade)
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

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
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

        public async Task<ConsumoPeriodoModel> GetConexaoTempoReal(ISession session, String Login_Radius, Sinapse.Model.AspNetCore.enumerados.Medida_Velocidade_ConexaoEnum Unidade_Medida_Velocidade)
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

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
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
                sql.AppendLine("SELECT connectinfo_start AS DATAHORA_INICIO_CONEXAO, connectinfo_stop AS DATAHORA_FIM_CONEXAO ");
                sql.AppendLine("FROM radacct ");
                sql.AppendLine("WHERE username = @USERNAME ");
                sql.AppendLine("AND ((connectinfo_start > @DATA_INICIO AND connectinfo_start < @DATA_FIM) ");
                sql.AppendLine("OR (connectinfo_stop > @DATA_INICIO AND connectinfo_stop < @DATA_FIM)) ");

                parametros.Add("@DATA_INICIO", String.Format("{0}", Data_Inicio.ToString("yyyy/MM/dd HH:mm")));
                parametros.Add("@DATA_FIM", String.Format("{0}", Data_Fim.ToString("yyyy/MM/dd HH:mm:ss")));
                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new
                        {
                            DataHora_Inicio_Conexao = Convert.ToDateTime(Convert.ToString(reader["DATAHORA_INICIO_CONEXAO"])).ToLocalTime(),
                            DataHora_Termino_Conexao = Convert.ToDateTime(Convert.ToString(reader["DATAHORA_FIM_CONEXAO"])).ToLocalTime()
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
                    var dataHora_Fim = ((DateTime)item.DataHora_Termino_Conexao);

                    var model = retorno.FirstOrDefault(p => p.Data_Conexao.Value == dataHora_Inicio.Date || p.Data_Conexao.Value == dataHora_Fim.Date);
                    if (model != null)
                    {
                        var horasOnline = model.Horas_Online_Offline;
                        if (dataHora_Inicio.Date == dataHora_Fim.Date) //Login e Logoff no mesmo dia
                        {
                            horasOnline += dataHora_Fim.Subtract(dataHora_Inicio).Hours;
                        }
                        else if (dataHora_Inicio.Date == model.Data_Conexao.Value.Date) //Login no dia mas Logoff não
                        {
                            horasOnline += model.Data_Conexao.Value.AddHours(23).AddMinutes(59).AddSeconds(59).Subtract(dataHora_Inicio).Hours;
                        }
                        else //Logoff no dia mas Login não
                        {
                            horasOnline += model.Data_Conexao.Value.AddHours(23).AddMinutes(59).AddSeconds(59).Subtract(dataHora_Fim).Hours;
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
                sql.AppendLine("SELECT connectinfo_start AS DATAHORA_INICIO_CONEXAO, connectinfo_stop AS DATAHORA_FIM_CONEXAO ");
                sql.AppendLine("FROM radacct ");
                sql.AppendLine("WHERE username = @USERNAME ");
                sql.AppendLine("AND ((connectinfo_start > @DATA_INICIO AND connectinfo_start < @DATA_FIM) ");
                sql.AppendLine("OR (connectinfo_stop > @DATA_INICIO AND connectinfo_stop < @DATA_FIM)) ");

                parametros.Add("@DATA_INICIO", String.Format("{0}", Data_Inicio.ToString("yyyy/MM/dd HH:mm")));
                parametros.Add("@DATA_FIM", String.Format("{0}", Data_Fim.ToString("yyyy/MM/dd HH:mm:ss")));
                parametros.Add("@USERNAME", Login_Radius);

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new
                        {
                            DataHora_Inicio_Conexao = Convert.ToDateTime(Convert.ToString(reader["DATAHORA_INICIO_CONEXAO"])).ToLocalTime(),
                            DataHora_Termino_Conexao = Convert.ToDateTime(Convert.ToString(reader["DATAHORA_FIM_CONEXAO"])).ToLocalTime()
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
                    var dataHora_Fim = ((DateTime)item.DataHora_Termino_Conexao);

                    var model = retorno.FirstOrDefault(p => p.Data_Conexao.Value == dataHora_Inicio.Date || p.Data_Conexao.Value == dataHora_Fim.Date);
                    if (model != null)
                    {
                        var horasOffline = model.Horas_Online_Offline;
                        if (dataHora_Inicio.Date == dataHora_Fim.Date) //Login e Logoff no mesmo dia
                        {
                            horasOffline -= dataHora_Fim.Subtract(dataHora_Inicio).Hours;
                        }
                        else if (dataHora_Inicio.Date == model.Data_Conexao.Value.Date) //Login no dia mas Logoff não
                        {
                            horasOffline -= model.Data_Conexao.Value.AddHours(23).AddMinutes(59).AddSeconds(59).Subtract(dataHora_Inicio).Hours;
                        }
                        else //Logoff no dia mas Login não
                        {
                            horasOffline -= model.Data_Conexao.Value.AddHours(23).AddMinutes(59).AddSeconds(59).Subtract(dataHora_Fim).Hours;
                        }

                        if (horasOffline < 0) horasOffline = 0;
                        retorno.First(p => p.Data_Conexao == model.Data_Conexao).Horas_Online_Offline = horasOffline;
                    }
                    else continue;
                }
            }

            return retorno;
        }

        public bool DesconectarLogin(LoginRadiusModel login, bool UsaRadiusSemContainer, bool waitResponse = true)
        {
            var retorno = false;
            using (var rc = new RemoteConnection(this.radius.Ip, Convert.ToInt32(this.radius.Porta_SSH), this.radius.Dominio, this.radius.Senha, Protocol.Ssh))
            {
                rc.Connect();

                if (rc.IsConnected())
                {
                    if (String.IsNullOrEmpty(login.Concentradora.PortaIncoming))
                        login.Concentradora.PortaIncoming = "3799";

                    string comando = UsaRadiusSemContainer == true ? "echo \"User-Name = " + login.Login + "\" | radclient " + login.Concentradora.Ip + ":" + login.Concentradora.PortaIncoming + " disconnect testing123"
                        : "docker exec -i radius bash -c 'echo \"User-Name = " + login.Login + "\" | radclient " + login.Concentradora.Ip + ":" + login.Concentradora.PortaIncoming + " disconnect testing123'";

                    //rc.ExecuteCommand(comando);

                    rc.InteractiveExecute<ShellStream>((shellStream) =>
                    {
                        shellStream.WriteLine(comando);

                        if (waitResponse)
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
                    });
                }
            }
            return retorno;
        }

        public async Task<bool> GetStatusSessao(ISession session, string login)
        {
            bool response = false;
            string dataEncerramento = "";

            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> parametros = new Dictionary<string, object>();

                using (var com = session.Connection.CreateCommand())
                {
                    sql.AppendLine("SELECT acctstarttime, acctstoptime");
                    sql.AppendLine("FROM radacct ");
                    sql.AppendLine("WHERE username = @USERNAME ");
                    sql.AppendLine("ORDER BY acctstarttime DESC");
                    sql.AppendLine("LIMIT 1");


                    parametros.Add("@USERNAME", login);

                    using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                    {
                        if (await reader.ReadAsync())
                        {
                            dataEncerramento = reader["acctstoptime"]?.ToString();
                        }
                    }
                }

                if(String.IsNullOrEmpty(dataEncerramento))
                {
                    response = true;
                }

            return response;
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

                //SQL COMENTADO POIS A PERFORMANCE NÃO ESTAVA BOA
                /*sql.AppendLine("SET lc_time_names = 'pt_PT'; ");
                sql.AppendLine("SELECT A.username, A.acctupdatetime, B.idCliente_PlanoProduto, A.framedipaddress, ");
                sql.AppendLine("A.framedipv6address, A.framedipv6prefix, A.callingstationid, COALESCE(E.value, F.value) AS NOME_POOL ");                
                sql.AppendLine("FROM radacct A ");
                sql.AppendLine("INNER JOIN radusergroup B ");
                sql.AppendLine("ON(A.username = B.username) ");
                sql.AppendLine("INNER JOIN radgroupcheck C ");
                sql.AppendLine("ON(B.groupname = C.groupname) ");
                sql.AppendLine("INNER JOIN radacct D ");                
                sql.AppendLine("ON(A.radacctid = D.radacctid AND A.radacctid = (SELECT radacctid FROM radacct WHERE username = A.username ORDER BY acctupdatetime DESC LIMIT 1 )) ");
                sql.AppendLine("LEFT JOIN radreply E ");
                sql.AppendLine("ON(E.username = A.username AND E.attribute IN ('Framed-Route','Framed-Pool')) ");
                sql.AppendLine("LEFT JOIN radcheck F ");
                sql.AppendLine("ON(F.username = A.username AND F.attribute = 'Pool-Name') ");         
                sql.AppendLine("WHERE B.groupname <> 'Inativo' AND C.value = 'S' ");
                sql.AppendLine("AND A.acctstarttime IS NOT NULL ");
                sql.AppendLine("AND A.acctupdatetime IS NOT NULL ");*/

                sql.AppendLine("SET lc_time_names = 'pt_PT'; ");
                sql.AppendLine("SELECT A.username, C.acctupdatetime, A.idCliente_PlanoProduto, C.framedipaddress, C.framedipv6address, C.framedipv6prefix, C.callingstationid, ");
                sql.AppendLine("COALESCE(D.value, E.value) AS NOME_POOL, G.reply, G.authdate ");
                sql.AppendLine("FROM radusergroup A ");
                sql.AppendLine("INNER JOIN radgroupcheck B ");
                sql.AppendLine("ON (A.groupname = B.groupname) ");
                sql.AppendLine("INNER JOIN radacct C ");
                sql.AppendLine("ON (C.username = A.username AND C.radacctid = (SELECT radacctid FROM radacct WHERE username = A.username ORDER BY acctupdatetime DESC LIMIT 1 )) ");
                sql.AppendLine("LEFT JOIN radreply D ");
                sql.AppendLine("ON (D.username = A.username AND D.attribute IN ('Framed-Route','Framed-Pool')) ");
                sql.AppendLine("LEFT JOIN radcheck E ");
                sql.AppendLine("ON (E.username = A.username AND E.attribute = 'Pool-Name') ");
                sql.AppendLine("LEFT JOIN radpostauth G ");
                sql.AppendLine("ON(A.username = G.username AND G.id = (SELECT id FROM radpostauth WHERE username = A.username ORDER BY authdate DESC LIMIT 1)) ");
                sql.AppendLine("WHERE A.groupname <> 'Inativo' AND B.value = 'S' AND C.acctstarttime IS NOT NULL AND C.acctupdatetime IS NOT NULL ");                              

                if (Data_Hora_Corte.HasValue)
                {
                    sql.AppendLine("AND (C.acctstoptime IS NULL OR C.acctstoptime > @DATA_CORTE_CONEXOES_MORTAS)");
                    parametros.Add("@DATA_CORTE_CONEXOES_MORTAS", Data_Hora_Corte.Value.ToString("yyyy/MM/dd HH:mm"));
                }

                sql.AppendLine("ORDER BY C.acctupdatetime DESC;");

                using (var reader = await Sinapse.DAO.AspNetCore.repository.CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    while (await reader.ReadAsync())
                    {
                        retorno.Add(new DadosConexoesAtivasModel
                        {
                            DataHora_Atualizacao = reader["acctupdatetime"] != DBNull.Value ? Convert.ToDateTime(Convert.ToString(reader["acctupdatetime"])) : (DateTime?)null,
                            Login_Radius = Convert.ToString(reader["username"]),
                            Id_Cliente_Plano_Produto = reader["idCliente_PlanoProduto"] != DBNull.Value ? Convert.ToInt64(reader["idCliente_PlanoProduto"]) : 0,
                            Endereco_IPv4 = Convert.ToString(reader["framedipaddress"]),
                            Endereco_IPv6 = Convert.ToString(reader["framedipv6address"]),
                            Endereco_IPv6_Prefix = Convert.ToString(reader["framedipv6prefix"]),
                            MAC = Convert.ToString(reader["callingstationid"]),
                            Nome_Pool_Concentradora = Convert.ToString(reader["NOME_POOL"]),
                            Ultimo_Reply = Convert.ToString(reader["reply"]),
                            DataHora_Ultimo_Reply = reader["authdate"] != DBNull.Value ? Convert.ToDateTime(Convert.ToString(reader["authdate"])) : (DateTime?)null
                        });
                    }
                }
            }

            return retorno;
        }

        private String OctetsToKbMbGb(Double octets_velocidade, Sinapse.Model.AspNetCore.enumerados.Medida_Velocidade_ConexaoEnum? unidade_Medida_Velocidade = null)
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
                        case Sinapse.Model.AspNetCore.enumerados.Medida_Velocidade_ConexaoEnum.Kbs:
                            velocidade_convertida = String.Format("{0} Kb", velocidade_Kb.ToString("0.##"));
                            break;
                        case Sinapse.Model.AspNetCore.enumerados.Medida_Velocidade_ConexaoEnum.Mps:
                            velocidade_convertida = String.Format("{0} Mb", velocidade_Mb.ToString("0.##"));
                            break;
                        case Sinapse.Model.AspNetCore.enumerados.Medida_Velocidade_ConexaoEnum.Gbs:
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
