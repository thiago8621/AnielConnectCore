using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    public class GeoJsonService
    {
        //
        public static T[,] ConvertToBidimensionalArray<T>(List<List<T>> request)
        {
            Dictionary<String, Object> dict = new Dictionary<String, Object>();

            var contagem = request[0].Count / 2;

            var retorno = new T[contagem, 2];
            foreach (var Lista in request)
            {
                for (Int32 contagemLista = 0, contagemDimensao = 0; contagemLista < Lista.Count; contagemDimensao++, contagemLista++)
                {
                    var contagemItens = 0;
                    //Devido a padrões diferentes, longitude é adicionado primeiro ao geojson, enquanto a latitude vem em seguida
                    var itemLat = Lista[contagemLista];
                    var itemLng = Lista[++contagemLista];

                    retorno.SetValue(itemLng, contagemDimensao, contagemItens);

                    contagemItens++;

                    retorno.SetValue(itemLat, contagemDimensao, contagemItens);
                }
            }
            return retorno;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Normally type Double</typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Double[,,] ConvertToTridimensionalArray<T>(List<List<T>> request)
        {
            Dictionary<String, Object> dict = new Dictionary<String, Object>();

            var contagem = request[0].Count / 2;

            var retorno = new Double[1, contagem, 2];
                Int32 contagemDimensao = 0;

            foreach (var Lista in request)
            {
                for (int contagemLista = 0; contagemLista < Lista.Count; contagemDimensao++, contagemLista++)
                {
                    var contagemItens = 0;
                    //Devido a padrões diferentes, longitude é adicionado primeiro ao geojson, enquanto a latitude vem em seguida
                    var itemLat = Lista[contagemLista];
                    var itemLng = Lista[++contagemLista];

                    retorno.SetValue(itemLng, 0, contagemDimensao, contagemItens);

                    contagemItens++;

                    retorno.SetValue(itemLat, 0, contagemDimensao, contagemItens);
                }

            }

            --contagemDimensao;

            /*  
                    =-=-=-          Dev Notes          =-=-=-
                    Erro ao realizar consulta no banco MongoDB
                polígono não tem o primeiro e último ponto iguais
                           ele precisa ser fechado.
               */
            if (retorno[0, contagemDimensao, 0] != retorno[0, 0, 0] || retorno[0, contagemDimensao, 1] != retorno[0, 0, 1])
            {
                retorno[0, contagemDimensao, 0] = retorno[0, 0, 0];
                retorno[0, contagemDimensao, 1] = retorno[0, 0, 1];
            }
            return retorno;

        }
        public static T[,,,] ConvertToQuadDimensionalArray<T>(List<List<T>> request)
        {
            Dictionary<String, Object> dict = new Dictionary<String, Object>();

            var contagem = request[0].Count / 2;

            var retorno = new T[contagem, 1, 1, 2];
            foreach (var Lista in request)
            {
                for (Int32 contagemLista = 0, contagemDimensao = 0; contagemLista < Lista.Count; contagemDimensao++, contagemLista++)
                {
                    var contagemItens = 0;

                    var itemLng = Lista[contagemLista];
                    retorno.SetValue(itemLng, contagemDimensao, contagemItens);

                    var itemLat = Lista[++contagemLista];
                    contagemItens++;

                    retorno.SetValue(itemLat, contagemDimensao, contagemItens);
                }
            }
            return retorno;

        }



    }
}
