using CorpseLib.ManagedObject;
using System.Collections.ObjectModel;
using CorpseLib.DataNotation;

namespace StreamGlass.Core.Profile
{
    public class Profile: Object<Profile>
    {
        private readonly StreamInfo m_StreamInfo = new();
        private readonly Dictionary<string, int> m_CommandLocation = [];
        private readonly List<ChatCommand> m_Commands = [];
        private readonly object m_Lock = new();
        private bool m_IsSelectable = true;

        public Profile(string name) : base(name) {}
        public Profile(string id, string name) : base(id, name) {}
        internal Profile(DataObject json) : base(json) {}

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

        private void TriggerCommand(string command, uint userType, bool isForced)
        {
            string[] arguments = command.Split(' ');
            if (m_CommandLocation.TryGetValue(arguments[0], out var contentIdx))
            {
                ChatCommand content = m_Commands[contentIdx];
                if (isForced || content.CanTrigger(userType))
                    content.Trigger(arguments);
                foreach (string child in content.Commands)
                    TriggerCommand(child, userType, isForced);
            }
            else
                Parent?.TriggerCommand(command, userType, isForced);
        }

        private void ForceOnMessage(UserMessage message)
        {
            string messageContent = message.Message;
            if (messageContent.Length > 0 && messageContent[0] == '!')
                TriggerCommand(messageContent[1..], message.SenderType, false);
        }

        internal void OnMessage(UserMessage message)
        {
            lock (m_Lock)
            {
                ForceOnMessage(message);
            }
        }

        private void ForceUpdate(long deltaTime, int nbMessage)
        {
            foreach (ChatCommand command in m_Commands)
            {
                command.Update(deltaTime, nbMessage);
                if (command.CanAutoTrigger())
                {
                    command.Trigger(command.AutoTriggerArguments);
                    foreach (string child in command.Commands)
                        TriggerCommand(child, uint.MaxValue, true);
                }
            }
            Parent?.ForceUpdate(deltaTime, nbMessage);
        }

        internal void Update(long deltaTime, int nbMessage)
        {
            lock (m_Lock)
            {
                ForceUpdate(deltaTime, nbMessage);
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
            return string.Empty;
        }

        private string GetStreamDescriptionOrParent()
        {
            if (m_StreamInfo.HaveStreamDescription())
                return m_StreamInfo.GetStreamDescription();
            else if (Parent != null)
                return Parent.GetStreamDescriptionOrParent();
            return string.Empty;
        }

        private CategoryInfo GetStreamCategoryOrParent()
        {
            if (m_StreamInfo.HaveStreamCategory())
                return m_StreamInfo.GetStreamCategory();
            else if (Parent != null)
                return Parent.GetStreamCategoryOrParent();
            return new(string.Empty);
        }

        private string GetStreamLanguageOrParent()
        {
            if (m_StreamInfo.HaveStreamLanguage())
                return m_StreamInfo.GetStreamLanguage();
            else if (Parent != null)
                return Parent.GetStreamLanguageOrParent();
            return string.Empty;
        }

        public string GetStreamTitle() => m_StreamInfo.GetStreamTitle();
        public string GetStreamDescription() => m_StreamInfo.GetStreamDescription();
        public CategoryInfo GetStreamCategory() => m_StreamInfo.GetStreamCategory();
        public string GetStreamLanguage() => m_StreamInfo.GetStreamLanguage();
        public void SaveStreamInfo(string title, string description, CategoryInfo category, string language) => m_StreamInfo.SaveStreamInfo(title, description, category, language);

        internal void UpdateStreamInfo() => StreamGlassCanals.Emit(StreamGlassCanals.UPDATE_STREAM_INFO, new UpdateStreamInfoArgs(GetStreamTitleOrParent(), GetStreamDescriptionOrParent(), GetStreamCategoryOrParent(), GetStreamLanguageOrParent()));

        protected override void Save(ref DataObject json)
        {
            List<DataObject> chatCommandArray = [];
            foreach (var command in m_Commands)
                chatCommandArray.Add(command.Serialize());
            json.Add("chat_commands", chatCommandArray);
            json.Add("is_selectable", m_IsSelectable);
            m_StreamInfo.Save(ref json);
        }

        protected override void Load(DataObject json)
        {
            m_IsSelectable = json.GetOrDefault("is_selectable", true);
            foreach (DataObject obj in json.GetList<DataObject>("chat_commands"))
                AddCommand(new(obj));
            m_StreamInfo.Load(json);
        }

        internal void SetIsSelectable(bool isSelectable) => m_IsSelectable = isSelectable;
    }
}
