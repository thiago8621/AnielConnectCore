using Aniel.Connect.Core;
using Sinapse.DAO;
using Sinapse.Integracao.Roteamento.Interface;
using Sinapse.Integracao.Roteamento.Models;
using System;
using System.Collections.Generic;

namespace Sinapse.Integracao.Roteamento.Repositorio
{
    public class OSRMRepository : BaseApiController, IOSRMRoteamento
    {
        public OSRMTableResponseModel GetResponseRotasOSRM(double latitude, double longitude, List<(double caixaLatitude, double caixaLongitude)> caixas)
        {         

            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();

                var url = CONSTANTES.URLROUTEOSRM;

                List<string> caixasLocalizacao = new List<string>();

                foreach (var item in caixas)
                {
                    caixasLocalizacao.Add($"{Convert.ToString(item.caixaLatitude).Replace(",", ".")},{Convert.ToString(item.caixaLongitude).Replace(",", ".")}");
                }

                OSRMTableResponseModel response = GetIntegracao<OSRMTableResponseModel>
                    ($@"{url}{Convert.ToString(longitude).Replace(",",".")},{Convert.ToString(latitude).Replace(",", ".")};{String.Join(";",caixasLocalizacao)}?annotations=distance&sources=0", headers);

                return response;                
            
            }
            catch (Exception ex)
            {
                throw ex;
                return null;
            }            

        }
    }
}