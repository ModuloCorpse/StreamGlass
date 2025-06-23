using CorpseLib;
using CorpseLib.DataNotation;
using StreamGlass.Core.Audio;

namespace StreamGlass.Twitch.Alerts
{
    public class AlertSettings(Sound? audio, string imgPath, string prefix, string chatMessage, bool isEnabled, bool haveChatMessage)
    {
        public class DataSerializer : ADataSerializer<AlertSettings>
        {
            protected override OperationResult<AlertSettings> Deserialize(DataObject reader)
            {
                if (
                    reader.TryGet("img", out string? imgPath) &&
                    reader.TryGet("prefix", out string? prefix) &&
                    reader.TryGet("msg", out string? chatMessage) &&
                    reader.TryGet("enable", out bool? isEnabled) &&
                    reader.TryGet("have_msg", out bool? haveChatMessage))
                {
                    reader.TryGet("sound", out Sound? sound);
                    return new(new(sound, imgPath!, prefix!, chatMessage!, isEnabled == true, haveChatMessage == true));
                }
                return new("Deserialization error", "Cannot deserialize AlertSettings");
            }

            protected override void Serialize(AlertSettings obj, DataObject writer)
            {
                if (obj.m_Audio != null)
                    writer["sound"] = obj.m_Audio;
                writer["img"] = obj.m_ImgPath;
                writer["prefix"] = obj.m_Prefix;
                writer["msg"] = obj.m_ChatMessage;
                writer["enable"] = obj.m_IsEnabled;
                writer["have_msg"] = obj.m_HaveChatMessage;
            }
        }

        private readonly Sound? m_Audio = audio;
        private readonly string m_ImgPath = imgPath;
        private readonly string m_Prefix = prefix;
        private readonly string m_ChatMessage = chatMessage;
        private readonly bool m_IsEnabled = isEnabled;
        private readonly bool m_HaveChatMessage = haveChatMessage;

        public Sound? Audio => m_Audio;
        public string ImgPath => m_ImgPath;
        public string Prefix => m_Prefix;
        public string ChatMessage => m_ChatMessage;
        public bool IsEnabled => m_IsEnabled;
        public bool HaveChatMessage => m_HaveChatMessage;

        public AlertSettings Duplicate() => new((m_Audio == null) ? null : new(m_Audio.File, m_Audio.Output, m_Audio.Cooldown), m_ImgPath, m_Prefix, m_ChatMessage, m_IsEnabled, m_HaveChatMessage);
    }
}
