using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class PerfilVisualizacaoVendaParamCategoriaCliente : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual PerfilVisualizacaoVenda PerfilVisualizacao { get; set; }

        public virtual string CodCategoriaCliente { get; set; }
    }
}
