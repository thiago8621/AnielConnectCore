using Sinapse.Model;
using Sinapse.Model.enumerados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.map
{
    public class ConcentradoraMap : ClassMapBase<Concentradora>
    {
        public ConcentradoraMap() : base()
        {
            Table("TB_CONCENTRADORA");

            Map(p => p.Nome, "NOME");
            Map(p => p.Tipo, "TIPO").CustomType<Marca_Modelo_Concentradora>();

            References(p => p.POP, "ID_POP");

            Map(p => p.Ip, "IP");

            Map(p => p.Login, "LOGIN");
            Map(p => p.Senha, "SENHA");

            Map(p => p.Secret, "SECRET");

            References(p => p.Radius, "ID_RADIUS");

            Map(p => p.PortaTelnet, "PORTA_TELNET");
            Map(p => p.PortaAPI, "PORTA_API");
            Map(p => p.PortaHTTP, "PORTA_HTTP");
            Map(p => p.PortaSSH, "PORTA_SSH");
            Map(p => p.PortaIncoming, "PORTA_INCOMING");

            HasMany(x => x.GruposArea).KeyColumn("ID_CONCENTRADORA").Cascade.All().Inverse();
        }
    }
}
