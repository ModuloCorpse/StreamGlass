using Quicksand.Web;
using StreamGlass.Twitch.IRC;
using StreamGlass.Twitch;
using Client = StreamGlass.Twitch.IRC.Client;
using System.Windows.Documents;
using System.Collections.Generic;

namespace StreamGlass
{
    public class TwitchBot: IListener
    {
        private readonly bool m_UseSameToken = false;
        private bool m_IsConnected = false;
        private string m_Channel = "";
        private readonly Settings m_Settings;
        private readonly Client m_Client;
        private readonly ProfileManager m_Manager;
        private readonly Authenticator m_Authenticator;
        private readonly StreamGlassWindow m_Form;

        public TwitchBot(Server webServer, Settings settings, ProfileManager manager, Client client, StreamGlassWindow form)
        {
            m_Settings = settings;
            m_Client = client;
            m_Manager = manager;
            m_Form = form;
            m_Authenticator = new(webServer, settings);
            m_Client.SetListener(this);
        }

        public void Connect()
        {
            if (!m_IsConnected)
            {
                OAuthToken? apiToken = m_Authenticator.Authenticate();
                if (apiToken != null)
                {
                    m_IsConnected = true;
                    API.Authenticate(m_Settings.Get("twitch", "public_key"), apiToken);
                    API.LoadGlobalEmoteSet();
                    API.LoadChannelEmoteSetFromLogin("chaporon_");
                    OAuthToken? ircToken;
                    if (m_UseSameToken)
                        ircToken = apiToken;
                    else
                        ircToken = m_Authenticator.Authenticate(m_Settings.Get("system", "browser"));
                    if (ircToken != null)
                    {
                        UserInfo? userInfoOfToken = API.GetUserInfoOfToken(ircToken);
                        m_Client.SetSelfUserInfo(userInfoOfToken);
                        if (userInfoOfToken != null)
                            API.LoadEmoteSetFromFollowedChannelOfID(userInfoOfToken.ID);
                        m_Client.Connect("StreamGlass", ircToken);
                    }
                }
            }
        }

        public void OnConnected()
        {
            m_Client.Join(m_Settings.Get("twitch", "channel"));
        }

        public void OnJoinedChannel(string channel)
        {
            if (m_Channel != channel)
            {
                m_Channel = channel;
                API.LoadChannelEmoteSetFromLogin(channel[1..]);
                m_Form.JoinChannel(channel);
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

        public void OnUserJoinedChannel(string login)
        {
            API.LoadEmoteSetFromFollowedChannelOfLogin(login);
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
