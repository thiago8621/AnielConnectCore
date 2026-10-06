using Sinapse.Util.AspNetCore.excecoes;
using System;

namespace Sinapse.Util.AspNetCore
{
    public static class IntService
    {
        public static Boolean Int16Valid(Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                try
                {
                    Int16.Parse(obj.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static Boolean Int32Valid(Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                try
                {
                    Int32.Parse(obj.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static Boolean Int64Valid(Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                try
                {
                    Int64.Parse(obj.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void ValidaNumerico(Type pTipo, String pValor)
        {
            if (pTipo == typeof(Int16))
            {
                if (!IntService.Int16Valid(pValor))
                {
                    throw new IntergerInvalidException("Valor");
                }
            }
            else if (pTipo == typeof(Int32))
            {
                if (!IntService.Int32Valid(pValor))
                {
                    throw new IntergerInvalidException("Valor");
                }
            }
            else if (pTipo == typeof(Int64))
            {
                if (!IntService.Int64Valid(pValor))
                {
                    throw new IntergerInvalidException("Valor");
                }
            }
            else if (pTipo == typeof(Double))
            {
                if (!DoubleService.DoubleValid(pValor))
                {
                    throw new DoubleInvalidException("Valor");
                }
            }
        }

        public static Boolean IsNumerico(Type pTipo)
        {
            return (pTipo == typeof(Int16) || pTipo == typeof(Int32) || pTipo == typeof(Int64) || pTipo == typeof(Double));
        }

        public static Object ConverteNumerico(Type pTipo, String pValor)
        {
            if (pTipo == typeof(Int16))
                return Int16.Parse(pValor);
            else if (pTipo == typeof(Int32))
                return Int32.Parse(pValor);
            else if (pTipo == typeof(Int64))
                return Int64.Parse(pValor);
            else if (pTipo == typeof(Double))
                return Double.Parse(pValor);
            else
                return Int64.Parse(pValor);
        }

        public static String ToTimeString(Int32? minutos)
        {
            if (!minutos.HasValue)
                return "00:00";

            Int32 _horas = Convert.ToInt32(minutos / 60);
            Int32 _minutos = minutos.Value - (_horas * 60);

            return $"{String.Format("{0:D2}", _horas)}:{String.Format("{0:D2}", _minutos)}";
        }

        public static String ToTime99String(Int32? minutos)
        {
            if (!minutos.HasValue)
                return "000:00";

            Int32 _horas = Convert.ToInt32(minutos / 60);
            Int32 _minutos = minutos.Value - (_horas * 60);

            return $"{String.Format("{0:D3}", _horas)}:{String.Format("{0:D2}", _minutos)}";
        }

        public static String ToTimeString(Double? minutos)
        {
            if (!minutos.HasValue)
                return "00:00";

            Int32 _horas = Convert.ToInt32(minutos / 60);
            Int32 _minutos = Convert.ToInt32(minutos.Value) - (_horas * 60);

            return $"{String.Format("{0:D2}", _horas)}:{String.Format("{0:D2}", _minutos)}";
        }

        public static Int32 GetMinutos(String Minutos)
        {
            if (String.IsNullOrEmpty(Minutos))
                return 0;
            else
            {
                var _split = Minutos.Split(':');
                return ((Convert.ToInt32(_split[0]) * 60) + Convert.ToInt32(_split[1]));
            }
        }

        public static Int32 IntAleatorio()
        {
            Random rnd = new Random();
            return rnd.Next(1, Int32.MaxValue);
        }

    }
}
