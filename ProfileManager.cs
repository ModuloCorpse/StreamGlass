using Newtonsoft.Json.Linq;
using StreamGlass.Command;
using StreamGlass.Twitch.IRC;
using System.IO;

namespace StreamGlass
{
    public class ProfileManager: ObjectManager<Profile>
    {
        private string m_Channel = "";
        private int m_NbMessage = 0;

        public ProfileManager(): base("profiles.json") {}

        public Profile NewProfile(string name)
        {
            Profile profile = new(name);
            if (!string.IsNullOrEmpty(m_Channel))
                profile.SetChannel(m_Channel);
            AddObject(profile);
            return profile;
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

        protected override Profile? DeserializeObject(JObject obj) => new(obj);
    }
}
