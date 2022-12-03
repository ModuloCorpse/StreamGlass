using System;
using System.Globalization;

namespace StreamGlass
{
    public class TranslationManager : ManagedObject.Manager<Translation>
    {
        public TranslationManager() : base("./locals") { }

        public Translation NewDefaultTranslation(CultureInfo cultureInfo)
        {
            Translation translation = new(cultureInfo, true);
            AddObject(translation);
            SetCurrentTranslation(cultureInfo.Name);
            return translation;
        }

        public Translation NewTranslation(CultureInfo cultureInfo)
        {
            Translation translation = new(cultureInfo, true);
            AddObject(translation);
            return translation;
        }

        public bool TryGetTranslation(string key, string[] parameters, out string translation)
        {
            if (CurrentObject != null)
                return CurrentObject.TryGetTranslation(key, parameters, out translation);
            translation = "";
            return false;
        }

        public bool TryGetTranslation(string key, out string translation)
        {
            if (CurrentObject != null)
                return CurrentObject.TryGetTranslation(key, Array.Empty<string>(), out translation);
            translation = "";
            return false;
        }

        public void SetCurrentTranslation(string cultureInfo) => SetCurrentObject(cultureInfo);

        protected override Translation? DeserializeObject(Json obj) => new(obj);
    }
}
