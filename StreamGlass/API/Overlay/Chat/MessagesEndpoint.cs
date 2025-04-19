using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core;
using StreamGlass.Core.StreamChat;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StreamGlass.API.Overlay.Chat
{
    public class MessagesEndpoint : AHTTPEndpoint, IMessageReceiver
    {
        private class Page
        {
            private readonly Page? m_NextPage = null;
            private readonly Guid m_ID = Guid.NewGuid();
            private readonly Message[] m_Messages;

            public Guid ID => m_ID;

            public Page(MessagesEndpoint endpoint, List<Message> messages)
            {
                List<Message> messagesRet = [];
                bool keepPaging = true;
                long length = 0;
                while (messages.Count > 0 && keepPaging)
                {
                    Message message = messages.First();
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
        private readonly ConcurrentDictionary<string, Message> m_Messages = [];

        public MessagesEndpoint() : base("/messages")
        {
            StreamGlassChat.RegisterMessageReceiver(this);
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
            else if (request.HaveParameter("id"))
            {
                string id = request.GetParameter("id");
                if (m_Messages.TryGetValue(id, out Message? message))
                {
                    List<Message> messages = [ message ];
                    return new(200, "Ok", JsonParser.NetStr(new DataObject() { { "messages", messages } }));
                }
                else
                    return new(404, "Message not found");
            }
            else
            {
                Page page;
                if (request.HaveParameter("limit"))
                {
                    int limit = int.Parse(request.GetParameter("limit"));
                    List<Message> limitedMessages = [.. m_Messages.Values.Skip(Math.Max(0, m_Messages.Count - limit))];
                    page = new(this, limitedMessages);
                }
                else
                    page = new(this, [.. m_Messages.Values]);
                return new(200, "Ok", JsonParser.NetStr(page.ToJObject()));
            }
        }

        public void AddMessage(Message message)
        {
            m_Messages.TryAdd(message.ID, message);
            StreamGlassCanals.Emit(StreamGlassCanals.OVERLAY_CHAT_MESSAGE, message);
        }

        public void RemoveMessages(string[] messageIDs)
        {
            foreach (string messageID in messageIDs)
                m_Messages.TryRemove(messageID, out Message? _);
            StreamGlassCanals.Emit(StreamGlassCanals.CHAT_DELETE_MESSAGES, new DeleteMessagesEventArgs(messageIDs));
        }
    }
}
