using System;

namespace Sinapse.Model.AspNetCore.nhibernate
{
    public interface IEndereco
    {
        String Cep { get; set; }
        String Logradouro { get; set; }
        String Bairro { get; set; }
        String Cidade { get; set; }
        String Estado { get; set; }
        String Pais { get; set; }
        String Numero { get; set; }
        String Complemento { get; set; }
        Double Longitude { get; set; }
        Double Latitude { get; set; }
    }
}
