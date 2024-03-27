using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core;

namespace StreamGlass.Twitch.API.Message
{
    public class ModerateAutoModEndpoint : AHTTPEndpoint
    {
        public ModerateAutoModEndpoint() : base("/automod") { }

        protected override Response OnPostRequest(Request request)
        {
            if (request.HaveParameter("allow"))
                StreamGlassCanals.Emit(TwitchPlugin.Canals.ALLOW_AUTOMOD, true);
            else if (request.HaveParameter("deny"))
                StreamGlassCanals.Emit(TwitchPlugin.Canals.ALLOW_AUTOMOD, false);
            return new(200, "Ok");
        }
    }
}
