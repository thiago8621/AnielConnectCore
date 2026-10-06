using Sinapse.Model.nhibernate;
using System;

namespace Sinapse.Model
{
    /// <summary>
    /// cadastro de endereços de atuação da caixa
    /// </summary>
    /// <param name="Id_EnderecoLogradouro">Caso tenha referencia a cadastro de endereço, será gravado o id do endereço logradouro</param> 
    /// <param name="Id_CondominioPredio">Caso seja um condomínio, seu Id será gravado</param> 
    /// <param name="Numero">Número do Endereço da Caixa</param> 
    public class Elemento_Rede_Enderecos : BaseModel
    {
        public virtual long? Id_EnderecoLogradouro { get; set; }
        public virtual long? Id_CondominioPredio { get; set; }
        public virtual Elemento_Rede Elemento_Rede { get; set; }
        public virtual string Numero { get; set; }
        public virtual long? Bloco { get; set; }
        public virtual long? Apto_numero { get; set; }
        public virtual bool? Todos_Bloco { get; set; }
        public virtual bool? Todos_Apto_Numero { get; set; }
    }
}
