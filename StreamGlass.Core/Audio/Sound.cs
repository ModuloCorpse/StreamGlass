using CorpseLib;
using CorpseLib.Json;

namespace StreamGlass.Core.Audio
{
    public class Sound
    {
        public class JsonSerializer : AJsonSerializer<Sound>
        {
            protected override OperationResult<Sound> Deserialize(JsonObject reader)
            {
                if (reader.TryGet("file", out string? file) &&
                    reader.TryGet("output", out string? output))
                    return new(new(file!, output!));
                return new("Deserialization error", "Cannot deserialize Sound");
            }

            protected override void Serialize(Sound obj, JsonObject writer)
            {
                writer["file"] = obj.m_File;
                writer["output"] = obj.m_Output;
            }
        }

        private readonly string m_File;
        private readonly string m_Output;

        public string File => m_File;
        public string Output => m_Output;

        public Sound(string file, string output)
        {
            m_File = file;
            m_Output = output;
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
