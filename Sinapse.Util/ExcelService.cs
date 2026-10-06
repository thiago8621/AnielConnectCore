using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Resources;
using Sinapse.Util.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    public static class ExcelService
    {
        public static DataTable GetDataTableFromSpreadsheet(Stream MyExcelStream, bool ReadOnly, String SheetName = "")
        {
            DataTable dt = new DataTable();
            using (SpreadsheetDocument sDoc = SpreadsheetDocument.Open(MyExcelStream, ReadOnly))
            {
                WorkbookPart workbookPart = sDoc.WorkbookPart;
                IEnumerable<Sheet> sheets = sDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = String.IsNullOrEmpty(SheetName) ? sheets.First().Id.Value : sheets.Where(p => p.Name == SheetName).First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)sDoc.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();

                foreach (Cell cell in rows.ElementAt(0))
                {
                    dt.Columns.Add(GetCellValue(sDoc, cell));
                }

                foreach (Row row in rows) //this will also include your header row...
                {
                    DataRow tempRow = dt.NewRow();

                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        Cell cell = row.Descendants<Cell>().ElementAt(i);
                        int actualCellIndex = CellReferenceToIndex(cell);
                        tempRow[actualCellIndex] = GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                    }

                    bool rowPreenchida = false;

                    for (int i = 0; i < tempRow.ItemArray.Length; i++)
                    {
                        if (!tempRow.IsNull(i))
                        {
                            rowPreenchida = true;
                            break;
                        }
                    }

                    if (rowPreenchida)
                        dt.Rows.Add(tempRow);
                }
            }
            dt.Rows.RemoveAt(0);
            return dt;
        }

        private static int CellReferenceToIndex(Cell cell)
        {
            //int index = 0;
            //string reference = cell.CellReference.ToString().ToUpper();
            //foreach (char ch in reference)
            //{
            //    if (Char.IsLetter(ch))
            //    {
            //        int value = (int)ch - (int)'A';
            //        index = (index == 0) ? value : ((index + 1) * 26) + value;
            //    }
            //    else
            //        return index;
            //}
            //return index;

            string columnReference = Regex.Replace(cell.CellReference.ToString().ToUpper(), @"[\d]", string.Empty);

            int columnNumber = -1;
            int mulitplier = 1;

            foreach (char c in columnReference.ToCharArray().Reverse())
            {
                columnNumber += mulitplier * ((int)c - 64);

                mulitplier = mulitplier * 26;
            }
            return columnNumber;
        }

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            try
            {
                SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
                string value = cell?.CellValue?.InnerXml;

                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                }
                else
                {
                    return value;
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public static ExcelServiceLog<T> CastDataTableImportacao<T>(DataTable dt)
            where T : IExcelService
        {
            Dictionary<String, Int32> camposIndices = CarregaCamposIndices(typeof(T), dt);
            Int32 Indice = GetIndiceKey(typeof(T), camposIndices);

            ExcelServiceLog<T> retorno = new ExcelServiceLog<T>
            {
                Items = new List<T>(),
                Log = new List<ExcelServiceLogItem>()
            };

            //Percorre as linhas
            foreach (DataRow row in dt.Rows)
            {
                T obj = (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);
                try
                {
                    List<ExcelServiceLogItem> errosInternos = ValidaProps(typeof(T), row, camposIndices, Indice);

                    if (errosInternos.Count() == 0)
                    {
                        //Percorre as colunas
                        foreach (String propCampo in camposIndices.Keys)
                        {
                            PropertyInfo prop = obj.GetType().GetProperty(propCampo);

                            if (row[camposIndices[propCampo]] == null)
                            {
                                obj.GetType().GetProperty(propCampo).SetValue(obj, null);
                            }
                            else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                            {
                                DateTime? tmp = null;
                                if (!String.IsNullOrEmpty(row[camposIndices[propCampo]].ToString()))
                                {
                                    if (row[camposIndices[propCampo]].ToString().Contains("/"))
                                    {
                                        tmp = Convert.ToDateTime(row[camposIndices[propCampo]]);
                                    }
                                    else
                                    {
                                        tmp = DateTime.FromOADate(Convert.ToDouble(row[camposIndices[propCampo]]));
                                    }
                                }
                                else
                                    tmp = (DateTime?)null;

                                obj.GetType().GetProperty(propCampo).SetValue(obj, tmp);
                            }
                            else if (prop.PropertyType == typeof(Int16) || prop.PropertyType == typeof(Int16?))
                            {
                                if (row[camposIndices[propCampo]] == DBNull.Value) 
                                {
                                    obj.GetType().GetProperty(propCampo).SetValue(obj, (Int16?)null);
                                }
                                else 
                                {
                                    obj.GetType().GetProperty(propCampo).SetValue(obj, Convert.ToInt16(row[camposIndices[propCampo]]));
                                }                                
                            }
                            else if (prop.PropertyType == typeof(Int32) || prop.PropertyType == typeof(Int32?))
                            {
                                if (row[camposIndices[propCampo]] == DBNull.Value)
                                {
                                    obj.GetType().GetProperty(propCampo).SetValue(obj, (Int32?)null);
                                }
                                else
                                {
                                    obj.GetType().GetProperty(propCampo).SetValue(obj, Convert.ToInt32(row[camposIndices[propCampo]]));
                                }
                            }
                            else if (prop.PropertyType == typeof(Int64) || prop.PropertyType == typeof(Int64?))
                            {
                                if (row[camposIndices[propCampo]] == DBNull.Value)
                                {
                                    obj.GetType().GetProperty(propCampo).SetValue(obj, (Int64?)null);
                                }
                                else
                                {
                                    obj.GetType().GetProperty(propCampo).SetValue(obj, Convert.ToInt64(row[camposIndices[propCampo]]));
                                }
                            }
                            else if (prop.PropertyType == typeof(Boolean) || prop.PropertyType == typeof(Boolean?))
                            {
                                obj.GetType().GetProperty(propCampo).SetValue(obj, Convert.ToBoolean(row[camposIndices[propCampo]].ToString() == "1"));
                            }

                            else if (prop.PropertyType == typeof(TimeSpan) || prop.PropertyType == typeof(TimeSpan?))
                            {
                                //obj.GetType().GetProperty(propCampo).SetValue(obj, TimeSpan.Parse(row[camposIndices[propCampo]].ToString().Replace(".", "")) );
                                obj.GetType().GetProperty(propCampo).SetValue(obj, new TimeSpan(Convert.ToInt64(row[camposIndices[propCampo]].ToString().Replace(".", ""))));
                            }
                            else //Outros Tipos
                            {
                                var value = row[camposIndices[propCampo]] == DBNull.Value ? null : row[camposIndices[propCampo]];
                                obj.GetType().GetProperty(propCampo).SetValue(obj, value);
                            }
                        }
                        obj.Importar = true;

                        retorno.Items.Add(obj);
                    }
                    else
                        retorno.Log.AddRange(errosInternos);
                }
                catch (Exception ex)
                {
                    retorno.Log.Add(new ExcelServiceLogItem
                    {
                        Chave = row[Indice].ToString(),
                        Funcionario = row[Indice].ToString(),
                        Erro = ex.Message
                    });
                }

            }

            return retorno;
        }

        public static List<Dictionary<String, Object>> CastDataTableToDictionary(DataTable dt)
        {
            List<Dictionary <String, Object>> retorno = new List<Dictionary<String, Object>>();

            Dictionary<Int32, String> indices = new Dictionary<Int32, string>();

            Int32 col = 0;
            foreach (var i in dt.Columns)
            {
                indices.Add(col, i.ToString().ToUpper());
                col++;
            }

            //Percorre as linhas
            foreach (DataRow row in dt.Rows)
            {
                Dictionary<String, Object> rowDic = new Dictionary<string, object>();
                    
                col = 0;
                foreach (var i in row.ItemArray)
                {
                    rowDic.Add(indices[col], i.ToString());
                    col++;
                }
                retorno.Add(rowDic);

            }

            return retorno;
        }

        private static List<ExcelServiceLogItem> ValidaProps(Type tipo, DataRow row, Dictionary<String, Int32> camposIndices, Int32 Indice)
        {
            List<ExcelServiceLogItem> retorno = new List<ExcelServiceLogItem>();

            foreach (String prop in camposIndices.Keys)
            {
                PropertyInfo property = tipo.GetProperty(prop);
                String headerExcel = ((ExcelServiceAttribute)property.GetCustomAttribute(typeof(ExcelServiceAttribute), false)).Coluna;

                //Requirido
                if ((property.GetCustomAttribute(typeof(RequiredAttribute), false) != null) &&
                     String.IsNullOrEmpty(Convert.ToString(row[camposIndices[prop]])))
                {
                    retorno.Add(new ExcelServiceLogItem
                    {
                        Chave = row[Indice].ToString(),
                        Funcionario = row[Indice].ToString(),
                        Erro = String.Format(RsBase.REQUIRED, headerExcel)
                    });
                    continue;
                }

                //Max Value
                MaxLengthAttribute maxLength = (MaxLengthAttribute)property.GetCustomAttribute(typeof(MaxLengthAttribute), false);
                if ((maxLength != null) && !String.IsNullOrEmpty(Convert.ToString(row[camposIndices[prop]])) &&
                    Convert.ToString(row[camposIndices[prop]]).Length > maxLength.Length)
                {
                    retorno.Add(new ExcelServiceLogItem
                    {
                        Chave = row[Indice].ToString(),
                        Funcionario = row[Indice].ToString(),
                        Erro = String.Format(RsBase.MAXLENGHT, headerExcel, maxLength.Length)
                    });
                    continue;
                }

                //ExcelService
                ExcelServiceValoresAttribute valores = (ExcelServiceValoresAttribute)property.GetCustomAttribute(typeof(ExcelServiceValoresAttribute), false);
                if (valores != null)
                {
                    if (row[camposIndices[prop]] != DBNull.Value && !valores.Valores.Contains(Convert.ToString(row[camposIndices[prop]])))
                    {
                        retorno.Add(new ExcelServiceLogItem
                        {
                            Chave = row[Indice].ToString(),
                            Funcionario = row[Indice].ToString(),
                            Erro = $"{headerExcel} - Valor inválido. Favor informar {String.Join(",", valores.Valores)}"
                        });
                        continue;

                        
                    }
                }

                if (property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Int32?))
                {
                    try
                    {
                        if (property.PropertyType == typeof(Int32))
                        {
                            Int32 Tmp = Convert.ToInt32(row[camposIndices[prop]]);
                        }
                        else
                        {
                            if (row[camposIndices[prop]] != DBNull.Value)
                            {

                                Int32 Tmp = Convert.ToInt32(row[camposIndices[prop]]);

                            }
                        }
                    }
                    catch
                    {
                        retorno.Add(new ExcelServiceLogItem
                        {
                            Chave = row[Indice].ToString(),
                            Funcionario = row[Indice].ToString(),
                            Erro = $"Por favor corrigir o valor inserido no Campo {headerExcel} na planilha importada"
                        });
                    }
                }

                if (property.PropertyType == typeof(Int64) || property.PropertyType == typeof(Int64?))
                {
                    try
                    {
                        if (property.PropertyType == typeof(Int64))
                        {
                            Int64 Tmp = Convert.ToInt64(row[camposIndices[prop]]);
                        }
                        else
                        {
                            if (row[camposIndices[prop]] != DBNull.Value)
                            {
                                Int64 Tmp = Convert.ToInt64(row[camposIndices[prop]]);
                            }
                        }
                    }
                    catch
                    {
                        retorno.Add(new ExcelServiceLogItem
                        {
                            Chave = row[Indice].ToString(),
                            Funcionario = row[Indice].ToString(),
                            Erro = $"Por favor corrigir o valor inserido no Campo {headerExcel} na planilha importada"
                        });
                    }
                }

                if (property.PropertyType == typeof(Int16) || property.PropertyType == typeof(Int16?))
                {
                    try
                    {
                        if (property.PropertyType == typeof(Int16))
                        {
                            Int16 Tmp = Convert.ToInt16(row[camposIndices[prop]]);
                        }
                        else
                        {
                            if (row[camposIndices[prop]] != DBNull.Value)
                            {

                                Int16 Tmp = Convert.ToInt16(row[camposIndices[prop]]);

                            }
                        }
                    }
                    catch
                    {
                        retorno.Add(new ExcelServiceLogItem
                        {
                            Chave = row[Indice].ToString(),
                            Funcionario = row[Indice].ToString(),
                            Erro = $"Por favor corrigir o valor inserido no Campo {headerExcel} na planilha importada"

                        });
                    }
                }

                if (property.PropertyType == typeof(DateTime))
                {
                    try
                    {
                        if (row[camposIndices[prop]].ToString().Contains("/"))
                        {
                            DateTime? tmp = Convert.ToDateTime(row[camposIndices[prop]]);
                        }
                        else
                        {
                            double d = Convert.ToDouble(row[camposIndices[prop]]);
                            DateTime conv = DateTime.FromOADate(d);
                        }
                    }
                    catch
                    {
                        retorno.Add(new ExcelServiceLogItem
                        {
                            Chave = row[Indice].ToString(),
                            Funcionario = row[Indice].ToString(),
                            Erro = $"Data {row[camposIndices[prop]]} Inválida"
                        });
                    }
                }
                else if (property.PropertyType == typeof(DateTime?))
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(row[camposIndices[prop]].ToString()))
                        {
                            if (row[camposIndices[prop]].ToString().Contains("/"))
                            {
                                DateTime? tmp = Convert.ToDateTime(row[camposIndices[prop]]);
                            }
                            else
                            {
                                double d = Convert.ToDouble(row[camposIndices[prop]]);
                                DateTime conv = DateTime.FromOADate(d);
                            }
                        }
                    }
                    catch
                    {
                        retorno.Add(new ExcelServiceLogItem
                        {
                            Chave = row[Indice].ToString(),
                            Funcionario = row[Indice].ToString(),
                            Erro = $"Data {row[camposIndices[prop]]} Inválida"
                        });
                    }
                }

            }

            return retorno;
        }

        public static Dictionary<String, Int32> CarregaCamposIndices(Type tipoObj, DataTable dt)
        {
            Dictionary<String, Int32> retorno = new Dictionary<String, Int32>();
            foreach (var prop in tipoObj.GetProperties())
            {
                var attr = prop.GetCustomAttributes(typeof(ExcelServiceAttribute), false);
                foreach (ExcelServiceAttribute item in attr)
                {
                    retorno.Add(prop.Name, dt.Columns.IndexOf(item.Coluna));
                }
            }
            return retorno;
        }

        public static Int32 GetIndiceKey(Type tipoObj, Dictionary<String, Int32> CamposIndices)
        {
            String coluna = GetCampoChave(tipoObj);
            return CamposIndices[coluna];
        }

        public static String GetCampoChave(Type tipoObj)
        {
            var attrs = tipoObj.GetCustomAttributes(typeof(ExcelServiceKeyAttribute), false);
            if (attrs.Count() == 0)
                throw new Exception("Faltou definir o Attributo ExcelServiceKey na Classe");
            return ((ExcelServiceKeyAttribute)attrs[0]).Coluna;
        }

        public static String GetSheetName(Type tipoObj)
        {
            var attrs = tipoObj.GetCustomAttributes(typeof(ExcelServiceSheetNameAttribute), false);
            if (attrs.Count() == 0)
                return String.Empty;
            return ((ExcelServiceSheetNameAttribute)attrs[0]).Sheet;
        }

        public static string ConvertDataTableToHTMLTable(DataTable dt)
        {
            string ret = "<table id=" + (char)34 + "tblExcel" + (char)34 + ">";
            ret += "<tr>";
            foreach (DataColumn col in dt.Columns)
            {
                ret += "<td class=" + (char)34 + "tdColumnHeader" + (char)34 + ">" + col.ColumnName + "</td>";
            }
            ret += "</tr>";
            foreach (DataRow row in dt.Rows)
            {
                ret += "<tr>";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ret += "<td class=" + (char)34 + "tdCellData" + (char)34 + ">" + row[i].ToString() + "</td>";
                }
                ret += "</tr>";
            }
            ret += "</table>";
            return ret;
        }
    }
}
