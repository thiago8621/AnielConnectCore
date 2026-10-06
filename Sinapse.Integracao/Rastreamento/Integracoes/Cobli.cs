using Aniel.Connect.Core;
using RestSharp;
using Sinapse.Integracao.Rastreamento.Cobli.Models;
using Sinapse.Integracao.SMS.Interface;
using Sinapse.Integracao.SMS.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Sinapse.Integracao.Rastreamento.Integracoes
{
    public class Cobli : BaseApiController, IntegracaoRastreamento
    {
        private string tokenApi { get; set; }
        public string Nome { get; set; }

        public Cobli(String tokenApi)
        {
            this.tokenApi = tokenApi;
            this.Nome = "Cobli";
        }

        public RastreamentoGetPathResponseModel GetPath(DateTime inicio, DateTime fim)
        {
            RastreamentoGetPathResponseModel retorno = new RastreamentoGetPathResponseModel();
            retorno.Dados = new List<PathRastreamento>();

            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("cobli-api-key", tokenApi);

                CobliPathResponse retornoApi = GetIntegracao<CobliPathResponse>($"https://api.cobli.co/public/v1/paths/summary?startDate={inicio.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")}&endDate={fim.ToString("yyyy-MM-ddT23:59:59.fffzzz")}&periodAggroupment=daily", headers);

                if (retornoApi.data != null)
                {
                    retorno.Sucesso = true;
                    foreach (var item in retornoApi.data)
                    {
                        foreach (var itemData in item.summaries)
                        {
                            retorno.Dados.Add(new PathRastreamento
                            {
                                nome = item?.driver.name != null ? item.driver.name.Trim() : null,
                                metrosPercorridos = itemData.trip.distance_in_meters,
                                data = itemData.start_address.date,
                                minutosParados = (itemData.stops.duration_in_seconds / 60)
                            });
                        }
                    }

                }
                else
                {
                    retorno.Sucesso = false;
                }
            }
            catch (Exception ex)
            {
                retorno.MensagemErro = ex.Message;
                retorno.Sucesso = false;
            }

            return retorno;
        }

        public RastreamentoLastLocationResponseModel LastLocation()
        {
            RastreamentoLastLocationResponseModel retorno = new RastreamentoLastLocationResponseModel();
            retorno.Dados = new List<LastLocationRastreamento>();

            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("cobli-api-key", tokenApi);

                VehicleResponseModel retornoApi = GetIntegracao<VehicleResponseModel>($"https://api.cobli.co/herbie-1.1/dash/device", headers);

                if (retornoApi != null && retornoApi.devices != null)
                {
                    retorno.Sucesso = true;
                    foreach (var item in retornoApi.devices)
                    {
                        retorno.Dados.Add(new LastLocationRastreamento
                        {
                            nome = item?.driver?.name != null ? item.driver.name.Trim() : null,
                            latitude = item.last_location.latitude,
                            longitude = item.last_location.longitude
                        });
                    }

                }
                else
                {
                    retorno.Sucesso = false;
                }
            }
            catch (Exception ex)
            {
                retorno.MensagemErro = ex.Message;
                retorno.Sucesso = false;
            }

            return retorno;
        }

        public TrajetoRealizadoResponseModel GetTrajeto(DateTime inicio, DateTime fim, string tecnico)
        {
            TrajetoRealizadoResponseModel retorno = new TrajetoRealizadoResponseModel();
            retorno.Dados = new List<TrajetoRealizado>();
            retorno.Paradas = new List<TrajetoRealizado>();
            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("cobli-api-key", tokenApi);

                DeviceResponseModel retornoDeviceApi = GetIntegracao<DeviceResponseModel>($"https://api.cobli.co/herbie-1.1/dash/device", headers);
                var deviceId = retornoDeviceApi.devices.Find(p => p?.driver?.name != null && p.driver.name.ToUpper().Trim() == tecnico.ToUpper().Trim())?.vehicle?.device_id;

                if (String.IsNullOrEmpty(deviceId) == false)
                {
                    var inicioMilisec = (long)(inicio.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
                    var fimMilisec = (long)(fim.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                    DetalhamentoViagemCobli retornoApi = GetIntegracao<DetalhamentoViagemCobli>($"https://api.cobli.co/herbie-1.1/dash/device-trip?start_time={inicioMilisec}&end_time={fimMilisec}&device_id={deviceId}", headers);

                    if (retornoApi?.path_geojson != null)
                    {
                        retorno.Sucesso = true;
                        for (int i = 0; i < retornoApi.path_geojson.geometry.coordinates.Count; i++)
                        {
                            JsonArray item = retornoApi.path_geojson.geometry.coordinates[i] as JsonArray;
                            double ticks = Convert.ToDouble(retornoApi.path_geojson.properties.timestamps[i]);
                            TimeSpan time = TimeSpan.FromMilliseconds(ticks);
                            DateTime datahora = (new DateTime(1970, 1, 1) + time).ToLocalTime();

                            retorno.Dados.Add(new TrajetoRealizado
                            {
                                Latitude = Convert.ToDouble(item.ElementAt(1)),
                                Longitude = Convert.ToDouble(item.ElementAt(0)),
                                DataHora = datahora,
                                DataHoraStr = datahora.ToString("dd/MM/yyyy HH:mm")
                            });
                        }

                        var primeiraLocalizacao = retornoApi.path_geojson?.properties?.total_distances_in_m?.FirstOrDefault();
                        var ultimaLocalizacao = retornoApi.path_geojson?.properties?.total_distances_in_m?.LastOrDefault();

                        if (primeiraLocalizacao != null && ultimaLocalizacao != null)
                        {
                            retorno.DistanciaTotalKm = (Convert.ToDouble(ultimaLocalizacao) - Convert.ToDouble(primeiraLocalizacao)) / 1000;
                        }


                        for (int i = 0; i < retornoApi.path_geojson.properties.stops.Count; i++)
                        {
                            Stop itemParada = retornoApi.path_geojson.properties.stops[i];
                            DateTime datahora = Convert.ToDateTime(itemParada.properties.start_time).ToLocalTime();
                            DateTime datahoraFim = Convert.ToDateTime(itemParada.properties.start_time).Add(TimeSpan.FromMilliseconds(itemParada.properties.idle_off_duration_in_millis)).ToLocalTime();
                            retorno.Paradas.Add(new TrajetoRealizado
                            {
                                Latitude = Convert.ToDouble(itemParada.geometry.coordinates[1]),
                                Longitude = Convert.ToDouble(itemParada.geometry.coordinates[0]),
                                DataHora = datahora,
                                DataHoraStr = datahora.ToString("dd/MM/yyyy HH:mm"),
                                DataHoraFim = datahoraFim,
                                DataHoraFimStr = datahoraFim.ToString("dd/MM/yyyy HH:mm"),
                                Geofence = String.Join(",", itemParada.properties.geofences.Select(p => p?.properties?.name).Where(p => p != null))
                            });
                        }

                    }
                    else
                    {
                        retorno.Sucesso = false;
                    }
                }
                else
                {
                    retorno.Sucesso = false;
                }
            }
            catch (Exception ex)
            {
                retorno.MensagemErro = ex.Message;
                retorno.Sucesso = false;
            }
            return retorno;
        }

        public OdometroResponseModel GetOdometro(string placa)
        {
            OdometroResponseModel retorno = new OdometroResponseModel();
            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("cobli-api-key", tokenApi);
                VeiculoResponseModel retornoDeviceApi = GetIntegracao<VeiculoResponseModel>($"https://api.cobli.co/public/v1/vehicles?limit=1000", headers);
                var vehicleId = retornoDeviceApi.data.Find(p => p?.license_plate == placa)?.id;

                if (String.IsNullOrEmpty(vehicleId) == false)
                {
                    var retornoOdometroApi = GetIntegracao<OdometroCobliResponse>($"https://api.cobli.co/public/v1/vehicles/" + vehicleId + "/odometer", headers);
                    if (retornoOdometroApi != null && retornoOdometroApi.data != null)
                    {
                        retorno.DistanciaTotalKm = retornoOdometroApi.data.odometer_in_km;
                        retorno.Sucesso = true;
                    }
                }

                return retorno;
            }
            catch (Exception ex)
            {
                retorno.Sucesso = false;
                retorno.MensagemErro = ex.Message;
            }

            return retorno;
        }

    }


    public class EnviarSMSResponseAPI
    {
        public string message { get; set; }
        public bool hasErrors { get; set; }
        public Error[] errors { get; set; }
    }

    public class Error
    {
        public string reference { get; set; }
        public string message { get; set; }
    }
}