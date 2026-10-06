using System;
using System.Collections.Generic;

namespace Sinapse.JSON.AspNetCore
{
    public class ObjectToJQueryList
    {
        List<JsonMapperJQuery> jQueryList = new List<JsonMapperJQuery>();

        public List<JsonMapperJQuery> GetJSonList()
        {
            return jQueryList;
        }

        public void AddItemList(Int64 idObject, String value)
        {
            jQueryList.Add(new JsonMapperJQuery()
                {
                    id = Convert.ToString(idObject),
                    label = value,
                    value = value
                });
        }

        public void AddItemList(String idObject, String value)
        {
            jQueryList.Add(new JsonMapperJQuery()
            {
                id = idObject,
                label = value,
                value = value
            });
        }

        public void AddItemList(Int64 idObject, String label, String value)
        {
            jQueryList.Add(new JsonMapperJQuery()
            {
                id = Convert.ToString(idObject),
                label = label,
                value = value
            });
        }

        public void AddItemList(String idObject, String label, String value)
        {
            jQueryList.Add(new JsonMapperJQuery()
            {
                id = idObject,
                label = label,
                value = value
            });
        }


    }
}
