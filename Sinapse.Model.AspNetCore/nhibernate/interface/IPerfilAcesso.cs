using System;

namespace Sinapse.Model.AspNetCore.nhibernate
{
    public interface IPerfilAcesso: IModel
    {
        String Descricao { get; set; }
    }
}
