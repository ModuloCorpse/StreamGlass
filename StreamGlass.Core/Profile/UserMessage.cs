using CorpseLib;
using CorpseLib.DataNotation;

namespace StreamGlass.Core.Profile
{
    public class UserMessage(uint user, string displayableMessage)
    {
        public class DataSerializer : ADataSerializer<UserMessage>
        {
            protected override OperationResult<UserMessage> Deserialize(DataObject reader)
            {
                if (reader.TryGet("message", out string? message) &&
                    reader.TryGet("user", out uint? user))
                    return new(new((uint)user!, message!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(UserMessage obj, DataObject writer)
            {
                writer["message"] = obj.m_Message;
                writer["user"] = obj.m_User;
            }
        }

        private readonly string m_Message = displayableMessage;
        private readonly uint m_User = user;

        public string Message => m_Message;
        public uint SenderType => m_User;
    }
}
