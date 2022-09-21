using Newtonsoft.Json.Linq;
using StreamGlass.Command;
using StreamGlass.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StreamGlass
{
    public class ProfileManager
    {
        private string m_Channel = "";
        private int m_NbMessage = 0;
        private readonly Dictionary<string, Profile> m_Profiles = new();
        private Profile? m_CurrentProfile = null;

        public Profile NewProfile(string name)
        {
            Profile profile = new(name);
            if (!string.IsNullOrEmpty(m_Channel))
                profile.SetChannel(m_Channel);
            m_Profiles[profile.ID] = profile;
            return profile;
        }

        internal void SetChannel(string channel)
        {
            m_Channel = channel;
            foreach (Profile profile in m_Profiles.Values)
                profile.SetChannel(channel);
        }

        public void SetCurrentProfile(string id)
        {
            if (m_Profiles.TryGetValue(id, out Profile? profile))
            {
                m_CurrentProfile = profile;
                m_CurrentProfile.Reset();
            }
        }

        internal void OnMessage(UserMessage message)
        {
            ++m_NbMessage;
            m_CurrentProfile?.OnMessage(message);
        }

        internal void Update(long deltaTime)
        {
            m_CurrentProfile?.Update(deltaTime, m_NbMessage);
            m_NbMessage = 0;
        }

        public void Load()
        {
            if (File.Exists("profiles.json"))
            {
                JObject json = JObject.Parse(File.ReadAllText("profiles.json"));
                JArray? profiles = (JArray?)json["profiles"];
                if (profiles != null)
                {
                    List<Tuple<string, string>> parentsLinks = new();
                    foreach (var profile in profiles)
                    {
                        if (profile is JObject profileObject)
                        {
                            Profile newProfile = new(profileObject);
                            m_Profiles[newProfile.ID] = newProfile;
                            string? parent = (string?)profileObject["parent"];
                            if (parent != null)
                                parentsLinks.Add(new(parent, newProfile.ID));
                        }
                    }
                    foreach (var parentsLink in parentsLinks)
                    {
                        string parentID = parentsLink.Item1;
                        string childID = parentsLink.Item2;
                        if (m_Profiles.TryGetValue(parentID, out Profile? parent) &&
                            m_Profiles.TryGetValue(childID, out Profile? child))
                            child.SetParent(parent);
                    }
                }
                string? currentCommandProfile = (string?)json["current"];
                if (currentCommandProfile != null)
                    SetCurrentProfile(currentCommandProfile);
            }
        }

        public List<Profile> Profiles => m_Profiles.Values.ToList();
        public string CurrentProfileID => (m_CurrentProfile != null) ? m_CurrentProfile.ID : "";

        public void Save()
        {
            JObject json = new();
            JArray profiles = new();
            foreach (var profile in m_Profiles)
                profiles.Add(profile.Value.Serialize());
            if (m_CurrentProfile != null)
                json["current"] = m_CurrentProfile.ID;
            json["profiles"] = profiles;
            File.WriteAllText("profiles.json", json.ToString());
        }
    }
}
