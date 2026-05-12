using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace APPLICATION_MSSQL_API.Common.Extensions
{
    public static class Extension
    {
        public static T ConvertToModel<T>(this object dt)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
            };
            var strDt = JsonConvert.SerializeObject(dt);
            return JsonConvert.DeserializeObject<T>(strDt, settings);
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }


    }

    public static class IListExtensions
    {
        public static bool IsEmpty(this IList iEnumerable)
        {
            if (iEnumerable == null)
            {
                return true;
            }
            else
            {
                return (iEnumerable.Count > 0) ? false : true;
            }
        }
    }
}