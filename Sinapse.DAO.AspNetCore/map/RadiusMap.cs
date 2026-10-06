using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerados;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.DAO.AspNetCore.map
{
    public class RadiusMap : ClassMapBase<Radius>
    {

        public RadiusMap() : base()
        {
            Table("TB_RADIUS");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_RADIUS");

            Map(x => x.Descricao, "DESCRICAO");
            Map(x => x.Status, "STATUS").CustomType<Status_Default>();

            Map(x => x.Ip, "IP");
            Map(x => x.Porta_SSH, "PORTA_SSH");
            Map(x => x.Timeout, "TIMEOUT");

            Map(x => x.Senha, "SENHA");
            Map(x => x.Dominio, "DOMINIO");

            Map(x => x.GrupoRadius, "GRUPO_RADIUS");

            #region CAMPOS ANTIGOS (mantidos para compatibilidade temporaria)

            Map(x => x.Usuario, "USUARIO");

            Map(x => x.Banco, "BANCO");
            Map(x => x.Usuario_Banco, "USUARIO_BANCO");
            Map(x => x.Senha_Banco, "SENHA_BANCO");
            Map(x => x.Porta_Banco, "PORTA_BANCO");

            #endregion
        }
    }
}
