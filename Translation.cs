using System.Collections.Generic;
using System.Globalization;

namespace StreamGlass
{
    public class Translation : ManagedObject.Object<Translation>
    {
        private readonly Dictionary<string, string> m_Translations = new();

        public Translation(CultureInfo cultureInfo, bool isSerializable) : base(cultureInfo.Name, cultureInfo.TextInfo.ToTitleCase(cultureInfo.NativeName), isSerializable) { }
        internal Translation(Json json) : base(json) { }

        public void AddTranslation(string key, string translation) => m_Translations[key] = translation;

        public bool TryGetTranslation(string key, string[] parameters, out string translation)
        {
            if (m_Translations.TryGetValue(key, out var ret))
            {
                for (int i = 0; i < parameters.Length; ++i)
                    ret = ret.Replace(string.Format("${0}", i + 1), parameters[i]);
                translation = ret;
                return true;
            }
            else if (Parent != null)
                return Parent.TryGetTranslation(key, parameters, out translation);
            translation = "";
            return false;
        }

        protected override void Save(ref Json json)
        {
            Json obj = new();
            obj.Set(m_Translations);
            json.Set("translations", obj);
        }

        protected override void Load(Json json)
        {
            if (json.TryGet("translations", out Json? translations))
            {
                foreach (var translation in translations!.ToDictionary<string>())
                    m_Translations[translation.Key] = translation.Value;
            }
        }
    }
}
