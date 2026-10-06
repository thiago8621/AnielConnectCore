using Aniel.Connect.Integracoes.SerasaExperian.Models;
using System.Threading.Tasks;

namespace Aniel.Connect.Integracoes.SerasaExperian.Interfaces
{
    public interface IConsultarSerasaService
    {
        Task<ConsultaSerasaResponse> ConsultarSerasaPJ (string documento);
        Task<ConsultaSerasaResponse> ConsultarSerasaPF (string documento);
    }
}