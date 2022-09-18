﻿using Newtonsoft.Json.Linq;
using Quicksand.Web;
using Quicksand.Web.Html;
using Quicksand.Web.Http;
using System.Diagnostics;

namespace StreamGlass.Twitch
{
    public class Authenticator
    {
        private class AuthenticatorWebController : Controler
        {
            private readonly Authenticator m_Authenticator;
            private readonly Div m_WaitDiv;
            private readonly Div m_CloseDiv = new();

            public AuthenticatorWebController(Authenticator authenticator, Model model, Div waitDiv): base(model)
            {
                m_Authenticator = authenticator;
                m_WaitDiv = waitDiv;
                m_CloseDiv.AddContent("You can close this page");
            }

            protected override void AfterGenerateModel() {}
            protected override void GenerateModel()
            {
                SetQuicksandFramework();
                m_Model.GetBody().AddChild(m_WaitDiv);
            }
            protected override void OnUpdate(long deltaTime) {}
            protected override void AfterGet(int clientID, Request request)
            {
                if (request.TryGetParameter("state", out string? state))
                {
                    List<string> scopes = new();
                    if (request.TryGetParameter("scope", out string? scope))
                        scopes.AddRange(scope.Replace("%3A", ":").Split('+'));
                    if (request.TryGetParameter("code", out string? token))
                    {
                        m_Authenticator.SetAuthToken(state, token, scopes);
                        m_Model.GetBody().RemoveChild(m_WaitDiv);
                        m_Model.GetBody().AddChild(m_CloseDiv);
                    }
                    else if (request.TryGetParameter("error", out string? error))
                    {
                        if (request.TryGetParameter("error_description", out string? errorDescription))
                            m_Authenticator.SetError(state, error, errorDescription.Replace('+', ' '));
                        else
                            m_Authenticator.SetError(state, error, "");
                        m_Model.GetBody().RemoveChild(m_WaitDiv);
                        m_Model.GetBody().AddChild(m_CloseDiv);
                    }
                }
            }

            protected override void OnWebsocketConnect(int clientID)
            {
                Redirect("/twitch_authenticated", true);
            }
        }

        private class AuthenticatorEndWebController : Controler
        {
            public AuthenticatorEndWebController() : base("You can close this page") {}

            protected override void AfterGenerateModel() { }
            protected override void GenerateModel()
            {
                Div closeDiv = new();
                closeDiv.AddContent("You can close this page");
                m_Model.GetBody().AddChild(closeDiv);
            }
            protected override void OnUpdate(long deltaTime) { }
        }

        private readonly static string REDIRECT_URI = "http://localhost:80/twitch_authenticate/";
        private readonly static string TWITCH_AUTH_URI = "https://id.twitch.tv/oauth2/authorize?response_type=code";
        private readonly Settings.TwitchSettings m_Settings;
        private string m_RefreshToken = "";
        private string m_State = "";
        private readonly List<string> m_Scopes = new() {
            "channel:manage:polls",
            "channel:read:polls",
            "chat:read",
            "chat:edit",
            "channel:moderate"
        };
        private TaskCompletionSource<string> m_Task = new TaskCompletionSource<string>();

        public Authenticator(Server webServer, Settings.TwitchSettings twitchSettings)
        {
            Div waitDiv = new();
            waitDiv.AddContent("Waiting for authentification");
            Model model = new("Authentification");
            model.GetBody().AddChild(waitDiv);
            ResourceManager resourceManager = webServer.GetResourceManager();
            resourceManager.AddResource("/twitch_authenticate", () => new AuthenticatorWebController(this, model, waitDiv));
            resourceManager.AddResource("/twitch_authenticated", () => new AuthenticatorEndWebController());
            m_Settings = twitchSettings;
        }

        public string Authenticate()
        {
            m_State = Guid.NewGuid().ToString();
            string scopeString = string.Join('+', m_Scopes).Replace(":", "%3A");
            string twitchAuthURI = string.Format("{0}&client_id={1}&redirect_uri={2}&scope={3}&state={4}",
                TWITCH_AUTH_URI, m_Settings.BotPublic, REDIRECT_URI, scopeString, m_State);
            Process myProcess = new();
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = twitchAuthURI;
            myProcess.Start();
            m_Task = new TaskCompletionSource<string>();
            if (m_Task.Task.Wait(TimeSpan.FromSeconds(300)))
                return m_Task.Task.Result;
            return "";
        }

        private string GetAccessToken(string request)
        {
            Response? response = new PostRequest("https://id.twitch.tv/oauth2/token", request).AddHeaderField("Content-Type", "application/x-www-form-urlencoded").Send();
            if (response != null)
            {
                string responseJsonStr = response.Body;
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
                    return access_token;
                }
            }
            m_RefreshToken = "";
            return "";
        }

        public string RefreshToken()
        {
            return GetAccessToken(string.Format("grant_type=refresh_token&refresh_token={0}&client_id={1}&client_secret={2}", m_RefreshToken, m_Settings.BotPublic, m_Settings.BotSecret));
        }

        internal static void Use(string _) {}

        internal void SetError(string state, string error, string message)
        {
            Use(error);
            Use(message);
            if (m_State == state)
                m_Task.SetResult("");
        }

        private bool CompareScopes(List<string> scopes)
        {
            return m_Scopes.All(item => scopes.Contains(item)) && scopes.All(item => m_Scopes.Contains(item));
        }

        internal void SetAuthToken(string state, string token, List<string> scopes)
        {
            if (m_State == state)
            {
                if (CompareScopes(scopes))
                    m_Task.SetResult(GetAccessToken(string.Format("client_id={0}&client_secret={1}&code={2}&grant_type=authorization_code&redirect_uri={3}", m_Settings.BotPublic, m_Settings.BotSecret, token, REDIRECT_URI)));
                else
                    m_Task.SetResult("");
            }
        }
    }
}
