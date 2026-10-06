using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class PerfilVisualizacaoServicoParamContratoProjeto : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual PerfilVisualizacaoServico PerfilVisualizacao { get; set; }

        public virtual string CodCt { get; set; }
        public virtual string Projeto { get; set; }
    }
}
