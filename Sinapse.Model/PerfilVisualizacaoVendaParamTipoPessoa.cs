using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class PerfilVisualizacaoVendaParamTipoPessoa : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual PerfilVisualizacaoVenda PerfilVisualizacao { get; set; }

        public virtual Tipo_Pessoa CodTipoPessoa { get; set; }
    }
}
