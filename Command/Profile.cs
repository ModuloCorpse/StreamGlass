using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using StreamGlass.StreamChat;

namespace StreamGlass.Command
{
    public class Profile: ManagedObject<Profile>
    {
        private string m_Channel = "";
        private string m_StreamTitle = "";
        private string m_StreamDescription = "";
        private string m_StreamCategory = "";
        private string m_StreamLanguage = "";
        private readonly Dictionary<string, int> m_CommandLocation = new();
        private readonly List<ChatCommand> m_Commands = new();
        private readonly IStreamChat m_StreamChat;

        public Profile(IStreamChat client, string name): base(name) => m_StreamChat = client;
        internal Profile(IStreamChat client, JObject json): base(json) => m_StreamChat = client;

        internal void SetChannel(string channel) => m_Channel = channel;

        private void TriggerCommand(string command, UserMessage.UserType userType, bool isForced)
        {
            string[] arguments = command.Split(' ');
            if (m_CommandLocation.TryGetValue(arguments[0], out var contentIdx))
            {
                ChatCommand content = m_Commands[contentIdx];
                if (isForced || content.CanTrigger(arguments.Length - 1, userType))
                    content.Trigger(arguments, m_StreamChat, m_Channel);
                List<string> childrens = content.GetCommands();
                foreach (string child in childrens)
                    TriggerCommand(child, userType, isForced);
            }
            else
                Parent?.TriggerCommand(command, userType, isForced);
        }

        internal void OnMessage(UserMessage message)
        {
            if (message.Message[0] == '!')
                TriggerCommand(message.Message[1..], message.SenderType, false);
        }

        internal void Update(long deltaTime, int nbMessage)
        {
            foreach (ChatCommand command in m_Commands)
            {
                command.Update(deltaTime, nbMessage);
                if (command.CanAutoTrigger())
                {
                    command.Trigger(command.GetDefaultArguments(), m_StreamChat, m_Channel);
                    List<string> childrens = command.GetCommands();
                    foreach (string child in childrens)
                        TriggerCommand(child, UserMessage.UserType.SELF, true);
                }
            }
            Parent?.Update(deltaTime, nbMessage);
        }

        public string GetStreamTitle()
        {
            if (!string.IsNullOrEmpty(m_StreamTitle))
                return m_StreamTitle;
            else if (Parent != null)
                return Parent.GetStreamTitle();
            return "";
        }

        public string GetStreamDescription()
        {
            if (!string.IsNullOrEmpty(m_StreamDescription))
                return m_StreamDescription;
            else if (Parent != null)
                return Parent.GetStreamTitle();
            return "";
        }

        public string GetStreamCategory()
        {
            if (!string.IsNullOrEmpty(m_StreamCategory))
                return m_StreamCategory;
            else if (Parent != null)
                return Parent.GetStreamCategory();
            return "";
        }

        public string GetStreamLanguage()
        {
            if (!string.IsNullOrEmpty(m_StreamLanguage))
                return m_StreamLanguage;
            else if (Parent != null)
                return Parent.GetStreamLanguage();
            return "";
        }

        internal void UpdateStreamInfo() => CanalManager.Emit(StreamGlassCanals.UPDATE_STREAM_INFO, new UpdateStreamInfoArgs(GetStreamTitle(), GetStreamDescription(), GetStreamCategory(), GetStreamLanguage()));

        internal void Reset()
        {
            foreach (ChatCommand command in m_Commands)
                command.Reset();
            Parent?.Reset();
        }

        protected override void Save(ref JObject json)
        {
            JArray chatCommandArray = new();
            foreach (var command in m_Commands)
            {
                chatCommandArray.Add(command.Serialize());
            }
            json["chat_commands"] = chatCommandArray;

            if (!string.IsNullOrEmpty(m_StreamTitle))
                json["stream_title"] = m_StreamTitle;
            if (!string.IsNullOrEmpty(m_StreamDescription))
                json["stream_description"] = m_StreamDescription;
            if (!string.IsNullOrEmpty(m_StreamCategory))
                json["stream_category"] = m_StreamCategory;
            if (!string.IsNullOrEmpty(m_StreamLanguage))
                json["stream_language"] = m_StreamLanguage;
        }

        protected override void Load(JObject json)
        {
            JArray? chatCommandArray = (JArray?)json["chat_commands"];
            if (chatCommandArray != null)
            {
                foreach (JObject obj in chatCommandArray.Cast<JObject>())
                {
                    string? commandName = JsonHelper.Get(obj, "name");
                    ChatCommand newCommand = new(obj);
                    if (commandName != null)
                        m_CommandLocation[commandName] = m_Commands.Count;
                    m_Commands.Add(newCommand);
                }
            }
            JValue? titleValue = (JValue?)json["stream_title"];
            if (titleValue != null)
                m_StreamTitle = titleValue.ToString();
            JValue? descriptionValue = (JValue?)json["stream_description"];
            if (descriptionValue != null)
                m_StreamDescription = descriptionValue.ToString();
            JValue? categoryValue = (JValue?)json["stream_category"];
            if (categoryValue != null)
                m_StreamCategory = categoryValue.ToString();
            JValue? languageValue = (JValue?)json["stream_language"];
            if (languageValue != null)
                m_StreamLanguage = languageValue.ToString();
        }
    }
}
