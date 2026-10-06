using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class ParametrosConsultaPreQualParamCategoriaCliente : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual ParametrosConsultaPreQual ParametrosConsultaPreQual { get; set; }

        public virtual string CodCategoriaCliente { get; set; }
    }
}
