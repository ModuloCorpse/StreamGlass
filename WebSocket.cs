using Quicksand.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamGlass
{
    public class WebSocket
    {
        private readonly AClientListener m_Listener;
        private readonly CancellationTokenSource m_Token = new();
        private readonly ClientWebSocket m_Socket = new();
        private readonly string m_URL;
        private bool m_IsConnected = false;

        public WebSocket(AClientListener listener, string url)
        {
            m_Listener = listener;
            m_URL = url;
        }

        public bool IsConnected() => m_IsConnected;

        public bool Connect(Dictionary<string, string> extensions)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            m_Socket.Options.RemoteCertificateValidationCallback += (o, c, ch, er) => true;
            foreach (var extension in extensions)
                m_Socket.Options.SetRequestHeader(extension.Key, extension.Value);
            m_Socket.ConnectAsync(new(m_URL), CancellationToken.None).Wait();
            Task.Factory.StartNew(
                async () =>
                {
                    StringBuilder builder = new();
                    var buffer = new ArraySegment<byte>(new byte[128]);
                    while (m_Socket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result = await m_Socket.ReceiveAsync(buffer, m_Token.Token);
                        byte[] bytes = buffer.Skip(buffer.Offset).Take(result.Count).ToArray();
                        string message = Encoding.UTF8.GetString(bytes);
                        if (result.MessageType == WebSocketMessageType.Close)
                            m_Listener.OnWebSocketClose(0, (short?)result.CloseStatus ?? 0, message);
                        else
                        {
                            builder.Append(message);
                            if (result.EndOfMessage)
                            {
                                m_Listener.OnWebSocketMessage(0, builder.ToString());
                                builder.Clear();
                            }
                        }
                    }
                }, m_Token.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            m_IsConnected = true;
            return true;
        }

        public bool Connect() => Connect(new());

        private async Task SendAsync(string message)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            await m_Socket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: m_Token.Token);
        }

        public void Send(string message)
        {
            var task = SendAsync(message);
            task.Wait();
        }

        public void Disconnect()
        {
            if (m_IsConnected)
            {
                m_Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).Wait();
                m_Token.Cancel();
                m_Listener.OnClientDisconnect(0);
                m_IsConnected = false;
            }
        }

        public bool IsWebSocket() => true;

        public bool IsServer() => false;
    }
}
