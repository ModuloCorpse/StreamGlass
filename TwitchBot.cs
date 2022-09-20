using Quicksand.Web;
using StreamGlass.Twitch.IRC;
using Client = StreamGlass.Twitch.IRC.Client;
using Message = StreamGlass.Twitch.IRC.Message;

namespace StreamGlass
{
    internal class TwitchBot: IListener
    {
        private bool m_IsConnected = false;
        private readonly Settings m_Settings;
        private readonly Client m_Client = new("irc.chat.twitch.tv", 6697, true);
        private readonly Twitch.Authenticator m_Authenticator;

        public TwitchBot(Server webServer, Settings settings)
        {
            m_Settings = settings;
            m_Authenticator = new(webServer, settings);
            m_Client.SetListener(this);
        }

        public void Connect()
        {
            if (!m_IsConnected)
            {
                m_IsConnected = true;
                m_Client.Connect("StreamGlass", m_Authenticator.Authenticate(m_Settings.Get("system", "browser")));
                m_Client.Join(m_Settings.Get("twitch", "channel"));
            }
        }

        public void OnConnected(Message message) {}

        public void OnJoinedChannel(Message message)
        {
            m_Client.SendMessage(message.GetCommand().GetChannel(), "Hello World! Je suis un bot connecté via StreamGlass!");
        }

        public void OnMessageReceived(Message message)
        {
            if (message.GetParameters() == "ping")
                m_Client.SendMessage(message.GetCommand().GetChannel(), "pong");
        }

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
