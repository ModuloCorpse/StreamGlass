using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core;

namespace StreamGlass.Twitch.API.Message
{
    public class ClearMessageEndpoint : AHTTPEndpoint
    {
        public ClearMessageEndpoint() : base("/clear_chat") { }

        protected override Response OnPostRequest(Request request)
        {
            StreamGlassCanals.Trigger(TwitchPlugin.Canals.CHAT_CLEAR);
            StreamGlassCanals.Trigger(StreamGlassCanals.PROFILE_RESET);
            return new(200, "Ok");
        }
    }
}
