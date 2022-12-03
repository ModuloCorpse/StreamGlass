using StreamGlass.StreamChat;

namespace StreamGlass.Profile
{
    public class ProfileManager: ManagedObject.Manager<Profile>
    {
        private string m_Channel = "";
        private int m_NbMessage = 0;
        private readonly IStreamChat m_StreamChat;

        public ProfileManager(IStreamChat client) : base("./profiles")
        {
            m_StreamChat = client;
            CanalManager.Register(StreamGlassCanals.CHAT_MESSAGE, (int _, UserMessage message) => OnChatMessage(message));
            CanalManager.Register(StreamGlassCanals.CHAT_JOINED, (int _, string channel) => OnJoinedChannel(channel));
            CanalManager.Register(StreamGlassCanals.STREAM_START, (int _) => OnStreamStart());
        }

        public Profile NewProfile(string name)
        {
            Profile profile = new(m_StreamChat, name);
            AddProfile(profile);
            return profile;
        }

        public void AddProfile(Profile profile)
        {
            if (!string.IsNullOrEmpty(m_Channel))
                profile.SetChannel(m_Channel);
            AddObject(profile);
        }

        public void UpdateProfile(Profile profile)
        {
            bool updateManager = false;
            if (profile.ID == CurrentObjectID)
                updateManager = true;
            SetObject(profile);
            if (updateManager)
                profile.UpdateStreamInfo();
        }

        public void UpdateStreamInfo() => CurrentObject?.UpdateStreamInfo();

        private void OnStreamStart()
        {
            Logger.Log("Event", "Stream Started");
            CurrentObject?.Reset();
        }

        private void OnJoinedChannel(string channel)
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
            if (message.SenderType != UserMessage.UserType.SELF)
            {
                ++m_NbMessage;
                CurrentObject?.OnMessage(message);
            }
        }

        internal void Update(long deltaTime)
        {
            CurrentObject?.Update(deltaTime, m_NbMessage);
            m_NbMessage = 0;
        }

        protected override Profile? DeserializeObject(Json obj) => new(m_StreamChat, obj);
    }
}
