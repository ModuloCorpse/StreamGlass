﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using StreamFeedstock;
using StreamFeedstock.ManagedObject;
using StreamGlass.StreamChat;

namespace StreamGlass.Profile
{
    public class Profile: Object<Profile>
    {
        private readonly StreamInfo m_StreamInfo = new();
        private readonly Dictionary<string, int> m_CommandLocation = new();
        private readonly List<ChatCommand> m_Commands = new();
        private readonly object m_Lock = new();

        public Profile(string name) : base(name) {}
        public Profile(string id, string name) : base(id, name) {}
        internal Profile(Json json) : base(json) {}

        public ReadOnlyCollection<ChatCommand> Commands => m_Commands.AsReadOnly();

        public void AddCommand(ChatCommand command)
        {
            if (!string.IsNullOrWhiteSpace(command.Name))
                m_CommandLocation[command.Name] = m_Commands.Count;
            m_Commands.Add(command);
        }

        private void TriggerCommand(IStreamChat streamChat, string channel, string command, UserMessage.UserType userType, bool isForced)
        {
            string[] arguments = command.Split(' ');
            if (m_CommandLocation.TryGetValue(arguments[0], out var contentIdx))
            {
                ChatCommand content = m_Commands[contentIdx];
                if (isForced || content.CanTrigger(arguments.Length - 1, userType))
                    content.Trigger(arguments, streamChat, channel);
                foreach (string child in content.Commands)
                    TriggerCommand(streamChat, channel, child, userType, isForced);
            }
            else
                Parent?.TriggerCommand(streamChat, channel, command, userType, isForced);
        }

        private void ForceOnMessage(UserMessage message, IStreamChat streamChat, string channel)
        {
            if (message.Message[0] == '!')
                TriggerCommand(streamChat, channel, message.Message[1..], message.SenderType, false);
        }

        internal void OnMessage(UserMessage message, IStreamChat streamChat, string channel)
        {
            lock (m_Lock)
            {
                ForceOnMessage(message, streamChat, channel);
            }
        }

        private void ForceUpdate(long deltaTime, int nbMessage, IStreamChat streamChat, string channel)
        {
            foreach (ChatCommand command in m_Commands)
            {
                command.Update(deltaTime, nbMessage);
                if (command.CanAutoTrigger())
                {
                    command.Trigger(command.AutoTriggerArguments, streamChat, channel);
                    foreach (string child in command.Commands)
                        TriggerCommand(streamChat, channel, child, UserMessage.UserType.SELF, true);
                }
            }
            Parent?.ForceUpdate(deltaTime, nbMessage, streamChat, channel);
        }

        internal void Update(long deltaTime, int nbMessage, IStreamChat streamChat, string channel)
        {
            lock (m_Lock)
            {
                ForceUpdate(deltaTime, nbMessage, streamChat, channel);
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

        private string GetStreamCategoryOrParent()
        {
            if (m_StreamInfo.HaveStreamCategory())
                return m_StreamInfo.GetStreamCategory();
            else if (Parent != null)
                return Parent.GetStreamCategoryOrParent();
            return "";
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
        public string GetStreamCategory() => m_StreamInfo.GetStreamCategory();
        public string GetStreamLanguage() => m_StreamInfo.GetStreamLanguage();
        public void SaveStreamInfo(string title, string description, string category, string language) => m_StreamInfo.SaveStreamInfo(title, description, category, language);

        internal void UpdateStreamInfo() => CanalManager.Emit(StreamGlassCanals.UPDATE_STREAM_INFO, new UpdateStreamInfoArgs(GetStreamTitleOrParent(), GetStreamDescriptionOrParent(), GetStreamCategoryOrParent(), GetStreamLanguageOrParent()));

        protected override void Save(ref Json json)
        {
            List<Json> chatCommandArray = new();
            foreach (var command in m_Commands)
                chatCommandArray.Add(command.Serialize());
            json.Set("chat_commands", chatCommandArray);
            m_StreamInfo.Save(ref json);
        }

        protected override void Load(Json json)
        {
            foreach (Json obj in json.GetList<Json>("chat_commands"))
                AddCommand(new(obj));
            m_StreamInfo.Load(json);
        }
    }
}
