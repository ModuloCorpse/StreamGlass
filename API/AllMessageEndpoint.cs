using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.StreamChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamGlass.API
{
    public class AllMessageEndpoint : AHTTPEndpoint
    {
        private readonly List<UserMessage> m_Messages = new();

        public AllMessageEndpoint() : base("/all_message") => StreamGlassCanals.CHAT_MESSAGE.Register((UserMessage? message) => { if (message != null) m_Messages.Add(message); });

        protected override Response OnGetRequest(Request request) => new(200, "Ok", new JObject() { { "messages", m_Messages } }.ToNetworkString());
    }
}
