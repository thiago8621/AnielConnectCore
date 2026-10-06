using System;
using System.IO;
using System.Linq;

namespace Sinapse.Util.AspNetCore
{
    public class FileUtil
    {
        public static void EmptyDirectory(String path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(String.Format("A pasta \"{0}\" não foi encontrada", path));
            }

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            foreach (var file in dirInfo.EnumerateFiles())
            {
                file.Delete();
            }

            foreach (var dir in dirInfo.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }

        public static Boolean VersaoMaior(String versaoAssembly, String versaoEsperada)
        {
            Int64 _versaoAssembly = Convert.ToInt64(String.Concat(from o in versaoAssembly.Split('.')
                                                                  select o.PadLeft(3,'0')));

            Int64 _versaoEsperada = Convert.ToInt64(String.Concat(from o in versaoEsperada.Split('.')
                                                                  select o.PadLeft(3, '0')));

            return _versaoAssembly < _versaoEsperada;
        }
    }
}
