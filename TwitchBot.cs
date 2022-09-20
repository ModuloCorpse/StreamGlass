using Quicksand.Web;
using StreamGlass.Twitch.IRC;
using Client = StreamGlass.Twitch.IRC.Client;

namespace StreamGlass
{
    public class TwitchBot: IListener
    {
        private bool m_IsConnected = false;
        private string m_Channel = "";
        private readonly Settings m_Settings;
        private readonly Client m_Client = new("irc.chat.twitch.tv", 6697, true);
        private readonly CommandManager m_CommandManager;
        private readonly Twitch.Authenticator m_Authenticator;

        public TwitchBot(Server webServer, Settings settings)
        {
            m_Settings = settings;
            m_Authenticator = new(webServer, settings);
            m_Client.SetListener(this);
            m_CommandManager = new(m_Client);
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

        public void OnConnected() {}

        public void OnJoinedChannel(string channel)
        {
            if (m_Channel != channel)
            {
                m_Channel = channel;
                m_CommandManager.SetChannel(channel);
                m_Client.SendMessage(channel, "Hello World! Je suis un bot connecté via StreamGlass!");
            }
        }

        public void OnMessageReceived(UserMessage message)
        {
            m_CommandManager.OnMessage(message);
        }

        public void Update(long deltaTime)
        {
            m_CommandManager.Update(deltaTime);
        }

        public void AddCommand(string command, string message, UserMessage.UserType userType = UserMessage.UserType.NONE) => m_CommandManager.AddCommand(command, message, userType);

        public void AddTimedCommand(long time, int nbMessage, string command) => m_CommandManager.AddTimedCommand(time, nbMessage, command);

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
