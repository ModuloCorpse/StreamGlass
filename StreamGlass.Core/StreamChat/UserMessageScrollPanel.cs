using CorpseLib.Translation;
using StreamGlass.Core.Controls;

namespace StreamGlass.Core.StreamChat
{
    public class UserMessageScrollPanel() : ScrollPanel<MessageControl>(), IMessageReceiver
    {
        public delegate void ContextMenuDelegate(Window window, Message message);

        private readonly Dictionary<TranslationKey, ContextMenuDelegate> m_ContextMenuActions = [];
        private readonly HashSet<string> m_ChatHighlightedUsers = [];
        private double m_MessageContentFontSize = 14;
        private bool m_ShowBadges = true;

        internal Dictionary<TranslationKey, ContextMenuDelegate> ContextMenuActions => m_ContextMenuActions;
        public double MessageContentFontSize => m_MessageContentFontSize;

        public void RegisterChatContextMenu(TranslationKey translationKey, ContextMenuDelegate contextMenuDelegate)
        {
            if (!m_ContextMenuActions.ContainsKey(translationKey))
            {
                m_ContextMenuActions[translationKey] = contextMenuDelegate;
                UpdateContextMenu();
            }
        }

        public void UnregisterChatContextMenu(TranslationKey translationKey)
        {
            if (m_ContextMenuActions.Remove(translationKey))
                UpdateContextMenu();
        }

        private void UpdateContextMenu() => Dispatcher.BeginInvoke(() =>
        {
            foreach (MessageControl message in Controls)
                message.UpdateContextMenu();
        });

        public void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (MessageControl message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        internal void ToggleHighlightedUser(string userID)
        {
            if (!m_ChatHighlightedUsers.Remove(userID))
                m_ChatHighlightedUsers.Add(userID);
        }

        public void AddMessage(Message message) => Dispatcher.BeginInvoke(() =>
        {
            Message? reply = null;
            if (!string.IsNullOrEmpty(message.ReplyID))
                reply = StreamGlassChat.GetMessage(message.ReplyID);
            MessageControl chatMessage = new(this, message, reply, m_MessageContentFontSize, m_ChatHighlightedUsers.Contains(message.User.ID), m_ShowBadges);
            chatMessage.MessageContent.Loaded += (sender, e) => { UpdateControlsPosition(); };
            AddControl(chatMessage);
        });

        private void RemoveMessage(string messageID)
        {
            foreach (MessageControl message in Controls)
            {
                if (message.ID == messageID)
                {
                    Remove(message);
                    return;
                }
            }
        }

        public void RemoveMessages(string[] messageIDs) => Dispatcher.BeginInvoke(() =>
        {
            foreach (string messageID in messageIDs)
                RemoveMessage(messageID);
        });
    }
}
