using System;
using System.Collections.Generic;

namespace Sinapse.Util.AspNetCore
{
    public static class DoubleService
    {
        public static Boolean DoubleValid(Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                try
                {
                    Double.Parse(obj.ToString());
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        // O método EscreverExtenso recebe um valor do tipo decimal
        public static string EscreverExtenso(Double valor)
        {
            if (valor <= 0 | valor >= 1000000000000000)
                return "Valor não suportado pelo sistema.";
            else
            {
                string strValor = valor.ToString("000000000000000.00");
                string valor_por_extenso = string.Empty;
                for (int i = 0; i <= 15; i += 3)
                {
                    valor_por_extenso += Escrever_Valor_Extenso(Convert.ToDouble(strValor.Substring(i, 3)));
                    if (i == 0 & valor_por_extenso != string.Empty)
                    {
                        if (Convert.ToInt32(strValor.Substring(0, 3)) == 1)
                            valor_por_extenso += " TRILHÃO" + ((Convert.ToDecimal(strValor.Substring(3, 12)) > 0) ? " E " : string.Empty);
                        else if (Convert.ToInt32(strValor.Substring(0, 3)) > 1)
                            valor_por_extenso += " TRILHÕES" + ((Convert.ToDecimal(strValor.Substring(3, 12)) > 0) ? " E " : string.Empty);
                    }
                    else if (i == 3 & valor_por_extenso != string.Empty)
                    {
                        if (Convert.ToInt32(strValor.Substring(3, 3)) == 1)
                            valor_por_extenso += " BILHÃO" + ((Convert.ToDecimal(strValor.Substring(6, 9)) > 0) ? " E " : string.Empty);
                        else if (Convert.ToInt32(strValor.Substring(3, 3)) > 1)
                            valor_por_extenso += " BILHÕES" + ((Convert.ToDecimal(strValor.Substring(6, 9)) > 0) ? " E " : string.Empty);
                    }
                    else if (i == 6 & valor_por_extenso != string.Empty)
                    {
                        if (Convert.ToInt32(strValor.Substring(6, 3)) == 1)
                            valor_por_extenso += " MILHÃO" + ((Convert.ToDecimal(strValor.Substring(9, 6)) > 0) ? " E " : string.Empty);
                        else if (Convert.ToInt32(strValor.Substring(6, 3)) > 1)
                            valor_por_extenso += " MILHÕES" + ((Convert.ToDecimal(strValor.Substring(9, 6)) > 0) ? " E " : string.Empty);
                    }
                    else if (i == 9 & valor_por_extenso != string.Empty)
                        if (Convert.ToInt32(strValor.Substring(9, 3)) > 0)
                            valor_por_extenso += " MIL" + ((Convert.ToDecimal(strValor.Substring(12, 3)) > 0) ? " E " : string.Empty);
                    if (i == 12)
                    {
                        if (valor_por_extenso.Length > 8)
                            if (valor_por_extenso.Substring(valor_por_extenso.Length - 6, 6) == "BILHÃO" | valor_por_extenso.Substring(valor_por_extenso.Length - 6, 6) == "MILHÃO")
                                valor_por_extenso += " DE";
                            else
                                if (valor_por_extenso.Substring(valor_por_extenso.Length - 7, 7) == "BILHÕES" | valor_por_extenso.Substring(valor_por_extenso.Length - 7, 7) == "MILHÕES" |
                                    valor_por_extenso.Substring(valor_por_extenso.Length - 8, 7) == "TRILHÕES")
                                valor_por_extenso += " DE";
                            else
                                    if (valor_por_extenso.Substring(valor_por_extenso.Length - 8, 8) == "TRILHÕES")
                                valor_por_extenso += " DE";
                        if (Convert.ToInt64(strValor.Substring(0, 15)) == 1)
                            valor_por_extenso += " REAL";
                        else if (Convert.ToInt64(strValor.Substring(0, 15)) > 1)
                            valor_por_extenso += " REAIS";
                        if (Convert.ToInt32(strValor.Substring(16, 2)) > 0 && valor_por_extenso != string.Empty)
                            valor_por_extenso += " E ";
                    }
                    if (i == 15)
                    {
                        if (Convert.ToInt32(strValor.Substring(16, 2)) == 1)
                        {
                            valor_por_extenso += " CENTAVO";
                        }
                        else if (Convert.ToInt32(strValor.Substring(16, 2)) > 1)
                        {
                            valor_por_extenso += " CENTAVOS";
                        }
                    }
                }
                return valor_por_extenso;
            }
        }
        static string Escrever_Valor_Extenso(Double valor)
        {
            if (valor <= 0)
                return string.Empty;
            else
            {
                string montagem = string.Empty;
                if (valor > 0 & valor < 1)
                {
                    valor *= 100;
                }
                string strValor = valor.ToString("000");
                int a = Convert.ToInt32(strValor.Substring(0, 1));
                int b = Convert.ToInt32(strValor.Substring(1, 1));
                int c = Convert.ToInt32(strValor.Substring(2, 1));
                if (a == 1) montagem += (b + c == 0) ? "CEM" : "CENTO";
                else if (a == 2) montagem += "DUZENTOS";
                else if (a == 3) montagem += "TREZENTOS";
                else if (a == 4) montagem += "QUATROCENTOS";
                else if (a == 5) montagem += "QUINHENTOS";
                else if (a == 6) montagem += "SEISCENTOS";
                else if (a == 7) montagem += "SETECENTOS";
                else if (a == 8) montagem += "OITOCENTOS";
                else if (a == 9) montagem += "NOVECENTOS";
                if (b == 1)
                {
                    if (c == 0) montagem += ((a > 0) ? " E " : string.Empty) + "DEZ";
                    else if (c == 1) montagem += ((a > 0) ? " E " : string.Empty) + "ONZE";
                    else if (c == 2) montagem += ((a > 0) ? " E " : string.Empty) + "DOZE";
                    else if (c == 3) montagem += ((a > 0) ? " E " : string.Empty) + "TREZE";
                    else if (c == 4) montagem += ((a > 0) ? " E " : string.Empty) + "QUATORZE";
                    else if (c == 5) montagem += ((a > 0) ? " E " : string.Empty) + "QUINZE";
                    else if (c == 6) montagem += ((a > 0) ? " E " : string.Empty) + "DEZESSEIS";
                    else if (c == 7) montagem += ((a > 0) ? " E " : string.Empty) + "DEZESSETE";
                    else if (c == 8) montagem += ((a > 0) ? " E " : string.Empty) + "DEZOITO";
                    else if (c == 9) montagem += ((a > 0) ? " E " : string.Empty) + "DEZENOVE";
                }
                else if (b == 2) montagem += ((a > 0) ? " E " : string.Empty) + "VINTE";
                else if (b == 3) montagem += ((a > 0) ? " E " : string.Empty) + "TRINTA";
                else if (b == 4) montagem += ((a > 0) ? " E " : string.Empty) + "QUARENTA";
                else if (b == 5) montagem += ((a > 0) ? " E " : string.Empty) + "CINQUENTA";
                else if (b == 6) montagem += ((a > 0) ? " E " : string.Empty) + "SESSENTA";
                else if (b == 7) montagem += ((a > 0) ? " E " : string.Empty) + "SETENTA";
                else if (b == 8) montagem += ((a > 0) ? " E " : string.Empty) + "OITENTA";
                else if (b == 9) montagem += ((a > 0) ? " E " : string.Empty) + "NOVENTA";
                if (strValor.Substring(1, 1) != "1" & c != 0 & montagem != string.Empty) montagem += " E ";
                if (strValor.Substring(1, 1) != "1")
                    if (c == 1) montagem += "UM";
                    else if (c == 2) montagem += "DOIS";
                    else if (c == 3) montagem += "TRÊS";
                    else if (c == 4) montagem += "QUATRO";
                    else if (c == 5) montagem += "CINCO";
                    else if (c == 6) montagem += "SEIS";
                    else if (c == 7) montagem += "SETE";
                    else if (c == 8) montagem += "OITO";
                    else if (c == 9) montagem += "NOVE";
                return montagem;
            }
        }

        public static string DecimalToExtenso(Decimal valor)
        {
            string[] struni = new string[] { "", "Um", "Dois", "Tres", "Quatro", "Cinco", "Seis", "Sete", "Oito", "Nove", "Dez", "Onze", "Doze", "Treze", "Quatorze", "Quinze", "Dezessis", "Dezessete", "Dezoito", "Dezenove", "Vinte" };
            string[] strdez = new string[] { "", "", "Vinte", "Trinta", "Quarenta", "Cinquenta", "Sessenta", "Setenta", "Oitenta", "Noventa" };
            List<string[]> strcen = new List<string[]>() { new string[] { "", "" }, new string[] { "Cem", "Cento" }, new string[] { "Duzentos", "Duzentos" }, new string[] { "Trezentos", "Trezentos" }, new string[] { "Quatrocentos", "Quatrocentos" }, new string[] { "Quinhentos", "Quinhentos" }, new string[] { "Seiscentos", "Seiscentos" }, new string[] { "Setecentos", "Setecentos" }, new string[] { "Oitocentos", "Oitocentos" }, new string[] { "Novecentos", "Novecentos" } };
            List<string[]> moeda = new List<string[]>() { new string[] { " Trilhao", " Trilhoes" }, new string[] { " Bilhao", " Bilhoes" }, new string[] { " Milhao", " Milhoes" }, new string[] { " Mil", " Mil" }, new string[] { " Real", " Reais" }, new string[] { " Centavo", " Centavos" }, };
            List<string[]> result = new List<string[]>();
            string[] arrValor = Decimal.Round(valor, 2).ToString("0|0|0,0|0|0,0|0|0,0|0|0,0|0|0.0|0").Replace(",", ",0|").Replace(",", ".").Split('.');
            for (int i = arrValor.Length - 1; i >= 0; i--)
            {
                string[] z = arrValor[i].Split('|');
                int a = Convert.ToInt32(z[0]);
                int b = Convert.ToInt32(z[1]);
                int c = Convert.ToInt32(z[2]);
                int d = Convert.ToInt32(b.ToString() + c.ToString());
                int k = Convert.ToInt32(a.ToString() + b.ToString() + c.ToString());
                string r = (d >= 1 && d <= 20) ? string.Format("{0}", k == 0 ? "" : struni[d]) : string.Format("{0}{1}{2}", strdez[b], c > 0 ? " e " : "", k == 0 ? "" : struni[c]);
                r = k < 100 ? r : string.Format("{0}{1}{2}", strcen[a][d == 0 ? 0 : 1], d == 0 ? "" : " e ", r);
                result.Add(new string[] { i.ToString(), k.ToString(), r, " e ", moeda[i][k == 1 ? 0 : 1] });
            }
            if (Convert.ToInt32(result[1][1]) == 0)
            {
                string xmoeda = result[1][4];
                for (int i = 2; i <= result.Count - 1; i++)
                {
                    if (Convert.ToInt32(result[i][1]) > 0)
                    {
                        result[i][4] += " " + (i == 3 || i == 4 || i == 5 ? " de " : "") + xmoeda;
                        result[i][3] = " e ";
                        break;
                    }
                }
            }
            for (int i = result.Count - 1; i >= 0; i--) { if (Convert.ToInt32(result[i][1]) == 0) result.Remove(result[i]); }
            result[0][3] = "";
            for (int i = 2; i <= result.Count - 1; i++) { result[i][3] = ", "; }
            string extenso = "";
            for (int i = 0; i <= result.Count - 1; i++) { extenso = result[i][2] + result[i][4] + result[i][3] + extenso; }
            return extenso.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
        }

    }
}
