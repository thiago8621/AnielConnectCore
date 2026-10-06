using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Util.AspNetCore
{
    public static class StringService
    {
        public static List<char> listaAcentos = new List<char>
        {
            'á','à','â','ã',
            'é','è','ê',
            'í','ï',
            'ó','ô','õ','ö',
            'ú',
            'ç',
            'ñ',
            'Á','À','Â','Ã',
            'É','È',
            'Í','Ï',
            'Ó','Ô','Õ','Ö',
            'Ú',
            'Ç',
            'Ñ'
        };

        public static String Truncate(String valor, Int32 tamanho)
        {

            if (valor == null)
                return null;

            if (valor.Length > tamanho)
                return valor.Substring(0, tamanho);
            else
                return valor;
        }

        public static String TruncateTresPontinhos(String valor, Int32 tamanho)
        {

            if (valor == null)
                return null;

            if (valor.Length > tamanho)
                return valor.Substring(0, tamanho).Length == tamanho ? String.Concat(valor.Substring(0, tamanho), "...") : valor.Substring(0, tamanho);
            else
                return valor;
        }

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string RemoveUntilChar(string text, char caractere)
        {
            string retorno = text;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == caractere)
                {
                    retorno = text.Substring(i + 1);           
                }
            }

            return retorno;
        }

        public static string FormatToFilename(string s)
        {
            List<char> charList = System.IO.Path.GetInvalidFileNameChars().ToList();
            charList.Add('.');
            charList.AddRange(listaAcentos);

            foreach (char c in charList)
            {
                s = s.Replace(c.ToString(), String.Empty);
            }

            s = s.Replace(" ", String.Empty);

            return s;
        }

        public static string HoraParaString(double horaMinuto)
        {
            if (horaMinuto == 0)
            {
                return null;
            }

            horaMinuto = Math.Round(horaMinuto, 2);
            double horaMinutoAbs = Math.Abs(horaMinuto);

            int horas = (int)Math.Floor(horaMinutoAbs);
            int minutos = Convert.ToInt32((horaMinutoAbs - horas) * 60);

            if (horaMinuto == horaMinutoAbs)
                return String.Format("{0:D2}:{1:D2}", horas, minutos);
            else
                return String.Format("-{0:D2}:{1:D2}", horas, minutos);
        }

        public static double StringParaHora(string horaStr)
        {
            double retorno;

            if (String.IsNullOrEmpty(horaStr))
                return 0;

            string[] arrHoras = horaStr.Split(':');

            double horas = Convert.ToDouble(arrHoras[0]);
            double minutos = Convert.ToDouble(arrHoras[1]) / 60;

            if (horas < 0)
                minutos = minutos * -1;

            retorno = horas + minutos;

            return retorno;
        }

        public static bool IsNumberString(string value)
        {
            foreach (char caractere in value)
            {
                if (!Char.IsDigit(caractere))
                    return false;
            }

            return true;
        }

        public static string AlfanumericoAleatorio(int tamanho)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, tamanho)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

        public static string RemoverAcentos(string texto)
        {
            string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçÑñ";
            string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCcNn";

            for (int i = 0; i < comAcentos.Length; i++)
            {
                texto = texto.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
            }

            texto.Replace("'", String.Empty);
            texto.Replace("`", String.Empty);
            texto.Replace("´", String.Empty);

            return texto;
        }
    }
}
