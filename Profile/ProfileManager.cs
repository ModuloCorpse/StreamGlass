using StreamFeedstock;
using StreamFeedstock.ManagedObject;
using StreamGlass.Connections;
using StreamGlass.StreamChat;

namespace StreamGlass.Profile
{
    public class ProfileManager: Manager<Profile>
    {
        private readonly ConnectionManager m_ConnectionManager;
        private string m_Channel = "";
        private int m_NbMessage = 0;

        public ProfileManager(ConnectionManager client) : base("./profiles")
        {
            m_ConnectionManager = client;
            CanalManager.Register<UserMessage>(StreamGlassCanals.CHAT_MESSAGE, (int _, object? message) => OnChatMessage((UserMessage?)message));
            CanalManager.Register<string>(StreamGlassCanals.CHAT_JOINED, (int _, object? channel) => OnJoinedChannel((string?)channel));
            CanalManager.Register(StreamGlassCanals.STREAM_START, (int _) => OnStreamStart());
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

        private void OnStreamStart()
        {
            Log.Str("Event", "Stream Started");
            CurrentObject?.Reset();
        }

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
            CanalManager.Emit(StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM, id);
        }

        private void OnChatMessage(UserMessage? message)
        {
            if (message == null)
                return;
            if (message.SenderType != User.Type.SELF)
            {
                ++m_NbMessage;
                CurrentObject?.OnMessage(message, m_ConnectionManager, m_Channel);
            }
        }

        internal void Update(long deltaTime)
        {
            CurrentObject?.Update(deltaTime, m_NbMessage, m_ConnectionManager, m_Channel);
            m_NbMessage = 0;
        }

        protected override Profile? DeserializeObject(Json obj)
        {
            Profile newProfile = new(obj);
            return newProfile;
        }
    }
}
