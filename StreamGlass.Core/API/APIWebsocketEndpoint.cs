using CorpseLib;
using CorpseLib.DataNotation;
using CorpseLib.Web.API.Event;
using CorpseLib.Web.Http;

namespace StreamGlass.Core.API
{
    public class APIWebsocketEndpoint() : EventEndpoint(false)
    {
        private class CustomEventHandler(APIWebsocketEndpoint manager, string eventType)
        {
            private readonly APIWebsocketEndpoint m_Manager = manager;
            private readonly HashSet<string> m_RegisteredClients = [];
            private readonly string m_EventType = eventType;

            public string EventType => m_EventType;

            public bool RegisterClient(string clientID) => m_RegisteredClients.Add(clientID);
            public bool UnregisterClient(string clientID) => m_RegisteredClients.Remove(clientID);
            public bool IsRegistered(string clientID) => m_RegisteredClients.Contains(clientID);

            public void Emit(DataObject eventData) => m_Manager.SendCustomEvent([.. m_RegisteredClients], "event", EventType, eventData);
        }

        private readonly Dictionary<string, CustomEventHandler> m_CustomEvents = [];

        internal void SendCustomEvent(string[] ids, string type, string eventType, DataObject data) => SendEvent(ids, type, new DataObject() { { "event", eventType }, { "data", data } });

        protected override OperationResult OnRegisterToUnknownEvent(string id, string eventType)
        {
            if (!m_CustomEvents.TryGetValue(eventType, out CustomEventHandler? customEventHandler))
            {
                customEventHandler = new(this, eventType);
                m_CustomEvents[eventType] = customEventHandler;
            }
            customEventHandler.RegisterClient(id);
            return new();
        }

        protected override OperationResult OnUnregisterToUnknownEvent(string id, string eventType)
        {
            if (m_CustomEvents.TryGetValue(eventType, out CustomEventHandler? customEventHandler))
            {
                customEventHandler.UnregisterClient(id);
                return new();
            }
            else
                return new("Unknown event", string.Format("Unknown event {0}", eventType));
        }

        public Response Emit(string eventType, DataObject data)
        {
            if (m_CustomEvents.TryGetValue(eventType, out CustomEventHandler? customEventHandler))
            {
                customEventHandler.Emit(data);
                return new(200, "Ok");
            }
            return new(404, "Not Found", string.Format("No such event {0}", eventType));
        }
    }
}
