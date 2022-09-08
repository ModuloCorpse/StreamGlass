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
                Script framework = new();
                framework.SetSrc("/framework.js");
                m_Model.GetHead().AddScript(framework);
                m_Model.GetBody().AddChild(m_WaitDiv);
            }
            protected override void OnUpdate(long deltaTime) { }
            protected override void BeforeGet(int clientID, Request request)
            {
                if (request.TryGetParameter("state", out var state))
                {
                    if (request.TryGetParameter("error", out var error))
                    {
                        if (request.TryGetParameter("error_description", out var errorDescription))
                            m_Authenticator.SetError(state, error, errorDescription.Replace('+', ' '));
                        else
                            m_Authenticator.SetError(state, error, "");
                        m_Model.GetBody().RemoveChild(m_WaitDiv);
                        m_Model.GetBody().AddChild(m_CloseDiv);
                    }
                }
            }

            protected override void OnAnchorParameters(int _, string __, Dictionary<string, string> parameters)
            {
                if (parameters.TryGetValue("state", out var state))
                {
                    List<string> scopes = new();
                    if (parameters.TryGetValue("scope", out var scope))
                        scopes.AddRange(scope.Replace("%3A", ":").Split('+'));
                    if (parameters.TryGetValue("token_type", out var tokenType) &&
                        tokenType == "bearer" &&
                        parameters.TryGetValue("access_token", out var token))
                    {
                        m_Authenticator.SetToken(state, token, scopes);
                        m_Model.GetBody().RemoveChild(m_WaitDiv);
                        m_Model.GetBody().AddChild(m_CloseDiv);
                    }
                }
            }
        }

        private class AwaitingToken
        {
            private readonly TaskCompletionSource<string> m_Task;
            private readonly List<string> m_Scopes;

            public AwaitingToken(TaskCompletionSource<string> task, List<string> scopes)
            {
                m_Task = task;
                m_Scopes = scopes;
            }

            public bool CompareScopes(List<string> scopes)
            {
                return m_Scopes.All(item => scopes.Contains(item)) && scopes.All(item => m_Scopes.Contains(item));
            }

            public void SetToken(string token)
            {
                m_Task.SetResult(token);
            }
        }

        private readonly static string REDIRECT_URI = "http://localhost:80/twitch_authenticate/";
        private readonly static string TWITCH_AUTH_URI = "https://id.twitch.tv/oauth2/authorize?response_type=token";
        private readonly string m_CliendID;
        private readonly Dictionary<string, AwaitingToken> m_AwaitingTokens = new();

        public Authenticator(Server webServer, string cliendID)
        {
            Div waitDiv = new();
            Model model = new("Authentification");
            model.GetBody().AddChild(waitDiv);

            m_CliendID = cliendID;
            webServer.AddResource("/twitch_authenticate", () => new AuthenticatorWebController(this, model, waitDiv));
        }

        public string Authenticate(List<string> scopes)
        {
            string stateString = Guid.NewGuid().ToString();
            string scopeString = string.Join('+', scopes).Replace(":", "%3A");
            string twitchAuthURI = string.Format("{0}&client_id={1}&redirect_uri={2}&scope={3}&state={4}",
                TWITCH_AUTH_URI, m_CliendID, REDIRECT_URI, scopeString, stateString);
            Process myProcess = new();
            // true is the default, but it is important not to set it to false
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = twitchAuthURI;
            myProcess.Start();
            var tokenTask = new TaskCompletionSource<string>();
            m_AwaitingTokens[stateString] = new(tokenTask, scopes);
            if (tokenTask.Task.Wait(TimeSpan.FromSeconds(300)))
            {
                m_AwaitingTokens.Remove(stateString);
                return tokenTask.Task.Result;
            }
            m_AwaitingTokens.Remove(stateString);
            return "";
        }

        internal void SetError(string state, string error, string message)
        {
            if (m_AwaitingTokens.ContainsKey(state))
                m_AwaitingTokens[state].SetToken("");
        }

        internal void SetToken(string state, string token, List<string> scopes)
        {
            if (m_AwaitingTokens.ContainsKey(state))
            {
                AwaitingToken awaitingToken = m_AwaitingTokens[state];
                if (awaitingToken.CompareScopes(scopes))
                    awaitingToken.SetToken(token);
                else
                    awaitingToken.SetToken("");
            }
        }
    }
}
