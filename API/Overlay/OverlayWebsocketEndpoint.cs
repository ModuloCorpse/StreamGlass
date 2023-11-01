using CorpseLib.Json;
using CorpseLib.Web.API.Event;
using System.IO;

namespace StreamGlass.API.Overlay
{
    public class OverlayWebsocketEndpoint : EventEndpoint
    {
        public OverlayWebsocketEndpoint() : base("/overlay", false) { }

        protected override void OnClientRegistered(CorpseLib.Web.API.API.APIProtocol client, string path)
        {
            base.OnClientRegistered(client, path);
            path = "." + path;
            if (Directory.Exists(path))
            {
                if (path[^1] != '/')
                    path += "/";
                path += "settings.json";
                if (File.Exists(path))
                {
                    try
                    {
                        JFile settings = JFile.LoadFromFile(path);
                        SendEvent(client, "parameters", settings);
                    } catch { }
                }
                else
                    SendEvent(client, "parameters", new JObject());
            }
        }

        protected override void OnUnknownRequest(CorpseLib.Web.API.API.APIProtocol client, string request, JFile json)
        {

        }
    }
}
