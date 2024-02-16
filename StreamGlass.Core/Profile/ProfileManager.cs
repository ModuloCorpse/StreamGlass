using CorpseLib.Json;
using CorpseLib.ManagedObject;
using StreamGlass.Core.Connections;
using TwitchCorpse;

namespace StreamGlass.Core.Profile
{
    public class ProfileManager: Manager<Profile>
    {
        private readonly ConnectionManager m_ConnectionManager;
        private int m_NbMessage = 0;

        public ProfileManager(ConnectionManager client) : base("./profiles")
        {
            m_ConnectionManager = client;
            StreamGlassCanals.Register<UserMessage>("chat_message", OnChatMessage);
            StreamGlassCanals.Register("stream_start", ResetCurrentProfile);
            StreamGlassCanals.Register("chat_clear", ResetCurrentProfile);
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

        public void SetCurrentProfile(string id)
        {
            if (SetCurrentObject(id))
                CurrentObject?.Reset();
            StreamGlassCanals.Emit("profile_changed_menu_item", id);
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
