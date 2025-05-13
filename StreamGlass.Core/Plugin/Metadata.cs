using System.IO;

namespace StreamGlass.Core.Plugin
{
    public class Metadata
    {
        private readonly Type? m_Type;
        private readonly DateTime m_LastModified;
        private readonly string m_Directory;
        private readonly string m_Namespace;
        private readonly string m_Filename;
        private readonly bool m_Native;
        private bool m_Enable;

        public Type? Type => m_Type;
        public DateTime LastModified => m_LastModified;
        public string Directory => m_Directory;
        public string Namespace => m_Namespace;
        public string Filename => m_Filename;
        public bool Native => m_Native;
        public bool Enable => m_Enable;

        private Metadata(Type? type, string directory, string @namespace, string filename, DateTime lastModified, bool native)
        {
            m_Type = type;
            m_LastModified = lastModified;
            m_Directory = directory;
            m_Namespace = @namespace;
            m_Filename = filename;
            m_Native = native;
            m_Enable = true;
        }

        public Metadata(Type? type, string directory, string filename, DateTime lastModified) : this(type, directory, Path.GetFileName((directory[^1] == '/') ? directory[..^1] : directory), filename, lastModified, false) { }

        public void SetEnable(bool enable) => m_Enable = enable;

        public static Metadata CreateNativeMetadata<T>(string @namespace, string filename) where T : APlugin => new(typeof(T), Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" ?? string.Empty, @namespace, filename, DateTime.Now, true);
    }
}
