using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace StreamGlass
{
    internal class TwitchBot
    {
        private readonly TwitchClient m_Client;

        public TwitchBot(string botUserName, string botToken, string channel)
        {
            ConnectionCredentials credentials = new(botUserName, botToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new(clientOptions);
            m_Client = new(customClient);
            m_Client.Initialize(credentials, channel);

            m_Client.OnLog += Client_OnLog;
            m_Client.OnJoinedChannel += Client_OnJoinedChannel;
            m_Client.OnMessageReceived += Client_OnMessageReceived;
            m_Client.OnWhisperReceived += Client_OnWhisperReceived;
            m_Client.OnNewSubscriber += Client_OnNewSubscriber;
            m_Client.OnConnected += Client_OnConnected;

            m_Client.Connect();
        }

        private void Client_OnLog(object? sender, OnLogArgs e)
        {
            Console.WriteLine($"{e.DateTime}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnConnected(object? sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
            m_Client.SendMessage(e.Channel, "Hey guys! I am a bot connected via TwitchLib!");
        }

        private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.Contains("badword"))
                m_Client.TimeoutUser(e.ChatMessage.Channel, e.ChatMessage.Username, TimeSpan.FromMinutes(30), "Bad word! 30 minute timeout!");
            else if (e.ChatMessage.Message == "ping")
                m_Client.SendMessage(e.ChatMessage.Channel, "pong");
        }

        private void Client_OnWhisperReceived(object? sender, OnWhisperReceivedArgs e)
        {
            if (e.WhisperMessage.Username == "my_friend")
                m_Client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }

        private void Client_OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
        {
            int tier = 1;
            bool isPrime = false;
            switch (e.Subscriber.SubscriptionPlan)
            {
                case SubscriptionPlan.Prime: isPrime = true; break;
                case SubscriptionPlan.Tier2: tier = 2; break;
                case SubscriptionPlan.Tier3: tier = 3; break;
            }
        }
    }
}
