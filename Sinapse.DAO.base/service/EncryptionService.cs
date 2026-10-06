using Sinapse.Util;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sinapse.DAO.service
{
    public class EncryptionService
    {
        public static string EncryptionMD5(string valor)
        {
            MD5 md5Hasher = MD5.Create();

            byte[] valorCriptografado = md5Hasher.ComputeHash(Encoding.Default.GetBytes(valor));

            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < valorCriptografado.Length; i++)
            {
                strBuilder.Append(valorCriptografado[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public static bool ComparerEncryptionMd5(string valor, string valorCriptografado)
        {

            //Truncado por causa de um problema gerado atraves da senha "@itserv2020#". card #728 do quadro connect
            string NovoValorCriptografado = EncryptionMD5(StringService.Truncate(valor,10));

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

        public static string EncryptString(string valor)
        {
            String retorno = String.Empty;

            MemoryStream memStream = null;
            try
            {
                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "s1N4pS3#"; // MUST be 8 characters
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = Encoding.UTF8.GetBytes(valor);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                memStream = new MemoryStream();
                ICryptoTransform transform = provider.CreateEncryptor(key, IV);
                CryptoStream cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();

                retorno = Convert.ToBase64String(memStream.ToArray());
            }
            catch
            {
                retorno = "";
            }

            return retorno;
        }

        public static string DecryptString(string valor)
        {
            String retorno = String.Empty;

            MemoryStream memStream = null;
            try
            {
                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "s1N4pS3#"; // MUST be 8 characters
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = new byte[valor.Length];
                byteInput = Convert.FromBase64String(valor);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                memStream = new MemoryStream();
                ICryptoTransform transform = provider.CreateDecryptor(key, IV);
                CryptoStream cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();

                Encoding encoding1 = Encoding.UTF8;
                retorno = encoding1.GetString(memStream.ToArray());
            }
            catch
            {
                retorno = "";
            }

            return retorno;
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


        public static String DescriptografarConfigSystem(String pConfigSystem)
        {
            using (MemoryStream stream = GenerateStreamFromString(pConfigSystem))
            {
                return GenerateStringFromStream(EnDecryptMemory(stream, 1));
            }
        }

        private static MemoryStream EnDecryptMemory(MemoryStream Stream, int Chave)
        {
            Byte[] Destino = new Byte[Stream.Length];

            Int32 i = 0;
            foreach (Byte c in Stream.ToArray())
            {
                Int32 t = (byte)(c ^ ~((int)(Chave >> i)));
                Destino[i] = (byte)t;

                i++;
            }

            return new MemoryStream(Destino);
        }

        /*
         procedure EnDecryptMemory(Var Stream : TMemoryStream; Chave : Word);
            var
                I : Integer;
                C : byte;
                Destino:TMemoryStream;
            begin
                Destino:=TMemoryStream.Create;
                try
                Stream.Position := 0;
                for I := 0 to Stream.Size - 1 do
                begin
                    Stream.Read(C, 1);
                    C := (C xor not(ord(chave shr I)));
                    Destino.Write(C,1);
                end;
                Stream.Clear;
                Stream.LoadFromStream(Destino);
                Destino.Free;
                finally
                end;
            end;
         */

        public static MemoryStream GenerateStreamFromString(string s)
        {
            byte[] byteArray = Encoding.Default.GetBytes(s);
            return new MemoryStream(byteArray);
        }

        public static String GenerateStringFromStream(MemoryStream s)
        {
            // convert stream to string
            StreamReader reader = new StreamReader(s);
            return reader.ReadToEnd();
        }
    }
}
