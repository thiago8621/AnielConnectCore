using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class PerfilVisualizacaoServicoParamSupervisor : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual PerfilVisualizacaoServico PerfilVisualizacao { get; set; }

        public virtual string CodSuperv { get; set; }
    }
}
