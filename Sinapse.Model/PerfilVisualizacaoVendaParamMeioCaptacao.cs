using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class PerfilVisualizacaoVendaParamMeioCaptacao : BaseModel
    {
        /// <summary>
        ///     Perfil de visualização ao qual esse parâmetro está associado
        /// </summary>
        public virtual PerfilVisualizacaoVenda PerfilVisualizacao { get; set; }

        public virtual string CodMeioCaptacao { get; set; }
    }
}
