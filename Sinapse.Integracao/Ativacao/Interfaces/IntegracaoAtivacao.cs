using Aniel.Connect.Core;
using Sinapse.Integracao.Ativacao.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Sinapse.Integracao.Ativacao.Interfaces
{
    public interface IntegracaoAtivacao
    {
        Task<List<AutorizarONUListModel>> ONUsPendentesRegistro(string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null);
        Task<List<StatusONU>> GetStatusONUsByPON(string numSlot, string numPON, List<string> listSerial, List<(string, string)> listSerialConvertido = null, Func<string, string> consultarOntId = null);

        Response AutorizarONU(ConfigurarONUModel onu, string scriptOLT, string scriptONU);

        Task<Response> AutorizarONUAsync(ConfigurarONUModel onu, string scriptOLT, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null);

        Task<Response> DesautorizarONU(ConfigurarONUModel onu, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null);

        Task<DiagnosticoONU> DiagnosticoONU(ConfigurarONUModel onu);

        Task<List<object>> ListaOnusConectadas();

        Task<Response> ReiniciarONU(ConfigurarONUModel onu);

        Task<Response> ResetarONU(ConfigurarONUModel onu);

        Task<InfoConfigONU> GetConfigEquipamento(ConfigurarONUModel onu);

        Task<InfoHardwareONU> GetInfoHardware(ConfigurarONUModel onu);

        Task<Response> ConfigurarONUAsync(ConfigurarONUModel onu, string scriptONU, string Detalhe, string Usuario, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null);
        Task<GetDadosONUOLT> GetDadosONUnaOLT(string Serial_Convertido, string Serial, string Usuario, string Detalhe, Func<String, String, String, Int64?, Task<Int64>> enviarLog = null);
        Task<TesteONUModels> GetDadosComplementaresONU(ConfigurarONUModel onu);
        Task<GetStatusADMeOperacionalONU> GetStatusADMeOperacionalONU(ConfigurarONUModel onu);

    }
}
