using Newtonsoft.Json.Linq;
using StreamGlass.Command;
using StreamGlass.StreamChat;

namespace StreamGlass
{
    public class ProfileManager: ObjectManager<Profile>
    {
        private string m_Channel = "";
        private int m_NbMessage = 0;
        private readonly IStreamChat m_StreamChat;

        public ProfileManager(IStreamChat client) : base("profiles.json")
        {
            m_StreamChat = client;
            CanalManager.Register(StreamGlassCanals.CHAT_MESSAGE, (int _, UserMessage message) => OnChatMessage(message));
            CanalManager.Register(StreamGlassCanals.CHAT_JOINED, (int _, string channel) => OnJoinedChannel(channel));
        }

        public Profile NewProfile(string name)
        {
            Profile profile = new(m_StreamChat, name);
            if (!string.IsNullOrEmpty(m_Channel))
                profile.SetChannel(m_Channel);
            AddObject(profile);
            return profile;
        }

        public void UpdateStreamInfo() => CurrentObject?.UpdateStreamInfo();

        public void OnJoinedChannel(string channel)
        {
            if (m_Channel != channel)
            {
                m_Channel = channel;
                foreach (Profile profile in Objects)
                    profile.SetChannel(channel);
            }
        }

        internal void SetChannel(string channel)
        {
            m_Channel = channel;
            foreach (Profile profile in Objects)
                profile.SetChannel(channel);
        }

        public void SetCurrentProfile(string id)
        {
            if (SetCurrentObject(id))
                CurrentObject?.Reset();
        }

        private void OnChatMessage(UserMessage message)
        {
            ++m_NbMessage;
            CurrentObject?.OnMessage(message);
        }

        internal void Update(long deltaTime)
        {
            CurrentObject?.Update(deltaTime, m_NbMessage);
            m_NbMessage = 0;
        }

        protected override Profile? DeserializeObject(JObject obj) => new(m_StreamChat, obj);
    }
}
