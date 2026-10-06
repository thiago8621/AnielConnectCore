using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.Rastreamento.Cobli.Models
{
    public class PathRastreamento
    {
        public string id { get; set; }
        public string nome { get; set; }
        public DateTime data { get; set; }
        public double? metrosPercorridos { get; set; }
        public double? minutosParados { get; set; }
    }

    public class LastLocationRastreamento
    {
        public string id { get; set; }
        public string nome { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
    }

    public class RastreamentoGetPathResponseModel
    {
        public virtual bool Sucesso { get; set; }
        public virtual string MensagemErro { get; set; }
        public virtual List<PathRastreamento> Dados { get; set; }
    }

    public class RastreamentoLastLocationResponseModel
    {
        public virtual bool Sucesso { get; set; }
        public virtual string MensagemErro { get; set; }
        public virtual List<LastLocationRastreamento> Dados { get; set; }
    }
    public class TrajetoRealizadoResponseModel
    {
        public virtual bool Sucesso { get; set; }
        public virtual string MensagemErro { get; set; }
        public virtual List<TrajetoRealizado> Dados { get; set; }
        public virtual List<TrajetoRealizado> Paradas { get; set; }
        public virtual double DistanciaTotalKm { get; set; }
        
    }

    public class TrajetoRealizado
    {
        public virtual String Ponto_Partida { get; set; }
        public virtual String Num_Obra { get; set; }
        public virtual String DataHoraStr { get; set; }
        public virtual DateTime DataHora { get; set; }
        public virtual String DataHoraFimStr { get; set; }
        public virtual DateTime DataHoraFim { get; set; }
        public String Cor { get; set; }
        public virtual Double? Latitude { get; set; }
        public virtual Double? Longitude { get; set; }
        public virtual String Status { get; set; }
        public virtual String Geofence { get; set; }
    }


    public class DeviceResponseModel
    {
        public List<Device> devices { get; set; }
    }

    public class OdometroResponseModel
    {
        public virtual bool Sucesso { get; set; }
        public virtual string MensagemErro { get; set; }
        public virtual double DistanciaTotalKm { get; set; }

    }
    public class VeiculoModel
    {
        public string id { get; set; }
        public string device_id { get; set; }
        public string last_driver { get; set; }
        public string license_plate { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public int? year { get; set; }
        public string version { get; set; }
        public string size { get; set; }
        public int? speed_limit_in_kmh { get; set; }
        public Fuel fuel { get; set; }
    }

    public class Fuel
    {
        public string type { get; set; }
        public string price { get; set; }
        public double consumption_per_km { get; set; }
    }

    public class Pagination
    {
        public object previous { get; set; }
        public string next { get; set; }
        public int page { get; set; }
    }

    public class VeiculoResponseModel
    {
        public List<VeiculoModel> data { get; set; }
        public Pagination pagination { get; set; }
    }

    



}