using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Entity
{
    public class Item
    {
        public Item() { }
        public Item(string descricao, double valor)
        {
            this.descricao = descricao;
            this.valor = valor;
            this.quantidade = 1;
        }

        public Item(string descricao, double valor, double desconto)
        {
            this.descricao = descricao;
            this.valor = valor;
            this.desconto = desconto;
            this.quantidade = 1;
        }

        public string descricao { get; set; }
        public double valor { get; set; }
        public int quantidade { get; set; }
        public double? desconto { get; set; }
    }
}
