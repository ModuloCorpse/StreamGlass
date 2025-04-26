using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;

namespace StreamGlass.Core.API.Event
{
    public class EventHTTPEndpoint(APIWebsocketEndpoint websocketEndpoint) : AHTTPEndpoint("/event/custom", false)
    {
        private readonly APIWebsocketEndpoint m_WebsocketEndpoint = websocketEndpoint;

        protected override Response OnPostRequest(Request request)
        {
            try
            {
                return m_WebsocketEndpoint.Emit(request.Path[^1], JsonParser.Parse(request.Body));
            } catch
            {
                return new(400, "Bad Request", "Body is not a valid json");
            }
        }
    }
}
