using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.Rastreamento.Cobli.Models
{

    public class EndAddress
    {
        public DateTime date { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class FuelCosts
    {
        public double trip { get; set; }
        public double stop { get; set; }
        public double total { get; set; }
    }
    public class CobliPathResponse
    {
        public List<CobliPath> data { get; set; }
    }

    public class CobliPath
    {
        public Vehicle vehicle { get; set; }
        public Driver driver { get; set; }
        public string aggroupment_type { get; set; }
        public List<Summary> summaries { get; set; }
    }

    public class StartAddress
    {
        public DateTime date { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class Stops
    {
        public int count { get; set; }
        public int count_in_geofence { get; set; }
        public double duration_in_seconds { get; set; }
        public double idle_time_duration { get; set; }
    }

    public class Summary
    {
        public StartAddress start_address { get; set; }
        public EndAddress end_address { get; set; }
        public Trip trip { get; set; }
        public Stops stops { get; set; }
        public FuelCosts fuel_costs { get; set; }
    }

    public class Trip
    {
        public double distance_in_meters { get; set; }
        public double duration_in_seconds { get; set; }
    }

    public class Driver
    {
        public string id { get; set; }
        public string fleet_id { get; set; }
        public string name { get; set; }
        public List<string> phone_numbers { get; set; }
        public bool active { get; set; }
        public object first_license_acquirement_date { get; set; }
        public DateTime license_expiration_date { get; set; }
        public string license_category { get; set; }
    }

    public class Group
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class LastLocation
    {
        public string device_id { get; set; }
        public int time { get; set; }
        public long time_in_millis { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int heading { get; set; }
        public int speed { get; set; }
        public bool ignition_on { get; set; }
        public long cobli_ingested_at { get; set; }
        public long processed_at { get; set; }
        public string source { get; set; }
    }

    public class VehicleResponseModel
    {
        public List<VehicleResponse> devices { get; set; }
    }

    public class VehicleResponse
    {
        public string esn { get; set; }
        public string type { get; set; }
        public Vehicle vehicle { get; set; }
        public Driver driver { get; set; }
        public string cobli_id { get; set; }
        public string fleet_id { get; set; }
        public LastLocation last_location { get; set; }
    }

    public class Vehicle
    {
        public string device_id { get; set; }
        public string vehicle_id { get; set; }
        public DateTime associated_at { get; set; }
        public string id { get; set; }
        public DateTime updated_at { get; set; }
        public string fleet_id { get; set; }
        public string license_plate { get; set; }
        public string size { get; set; }
        public string fuel_type { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public int year { get; set; }
        public object version { get; set; }
        public double fuel_price { get; set; }
        public int fuel_consumption { get; set; }
        public bool using_default_price { get; set; }
        public bool using_default_consumption { get; set; }
        public bool is_suggestion { get; set; }
        public List<Group> groups { get; set; }
        public int speed_limit_in_kmh { get; set; }
    }

    public class Device
    {
        public string device_id { get; set; }
        public string icc_id { get; set; }
        public string imei { get; set; }
        public string fleet_id { get; set; }
        public DateTime associated_at { get; set; }
        public string cobli_id { get; set; }
        public string chip_number { get; set; }
        public string chip_operator { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string esn { get; set; }
        public Vehicle vehicle { get; set; }
        public Driver driver { get; set; }
        public LastLocation last_location { get; set; }
    }

    public class Driver2
    {
        public string id { get; set; }
        public string fleet_id { get; set; }
        public string name { get; set; }
        public List<string> phone_numbers { get; set; }
        public bool active { get; set; }
        public object first_license_acquirement_date { get; set; }
        public object license_expiration_date { get; set; }
        public object license_category { get; set; }
        public List<PeriodDriven> period_driven { get; set; }
    }

    public class Geofence
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<object> coordinates { get; set; }
    }

    public class PathGeojson
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class PeriodDriven
    {
        public long start_timestamp { get; set; }
        public long end_timestamp { get; set; }
    }

    public class Properties
    {
        public string device_id { get; set; }
        public List<object> timestamps { get; set; }
        public List<object> speeds_in_km { get; set; }
        public List<object> total_distances_in_m { get; set; }
        public List<object> headings { get; set; }
        public List<Stop> stops { get; set; }
        public DateTime start_time { get; set; }
        public int idle_off_duration_in_millis { get; set; }
        public int idle_on_duration_in_millis { get; set; }
        public List<Geofence> geofences { get; set; }
        public Driver driver { get; set; }
        public int total_distance_in_meters { get; set; }
        public List<double> coordinate { get; set; }
        public string address { get; set; }
        public string name { get; set; }
        public int radius { get; set; }
        public string id { get; set; }
        public long created_at { get; set; }
        public string category { get; set; }
    }

    public class DetalhamentoViagemCobli
    {
        public string device_id { get; set; }
        public Vehicle vehicle { get; set; }
        public Device device { get; set; }
        public List<Driver> drivers { get; set; }
        public PathGeojson path_geojson { get; set; }
    }

    public class Stop
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class OdometroCobliResponse
    {
        public OdometroCobliData data { get; set; }
    }

    public class OdometroCobliData
    {
        public double odometer_in_km { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime odometer_date_at { get; set; }
        public Vehicle vehicle { get; set; }
    }





}