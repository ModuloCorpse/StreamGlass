using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Web;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core;
using StreamGlass.Core.API.Overlay;
using StreamGlass.Core.StreamChat;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using StreamGlass.Twitch.Moderation;

namespace StreamGlass.API.Overlay.Chat
{
    public class ChatRootEndpoint : AssemblyResource, IMessageReceiver
    {
        private class Page
        {
            private readonly Page? m_NextPage = null;
            private readonly Guid m_ID = Guid.NewGuid();
            private readonly Message[] m_Messages;

            public Guid ID => m_ID;

            public Page(ChatRootEndpoint endpoint, List<Message> messages)
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
        private readonly Dictionary<string, WebsocketReference> m_Clients = [];
        private readonly ConcurrentDictionary<string, Message> m_Messages = [];

        public ChatRootEndpoint() : base(Assembly.GetCallingAssembly(), "StreamGlass.API.Overlay.Chat.chat.html", MIME.TEXT.HTML)
        {
            StreamGlassChat.RegisterMessageReceiver(this);
        }

        private void AddPage(Page page) => m_Pages[page.ID] = page;

        private void SendMessage(string type, DataObject payload)
        {
            DataObject message = new() { { "type", type }, { "payload", payload } };
            string messageStr = JsonParser.NetStr(message);
            foreach (WebsocketReference wsReference in m_Clients.Values)
                wsReference.Send(messageStr);
        }

        private void SendMessage(WebsocketReference wsReference, string type, DataObject payload)
        {
            DataObject message = new() { { "type", type }, { "payload", payload } };
            string messageStr = JsonParser.NetStr(message);
            wsReference.Send(messageStr);
        }

        protected void GetPage(WebsocketReference wsReference, DataObject payload)
        {
            if (payload.TryGet("page", out Guid guid))
            {
                if (m_Pages.TryGetValue(guid, out Page? page))
                {
                    SendMessage(wsReference, "page", page.ToJObject());
                    m_Pages.Remove(guid);
                }
                else
                    SendMessage(wsReference, "error", new DataObject() { { "message", "Page not found" } });
            }
            else if (payload.TryGet("id", out string? id))
            {
                if (m_Messages.TryGetValue(id!, out Message? message))
                    SendMessage(wsReference, "message", new DataObject() { { "message", message } });
                else
                    SendMessage(wsReference, "error", new DataObject() { { "message", "Message not found" } });
            }
            else
            {
                Page page;
                if (payload.TryGet("limit", out int limit))
                {
                    List<Message> limitedMessages = [.. m_Messages.Values.Skip(Math.Max(0, m_Messages.Count - limit))];
                    page = new(this, limitedMessages);
                }
                else
                    page = new(this, [.. m_Messages.Values]);
                SendMessage(wsReference, "page", page.ToJObject());
            }
        }

        public void AddMessage(Message message)
        {
            //TODO Handle clients that are retrieving pages
            m_Messages.TryAdd(message.ID, message);
            SendMessage("message", new DataObject() { { "message", message } });
        }

        public void RemoveMessages(string[] messageIDs)
        {
            foreach (string messageID in messageIDs)
                m_Messages.TryRemove(messageID, out Message? _);
            SendMessage("delete", new DataObject() { { "messages", messageIDs } });
        }

        protected override void OnClientRegistered(Path path, WebsocketReference wsReference) => m_Clients[wsReference.ClientID] = wsReference;

        protected override void OnClientMessage(Path path, WebsocketReference wsReference, string message)
        {
            DataObject data = JsonParser.Parse(message);
            if (data.TryGet("type", out string? type) && data.TryGet("payload", out DataObject? payload))
            {
                if (type == "page")
                    GetPage(wsReference, payload!);
            }
        }

        protected override void OnClientUnregistered(Path path, WebsocketReference wsReference) => m_Clients.Remove(wsReference.ClientID, out WebsocketReference? _);
    }
}
