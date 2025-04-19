using CorpseLib;
using CorpseLib.DataNotation;

namespace StreamGlass.Core.Profile
{
    public class SendMessageEventArgs(string message, bool isShared)
    {
        public class DataSerializer : ADataSerializer<SendMessageEventArgs>
        {
            protected override OperationResult<SendMessageEventArgs> Deserialize(DataObject reader)
            {
                if (reader.TryGet("message", out string? message) && reader.TryGet("is_shared", out bool? isShared))
                    return new(new(message!, (bool)isShared!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(SendMessageEventArgs obj, DataObject writer)
            {
                writer["message"] = obj.m_Message;
                writer["is_shared"] = obj.m_IsShared;
            }
        }

        private readonly string m_Message = message;
        private readonly bool m_IsShared = isShared;

        public string Message => m_Message;
        public bool IsShared => m_IsShared;
    }
}
