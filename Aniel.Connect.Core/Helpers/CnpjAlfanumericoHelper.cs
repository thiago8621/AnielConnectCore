using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace SeuProjeto.Core.Helpers
{
    /// <summary>
    /// Helper para validação e formatação de CNPJ alfanumérico.
    /// Nota Técnica Conjunta nº 2025.001 da Receita Federal.
    ///
    /// USO — Validação direta:
    ///   CnpjHelper.Validar(valor)
    ///
    /// USO — Model:
    ///   [CnpjValido]
    ///   public string CNPJ { get; set; }
    /// </summary>
    public static class CnpjHelper
    {
        /// <summary>
        /// Remove máscara e força maiúsculas. Retorna somente A-Z e 0-9.
        /// </summary>
        public static string Limpar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
            return Regex.Replace(valor.ToUpperInvariant(), @"[^A-Z0-9]", "");
        }

        /// <summary>
        /// Valida matematicamente um CNPJ numérico (padrão antigo) ou alfanumérico (novo padrão).
        /// </summary>
        public static bool Validar(string valor)
        {
            var cnpj = Limpar(valor);

            if (cnpj.Length != 14) return false;
            if (!cnpj.Substring(12).All(char.IsDigit)) return false;
            if (cnpj.Distinct().Count() == 1) return false;

            var base12 = cnpj.Substring(0, 12);
            var primeiroDigito = calcularDV(base12, 5);
            var segundoDigito = calcularDV(base12 + primeiroDigito, 6);

            return cnpj.Substring(12) == $"{primeiroDigito}{segundoDigito}";
        }

        /// <summary>
        /// Formata para exibição: AA.AAA.AAA/AAAA-00.
        /// Retorna o valor original se não tiver 14 caracteres limpos.
        /// </summary>
        public static string Formatar(string valor)
        {
            var c = Limpar(valor);
            if (c.Length != 14) return valor;

            return c.Substring(0, 2) + "." +
                   c.Substring(2, 3) + "." +
                   c.Substring(5, 3) + "/" +
                   c.Substring(8, 4) + "-" +
                   c.Substring(12, 2);
        }

        private static int calcularDV(string digitos, int pesoInicial)
        {
            int peso = pesoInicial, soma = 0;
            foreach (char c in digitos)
            {
                soma += (c - 48) * peso--;
                if (peso < 2) peso = 9;
            }
            int resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
    }

    /// <summary>
    /// DataAnnotation para validação de CNPJ em Models.
    /// Usa CnpjHelper.Validar() internamente.
    /// </summary>
    public class CnpjValidoAttribute : ValidationAttribute
    {
        public CnpjValidoAttribute()
        {
            ErrorMessage = "CNPJ inválido. Verifique o CNPJ informado.";
        }

        public override bool IsValid(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString())) return true;
            return CnpjHelper.Validar(value.ToString());
        }
    }
}