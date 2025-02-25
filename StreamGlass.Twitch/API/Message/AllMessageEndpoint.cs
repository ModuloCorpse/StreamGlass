using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core;

namespace StreamGlass.Twitch.API.Message
{
    public class AllMessageEndpoint : AHTTPEndpoint
    {
        private class Page
        {
            private readonly Page? m_NextPage = null;
            private readonly Guid m_ID = Guid.NewGuid();
            private readonly Twitch.Message[] m_Messages;

            public Guid ID => m_ID;

            public Page(AllMessageEndpoint endpoint, List<Twitch.Message> messages)
            {
                List<Twitch.Message> messagesRet = [];
                bool keepPaging = true;
                long length = 0;
                while (messages.Count > 0 && keepPaging)
                {
                    Twitch.Message message = messages.First();
                    DataObject node = (DataObject)DataHelper.Cast(message);
                    long nodeLength = JsonParser.NetStr(node).Length;
                    if (length == 0 || length + nodeLength < 2000)
                    {
                        length += nodeLength;
                        messages.RemoveAt(0);
                        messagesRet.Add(message);
                        keepPaging = length < 1000;
                    }
                    else
                        keepPaging = false;
                }

                m_Messages = [.. messagesRet];
                if (messages.Count > 0)
                {
                    m_NextPage = new(endpoint, messages);
                    endpoint.AddPage(m_NextPage);
                }
            }

            public DataObject ToJObject()
            {
                if (m_NextPage == null)
                    return new DataObject() { { "messages", m_Messages } };
                else
                    return new DataObject() { { "messages", m_Messages }, { "page", m_NextPage.m_ID.ToString() } };
            }
        }

        private readonly Dictionary<Guid, Page> m_Pages = [];
        private readonly List<Twitch.Message> m_Messages = [];
        private readonly object m_MessageLock = new();

        public AllMessageEndpoint() : base("/all_message")
        {
            StreamGlassCanals.Register<Twitch.Message>(TwitchPlugin.Canals.OVERLAY_CHAT_MESSAGE, OnOverlayChatMessage);
            StreamGlassCanals.Register(TwitchPlugin.Canals.CHAT_CLEAR, OnChatClear);
            StreamGlassCanals.Register<string>(TwitchPlugin.Canals.CHAT_CLEAR_USER, OnChatClearUser);
            StreamGlassCanals.Register<string>(TwitchPlugin.Canals.CHAT_CLEAR_MESSAGE, OnChatClearMessage);
        }

        private void OnOverlayChatMessage(Twitch.Message? message)
        {
            lock (m_MessageLock)
            {
                if (message != null)
                    m_Messages.Add(message);
            }
        }

        private void OnChatClear()
        {
            lock (m_MessageLock)
            {
                m_Messages.Clear();
            }
        }

        private void OnChatClearUser(string? userID)
        {
            lock (m_MessageLock)
            {
                if (userID != null)
                    m_Messages.RemoveAll(message => message.UserID == userID);
            }
        }

        private void OnChatClearMessage(string? messageID)
        {
            lock (m_MessageLock)
            {
                if (messageID != null)
                    m_Messages.RemoveAll(message => message.ID == messageID);
            }
        }

        private void AddPage(Page page) => m_Pages[page.ID] = page;

        protected override Response OnGetRequest(Request request)
        {
            if (request.HaveParameter("page"))
            {
                Guid guid = Guid.Parse(request.GetParameter("page"));
                if (m_Pages.TryGetValue(guid, out Page? page))
                {
                    Response response = new(200, "Ok", JsonParser.NetStr(page.ToJObject()));
                    m_Pages.Remove(guid);
                    return response;
                }
                else
                    return new(404, "Page not found");
            }
            else
            {
                Page page;
                if (request.HaveParameter("limit"))
                {
                    int limit = int.Parse(request.GetParameter("limit"));
                    List<Twitch.Message> limitedMessages = [.. m_Messages.Skip(Math.Max(0, m_Messages.Count - limit))];
                    page = new(this, limitedMessages);
                }
                else
                    page = new(this, [.. m_Messages]);
                return new(200, "Ok", JsonParser.NetStr(page.ToJObject()));
            }
        }
    }
}
