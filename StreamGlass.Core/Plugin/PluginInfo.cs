using Version = CorpseLib.Version;

namespace StreamGlass.Core.Plugin
{
    public class PluginInfo(Version version, string author)
    {
        private readonly Version m_Version = version;
        private readonly string m_Author = author;

        public Version Version => m_Version;
        public string Author => m_Author;

        public PluginInfo(string version, string author) : this(new Version(version), author) { }
    }
}
