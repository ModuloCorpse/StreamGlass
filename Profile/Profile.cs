using System.Collections.Generic;
using System.Collections.ObjectModel;
using StreamFeedstock;
using StreamFeedstock.ManagedObject;
using StreamGlass.Connections;
using StreamGlass.StreamChat;

namespace StreamGlass.Profile
{
    public class Profile: Object<Profile>
    {
        private readonly StreamInfo m_StreamInfo = new();
        private readonly Dictionary<string, int> m_CommandLocation = new();
        private readonly List<ChatCommand> m_Commands = new();
        private readonly object m_Lock = new();
        private bool m_IsSelectable = true;

        public Profile(string name) : base(name) {}
        public Profile(string id, string name) : base(id, name) {}
        internal Profile(Json json) : base(json) {}

        public ReadOnlyCollection<ChatCommand> Commands => m_Commands.AsReadOnly();
        public bool IsSelectable => m_IsSelectable;

        public void AddCommand(ChatCommand command)
        {
            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                int commandLocation = m_Commands.Count;
                m_CommandLocation[command.Name] = commandLocation;
                foreach (string alias  in command.Aliases)
                    m_CommandLocation[alias] = commandLocation;
            }
            m_Commands.Add(command);
        }

        private void TriggerCommand(ConnectionManager connectionManager, string channel, string command, User.Type userType, bool isForced)
        {
            string[] arguments = command.Split(' ');
            if (m_CommandLocation.TryGetValue(arguments[0], out var contentIdx))
            {
                ChatCommand content = m_Commands[contentIdx];
                if (isForced || content.CanTrigger(userType))
                    content.Trigger(arguments, connectionManager, channel);
                foreach (string child in content.Commands)
                    TriggerCommand(connectionManager, channel, child, userType, isForced);
            }
            else
                Parent?.TriggerCommand(connectionManager, channel, command, userType, isForced);
        }

        private void ForceOnMessage(UserMessage message, ConnectionManager connectionManager, string channel)
        {
            string messageContent = message.Message.ToString();
            if (messageContent.Length > 0 && messageContent[0] == '!')
                TriggerCommand(connectionManager, channel, messageContent[1..], message.SenderType, false);
        }

        internal void OnMessage(UserMessage message, ConnectionManager connectionManager, string channel)
        {
            lock (m_Lock)
            {
                ForceOnMessage(message, connectionManager, channel);
            }
        }

        private void ForceUpdate(long deltaTime, int nbMessage, ConnectionManager connectionManager, string channel)
        {
            foreach (ChatCommand command in m_Commands)
            {
                command.Update(deltaTime, nbMessage);
                if (command.CanAutoTrigger())
                {
                    command.Trigger(command.AutoTriggerArguments, connectionManager, channel);
                    foreach (string child in command.Commands)
                        TriggerCommand(connectionManager, channel, child, User.Type.SELF, true);
                }
            }
            Parent?.ForceUpdate(deltaTime, nbMessage, connectionManager, channel);
        }

        internal void Update(long deltaTime, int nbMessage, ConnectionManager connectionManager, string channel)
        {
            lock (m_Lock)
            {
                ForceUpdate(deltaTime, nbMessage, connectionManager, channel);
            }
        }

        private void ForceReset()
        {
            foreach (ChatCommand command in m_Commands)
                command.Reset();
            Parent?.ForceReset();
        }

        internal void Reset() { lock (m_Lock) { ForceReset(); } }

        private string GetStreamTitleOrParent()
        {
            if (m_StreamInfo.HaveStreamTitle())
                return m_StreamInfo.GetStreamTitle();
            else if (Parent != null)
                return Parent.GetStreamTitleOrParent();
            return "";
        }

        private string GetStreamDescriptionOrParent()
        {
            if (m_StreamInfo.HaveStreamDescription())
                return m_StreamInfo.GetStreamDescription();
            else if (Parent != null)
                return Parent.GetStreamDescriptionOrParent();
            return "";
        }

        private CategoryInfo GetStreamCategoryOrParent()
        {
            if (m_StreamInfo.HaveStreamCategory())
                return m_StreamInfo.GetStreamCategory();
            else if (Parent != null)
                return Parent.GetStreamCategoryOrParent();
            return new("");
        }

        private string GetStreamLanguageOrParent()
        {
            if (m_StreamInfo.HaveStreamLanguage())
                return m_StreamInfo.GetStreamLanguage();
            else if (Parent != null)
                return Parent.GetStreamLanguageOrParent();
            return "";
        }

        public string GetStreamTitle() => m_StreamInfo.GetStreamTitle();
        public string GetStreamDescription() => m_StreamInfo.GetStreamDescription();
        public CategoryInfo GetStreamCategory() => m_StreamInfo.GetStreamCategory();
        public string GetStreamLanguage() => m_StreamInfo.GetStreamLanguage();
        public void SaveStreamInfo(string title, string description, CategoryInfo category, string language) => m_StreamInfo.SaveStreamInfo(title, description, category, language);

        internal void UpdateStreamInfo() => CanalManager.Emit(StreamGlassCanals.UPDATE_STREAM_INFO, new UpdateStreamInfoArgs(GetStreamTitleOrParent(), GetStreamDescriptionOrParent(), GetStreamCategoryOrParent(), GetStreamLanguageOrParent()));

        protected override void Save(ref Json json)
        {
            List<Json> chatCommandArray = new();
            foreach (var command in m_Commands)
                chatCommandArray.Add(command.Serialize());
            json.Set("chat_commands", chatCommandArray);
            json.Set("is_selectable", m_IsSelectable);
            m_StreamInfo.Save(ref json);
        }

        protected override void Load(Json json)
        {
            m_IsSelectable = json.GetOrDefault("is_selectable", true);
            foreach (Json obj in json.GetList<Json>("chat_commands"))
                AddCommand(new(obj));
            m_StreamInfo.Load(json);
        }

        internal void SetIsSelectable(bool isSelectable) => m_IsSelectable = isSelectable;
    }
}
