using CorpseLib.Json;
using CorpseLib.ManagedObject;
using StreamGlass.Core.Connections;
using TwitchCorpse;

namespace StreamGlass.Core.Profile
{
    public class ProfileManager: Manager<Profile>
    {
        private readonly ConnectionManager m_ConnectionManager;
        private string m_Channel = string.Empty;
        private int m_NbMessage = 0;

        public ProfileManager(ConnectionManager client) : base("./profiles")
        {
            m_ConnectionManager = client;
            StreamGlassCanals.CHAT_MESSAGE.Register(OnChatMessage);
            StreamGlassCanals.CHAT_JOINED.Register(OnJoinedChannel);
            StreamGlassCanals.STREAM_START.Register(ResetCurrentProfile);
            StreamGlassCanals.CHAT_CLEAR.Register(ResetCurrentProfile);
        }

        public Profile NewProfile(string name)
        {
            Profile profile = new(name);
            AddProfile(profile);
            return profile;
        }

        public void AddProfile(Profile profile) => AddObject(profile);

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

        private void ResetCurrentProfile() => CurrentObject?.Reset();

        private void OnJoinedChannel(string? channel)
        {
            if (channel == null)
                return;
            SetChannel(channel);
        }

        internal void SetChannel(string channel) => m_Channel = channel;

        public void SetCurrentProfile(string id)
        {
            if (SetCurrentObject(id))
                CurrentObject?.Reset();
            StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM.Emit(id);
        }

        private void OnChatMessage(UserMessage? message)
        {
            if (message == null)
                return;
            if (message.SenderType != TwitchUser.Type.SELF)
            {
                ++m_NbMessage;
                CurrentObject?.OnMessage(message, m_ConnectionManager);
            }
        }

        public void Update(long deltaTime)
        {
            CurrentObject?.Update(deltaTime, m_NbMessage, m_ConnectionManager);
            m_NbMessage = 0;
        }

        protected override Profile? DeserializeObject(JFile obj) => new(obj);
    }
}
