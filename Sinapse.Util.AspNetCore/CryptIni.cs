using System;
using System.IO;
using System.Text;

namespace Sinapse.Util.AspNetCore
{
    public static class CryptIni
    {
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


        public static String GenerateStringFromStream(MemoryStream s)
        {
            // convert stream to string
            StreamReader reader = new StreamReader(s);
            return reader.ReadToEnd();
        }

        public static MemoryStream GenerateStreamFromString(string s)
        {
            byte[] byteArray = Encoding.Default.GetBytes(s);
            return new MemoryStream(byteArray);
        }
    }
}
