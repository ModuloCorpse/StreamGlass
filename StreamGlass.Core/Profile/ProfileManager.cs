using CorpseLib.DataNotation;
using CorpseLib.ManagedObject;

namespace StreamGlass.Core.Profile
{
    public class ProfileManager: Manager<Profile>
    {
        private int m_NbMessage = 0;

        public ProfileManager() : base("./profiles")
        {
            StreamGlassCanals.Register<UserMessage>(StreamGlassCanals.CHAT_MESSAGE, OnChatMessage);
            StreamGlassCanals.Register(StreamGlassCanals.PROFILE_RESET, ResetCurrentProfile);
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
            StreamGlassCanals.Emit(StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM, id);
        }

        private void OnChatMessage(UserMessage? message)
        {
            if (message == null)
                return;
            if (message.SenderType != uint.MaxValue)
            {
                ++m_NbMessage;
                CurrentObject?.OnMessage(message);
            }
        }

        public void Update(long deltaTime)
        {
            CurrentObject?.Update(deltaTime, m_NbMessage);
            m_NbMessage = 0;
        }

        protected override Profile? DeserializeObject(DataObject obj) => new(obj);
    }
}
