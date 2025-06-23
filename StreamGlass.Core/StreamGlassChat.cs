using CorpseLib.DataNotation;
using StreamGlass.Core.StreamChat;

namespace StreamGlass.Core
{
    public static class StreamGlassChat
    {
        private static MessageManager? ms_MessageManager = null;
        //TODO Make it internal or private or not exposing it
        public static UserMessageScrollPanel StreamChatPanel => ms_MessageManager!.StreamChatPanel;

        public static void Init(TickManager tickManager) => ms_MessageManager ??= new MessageManager(tickManager);
        public static Message? GetMessage(string id) => ms_MessageManager!.GetMessage(id);
        public static void SendMessage(string id, string message) => ms_MessageManager!.SendMessage(id, message);
        public static void SendMessage(string id, DataObject messageData) => ms_MessageManager!.SendMessage(id, messageData);
        public static void SendMessage(string[] sourceIDs, string message) => ms_MessageManager!.SendMessage(sourceIDs, message);
        public static void SendMessage(string[] sourceIDs, DataObject messageData) => ms_MessageManager!.SendMessage(sourceIDs, messageData);
        public static MessageSource GetOrCreateMessageSource(string id, string description, string logoPath, IMessageSourceHandler handler) => ms_MessageManager!.GetOrCreateMessageSource(id, description, logoPath, handler);
        public static void RegisterMessageReceiver(IMessageReceiver messageReceiver) => ms_MessageManager!.AddMessageReceiver(messageReceiver);
        public static void UnregisterMessageReceiver(IMessageReceiver messageReceiver) => ms_MessageManager!.RemoveMessageReceiver(messageReceiver);
    }
}
