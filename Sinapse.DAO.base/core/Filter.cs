using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Model.core
{
    [Serializable]
    public class Filter
    {
        public Int32 Id { get; set; }

        public String DisplayNomeCampo { get; set; }

        public String NomeCampo { get; set; }

        public SearchType Expressao { get; set; }

        public Dictionary<FilterValueIdentity, Object> Valores;

        public Filter()
        {
            Valores = new Dictionary<FilterValueIdentity, Object>();
        }

        public Object GetBeginWithValue()
        {
            return Valores[FilterValueIdentity.BeginWith];
        }


        public Object GetLowerValue()
        {
            return Valores[FilterValueIdentity.Lower];
        }
        public Object GetHigherValue()
        {
            return Valores[FilterValueIdentity.Higher];
        }
        public Object GetLowerEqualValue()
        {
            return Valores[FilterValueIdentity.LowerEqual];
        }
        public Object GetHigherEqualValue()
        {
            return Valores[FilterValueIdentity.HigherEqual];
        }


        public Object GetUniqueValue()
        {
            if (Expressao == SearchType.Equal)
            {
                return Valores[FilterValueIdentity.Equal];
            }
            else
            {
                return Valores[FilterValueIdentity.NotEqual];
            }
        }

        public Object GetContains()
        {
            return Valores[FilterValueIdentity.Contains];
        }

        public void AddValorEqual(object valor)
        {
            Valores.Add(FilterValueIdentity.Equal, valor);
        }

        public void AddValorContains(object valor)
        {
            Valores.Add(FilterValueIdentity.Contains, valor);
        }

        public void AddValorNotEqual(object valor)
        {
            Valores.Add(FilterValueIdentity.NotEqual, valor);
        }

        public void AddValorBetween(object valorMenor, object valorMaior)
        {
            Valores.Add(FilterValueIdentity.Lower, valorMenor);
            Valores.Add(FilterValueIdentity.Higher, valorMaior);
        }

        public void AddValorBeginWith(object valor)
        {
            Valores.Add(FilterValueIdentity.BeginWith, valor);
        }

        public void AddValorLower(object valor)
        {
            Valores.Add(FilterValueIdentity.Lower, valor);
        }
        public void AddValorLowerEqual(object valor)
        {
            Valores.Add(FilterValueIdentity.LowerEqual, valor);
        }
        public void AddValorHigher(object valor)
        {
            Valores.Add(FilterValueIdentity.Higher, valor);
        }
        public void AddValorHigherEqual(object valor)
        {
            Valores.Add(FilterValueIdentity.HigherEqual, valor);
        }


    }
}