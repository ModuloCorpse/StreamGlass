using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core;

namespace StreamGlass.Twitch.API.Message
{
    public class ClearMessageEndpoint : AHTTPEndpoint
    {
        public ClearMessageEndpoint() : base("/clear_chat") { }

        protected override Response OnDeleteRequest(Request request)
        {
            StreamGlassCanals.Trigger("chat_clear");
            return new(200, "Ok");
        }
    }
}
