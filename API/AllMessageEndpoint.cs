using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.StreamChat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamGlass.API
{
    public class AllMessageEndpoint : AHTTPEndpoint
    {
        private class Page
        {
            private readonly Page? m_NextPage = null;
            private readonly Guid m_ID = Guid.NewGuid();
            private readonly UserMessage[] m_Messages;

            public Guid ID => m_ID;

            public Page(AllMessageEndpoint endpoint, List<UserMessage> messages)
            {
                List<UserMessage> messagesRet = [];
                bool keepPaging = true;
                long length = 0;
                while (messages.Count > 0 && keepPaging)
                {
                    UserMessage message = messages.First();
                    JObject node = (JObject)JHelper.Cast(message);
                    long nodeLength = node.ToNetworkString().Length;
                    if (length + nodeLength < 2000)
                    {
                        length += nodeLength;
                        messages.RemoveAt(0);
                        messagesRet.Add(message);
                        keepPaging = (length < 1000);
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

            public JObject ToJObject()
            {
                if (m_NextPage == null)
                    return new JObject() { { "messages", m_Messages } };
                else
                    return new JObject() { { "messages", m_Messages }, { "page", m_NextPage.m_ID.ToString() } };
            }
        }

        private readonly Dictionary<Guid, Page> m_Pages = [];
        private readonly List<UserMessage> m_Messages = [];

        public AllMessageEndpoint() : base("/all_message")
        {
            StreamGlassCanals.CHAT_MESSAGE.Register((UserMessage? message) => { if (message != null) m_Messages.Add(message); });
            StreamGlassCanals.CHAT_CLEAR.Register(m_Messages.Clear);
            StreamGlassCanals.CHAT_CLEAR_MESSAGE.Register((string? messageID) => { if (messageID != null) m_Messages.RemoveAll(message => message.ID == messageID); });
            StreamGlassCanals.CHAT_CLEAR_USER.Register((string? userID) => { if (userID != null) m_Messages.RemoveAll(message => message.UserID == userID); });
        }

        private void AddPage(Page page) => m_Pages[page.ID] = page;

        protected override Response OnGetRequest(Request request)
        {
            if (request.HaveParameter("page"))
            {
                Guid guid = Guid.Parse(request.GetParameter("page"));
                if (m_Pages.TryGetValue(guid, out Page? page))
                {
                    Response response = new(200, "Ok", page.ToJObject().ToNetworkString());
                    m_Pages.Remove(guid);
                    return response;
                }
                else
                    return new(404, "Page not found");
            }
            else
            {
                Page page = new(this, new(m_Messages));
                return new(200, "Ok", page.ToJObject().ToNetworkString());
            }
        }
    }
}
