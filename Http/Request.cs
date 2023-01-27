using Quicksand.Web;
using StreamFeedstock;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace StreamGlass.Http
{
    public abstract class Request
    {
        protected readonly HttpClient m_Client = new();
        protected readonly HttpContent? m_ContentToSend = null;
        protected readonly URL m_URL;
        private readonly OAuthToken? m_Token;
        private HttpResponseMessage? m_Response = null;

        protected Request(URL url, HttpContent? contentToSend, OAuthToken? token)
        {
            m_URL = url;
            m_ContentToSend = contentToSend;
            if (token != null)
            {
                m_Token = token;
                m_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                m_Client.DefaultRequestHeaders.Add("Client-Id", token.ClientID);
            }
        }

        public void AddHeaderField(string name, string value) => m_Client.DefaultRequestHeaders.Add(name, value);
        public void AddHeaderField(AuthenticationHeaderValue authenticationHeaderValue) => m_Client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;

        protected abstract Task<HttpResponseMessage> SendRequest();

        private void SyncSend()
        {
            Task<HttpResponseMessage> task = SendRequest();
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

    public class DeleteRequest : Request
    {
        public DeleteRequest(URL url, OAuthToken? token = null) : base(url, null, token) { }
        public DeleteRequest(string url, OAuthToken? token = null) : base(new(url), null, token) { }
        protected override Task<HttpResponseMessage> SendRequest() => m_Client.DeleteAsync(m_URL.ToString());
    }

    public class GetRequest : Request
    {
        public GetRequest(URL url, OAuthToken? token = null) : base(url, null, token) { }
        public GetRequest(string url, OAuthToken? token = null) : base(new(url), null, token) { }

        protected override Task<HttpResponseMessage> SendRequest() => m_Client.GetAsync(m_URL.ToString());
    }

    public class PostRequest : Request
    {
        public PostRequest(URL url, OAuthToken? token = null) : base(url, null, token) { }
        public PostRequest(string url, OAuthToken? token = null) : base(new(url), null, token) { }
        public PostRequest(URL url, string contentToSend, OAuthToken? token = null) : base(url, new StringContent(contentToSend, Encoding.UTF8, "text/plain"), token) { }
        public PostRequest(string url, string contentToSend, OAuthToken? token = null) : base(new(url), new StringContent(contentToSend, Encoding.UTF8, "text/plain"), token) { }
        public PostRequest(URL url, string contentToSend, string mimeType, OAuthToken? token = null) : base(url, new StringContent(contentToSend, Encoding.UTF8, mimeType), token) { }
        public PostRequest(string url, string contentToSend, string mimeType, OAuthToken? token = null) : base(new(url), new StringContent(contentToSend, Encoding.UTF8, mimeType), token) { }
        public PostRequest(URL url, Json contentToSend, OAuthToken? token = null) : base(url, new StringContent(contentToSend.ToNetworkString(), Encoding.UTF8, "application/json"), token) { }
        public PostRequest(string url, Json contentToSend, OAuthToken? token = null) : base(new(url), new StringContent(contentToSend.ToNetworkString(), Encoding.UTF8, "application/json"), token) { }

        protected override Task<HttpResponseMessage> SendRequest() => m_Client.PostAsync(m_URL.ToString(), m_ContentToSend);
    }

    public class PatchRequest : Request
    {
        public PatchRequest(URL url, OAuthToken? token = null) : base(url, null, token) { }
        public PatchRequest(string url, OAuthToken? token = null) : base(new(url), null, token) { }
        public PatchRequest(URL url, string contentToSend, OAuthToken? token = null) : base(url, new StringContent(contentToSend, Encoding.UTF8, "text/plain"), token) { }
        public PatchRequest(string url, string contentToSend, OAuthToken? token = null) : base(new(url), new StringContent(contentToSend, Encoding.UTF8, "text/plain"), token) { }
        public PatchRequest(URL url, string contentToSend, string mimeType, OAuthToken? token = null) : base(url, new StringContent(contentToSend, Encoding.UTF8, mimeType), token) { }
        public PatchRequest(string url, string contentToSend, string mimeType, OAuthToken? token = null) : base(new(url), new StringContent(contentToSend, Encoding.UTF8, mimeType), token) { }
        public PatchRequest(URL url, Json contentToSend, OAuthToken? token = null) : base(url, new StringContent(contentToSend.ToNetworkString(), Encoding.UTF8, "application/json"), token) { }
        public PatchRequest(string url, Json contentToSend, OAuthToken? token = null) : base(new(url), new StringContent(contentToSend.ToNetworkString(), Encoding.UTF8, "application/json"), token) { }

        protected override Task<HttpResponseMessage> SendRequest() => m_Client.PatchAsync(m_URL.ToString(), m_ContentToSend);
    }
}
