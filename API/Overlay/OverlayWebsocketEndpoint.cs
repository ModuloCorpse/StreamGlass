using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.API.Event;
using CorpseLib.Web.Http;
using System.Collections.Generic;
using System.Linq;

namespace StreamGlass.API.Overlay
{
    public class OverlayWebsocketEndpoint : EventEndpoint
    {
        private class CustomEventHandler(OverlayWebsocketEndpoint manager, string eventType)
        {
            private readonly OverlayWebsocketEndpoint m_Manager = manager;
            private readonly HashSet<string> m_RegisteredClients = [];
            private readonly string m_EventType = eventType;

            public string EventType => m_EventType;

            public bool RegisterClient(string clientID) => m_RegisteredClients.Add(clientID);
            public bool UnregisterClient(string clientID) => m_RegisteredClients.Remove(clientID);
            public bool IsRegistered(string clientID) => m_RegisteredClients.Contains(clientID);

            public void Emit(JObject eventData) => m_Manager.SendEvent([.. m_RegisteredClients], "event", EventType, eventData);
        }

        private readonly PathTree<Dictionary<string, CustomEventHandler>> m_CustomEvents = new();

        public OverlayWebsocketEndpoint() : base("/overlay", false) { }

        internal void SendEvent(string[] ids, string type, string eventType, JObject data) => SendEvent(ids, type, new JObject() { { "event", eventType }, { "data", data } });

        protected override void OnUnknownRequest(CorpseLib.Web.API.API.APIProtocol client, string request, JFile json)
        {
            if (request == "custom-subscribe" || request == "custom-unsubscribe")
            {
                if (json.TryGet("event", out string? eventType))
                {
                    Dictionary<string, CustomEventHandler>? events = m_CustomEvents.GetValue(client.WebSocketPath);
                    if (events == null)
                    {
                        m_CustomEvents.AddValue(client.WebSocketPath, [], true);
                        events = m_CustomEvents.GetValue(client.WebSocketPath)!;
                    }
                    if (!events.TryGetValue(eventType!, out CustomEventHandler? customEventHandler))
                    {
                        customEventHandler = new(this, eventType!);
                        events[eventType!] = customEventHandler;
                    }
                    if (request == "custom-subscribe")
                    {
                        customEventHandler.RegisterClient(client.ID);
                        SendEvent(client, "subscribed", new JObject() { { "event", eventType } });
                        return;
                    }
                    else
                    {
                        customEventHandler.UnregisterClient(client.ID);
                        SendEvent(client, "unsubscribed", new JObject() { { "event", eventType } });
                        return;
                    }
                }
                else
                {
                    SendEvent(client, "error", new JObject() { { "error", "No 'event' given" } });
                    return;
                }
            }
            else
                SendEvent(client, "error", new JObject() { { "error", string.Format("Unknown request {0}", request) } });
        }

        public Response Emit(Path path, string eventType, JObject data)
        {
            Dictionary<string, CustomEventHandler>? events = m_CustomEvents.GetValue(path);
            if (events != null)
            {
                if (events.TryGetValue(eventType, out CustomEventHandler? customEventHandler))
                {
                    customEventHandler.Emit(data);
                    return new(200, "Ok");
                }
                return new(404, "Not Found", string.Format("No such event {0}", eventType));
            }
            return new(404, "Not Found", string.Format("No such path {0}", path.FullPath));
        }
    }
}
