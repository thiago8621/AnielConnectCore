using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class PerfilVisualizacaoVendaParamSetorDepto : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual PerfilVisualizacaoVenda PerfilVisualizacao { get; set; }

        public virtual short CodDepto { get; set; }
    }
}
