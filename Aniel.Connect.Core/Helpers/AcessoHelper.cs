using Aniel.Connect.Models.T5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.Helpers
{
    public class AcessoHelper
    {
        public static Boolean TemAcesso(List<AreaModuloModel> Areas, String Cod_Area)
        {
            try
            {
                return Areas.Find(p => p.Cod_Area == Cod_Area) != null;
            }
            catch
            {
                return false;
            }
        }

        public static Boolean TemAcessos(List<AreaModuloModel> Areas, params String[] Cod_Areas)
        {
            try
            {
                for (int i = 0; i < Cod_Areas.Length; i++)
                {
                    if (Areas.Find(p => p.Cod_Area == Cod_Areas[i]) != null)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
