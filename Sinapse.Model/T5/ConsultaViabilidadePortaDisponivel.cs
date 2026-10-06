using Sinapse.Model.nhibernate;

namespace Sinapse.Model
{
    public class ConsultaViabilidadePortaDisponivel : BaseModel
    {
        public virtual Elemento_Rede_Porta Porta { get; set; }

        /// <summary>
        ///     Dados da consulta que, no momento da sua execução, possuia a
        ///     caixa a qual a porta pertence como um caixa disponível para o
        ///     endereço onde estava sendo consultada a viabilidade
        /// </summary>
        public virtual ConsultaViabilidadeDados Consulta { get; set; }
    }
}
