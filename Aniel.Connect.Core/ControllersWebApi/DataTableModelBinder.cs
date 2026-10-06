using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Aniel.Connect.Core.ControllersWebApi
{
    public class DataTableModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(DataTableRequest))
            {
                return false;
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
            var o = 0;
            var order = new List<DataTableOrder>();
            while (GetValue(bindingContext, "order[" + o + "].column") != null)
            {
                order.Add(new DataTableOrder
                {
                    column = Convert.ToInt32(GetValue(bindingContext, "order[" + o + "].column")),
                    dir = GetValue(bindingContext, "order[" + o + "].dir")
                });
                o++;
            }
            model.order = order.ToArray();

            // Columns
            var c = 0;
            var columns = new List<DataTableColumn>();
            while (GetValue(bindingContext, "columns[" + c + "].data") != null)
            {
                columns.Add(new DataTableColumn
                {
                    data = GetValue(bindingContext, "columns[" + c + "].data"),
                    name = GetValue(bindingContext, "columns[" + c + "].name"),
                    orderable = Convert.ToBoolean(GetValue(bindingContext, "columns[" + c + "].orderable")),
                    searchable = Convert.ToBoolean(GetValue(bindingContext, "columns[" + c + "].searchable")),
                    search = new DataTableSearch
                    {
                        value = GetValue(bindingContext, "columns[" + c + "][search].value"),
                        regex = Convert.ToBoolean(GetValue(bindingContext, "columns[" + c + "].search.regex"))
                    }
                });
                c++;
            }
            model.columns = columns.ToArray();

            bindingContext.Model = model;

            return true;
        }

        private string GetValue(ModelBindingContext context, string key)
        {
            var result = context.ValueProvider.GetValue(key);
            return result == null ? null : result.AttemptedValue;
        }
    }
}
