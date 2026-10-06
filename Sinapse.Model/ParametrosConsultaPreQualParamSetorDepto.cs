using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class ParametrosConsultaPreQualParamSetorDepto : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual ParametrosConsultaPreQual ParametrosConsultaPreQual { get; set; }

        public virtual short CodDepto { get; set; }
    }
}
