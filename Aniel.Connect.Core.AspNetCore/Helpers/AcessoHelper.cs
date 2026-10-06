using Aniel.Connect.AspNetCore.Models.T5;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aniel.Connect.Core.AspNetCore.Helpers
{
    public class AcessoHelper
    {
        public static Boolean TemAcesso(IEnumerable<AreaModuloModel> Areas, String Cod_Area)
        {
            try
            {
                return Areas.FirstOrDefault(p => p.Cod_Area == Cod_Area) != null;
            }
            catch
            {
                return false;
            }
        }

        public static Boolean TemAcessos(IEnumerable<AreaModuloModel> Areas, params String[] Cod_Areas)
        {
            try
            {
                for (int i = 0; i < Cod_Areas.Length; i++)
                {
                    if (Areas.FirstOrDefault(p => p.Cod_Area == Cod_Areas[i]) != null)
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
