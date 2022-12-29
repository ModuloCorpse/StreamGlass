using Newtonsoft.Json.Linq;
using Quicksand.Web;
using Quicksand.Web.Http;
using Quicksand.Web.WebSocket;
using StreamFeedstock;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace StreamGlass.Http
{
    public class Request
    {
        public enum RequestType
        {
            POST,
            GET,
            PATCH
        }

        private readonly HttpClient m_Client = new();
        private readonly OAuthToken? m_Token;
        private HttpResponseMessage? m_Response = null;
        private readonly HttpContent? m_ContentToSend = null;
        private readonly URL m_URL;
        private readonly RequestType m_Type;

        private Request(URL url, RequestType type, HttpContent? contentToSend, OAuthToken? token)
        {
            m_URL = url;
            m_Type = type;
            m_ContentToSend = contentToSend;
            if (token != null)
            {
                m_Token = token;
                m_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                m_Client.DefaultRequestHeaders.Add("Client-Id", token.ClientID);
            }
        }

        public Request(URL url, RequestType type, OAuthToken? token = null) : this(url, type, null, token) { }
        public Request(string url, RequestType type, OAuthToken? token = null) : this(new(url), type, null, token) { }

        public Request(URL url, string contentToSend, RequestType type, OAuthToken? token = null) : this(url, type, new StringContent(contentToSend, Encoding.UTF8, "text/plain"), token) { }
        public Request(string url, string contentToSend, RequestType type, OAuthToken? token = null) : this(new(url), type, new StringContent(contentToSend, Encoding.UTF8, "text/plain"), token) { }

        public Request(URL url, string contentToSend, string mimeType, RequestType type, OAuthToken? token = null) : this(url, type, new StringContent(contentToSend, Encoding.UTF8, mimeType), token) { }
        public Request(string url, string contentToSend, string mimeType, RequestType type, OAuthToken? token = null) : this(new(url), type, new StringContent(contentToSend, Encoding.UTF8, mimeType), token) { }

        public Request(URL url, Json contentToSend, RequestType type, OAuthToken? token = null) : this(url, type, new StringContent(contentToSend.ToNetworkString(), Encoding.UTF8, "application/json"), token) { }
        public Request(string url, Json contentToSend, RequestType type, OAuthToken? token = null) : this(new(url), type, new StringContent(contentToSend.ToNetworkString(), Encoding.UTF8, "application/json"), token) { }

        public void AddHeaderField(string name, string value) => m_Client.DefaultRequestHeaders.Add(name, value);
        public void AddHeaderField(AuthenticationHeaderValue authenticationHeaderValue) => m_Client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;

        private void SyncSend()
        {
            Task<HttpResponseMessage> task = m_Type switch
            {
                RequestType.POST => m_Client.PostAsync(m_URL.ToString(), m_ContentToSend),
                RequestType.GET => m_Client.GetAsync(m_URL.ToString()),
                RequestType.PATCH => m_Client.PatchAsync(m_URL.ToString(), m_ContentToSend),
                _ => throw new System.NotImplementedException()
            };
            task.Wait();
            m_Response = task.Result;
        }

        public void Send()
        {
            SyncSend();
            if (m_Token != null && (int)m_Response!.StatusCode == 401)
            {
                Logger.Log("HTTP", "<= Refreshing OAuth token");
                m_Token.Refresh();
                m_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", m_Token.Token);
                m_Client.DefaultRequestHeaders.Add("Client-Id", m_Token.ClientID);
                SyncSend();
            }
        }

        public int GetStatusCode() => (int?)m_Response?.StatusCode ?? -1;

        public string GetResponse()
        {
            if (m_Response != null)
            {
                HttpContent responseContent = m_Response.Content;
                var getStreamTask = responseContent.ReadAsStreamAsync();
                getStreamTask.Wait();
                using var reader = new StreamReader(getStreamTask.Result);
                var readTask = reader.ReadToEndAsync();
                readTask.Wait();
                return readTask.Result;
            }
            return "";
        }
    }
}
