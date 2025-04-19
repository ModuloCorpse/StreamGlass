using CorpseLib.Web.Http;

namespace StreamGlass.API.Overlay
{
    public abstract class AOverlayResource(bool isHTTPEndpoint, bool isWebsocketEndpoint)
    {
        private readonly bool m_IsHTTPEndpoint = isHTTPEndpoint;
        private readonly bool m_IsWebsocketEndpoint = isWebsocketEndpoint;

        public bool IsHTTPEndpoint => m_IsHTTPEndpoint;
        public bool IsWebsocketEndpoint => m_IsWebsocketEndpoint;

        //HTTP
        internal Response OnRequest(Request request) => request.Method switch
        {
            Request.MethodType.GET => OnGetRequest(request),
            Request.MethodType.HEAD => OnHeadRequest(request),
            Request.MethodType.POST => OnPostRequest(request),
            Request.MethodType.PUT => OnPutRequest(request),
            Request.MethodType.DELETE => OnDeleteRequest(request),
            Request.MethodType.CONNECT => OnConnectRequest(request),
            Request.MethodType.OPTIONS => OnOptionsRequest(request),
            Request.MethodType.TRACE => OnTraceRequest(request),
            Request.MethodType.PATCH => OnPatchRequest(request),
            _ => new(400, "Bad Request")
        };

        protected virtual Response OnGetRequest(Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnHeadRequest(Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnPostRequest(Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnPutRequest(Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnDeleteRequest(Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnConnectRequest(Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnOptionsRequest(Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnTraceRequest(Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnPatchRequest(Request request) => new(405, "Method Not Allowed");

        //Websocket
        internal void RegisterClient(CorpseLib.Web.API.API.APIProtocol client, Path path) => OnClientRegistered(client, path);
        protected virtual void OnClientRegistered(CorpseLib.Web.API.API.APIProtocol client, Path path) { }
        internal void ClientMessage(string id, string message) => OnClientMessage(id, message);
        protected virtual void OnClientMessage(string id, string message) { }
        internal void ClientUnregistered(string id) => OnClientUnregistered(id);
        protected virtual void OnClientUnregistered(string id) { }
    }
}
