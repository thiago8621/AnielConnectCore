using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace Sinapse.Util.AspNetCore
{
    public class IniParser : IDisposable
    {
        private readonly Hashtable keyPairs = new Hashtable();
        private readonly String iniFilePath;

        private struct SectionPair
        {
            public String Section;
            public String Key;
        }

        public IniParser()
        {

        }

        /// <summary>
        /// Opens the INI file at the given path and enumerates the values in the IniParser.
        /// </summary>
        /// <param name="iniPath">Full path to INI file.</param>
        public IniParser(String iniPath)
        {
            TextReader iniFile = null;
            String strLine = null;
            String currentRoot = null;
            String[] keyPair = null;

            iniFilePath = iniPath;

            if (File.Exists(iniPath))
            {
                try
                {
                    iniFile = new StreamReader(iniPath);

                    strLine = iniFile.ReadLine();

                    while (strLine != null)
                    {
                        strLine = strLine.Trim().ToUpper();

                        if (strLine != "")
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            else
                            {
                                try
                                {
                                    keyPair = strLine.Split(new char[] { '=' }, 2);

                                    SectionPair sectionPair;
                                    String value = null;

                                    if (currentRoot == null)
                                        currentRoot = "ROOT";

                                    sectionPair.Section = currentRoot;
                                    sectionPair.Key = keyPair[0];

                                    if (keyPair.Length > 1)
                                        value = keyPair[1];

                                    keyPairs.Add(sectionPair, value);
                                }
                                catch
                                {

                                }
                            }
                        }

                        strLine = iniFile.ReadLine();
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                }
            }
            else
                throw new FileNotFoundException("Unable to locate " + iniPath);

        }


        public IniParser LoadIniString(String StrIni)
        {
            String currentRoot = null;
            String[] keyPair = null;

            try
            {
                foreach (String iniStrLinha in Regex.Split(StrIni, "\r\n"))
                {
                    String iniStr = iniStrLinha.Trim().ToUpper();

                    if (iniStr != "")
                    {
                        if (iniStr.StartsWith("[") && iniStr.EndsWith("]"))
                        {
                            currentRoot = iniStr.Substring(1, iniStr.Length - 2);
                        }
                        else
                        {
                            try
                            {
                                keyPair = iniStr.Split(new char[] { '=' }, 2);

                                SectionPair sectionPair;
                                String value = null;

                                if (currentRoot == null)
                                    currentRoot = "ROOT";

                                sectionPair.Section = currentRoot;
                                sectionPair.Key = keyPair[0];

                                if (keyPair.Length > 1)
                                    value = keyPair[1];

                                keyPairs.Add(sectionPair, value);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                return this;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Returns the value for the given section, key pair.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="settingName">Key name.</param>
        public String GetSetting(String sectionName, String settingName)
        {
            SectionPair sectionPair = new SectionPair
            {
                Section = sectionName.ToUpper(),
                Key = settingName.ToUpper()
            };

            return (String)keyPairs[sectionPair];
        }

        public Boolean SectionExists(String sectionName)
        {

            Boolean retorno = false;

            foreach (SectionPair sectionPair in keyPairs.Keys)
            {
                retorno = sectionPair.Section == sectionName;

                if (retorno) break;
            }


            return retorno;

        }

        public Boolean KeyExists(String sectionName, String settingName)
        {
            try
            {
                SectionPair sectionPair = new SectionPair
                {
                    Section = sectionName.ToUpper(),
                    Key = settingName.ToUpper()
                };

                return keyPairs[sectionPair] != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enumerates all lines for given section.
        /// </summary>
        /// <param name="sectionName">Section to enum.</param>
        public String[] EnumSection(String sectionName)
        {
            ArrayList tmpArray = new ArrayList();

            foreach (SectionPair pair in keyPairs.Keys)
            {
                if (pair.Section == sectionName.ToUpper())
                    tmpArray.Add(pair.Key);
            }

            return (String[])tmpArray.ToArray(typeof(String));
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        /// <param name="settingValue">Value of key.</param>
        public void AddSetting(String sectionName, String settingName, String settingValue)
        {
            SectionPair sectionPair = new SectionPair { Section = sectionName.ToUpper(), Key = settingName.ToUpper() };

            if (keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);

            keyPairs.Add(sectionPair, settingValue);
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved with a null value.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        public void AddSetting(String sectionName, String settingName)
        {
            AddSetting(sectionName, settingName, null);
        }

        /// <summary>
        /// Remove a setting.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        public void DeleteSetting(String sectionName, String settingName)
        {
            SectionPair sectionPair = new SectionPair { Section = sectionName.ToUpper(), Key = settingName.ToUpper() };

            if (keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);
        }

        /// <summary>
        /// Save settings to new file.
        /// </summary>
        /// <param name="newFilePath">New file path.</param>
        public void SaveSettings(String newFilePath)
        {
            ArrayList sections = new ArrayList();
            String tmpValue = "";
            String strToSave = "";

            foreach (SectionPair sectionPair in keyPairs.Keys)
            {
                if (!sections.Contains(sectionPair.Section))
                    sections.Add(sectionPair.Section);
            }

            foreach (String section in sections)
            {
                strToSave += (String.Format("[{0}]\r\n", section));

                foreach (SectionPair sectionPair in keyPairs.Keys)
                {
                    if (sectionPair.Section == section)
                    {
                        tmpValue = (String)keyPairs[sectionPair];

                        if (tmpValue != null)
                            tmpValue = "=" + tmpValue;

                        strToSave += (String.Format("{0}{1}\r\n", sectionPair.Key, tmpValue));
                    }
                }

                strToSave += Environment.NewLine;
            }

            try
            {
                using (TextWriter tw = new StreamWriter(newFilePath))
                {
                    tw.Write(strToSave);
                    tw.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Save settings back to ini file.
        /// </summary>
        public void SaveSettings()
        {
            SaveSettings(iniFilePath);
        }


        #region Disposable
        bool disposed = false;

        // método privado para controle
        // da liberação dos recursos
        private void Dispose(bool disposing)
        {
            // Verifique se Dispose já foi chamado.
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Liberando recursos gerenciados

                }

                // Seta a variável booleana para true,
                // indicando que os recursos já foram liberados
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
