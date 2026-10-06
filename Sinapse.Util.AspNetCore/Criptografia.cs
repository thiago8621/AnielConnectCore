using System;
using System.Security.Cryptography;
using System.Text;

namespace Sinapse.Util.AspNetCore
{
    public class Criptografia
    {
        public static String MD5Hash(string valor)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();

                //converter o valor recebido em bytes e encriptar
                byte[] byteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(valor));

                //retornar o valor do hash em string...
                return BitConverter.ToString(byteHash).Replace("-", string.Empty);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar hash md5: " + e.Message);
            }
        }

        public static bool ComparerEncryptionMd5(string valor, string valorCriptografado)
        {

            string NovoValorCriptografado = MD5Hash(valor);

            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (comparer.Compare(NovoValorCriptografado, valorCriptografado) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool ComparerEncryptionAniel(string valor, string valorCriptografado)
        {

            String NovoValorCriptografado = CriptografaAniel(valor);

            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (comparer.Compare(NovoValorCriptografado, valorCriptografado) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static String CriptografaAniel(String pS)
        {
            String Aux = String.Empty;

            for (int i = 0; i < pS.Length; i++)
            {
                if (pS[i] == '0') Aux += 'K';
                else if (pS[i] == '1') Aux += '!';
                else if (pS[i] == '2') Aux += 'L';
                else if (pS[i] == '3') Aux += '#';
                else if (pS[i] == '4') Aux += 'N';
                else if (pS[i] == '5') Aux += '%';
                else if (pS[i] == '6') Aux += 'G';
                else if (pS[i] == '7') Aux += 'T';
                else if (pS[i] == '8') Aux += '*';
                else if (pS[i] == '9') Aux += 'F';
            }
            return Aux;
        }

        public static String DescriptografaAniel(String pS)
        {
            String Aux = String.Empty;

            for (int i = 0; i < pS.Length; i++)
            {
                if (pS[i] == 'K') Aux += '0';
                else if (pS[i] == '!') Aux += '1';
                else if (pS[i] == 'L') Aux += '2';
                else if (pS[i] == '#') Aux += '3';
                else if (pS[i] == 'N') Aux += '4';
                else if (pS[i] == '%') Aux += '5';
                else if (pS[i] == 'G') Aux += '6';
                else if (pS[i] == 'T') Aux += '7';
                else if (pS[i] == '*') Aux += '8';
                else if (pS[i] == 'F') Aux += '9';
            }
            return Aux;
        }

    }
}
