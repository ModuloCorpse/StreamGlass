using Newtonsoft.Json.Linq;
using Quicksand.Web.Http;
using System.Collections.Generic;
using System.Linq;

namespace StreamGlass.Twitch
{
    public class OAuthToken
    {
        public delegate void RefreshEventHandler(OAuthToken refreshedToken);

        private readonly List<string> m_Scopes;
        private readonly string m_PublicKey;
        private readonly string m_Secret;
        private string m_RefreshToken = "";
        private string m_AccessToken = "";
        public event RefreshEventHandler? Refreshed;

        internal OAuthToken(List<string> scopes, string publicKey, string secret, string token)
        {
            m_Scopes = scopes;
            m_PublicKey = publicKey;
            m_Secret = secret;
            GetAccessToken(string.Format("client_id={0}&client_secret={1}&code={2}&grant_type=authorization_code&redirect_uri={3}", m_PublicKey, m_Secret, token, Authenticator.REDIRECT_URI));
        }

        public string Token => m_AccessToken;

        private bool CompareScopes(List<string> scopes)
        {
            return m_Scopes.All(item => scopes.Contains(item)) && scopes.All(item => m_Scopes.Contains(item));
        }

        private void GetAccessToken(string request)
        {
            Response? response = new PostRequest("https://id.twitch.tv/oauth2/token", request).AddHeaderField("Content-Type", "application/x-www-form-urlencoded").Send();
            if (response != null)
            {
                string responseJsonStr = response.Body;
                if (string.IsNullOrWhiteSpace(responseJsonStr))
                    return;
                JObject responseJson = JObject.Parse(responseJsonStr);
                string? access_token = (string?)responseJson["access_token"];
                string? refresh_token = (string?)responseJson["refresh_token"];
                string? token_type = (string?)responseJson["token_type"];
                List<string>? scope = responseJson["scope"]?.ToObject<List<string>>();
                if (access_token != null &&
                    refresh_token != null &&
                    token_type != null && token_type == "bearer" &&
                    scope != null && CompareScopes(scope))
                {
                    m_RefreshToken = refresh_token;
                    m_AccessToken = access_token;
                }
            }
        }

        public void Refresh()
        {
            GetAccessToken(string.Format("grant_type=refresh_token&refresh_token={0}&client_id={1}&client_secret={2}", m_RefreshToken, m_PublicKey, m_Secret));
            Refreshed?.Invoke(this);
        }
    }
}
