using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.StreamChat;
using System.Collections.Generic;

namespace StreamGlass.API
{
    public class AllMessageEndpoint : AHTTPEndpoint
    {
        private readonly List<UserMessage> m_Messages = new();

        public AllMessageEndpoint() : base("/all_message")
        {
            StreamGlassCanals.CHAT_MESSAGE.Register((UserMessage? message) => { if (message != null) m_Messages.Add(message); });
            StreamGlassCanals.CHAT_CLEAR.Register(() => m_Messages.Clear());
            StreamGlassCanals.CHAT_CLEAR_MESSAGE.Register((string? messageID) => { if (messageID != null) m_Messages.RemoveAll(message => message.ID == messageID); });
            StreamGlassCanals.CHAT_CLEAR_USER.Register((string? userID) => { if (userID != null) m_Messages.RemoveAll(message => message.UserID == userID); });
        }

        protected override Response OnGetRequest(Request request) => new(200, "Ok", new JObject() { { "messages", m_Messages } }.ToNetworkString());
    }
}
