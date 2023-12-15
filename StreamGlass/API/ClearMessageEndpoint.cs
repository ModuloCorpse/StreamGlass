using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core;

namespace StreamGlass.API
{
    public class ClearMessageEndpoint : AHTTPEndpoint
    {
        public ClearMessageEndpoint() : base("/clear_chat") { }

        protected override Response OnDeleteRequest(Request request)
        {
            StreamGlassCanals.CHAT_CLEAR.Trigger();
            return new(200, "Ok");
        }
    }
}
