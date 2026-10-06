using Sinapse.Model.nhibernate;
using System;

namespace Sinapse.Model
{
    public class Meta : BaseModel
    {
        public virtual String Descricao { get; set; }

        public virtual Int16 Ano { get; set; }

        public virtual Boolean Status { get; set; }

        public virtual Int32 Janeiro { get; set; }

        public virtual Int32 Fevereiro { get; set; }

        public virtual Int32 Marco { get; set; }

        public virtual Int32 Abril { get; set; }

        public virtual Int32 Maio { get; set; }

        public virtual Int32 Junho { get; set; }

        public virtual Int32 Julho { get; set; }

        public virtual Int32 Agosto { get; set; }

        public virtual Int32 Setembro { get; set; }

        public virtual Int32 Outubro { get; set; }

        public virtual Int32 Novembro { get; set; }

        public virtual Int32 Dezembro { get; set; }

        public virtual Int32 GetMeta(Int32 mes)
        {
            switch (mes)
            {
                case 1: return Janeiro;
                case 2: return Fevereiro;
                case 3: return Marco;
                case 4: return Abril;
                case 5: return Maio;
                case 6: return Junho;
                case 7: return Julho;
                case 8: return Agosto;
                case 9: return Setembro;
                case 10: return Outubro;
                case 11: return Novembro;
                case 12: return Dezembro;
            }
            return 0;
        }
    }
}
