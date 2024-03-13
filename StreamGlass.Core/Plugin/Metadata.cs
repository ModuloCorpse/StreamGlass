using System.IO;

namespace StreamGlass.Core.Plugin
{
    public class Metadata(Type? type, string directory, string filename, DateTime lastModified)
    {
        private readonly Type? m_Type = type;
        private readonly DateTime m_LastModified = lastModified;
        private readonly string m_Directory = directory;
        private readonly string m_Filename = filename;
        private readonly bool m_Native = false;

        public Type? Type => m_Type;
        public DateTime LastModified => m_LastModified;
        public string Directory => m_Directory;
        public string Filename => m_Filename;
        public bool Native => m_Native;

        public Metadata(Type? type, string directory, string filename, DateTime lastModified, bool native)
            : this(type, directory, filename, lastModified) => m_Native = native;

        public static Metadata CreateNativeMetadata<T>(string filename) where T : APlugin => new(typeof(T), Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" ?? string.Empty, filename, DateTime.Now, true);
    }
}
