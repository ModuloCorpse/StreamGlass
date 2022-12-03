using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StreamGlass
{
    public class Json : IEnumerable<KeyValuePair<string, JToken>> //Allow to iterate like it's a JObject, but not a fan of iterating on JToken
    {
        private readonly JObject m_Json;

        public static Json LoadFromFile(string path) => new(File.ReadAllText(path));

        public Json() => m_Json = new();
        public Json(string content) => m_Json = JObject.Parse(content);
        private Json(JObject json) => m_Json = json;

        private static T Cast<T>(JToken token)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)token.ToString();
            else if (typeof(T) == typeof(Json))
                return (T)(object)new Json(token.ToObject<JObject>()!);
            else if (typeof(T).IsEnum)
                return (T)(object)token.ToObject<int>();
            else
                return token.ToObject<T>()!;
        }

        public T? Get<T>(string key)
        {
            JToken? token = m_Json[key];
            if (token != null)
                return Cast<T>(token);
            return default;
        }

        public Dictionary<string, T> ToDictionary<T>()
        {
            Dictionary<string, T> ret = new();
            foreach (var pair in m_Json)
            {
                if (pair.Value != null)
                    ret[pair.Key] = Cast<T>(pair.Value);
            }
            return ret;
        }

        public T GetOrDefault<T>(string key, T defaultReturn)
        {
            T? ret = Get<T>(key);
            if (ret != null)
                return ret;
            return defaultReturn;
        }

        public bool TryGet<T>(string key, out T? ret)
        {
            T? tmp = Get<T>(key);
            if (tmp != null)
            {
                ret = tmp;
                return true;
            }
            ret = default;
            return false;
        }

        public List<T> GetList<T>(string key)
        {
            List<T> ret = new();
            JArray? arr = (JArray?)m_Json[key];
            if (arr != null)
            {
                foreach (JToken item in arr.Cast<JToken>())
                    ret.Add(Cast<T>(item));
            }
            return ret;
        }

        private static JToken Uncast<T>(T item)
        {
            if (item is Json json)
                return json.m_Json;
            else if (typeof(T).IsEnum)
                return JToken.FromObject((int)(object)item!);
            else
                return JToken.FromObject(item!);
        }

        private static JArray ToArray<T>(IEnumerable<T> arr)
        {
            JArray ret = new();
            foreach (var item in arr)
                ret.Add(Uncast(item));
            return ret;
        }

        public void Set<T>(string key, T obj) => m_Json[key] = Uncast(obj);
        public void Set<T>(string key, List<T> obj) => m_Json[key] = ToArray(obj);
        public void Set<T>(Dictionary<string, T> obj)
        {
            foreach (var pair in obj)
                m_Json[pair.Key] = Uncast(pair.Value);
        }

        public override string ToString() => m_Json.ToString();
        public string ToNetworkString() => m_Json.ToString(Newtonsoft.Json.Formatting.None);
        public void WriteToFile(string path) => File.WriteAllText(path, ToString());

        public IEnumerator<KeyValuePair<string, JToken>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, JToken>>)m_Json).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Json).GetEnumerator();
    }
}
