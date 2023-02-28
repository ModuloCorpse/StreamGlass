using Quicksand.Web;
using Quicksand.Web.Html;
using StreamGlass.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
            protected override void AfterGet(int clientID, Quicksand.Web.Http.Request request)
            {
                if (request.TryGetParameter("state", out string? state))
                {
                    List<string> scopes = new();
                    if (request.TryGetParameter("scope", out string? scope))
                        scopes.AddRange(scope!.Replace("%3A", ":").Split('+'));
                    if (request.TryGetParameter("code", out string? token))
                        m_Authenticator.SetAuthToken(state!, token!, scopes);
                    else if (request.TryGetParameter("error", out string? error))
                    {
                        if (request.TryGetParameter("error_description", out string? errorDescription))
                            m_Authenticator.SetError(state!, error!, errorDescription!.Replace('+', ' '));
                        else
                            m_Authenticator.SetError(state!, error!, "");
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

        internal readonly static string REDIRECT_URI = "http://localhost:80/twitch_authenticate/";
        private readonly static string TWITCH_AUTH_URI = "https://id.twitch.tv/oauth2/authorize?response_type=code";
        private readonly Settings.Data m_Settings;
        private readonly List<string> m_Scopes = new() {
            "bits:read",
            "channel:manage:broadcast",
            "channel:manage:moderators",
            "channel:manage:polls",
            "channel:manage:redemptions",
            "channel:moderate",
            "channel:read:polls",
            "channel:read:redemptions",
            "channel:read:subscriptions",
            "chat:read",
            "chat:edit",
            "moderator:manage:automod",
            "moderator:read:automod_settings",
            "moderator:read:followers",
            "moderator:manage:banned_users",
            "moderator:manage:blocked_terms",
            "moderator:manage:chat_messages",
            "moderation:read",
            "user:read:email",
            "whispers:read"
        };
        private TaskCompletionSource<string> m_Task = new();
        private string m_State = "";

        public Authenticator(Server webServer, Settings.Data settings)
        {
            Div waitDiv = new();
            waitDiv.AddContent("Waiting for authentification");
            Model model = new("Authentification");
            model.GetBody().AddChild(waitDiv);
            ResourceManager resourceManager = webServer.GetResourceManager();
            resourceManager.AddResource("/twitch_authenticate", () => new AuthenticatorWebController(this, model, waitDiv));
            resourceManager.AddResource("/twitch_authenticated", () => new AuthenticatorEndWebController());

            m_Settings = settings;
        }

        public OAuthToken? Authenticate(string browser = "")
        {
            m_State = Guid.NewGuid().ToString();
            string scopeString = string.Join('+', m_Scopes).Replace(":", "%3A");
            string twitchAuthURI = string.Format("{0}&client_id={1}&redirect_uri={2}&scope={3}&state={4}",
                TWITCH_AUTH_URI, m_Settings.Get("twitch", "public_key"), REDIRECT_URI, scopeString, m_State);
            Process myProcess = new();
            myProcess.StartInfo.UseShellExecute = true;
            if (string.IsNullOrWhiteSpace(browser))
                myProcess.StartInfo.FileName = twitchAuthURI;
            else
            {
                myProcess.StartInfo.FileName = browser;
                myProcess.StartInfo.Arguments = twitchAuthURI;
            }
            myProcess.Start();
            m_Task = new TaskCompletionSource<string>();
            if (m_Task.Task.Wait(TimeSpan.FromSeconds(5)))
                return new("https://id.twitch.tv/oauth2/token", m_Scopes, m_Settings.Get("twitch", "public_key"), m_Settings.Get("twitch", "secret_key"), m_Task.Task.Result);
            return null;
        }

        internal static void Use(string _) {}

        internal void SetError(string state, string error, string message)
        {
            Use(error);
            Use(message);
            if (m_State == state)
                m_Task.SetResult("");
        }

        internal void SetAuthToken(string state, string token, List<string> scopes)
        {
            if (m_State == state)
            {
                if (m_Scopes.All(item => scopes.Contains(item)) && scopes.All(item => m_Scopes.Contains(item)))
                    m_Task.SetResult(token);
                else
                    m_Task.SetResult("");
            }
        }
    }
}
