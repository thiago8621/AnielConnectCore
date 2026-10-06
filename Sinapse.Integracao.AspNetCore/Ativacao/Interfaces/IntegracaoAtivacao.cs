using Aniel.Connect.Core.AspNetCore;
using Sinapse.Integracao.AspNetCore.Ativacao.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Integracao.AspNetCore.Ativacao.Interfaces
{
    public interface IntegracaoAtivacao
    {
        Task<List<AutorizarONUListModel>> ONUsPendentesRegistro();
        Task<List<StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listSerial, Func<string, Task<string>> consultarOntId = null, List<(string, string)> listaAuxiliar = null);

        Response AutorizarONU(ConfigurarONUModel onu, string scriptOLT, string scriptONU);

        Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU);

        Task<Response> DesautorizarONU(ConfigurarONUModel onu);

        Task<DiagnosticoONU> DiagnosticoONU(ConfigurarONUModel onu);

        Task<List<object>> ListaOnusConectadas();

        Task<Response> ReiniciarONU(ConfigurarONUModel onu);

        Task<Response> ResetarONU(ConfigurarONUModel onu);

        Task<InfoConfigONU> GetConfigEquipamento(ConfigurarONUModel onu);

        Task<InfoHardwareONU> GetInfoHardware(ConfigurarONUModel onu);
    }
}
