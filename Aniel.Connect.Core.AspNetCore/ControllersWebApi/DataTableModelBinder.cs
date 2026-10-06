using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.AspNetCore.ControllersWebApi
{
    public class DataTableModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(DataTableRequest))
            {
                return Task.CompletedTask;
            }

            var model = (DataTableRequest)bindingContext.Model ?? new DataTableRequest();

            model.draw = Convert.ToInt32(GetValue(bindingContext, "draw"));
            model.start = Convert.ToInt32(GetValue(bindingContext, "start"));
            model.length = Convert.ToInt32(GetValue(bindingContext, "length"));

            // Search
            model.search = new DataTableSearch
            {
                value = GetValue(bindingContext, "search.value"),
                regex = Convert.ToBoolean(GetValue(bindingContext, "search.regex"))
            };

            // Order
            var orderIndex = 0;
            var order = new List<DataTableOrder>();

            while (GetValue(bindingContext, "order[" + orderIndex + "].column") != null)
            {
                order.Add(new DataTableOrder
                {
                    column = Convert.ToInt32(GetValue(bindingContext, "order[" + orderIndex + "].column")),
                    dir = GetValue(bindingContext, "order[" + orderIndex + "].dir")
                });
                orderIndex++;
            }
            model.order = order.ToArray();

            // Columns
            var columnIndex = 0;
            var columns = new List<DataTableColumn>();

            while (GetValue(bindingContext, "columns[" + columnIndex + "].data") != null)
            {
                columns.Add(new DataTableColumn
                {
                    data = GetValue(bindingContext, "columns[" + columnIndex + "].data"),
                    name = GetValue(bindingContext, "columns[" + columnIndex + "].name"),
                    orderable = Convert.ToBoolean(GetValue(bindingContext, "columns[" + columnIndex + "].orderable")),
                    searchable = Convert.ToBoolean(GetValue(bindingContext, "columns[" + columnIndex + "].searchable")),
                    search = new DataTableSearch
                    {
                        value = GetValue(bindingContext, "columns[" + columnIndex + "][search].value"),
                        regex = Convert.ToBoolean(GetValue(bindingContext, "columns[" + columnIndex + "].search.regex"))
                    }
                });
                columnIndex++;
            }
            model.columns = columns.ToArray();

            bindingContext.Model = model;

            return Task.CompletedTask;
        }

        private string GetValue(ModelBindingContext context, string key)
        {
            var result = context.ValueProvider.GetValue(key);
            return result == null ? null : result.FirstValue;
        }
    }
}
