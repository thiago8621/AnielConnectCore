using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class ConsultaViabilidadeMotivo : BaseModel
    {
        public virtual ConsultaViabilidade Consulta { get; set; }
        public virtual ConsultaViabilidadeEtapa Etapa { get; set; }
        public virtual long IdMotivo { get; set; }
        public virtual string Motivo { get; set; }
    }
}
