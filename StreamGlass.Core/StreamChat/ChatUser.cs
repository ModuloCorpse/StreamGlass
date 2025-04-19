using CorpseLib.DataNotation;
using CorpseLib;
using System.Windows.Media;

namespace StreamGlass.Core.StreamChat
{
    public class ChatUser
    {
        public class DataSerializer : ADataSerializer<ChatUser>
        {
            protected override OperationResult<ChatUser> Deserialize(DataObject reader)
            {
                if (reader.TryGet("chat_id", out string? chatID) &&
                    reader.TryGet("id", out string? id) &&
                    reader.TryGet("name", out string? name) &&
                    reader.TryGet("user_type", out uint? userType))
                    return new(new(chatID!, id!, name!, (uint)userType!, reader.GetOrDefault<Color?>("color", null)));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(ChatUser obj, DataObject writer)
            {
                writer["chat_id"] = obj.m_ChatID;
                writer["id"] = obj.m_ID;
                writer["name"] = obj.m_Name;
                writer["user_type"] = obj.m_UserType;
                if (obj.m_Color != null)
                    writer["color"] = obj.m_Color;
            }
        }

        private Color? m_Color;
        private readonly string m_ChatID;
        private readonly string m_ID;
        private string m_Name;
        private uint m_UserType;

        public string ChatID => m_ChatID;
        public string ID => m_ID;
        public string Name => m_Name;
        public Color? Color => m_Color;
        public uint UserType => m_UserType;

        internal ChatUser(string chatID, string id, string name, uint userType, Color? color = null)
        {
            m_ChatID = chatID;
            m_ID = id;
            m_Name = name;
            m_UserType = userType;
            m_Color = color;
        }

        public void SetName(string name) => m_Name = name;
        public void SetColor(Color? color) => m_Color = color;
        public void SetUserType(uint userType) => m_UserType = userType;
    }
}
