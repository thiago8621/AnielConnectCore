using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Sinapse.Util
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

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        /// <summary>
        ///     Gera uma sequência de caracteres alfanuméricos com o tamanho especificado.
        /// </summary>
        /// <param name="length">Tamanho da string</param>
        public static string GetRandomString(int length)
        {
            byte[] randomByteArr = new byte[length];

            var rngProvider = new RNGCryptoServiceProvider();

            for (int i = 0; i < length; i++)
            {
                var randomByte = new byte[1];

                // Gera um caractere alfanumérico aleatório (inclui letras maiúsculas e minúsculas)
                do
                {
                    rngProvider.GetNonZeroBytes(randomByte);
                }
                while (!(randomByte[0] >= 0x30 && randomByte[0] <= 0x39) && // Código hexadecimal para números
                       !(randomByte[0] >= 0x41 && randomByte[0] <= 0x5A) && // Código hexadecimal para letras maiúsculas
                       !(randomByte[0] >= 0x61 && randomByte[0] <= 0x7A)); // Código hexadecimal para letras minúsculas

                randomByteArr[i] = randomByte[0];
            }

            return Encoding.UTF8.GetString(randomByteArr);
        }


        /// <summary>
        ///     Gera uma sequência de caracteres com o indíce.
        /// </summary>
        /// <param name="source">String de Origem</param>
        /// <param name="matchString">String de indíce</param>
        public static IEnumerable<int> GetAllIndexes(this string source, string matchString)
        {
            matchString = Regex.Escape(matchString); foreach (Match match in Regex.Matches(source, matchString))
            {
                yield return match.Index;
            }
        }

        /// <summary>
        /// Formata a cadeia de caracteres de forma a remover todos os espaços extras
        /// </summary>
        /// <param name="source">Cadeia de caracteres a ser formatada</param>
        /// <returns></returns>
        public static String GetFormatStringSingleWhiteSpace(string source) 
        {                                
            return Regex.Replace(source, @"\s+", " ").Trim();            
        }

        public static bool ValidarCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            // Remove tudo que não for número
            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

            if (cnpj.Length != 14)
                return false;

            // Elimina CNPJs inválidos conhecidos (todos números iguais)
            if (new string(cnpj[0], 14) == cnpj)
                return false;

            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = (soma % 11);
            resto = resto < 2 ? 0 : 11 - resto;

            string digito = resto.ToString();
            tempCnpj += digito;
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            resto = resto < 2 ? 0 : 11 - resto;

            digito += resto.ToString();

            return cnpj.EndsWith(digito);
        }

        public static bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove caracteres não numéricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais
            if (cpf.All(c => c == cpf[0]))
                return false;

            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            string digito = resto.ToString();
            tempCpf += digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }
    }
}
