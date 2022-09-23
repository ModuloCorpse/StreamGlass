using Quicksand.Web;
using StreamGlass.Twitch.IRC;
using StreamGlass.Twitch;
using Client = StreamGlass.Twitch.IRC.Client;

namespace StreamGlass
{
    public class TwitchBot: IListener
    {
        private bool m_IsConnected = false;
        private string m_Channel = "";
        private string m_Token = "";
        private readonly Settings m_Settings;
        private readonly Client m_Client = new("irc.chat.twitch.tv", 6697, true);
        private readonly ProfileManager m_Manager;
        private readonly Authenticator m_Authenticator;
        private readonly StreamGlassWindow m_Form;

        public TwitchBot(Server webServer, Settings settings, ProfileManager manager, StreamGlassWindow form)
        {
            m_Settings = settings;
            m_Manager = manager;
            m_Form = form;
            m_Authenticator = new(webServer, settings);
            m_Client.SetListener(this);
            API.Init(m_Client, m_Authenticator);
        }

        public void Connect()
        {
            if (!m_IsConnected)
            {
                m_IsConnected = true;
                m_Token = m_Authenticator.Authenticate(m_Settings.Get("system", "browser"));
                API.Authenticate(m_Settings.Get("twitch", "public_key"), m_Token);
                API.LoadGlobalEmoteSet();
                string userID = API.GetSelfUserID();
                m_Client.SetSelfUserInfo(API.GetUserInfoFromID(userID, new()));
                m_Client.Connect("StreamGlass", m_Token);
                m_Client.Join(m_Settings.Get("twitch", "channel"));
            }
        }

        public void OnConnected() {}

        public void OnJoinedChannel(string channel)
        {
            if (m_Channel != channel)
            {
                m_Channel = channel;
                API.LoadChannelEmoteSetFromLogin(channel[1..]);
                m_Manager.SetChannel(channel);
                m_Client.SendMessage(channel, "Hello World! Je suis un bot connecté via StreamGlass!");
            }
        }

        public void OnMessageReceived(UserMessage message)
        {
            m_Form.AddMessage(message);
            m_Manager.OnMessage(message);
        }

        public void Update(long _) {}

        /*private void Client_OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
        {
            int tier = 1;
            bool isPrime = false;
            switch (e.Subscriber.SubscriptionPlan)
            {
                case SubscriptionPlan.Prime: isPrime = true; break;
                case SubscriptionPlan.Tier2: tier = 2; break;
                case SubscriptionPlan.Tier3: tier = 3; break;
            }
        }*/
    }
}
