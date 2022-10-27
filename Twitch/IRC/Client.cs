using StreamGlass.StreamChat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static StreamGlass.StreamChat.UserMessage;
using static StreamGlass.Twitch.IRC.Message;

namespace StreamGlass.Twitch.IRC
{
    public class Client
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

        private UserInfo? m_SelfUserInfo = null;
        private string m_ChatColor = "";
        private readonly string m_IP;
        private readonly int m_Port;
        private readonly Socket m_Socket;
        private Stream m_Stream;
        private byte[] m_Buffer = new byte[1024];
        private string m_ReadBuffer = "";
        private string m_Channel = "";
        private readonly bool m_IsSecured = false;
        private readonly bool m_CanConnect;
        private OAuthToken? m_AccessToken = null;
        private string m_UserName = "";

        public Client(string ip, int port = 80, bool isSecured = false)
        {
            m_IP = ip;
            m_Port = port;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Stream = Stream.Null;
            m_CanConnect = true;
            m_IsSecured = isSecured;
        }

        public void SetSelfUserInfo(UserInfo? info) => m_SelfUserInfo = info;

        public bool Connect(string username, OAuthToken token)
        {
            if (!m_CanConnect)
                return false;
            foreach (IPAddress ip in Dns.GetHostEntry(m_IP).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    IPEndPoint endpoint = new(ip, m_Port);
                    m_Socket.Connect(endpoint);
                    m_Stream = new NetworkStream(m_Socket);
                    if (m_IsSecured)
                    {
                        SslStream sslStream = new(m_Stream, leaveInnerStreamOpen: false);
                        var options = new SslClientAuthenticationOptions()
                        {
                            TargetHost = m_IP,
                            RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => errors == SslPolicyErrors.None,
                        };

                        sslStream.AuthenticateAsClientAsync(options, CancellationToken.None).Wait();
                        m_Stream = sslStream;
                    }
                    StartReceiving();
                    m_UserName = username;
                    m_AccessToken = token;
                    m_AccessToken.Refreshed += (_) => SendAuth();
                    SendAuth();
                    return true;
                }
            }
            return false;
        }

        internal void SendAuth()
        {
            if (m_AccessToken == null)
                return;
            Send(new("CAP REQ", parameters: "twitch.tv/membership twitch.tv/tags twitch.tv/commands"));
            Send(new("PASS", channel: string.Format("oauth:{0}", m_AccessToken.Token)));
            if (m_SelfUserInfo != null)
                Send(new("NICK", channel: m_SelfUserInfo.DisplayName));
            else
                Send(new("NICK", channel: m_UserName));
        }

        internal void StartReceiving()
        {
            m_Buffer = new byte[1024];
            m_Stream.BeginRead(m_Buffer, 0, m_Buffer.Length, ReceiveCallback, null);
        }

        private string ComputeEmotelessString(Message message, ref List<Tuple<int, string>> emoteRet)
        {
            string replacement = "     ";
            int offset = 0;
            string ret = message.Parameters;
            List<SimpleEmote> emotes = message.Emotes;
            foreach (SimpleEmote emote in emotes)
            {
                EmoteInfo? emoteInfo = API.GetEmoteFromID(emote.ID);
                if (emoteInfo != null)
                {
                    int emoteLength = (emote.End + 1) - emote.Start;
                    int idx = emote.Start + offset;
                    offset += replacement.Length - emoteLength;
                    ret = ret[..idx] + replacement + ret[(idx + emoteLength)..];
                    emoteRet.Add(new(idx, emote.ID));
                }
            }
            return ret;
        }

        private string GetUserMessageColor(string username, string color)
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

        private void CreateUsermessage(Message message, bool self)
        {
            string username;
            if (self)
            {
                if (m_SelfUserInfo != null)
                    username = m_SelfUserInfo.DisplayName;
                else
                    username = "StreamGlass";
            }
            else
            {
                username = message.GetTag("display-name");
                if (string.IsNullOrEmpty(username))
                    username = message.Nick;
            }
            UserType userType = UserType.NONE;
            if (self)
                userType = UserType.SELF;
            else
            {
                if (message.GetTag("mod") == "1")
                    userType = UserType.MOD;
                switch (message.GetTag("user-type"))
                {
                    case "admin": userType = UserType.ADMIN; break;
                    case "global_mod": userType = UserType.GLOBAL_MOD; break;
                    case "staff": userType = UserType.STAFF; break;
                }
                if (message.HaveBadge("broadcaster"))
                    userType = UserType.BROADCASTER;
            }
            List<Tuple<int, string>> emotes = new();
            UserMessage userMessage = new(message.GetTag("id"),
                username,
                GetUserMessageColor(username, (self) ? m_ChatColor : message.GetTag("color")),
                message.Parameters,
                ComputeEmotelessString(message, ref emotes),
                message.GetCommand().Channel,
                userType);
            foreach (Tuple<int, string> emote in emotes)
                userMessage.AddEmote(emote.Item1, emote.Item2);
            CanalManager.Emit(StreamGlassCanals.CHAT_MESSAGE, userMessage);
        }

        private void TreatReceivedBuffer(string dataBuffer)
        {
            int position;
            do
            {
                position = dataBuffer.IndexOf("\r\n");
                if (position >= 0)
                {
                    string data = dataBuffer[..position];
                    Logger.Log("Twitch IRC", string.Format("<= {0}", data));
                    dataBuffer = dataBuffer[(position + 2)..];
                    Message? message = Message.Parse(data);
                    if (message != null)
                    {
                        switch (message.GetCommand().Name)
                        {
                            case "PING":
                                Send(new("PONG", parameters: message.Parameters)); break;
                            case "USERSTATE":
                                m_Channel = message.GetCommand().Channel;
                                CanalManager.Emit(StreamGlassCanals.CHAT_JOINED, m_Channel);
                                break;
                            case "JOIN":
                                if (m_SelfUserInfo?.Login != message.Nick)
                                    CanalManager.Emit(StreamGlassCanals.USER_JOINED, message.Nick);
                                break;
                            case "USERLIST":
                                string[] users = message.Parameters.Split(' ');
                                foreach (string user in users)
                                {
                                    if (m_SelfUserInfo?.Login != user)
                                        CanalManager.Emit(StreamGlassCanals.USER_JOINED, user);
                                }
                                break;
                            case "PRIVMSG":
                                CreateUsermessage(message, false);
                                if (m_SelfUserInfo?.Login != message.Nick)
                                    CanalManager.Emit(StreamGlassCanals.USER_JOINED, message.Nick);
                                break;
                            case "LOGGED":
                                CanalManager.Emit(StreamGlassCanals.CHAT_CONNECTED);
                                if (!string.IsNullOrEmpty(m_Channel))
                                    Send(new("JOIN", parameters: m_Channel));
                                break;
                            case "GLOBALUSERSTATE":
                                m_ChatColor = message.GetTag("color");
                                break;
                        }
                    }
                }
            } while (position >= 0);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int bytesRead = m_Stream.EndRead(AR);
                if (bytesRead > 1)
                {
                    byte[] buffer = new byte[bytesRead];
                    for (int i = 0; i < bytesRead; ++i)
                        buffer[i] = m_Buffer[i];
                    while (bytesRead == m_Buffer.Length)
                    {
                        int bufferLength = buffer.Length;
                        bytesRead = m_Stream.Read(m_Buffer, 0, m_Buffer.Length);
                        Array.Resize(ref buffer, bufferLength + bytesRead);
                        for (int i = 0; i < bytesRead; ++i)
                            buffer[i + bufferLength] = m_Buffer[i];
                    }
                    m_ReadBuffer += Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    string dataBuffer;
                    int position = m_ReadBuffer.LastIndexOf("\r\n");
                    if (position >= 0)
                    {
                        dataBuffer = m_ReadBuffer[..(position + 2)];
                        m_ReadBuffer = m_ReadBuffer[(position + 2)..];
                        Task.Factory.StartNew(() => TreatReceivedBuffer(dataBuffer));
                    }
                    StartReceiving();
                }
            }
            catch (Exception e)
            {
                Logger.Log("Network", string.Format("On receive exception: {0}", e));
                if (!m_Socket.Connected)
                    Disconnect();
                else
                    StartReceiving();
            }
        }

        public void Disconnect()
        {
            m_Socket.Close();
            m_Stream.Close();
        }

        public void Join(string channel) => Send(new("JOIN", channel: string.Format("#{0}", channel)));

        public void SendMessage(string channel, string message)
        {
            Message messageToSend = new("PRIVMSG", channel: channel, parameters: message);
            Send(messageToSend);
            CreateUsermessage(messageToSend, true);
        }

        private void Send(Message message)
        {
            try
            {
                string messageData = message.ToString();
                Logger.Log("Twitch IRC", string.Format("=> {0}", Regex.Replace(messageData.Trim(), "(oauth:).+", "oauth:*****")));
                byte[] data = Encoding.UTF8.GetBytes(messageData);
                if (m_Stream.CanWrite)
                    m_Stream.Write(data);
            }
            catch (Exception e)
            {
                Logger.Log("Network", string.Format("On send exception: {0}", e));
            }
        }
    }
}
