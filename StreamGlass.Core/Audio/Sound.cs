using CorpseLib;
using CorpseLib.DataNotation;

namespace StreamGlass.Core.Audio
{
    public class Sound
    {
        public class DataSerializer : ADataSerializer<Sound>
        {
            protected override OperationResult<Sound> Deserialize(DataObject reader)
            {
                if (reader.TryGet("file", out string? file) &&
                    reader.TryGet("output", out string? output))
                    return new(new(file!, output!, reader.GetOrDefault("cooldown", TimeSpan.Zero)));
                return new("Deserialization error", "Cannot deserialize Sound");
            }

            protected override void Serialize(Sound obj, DataObject writer)
            {
                writer["file"] = obj.m_File;
                writer["output"] = obj.m_Output;
                writer["cooldown"] = obj.m_Cooldown;
            }
        }

        private readonly string m_File;
        private readonly string m_Output;
        private readonly TimeSpan m_Cooldown;

        public string File => m_File;
        public string Output => m_Output;
        public TimeSpan Cooldown => m_Cooldown;

        public Sound(string file, string output, TimeSpan cooldown)
        {
            m_File = file;
            m_Output = output;
            m_Cooldown = cooldown;
            if (!string.IsNullOrEmpty(m_File))
                SoundManager.LoadSound(m_File);
        }

        ~Sound()
        {
            if (!string.IsNullOrEmpty(m_File))
                SoundManager.UnloadSound(m_File);
        }
    }
}
