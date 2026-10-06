using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class PerfilVisualizacaoServicoParamArea : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual PerfilVisualizacaoServico PerfilVisualizacao { get; set; }

        public virtual string CodEscritorio { get; set; }
        public virtual string CodArea { get; set; }
        public virtual int? CodLocalidade { get; set; }
    }
}
