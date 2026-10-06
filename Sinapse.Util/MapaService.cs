using CrazyBeavers.Geodesy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    public static class MapaService
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitudeDe"></param>
        /// <param name="longitudeDe"></param>
        /// <param name="latitudePara"></param>
        /// <param name="longitudePara"></param>
        /// <returns></returns>
        public static Double GetMenorDistancia(Double? latitudeDe, Double? longitudeDe, Double? latitudePara, Double? longitudePara)
        {
            if (!latitudeDe.HasValue || !longitudeDe.HasValue || !latitudePara.HasValue || !longitudePara.HasValue)
                return Double.MaxValue;

            //var sCoord = new GeoCoordinate(latitudeDe, longitudeDe);
            //var eCoord = new GeoCoordinate(latitudePara, longitudePara);

            // Create a pair of coordinates
            GeodeticCoordinate coordinate1 = new GeodeticCoordinate() { Latitude = latitudeDe.Value, Longitude = longitudeDe.Value };
            GeodeticCoordinate coordinate2 = new GeodeticCoordinate() { Latitude = latitudePara.Value, Longitude = longitudePara.Value };
            // Calculate the distance between the coordinates using the haversine formula
            double distance = DistanceCalculator.Haversine(coordinate1, coordinate2);

            return (distance * 1852); // / 60) * 1852;

            //return DistanceBetweenPlaces(longitudeDe, latitudeDe, longitudePara, latitudePara);   //(sCoord.GetDistanceTo(eCoord) / 60) * 1.852; // getDistance(sCoord, eCoord);  
        }

    }
}
