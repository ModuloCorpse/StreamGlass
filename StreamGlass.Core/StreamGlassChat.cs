using CorpseLib.DataNotation;
using CorpseLib.StructuredText;
using CorpseLib.Translation;
using CorpseLib.Wpf;
using StreamGlass.Core.StreamChat;
using static StreamGlass.Core.StreamChat.MessageSource;
using static StreamGlass.Core.StreamChat.UserMessageScrollPanel;

namespace StreamGlass.Core
{
    public static class StreamGlassChat
    {
        private static MessageManager? ms_MessageManager = null;
        //TODO Make it internal or private or not exposing it
        public static UserMessageScrollPanel StreamChatPanel => ms_MessageManager!.StreamChatPanel;

        public static void Init(TickManager tickManager)
        {
            if (ms_MessageManager == null)
                ms_MessageManager = new MessageManager(tickManager);
        }
        public static Message? GetMessage(string id) => ms_MessageManager!.GetMessage(id);
        public static void SendMessage(string id, string message) => ms_MessageManager!.SendMessage(id, message);
        public static void SendMessage(string id, DataObject messageData) => ms_MessageManager!.SendMessage(id, messageData);
        public static void SendMessage(string[] sourceIDs, string message) => ms_MessageManager!.SendMessage(sourceIDs, message);
        public static void SendMessage(string[] sourceIDs, DataObject messageData) => ms_MessageManager!.SendMessage(sourceIDs, messageData);
        public static MessageSource GetOrCreateMessageSource(string id, string description, string logoPath, SendMessageDelegate sendMessageDelegate) => ms_MessageManager!.GetOrCreateMessageSource(id, description, logoPath, sendMessageDelegate);
        public static void RegisterMessageReceiver(IMessageReceiver messageReceiver) => ms_MessageManager!.AddMessageReceiver(messageReceiver);
        public static void UnregisterMessageReceiver(IMessageReceiver messageReceiver) => ms_MessageManager!.RemoveMessageReceiver(messageReceiver);

        public static void RegisterChatContextMenu(TranslationKey translationKey, ContextMenuDelegate contextMenuDelegate) => StreamChatPanel.RegisterChatContextMenu(translationKey, contextMenuDelegate);
        public static void UnregisterChatContextMenu(TranslationKey translationKey) => StreamChatPanel.UnregisterChatContextMenu(translationKey);
    }
}
