using CorpseLib.Translation;

namespace StreamGlass.Core.Actions
{
    public class Action(TranslationKey name, TranslationKey description)
    {
        private readonly TranslationKey m_NameKey = name;
        private readonly TranslationKey m_DescriptionKey = description;

        public string Name => m_NameKey.ToString();
        public string Description => m_DescriptionKey.ToString();
    }
}
