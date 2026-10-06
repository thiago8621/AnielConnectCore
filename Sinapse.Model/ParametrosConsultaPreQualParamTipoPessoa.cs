using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class ParametrosConsultaPreQualParamTipoPessoa : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual ParametrosConsultaPreQual ParametrosConsultaPreQual { get; set; }

        public virtual Tipo_Pessoa CodTipoPessoa { get; set; }
    }
}
