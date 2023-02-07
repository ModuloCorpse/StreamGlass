using Quicksand.Web;
using Quicksand.Web.WebSocket;
using StreamFeedstock;
using StreamFeedstock.StructuredText;
using StreamGlass.Events;
using StreamGlass.Http;
using StreamGlass.StreamChat;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static StreamGlass.StreamChat.UserMessage;

namespace StreamGlass.Twitch.IRC
{
    public class ChatClient: AClientListener
    {
        private static readonly List<string> ms_Colors = new()
        {
            "#ff0000",
            "#00ff00",
            "#0000ff",
            "#b22222",
            "#ff7f50",
            "#9acd32",
            "#ff4500",
            "#2e8b57",
            "#daa520",
            "#d2691e",
            "#5f9ea0",
            "#1e90ff",
            "#ff69b4",
            "#8a2be2",
            "#00ff7f"
        };

        private readonly API m_API;
        private readonly Settings.Data m_Settings;
        private readonly Quicksand.Web.WebSocket.Client m_Client;
        private User? m_SelfUserInfo = null;
        private OAuthToken? m_AccessToken = null;
        private string m_ChatColor = "";
        private string m_Channel = "";
        private string m_UserName = "";
        private bool m_CanReconnect = true;

        public ChatClient(API api, Settings.Data settings)
        {
            m_API = api;
            m_Settings = settings;
            m_Client = new(this, "wss://irc-ws.chat.twitch.tv:443");
        }

        public void SetSelfUserInfo(User? info) => m_SelfUserInfo = info;

        public void Init(string username, OAuthToken token)
        {
            m_UserName = username;
            m_AccessToken = token;
            m_AccessToken.Refreshed += (_) => SendAuth();
            m_Client.Connect();
        }

        private void SendAuth()
        {
            if (m_AccessToken == null)
                return;
            SendMessage(new("CAP REQ", parameters: "twitch.tv/membership twitch.tv/tags twitch.tv/commands"));
            SendMessage(new("PASS", channel: string.Format("oauth:{0}", m_AccessToken.Token)));
            SendMessage(new("NICK", channel: (m_SelfUserInfo != null) ? m_SelfUserInfo.DisplayName : m_UserName));
        }

        private static string GetUserMessageColor(string username, string color)
        {
            if (string.IsNullOrEmpty(color))
            {
                int colorIdx = 0;
                foreach (char c in username)
                    colorIdx += c;
                colorIdx %= ms_Colors.Count;
                return ms_Colors[colorIdx];
            }
            return color;
        }

        private void CreateUserMessage(Message message, bool highlight, bool self)
        {
            string displayName;
            if (self)
            {
                if (m_SelfUserInfo != null)
                    displayName = m_SelfUserInfo.DisplayName;
                else
                    displayName = "StreamGlass";
            }
            else
                displayName = message.GetTag("display-name");
            string userID = message.GetTag("user-id");
            User.Type userType = m_API.GetUserType(self, message.GetTag("mod") == "1", message.GetTag("user-type"), userID);
            User user = new(userID, message.Nick, displayName, userType);
            Text displayableMessage = Helper.Convert(m_API, message.Parameters, message.Emotes);
            CanalManager.Emit(StreamGlassCanals.CHAT_MESSAGE, new UserMessage(user, highlight, message.GetTag("id"),
                GetUserMessageColor(displayName, (self) ? m_ChatColor : message.GetTag("color")),
                message.GetCommand().Channel, displayableMessage));
            if (message.HaveTag("bits"))
            {
                int bits = int.Parse(message.GetTag("bits"));
                CanalManager.Emit(StreamGlassCanals.DONATION, new DonationEventArgs(displayName, bits!, "Bits", displayableMessage));
            }
        }

        private void TreatUserNotice(Message message)
        {
            if (m_Settings.Get("twitch", "sub_mode") == "all")
                return;
            CreateUserMessage(message, true, false);
            if (message.HaveTag("msg-id"))
            {
                string noticeType = message.GetTag("msg-id");
                if (noticeType == "sub" || noticeType == "resub")
                {
                    if (message.HaveTag("msg-param-sub-plan") &&
                        message.HaveTag("msg-param-cumulative-months") &&
                        message.HaveTag("msg-param-should-share-streak"))
                    {
                        string username = message.GetTag("display-name");
                        if (string.IsNullOrEmpty(username))
                            username = message.Nick;
                        int followTier;
                        switch (message.GetTag("msg-param-sub-plan"))
                        {
                            case "1000": followTier = 1; break;
                            case "2000": followTier = 2; break;
                            case "3000": followTier = 3; break;
                            case "Prime": followTier = 4; break;
                            default: return;
                        }
                        int cumulativeMonth = int.Parse(message.GetTag("msg-param-cumulative-months"));
                        bool shareStreakMonth = message.GetTag("msg-param-cumulative-months") == "1";
                        int streakMonth = (message.HaveTag("msg-param-streak-months")) ? int.Parse(message.GetTag("msg-param-streak-months")) : -1;
                        Text displayableMessage = Helper.Convert(m_API, message.Parameters, message.Emotes);
                        CanalManager.Emit(StreamGlassCanals.FOLLOW, new FollowEventArgs(username, displayableMessage, followTier, false, cumulativeMonth, (shareStreakMonth) ? streakMonth : -1, -1));
                    }
                }
            }
        }

        private void LoadEmoteSets(Message message)
        {
            if (message.HaveTag("emote-sets"))
            {
                string emoteSetsStr = message.GetTag("emote-sets");
                string[] emoteSetIDs = emoteSetsStr.Split(',');
                foreach (string emoteSetID in emoteSetIDs)
                    m_API.LoadEmoteSet(emoteSetID);
            }
        }

        internal void TreatReceivedBuffer(string dataBuffer)
        {
            int position;
            do
            {
                position = dataBuffer.IndexOf("\r\n");
                if (position >= 0)
                {
                    string data = dataBuffer[..position];
                    Log.Str("Twitch IRC", string.Format("<= {0}", data));
                    dataBuffer = dataBuffer[(position + 2)..];
                    Message? message = Message.Parse(data, m_API);
                    if (message != null)
                    {
                        switch (message.GetCommand().Name)
                        {
                            case "PING":
                                {
                                    SendMessage(new("PONG", parameters: message.Parameters));
                                    break;
                                }
                            case "USERSTATE":
                                {
                                    LoadEmoteSets(message);
                                    m_Channel = message.GetCommand().Channel;
                                    CanalManager.Emit(StreamGlassCanals.CHAT_JOINED, m_Channel);
                                    break;
                                }
                            case "JOIN":
                                {
                                    if (m_SelfUserInfo?.Name != message.Nick)
                                    {
                                        User? user = m_API.GetUserInfoFromLogin(message.Nick);
                                        if (user != null)
                                            CanalManager.Emit(StreamGlassCanals.USER_JOINED, user);
                                    }
                                    break;
                                }
                            case "USERLIST":
                                {
                                    string[] users = message.Parameters.Split(' ');
                                    foreach (string userLogin in users)
                                    {
                                        if (m_SelfUserInfo?.Name != userLogin)
                                        {
                                            User? user = m_API.GetUserInfoFromLogin(userLogin);
                                            if (user != null)
                                                CanalManager.Emit(StreamGlassCanals.USER_JOINED, user);
                                        }
                                    }
                                    break;
                                }
                            case "PRIVMSG":
                                {
                                    CreateUserMessage(message, false, false);
                                    break;
                                }
                            case "USERNOTICE":
                                {
                                    TreatUserNotice(message);
                                    break;
                                }
                            case "LOGGED":
                                {
                                    CanalManager.Emit(StreamGlassCanals.CHAT_CONNECTED);
                                    if (!string.IsNullOrEmpty(m_Channel))
                                        SendMessage(new("JOIN", parameters: m_Channel));
                                    break;
                                }
                            case "GLOBALUSERSTATE":
                                {
                                    LoadEmoteSets(message);
                                    m_ChatColor = message.GetTag("color");
                                    break;
                                }
                        }
                    }
                }
            } while (position >= 0);
        }

        public void Join(string channel) => SendMessage(new("JOIN", channel: string.Format("#{0}", channel)));

        public void SendMessage(string channel, string message)
        {
            Message messageToSend = new("PRIVMSG", channel: channel, parameters: message);
            SendMessage(messageToSend);
            CreateUserMessage(messageToSend, false, true);
        }

        private void SendMessage(Message message)
        {
            try
            {
                string messageData = message.ToString();
                Log.Str("Twitch IRC", string.Format("=> {0}", Regex.Replace(messageData.Trim(), "(oauth:).+", "oauth:*****")));
                m_Client.Send(messageData);
            }
            catch (Exception e)
            {
                Log.Str("Twitch IRC", string.Format("On send exception: {0}", e));
            }
        }

        private async Task TryReconnect()
        {
            Log.Str("Twitch IRC", "Trying to reconnect");
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                if (m_Client.Connect())
                {
                    Log.Str("Twitch IRC", "Reconnected");
                    return;
                }
            }
        }

        public override void OnClientDisconnect(int clientID)
        {
            Log.Str("Twitch IRC", "Disconnecting");
            if (m_CanReconnect && m_Settings.Get("twitch", "auto_connect") == "true")
                _ = TryReconnect();
        }

        public override void OnWebSocketMessage(int clientID, string message)
        {
            TreatReceivedBuffer(message);
        }

        public override void OnWebSocketOpen(int clientID, Quicksand.Web.Http.Response response)
        {
            SendAuth();
        }

        public override void OnWebSocketClose(int clientID, short code, string closeMessage)
        {
            Log.Str("Twitch IRC", string.Format("<=[Error] WebSocket closed ({0}): {1}", code, closeMessage));
        }

        public override void OnWebSocketError(int clientID, string error)
        {
            Log.Str("Twitch IRC", string.Format("<=[Error] WebSocket error: {0}", error));
        }

        public override void OnWebSocketFrame(int clientID, Frame frame)
        {
            Log.Str("Twitch IRC", string.Format("<=[WS] {0}", frame.ToString().Trim()));
        }

        public override void OnWebSocketFrameSent(int clientID, Frame frame)
        {
            Log.Str("Twitch IRC", string.Format("[WS]=> {0}", frame.ToString().Trim()));
        }

        internal void Disconnect()
        {
            m_CanReconnect = false;
            m_Client.Disconnect();
        }

        internal void Reconnect()
        {
            if (!m_Client.IsConnected())
                _ = TryReconnect();
        }
    }
}
