﻿using Quicksand.Web.Http;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace StreamGlass.Twitch.IRC
{
    public class Client
    {
        private readonly string m_IP;
        private readonly int m_Port;
        private readonly Socket m_Socket;
        private Stream m_Stream;
        private byte[] m_Buffer = new byte[1024];
        private string m_ReadBuffer = "";
        private readonly bool m_IsSecured = false;
        private readonly bool m_CanConnect;
        private IListener? m_Listener = null;

        public Client(string ip, int port = 80, bool isSecured = false)
        {
            m_IP = ip;
            m_Port = port;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Stream = Stream.Null;
            m_CanConnect = true;
            m_IsSecured = isSecured;
        }

        public void SetListener(IListener listener) => m_Listener = listener;

        public bool Connect(string username, string token)
        {
            if (!m_CanConnect)
                return false;
            try
            {
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
                        Send(new("PASS", channel: string.Format("oauth:{0}", token)));
                        Send(new("NICK", channel: username));
                        return true;
                    }
                }
                return false;
            }
            catch { return false; }
        }

        internal void StartReceiving()
        {
            try
            {
                m_Buffer = new byte[1024];
                m_Stream.BeginRead(m_Buffer, 0, m_Buffer.Length, ReceiveCallback, null);
            }
            catch { }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int bytesRead = m_Stream.EndRead(AR);
                if (bytesRead > 1)
                {
                    Array.Resize(ref m_Buffer, bytesRead);
                    m_ReadBuffer += Encoding.UTF8.GetString(m_Buffer, 0, m_Buffer.Length);
                    int position;
                    do
                    {
                        position = m_ReadBuffer.IndexOf("\r\n");
                        if (position >= 0)
                        {
                            string data = m_ReadBuffer[..position];
                            m_ReadBuffer = m_ReadBuffer[(position + 2)..];
                            Message? message = Message.Parse(data);
                            if (message != null)
                            {
                                switch (message.GetCommand().GetName())
                                {
                                    case "PING":
                                        Send(new("PONG", parameters: message.GetParameters())); break;
                                    case "JOIN":
                                        m_Listener?.OnJoinedChannel(message); break;
                                    case "PRIVMSG":
                                        m_Listener?.OnMessageReceived(message); break;
                                    case "LOGGED":
                                        m_Listener?.OnConnected(message); break;
                                }
                            }
                        }
                    } while (position >= 0);
                    StartReceiving();
                }
            }
            catch
            {
                if (!m_Socket.Connected)
                    Disconnect();
                else
                    StartReceiving();
            }
        }

        internal void Disconnect()
        {
            m_Stream.Close();
            m_Socket.Disconnect(true);
        }

        public void Join(string channel) => Send(new("JOIN", channel: string.Format("#{0}", channel)));

        public void SendMessage(string channel, string message) => Send(new("PRIVMSG", channel: channel, parameters: message));

        private void Send(Message message)
        {
            try
            {
                string messageData = message.ToString();
                byte[] data = Encoding.UTF8.GetBytes(messageData);
                if (m_Stream.CanWrite)
                    m_Stream.Write(data);
            }
            catch { }
        }
    }
}
