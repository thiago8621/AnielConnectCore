using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Model.AspNetCore.core
{
    [Serializable]
    public class Filters
    {
        private List<Filter> FilterList = null;

        private List<OrderBy> OrderList = null;

        private List<FilterFetchMode> FetchModeList = null;

        private Int16 FirstResult = 0;

        private Int16 MaxResults = 0;

        public Filters()
        {
            FilterList = new List<Filter>();
            OrderList = new List<OrderBy>();
            FetchModeList = new List<FilterFetchMode>();
        }

        #region Filter
        public Filters AddFilter(String fieldName, SearchType expression, params Object[] values)
        {
            return AddFilter(fieldName, fieldName, expression, values);
        }

        public Filters SetFetchMode(String fieldName, SelectMode fetchMode)
        {
            FetchModeList.Add(new FilterFetchMode() { FieldName = fieldName, FetchModeField = fetchMode });

            return this;
        }


        public void AlterFilter(Int64 idFiltroOld, String displayName, String fieldName, SearchType expression, params Object[] values)
        {
            Filter filtroOld = FilterList.Find(f => f.Id == idFiltroOld);

            Filter filtroNew = GetNewFilter(filtroOld.Id, displayName, fieldName, expression, values);

            FilterList[FilterList.FindIndex(f => f.Id == idFiltroOld)] = filtroNew;
        }

        public Filters AddFilter(String displayName, String fieldName, SearchType expression, params Object[] values)
        {
            Filter filtro = GetNewFilter(
                                        FilterList.Count == 0 ? 1 : FilterList.Max(m => m.Id) + 1,
                                        displayName,
                                        fieldName,
                                        expression,
                                        values
                                        );

            FilterList.Add(filtro);

            return this;
        }

        public Filters AddFilter(String fieldName, SearchType expression, Object lowerValue, Object higherValue)
        {
            AddFilter(fieldName, fieldName, expression, lowerValue, higherValue);

            return this;
        }

        public Filters AddFilter(String displayName, String fieldName, SearchType expression, Object lowerValue, Object higherValue)
        {
            Filter filtro = GetNewFilter(
                                        FilterList.Count == 0 ? 1 : FilterList.Max(m => m.Id) + 1,
                                        displayName,
                                        fieldName,
                                        expression,
                                        lowerValue,
                                        higherValue
                                        );

            filtro.AddValorBetween(lowerValue, higherValue);

            FilterList.Add(filtro);

            return this;
        }

        private static Filter GetNewFilter(Int32 id, String displayName, String fieldName, SearchType expression, params Object[] values)
        {
            Filter filtro = new Filter
            {
                Id = id,
                DisplayNomeCampo = displayName,
                NomeCampo = fieldName,
                Expressao = expression
            };

            if (values == null)
            {
                switch (expression)
                {
                    case SearchType.Equal: filtro.AddValorEqual(null);
                        break;
                    case SearchType.NotEqual: filtro.AddValorNotEqual(null);
                        break;
                }
            }
            else
            {
                if (values.Length > 1 && expression == SearchType.Between)
                {
                    filtro.AddValorLower(values[0]);
                    filtro.AddValorHigher(values[1]);
                }
                else
                {
                    foreach (object obj in values)
                    {
                        switch (expression)
                        {
                            case SearchType.Equal: filtro.AddValorEqual(obj);
                                break;
                            case SearchType.NotEqual: filtro.AddValorNotEqual(obj);
                                break;
                            case SearchType.BeginWith: filtro.AddValorBeginWith(obj);
                                break;
                            case SearchType.Contains: filtro.AddValorContains(obj);
                                break;
                            case SearchType.Lower: filtro.AddValorLower(obj);
                                break;
                            case SearchType.LowerEqual: filtro.AddValorLowerEqual(obj);
                                break;
                            case SearchType.Higher: filtro.AddValorHigher(obj);
                                break;
                            case SearchType.HigherEqual: filtro.AddValorHigherEqual(obj);
                                break;

                        }
                    }
                }
            }
            return filtro;
        }

        private static Filter GetNewFilter(Int32 id, String displayName, String fieldName, SearchType expression, Object lowerValue, Object higherValue)
        {
            Filter filtro = new Filter
            {
                Id = id,
                DisplayNomeCampo = displayName,
                NomeCampo = fieldName,
                Expressao = expression
            };

            return filtro;
        }


        public void RemoveFilter(String fieldName)
        {
            Filter filtro = FilterList.Find(f => f.NomeCampo == fieldName);
            if (filtro != null)
                FilterList.Remove(filtro);
        }

        public void RemoveFilter(Int32 pId)
        {
            Filter filtro = FilterList.Find(f => f.Id == pId);
            if (filtro != null)
                FilterList.Remove(filtro);
        }

        public Boolean FilterExists(String fieldName)
        {
            Filter filtro = FilterList.Find(f => f.NomeCampo == fieldName);
            return filtro != null;
        }

        public List<Filter> GetFilterList()
        {
            return FilterList;
        }

        public List<FilterFetchMode> GetFetchMode()
        {
            return FetchModeList;
        }

        public void SetFilterList(List<Filter> lista)
        {
            FilterList = lista;
        }

        public Filter GetFilter(Int32 pId)
        {
            return FilterList.Find(f => f.Id == pId);
        }

        public Filters SetFirstResult(Int16 qtde)
        {
            FirstResult = qtde;

            return this;
        }

        public Filters SetMaxResults(Int16 pageSize)
        {
            MaxResults = pageSize;

            return this;
        }

        public Int16 GetFirstResult()
        {
            return FirstResult;
        }

        public Int16 GetMaxResults()
        {
            return MaxResults;
        }

        #endregion

        #region Order
        public Filters AddOrder(String nomeCampo, OrderByIdentity direction)
        {
            OrderBy order = new OrderBy()
            {
                Id = OrderList.Count == 0 ? 1 : OrderList.Max(m => m.Id) + 1,
                NomeCampo = nomeCampo,
                Direction = direction
            };

            OrderList.Add(order);

            return this;
        }

        public List<OrderBy> GetOrderList()
        {
            return OrderList;
        }
        #endregion

        public void Clear()
        {
            FilterList.Clear();
            OrderList.Clear();
        }




    }
}