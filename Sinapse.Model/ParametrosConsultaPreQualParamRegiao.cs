using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class ParametrosConsultaPreQualParamRegiao : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual ParametrosConsultaPreQual ParametrosConsultaPreQual { get; set; }

        public virtual int? CodCidade { get; set; }
        public virtual int? CodUF { get; set; }
        public virtual int? CodBairro { get; set; }
        public virtual bool TodasRegioes { get; set; }
    }
}
