using CorpseLib.DataNotation;
using System.IO;

namespace StreamGlass.Core.Stat
{
    public class StringSourceFile : StringSourceAggregator
    {
        private string m_Path = string.Empty;

        internal string Path => m_Path;

        internal void SetPath(string path) => m_Path = path;

        internal void Duplicate(StringSourceFile other)
        {
            Copy(other);
            m_Path = other.m_Path;
        }

        protected override void OnSave(DataObject json)
        {
            json["path"] = m_Path;
        }

        protected override void OnLoad(DataObject json)
        {
            if (json.TryGet("path", out string? path))
                m_Path = path!;
        }

        protected override string GetAggregatorType() => "file";
        protected override void OnAggregate(string text) => File.WriteAllText(Path, text);
    }
}
