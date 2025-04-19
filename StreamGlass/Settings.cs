using CorpseLib;
using CorpseLib.DataNotation;
using StreamGlass.Core.Controls;

namespace StreamGlass
{
    public class Settings
    {
        public class ChatSettings
        {
            public class DataSerializer : ADataSerializer<ChatSettings>
            {
                protected override OperationResult<ChatSettings> Deserialize(DataObject reader)
                {
                    ChatSettings chatSettings = new();
                    if (reader.TryGet("display", out ScrollPanelDisplayType display))
                        chatSettings.DisplayType = display;
                    if (reader.TryGet("font", out double font))
                        chatSettings.MessageFontSize = font;
                    return new(chatSettings);
                }

                protected override void Serialize(ChatSettings obj, DataObject writer)
                {
                    writer["display"] = obj.DisplayType;
                    writer["font"] = obj.MessageFontSize;
                }
            }

            public ScrollPanelDisplayType DisplayType = ScrollPanelDisplayType.TOP_TO_BOTTOM;
            public double MessageFontSize = 14;
        }

        public class DataSerializer : ADataSerializer<Settings>
        {
            protected override OperationResult<Settings> Deserialize(DataObject reader)
            {
                Settings settings = new();
                if (reader.TryGet("chat", out ChatSettings? chat) && chat != null)
                    settings.Chat = chat;
                if (reader.TryGet("language", out string? language) && language != null)
                    settings.CurrentLanguage = language;
                return new(settings);
            }

            protected override void Serialize(Settings obj, DataObject writer)
            {
                writer["chat"] = obj.Chat;
                writer["language"] = obj.CurrentLanguage;
            }
        }

        public ChatSettings Chat = new();
        public string CurrentLanguage = string.Empty;
    }
}
