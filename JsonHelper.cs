using Newtonsoft.Json.Linq;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace StreamGlass
{
    public static class JsonHelper
    {
        public static T? Get<T>(JObject obj, string key)
        {
            JToken? token = obj[key];
            if (token != null)
                return token.ToObject<T>();
            return default(T);
        }
        public static string? Get(JObject obj, string key) => (string?)obj[key];

        public static T GetOrDefault<T>(JObject obj, string key, T defaultReturn)
        {
            T? ret = Get<T>(obj, key);
            if (ret != null)
                return ret;
            return defaultReturn;
        }

        public static List<T> GetList<T>(JObject obj, string key)
        {
            List<T> ret = new();
            JArray? arr = (JArray?)obj[key];
            if (arr != null)
            {
                foreach (JToken item in arr.Cast<JToken>())
                {
                    T? itemValue = item.ToObject<T>();
                    if (itemValue != null)
                        ret.Add(itemValue);
                }
            }
            return ret;
        }

        public static JArray ToArray<T>(IEnumerable<T> arr)
        {
            JArray ret = new();
            foreach (var item in arr)
                ret.Add(item);
            return ret;
        }
    }
}
