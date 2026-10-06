using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Sinapse.Util
{
    public class CSVUtil
    {
        /// <summary>
        /// Executa e escreve as informações dos objetos dentro de um arquivo CRV
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool WriteCSVArquivo<T>  (string path, IEnumerable<T> objeto) where T : class
        {
            try
            {
                if (!File.Exists(path))
                {
                    using (File.Create(path)) { }
                }

                var configFile = new CsvConfiguration(new CultureInfo("pt-BR"))
                {
                    HasHeaderRecord = true
                };

                using (StreamWriter streamWriter = new StreamWriter(path, true, Encoding.UTF8))
                using (CsvWriter csvWriter = new CsvWriter(streamWriter, configFile))
                {
                    csvWriter.WriteRecords(objeto);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Não foi possivel criar arquivo CSV. Exception: {ex.Message}");
            }
            
            return true;
        }
    }
}
