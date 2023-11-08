using CorpseLib.Json;
using CorpseLib.Web.API.Event;

namespace StreamGlass.API.Overlay
{
    public class OverlayWebsocketEndpoint : EventEndpoint
    {
        public OverlayWebsocketEndpoint() : base("/overlay", false) { }

        protected override void OnUnknownRequest(CorpseLib.Web.API.API.APIProtocol client, string request, JFile json)
        {

        }
    }
}
