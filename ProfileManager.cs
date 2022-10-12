using Newtonsoft.Json.Linq;
using StreamGlass.Command;
using StreamGlass.Twitch.IRC;

namespace StreamGlass
{
    public class ProfileManager: ObjectManager<Profile>
    {
        private string m_Channel = "";
        private int m_NbMessage = 0;
        private readonly Client m_Client;

        public ProfileManager(Client client): base("profiles.json") => m_Client = client;

        public Profile NewProfile(string name)
        {
            Profile profile = new(m_Client, name);
            if (!string.IsNullOrEmpty(m_Channel))
                profile.SetChannel(m_Channel);
            AddObject(profile);
            return profile;
        }

        public bool UpdateStreamInfo(string broadcasterID) => (CurrentObject != null) && CurrentObject.UpdateStreamInfo(broadcasterID);

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

        internal void OnMessage(UserMessage message)
        {
            ++m_NbMessage;
            CurrentObject?.OnMessage(message);
        }

        internal void Update(long deltaTime)
        {
            CurrentObject?.Update(deltaTime, m_NbMessage);
            m_NbMessage = 0;
        }

        protected override Profile? DeserializeObject(JObject obj) => new(m_Client, obj);
    }
}
