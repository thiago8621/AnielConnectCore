using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class PerfilVisualizacaoVendaParamRegiao : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual PerfilVisualizacaoVenda PerfilVisualizacao { get; set; }

        public virtual int? CodCidade { get; set; }
        public virtual int? CodUF { get; set; }
    }
}
