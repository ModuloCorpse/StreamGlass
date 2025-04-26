using CorpseLib.Web.API;
using CorpseLib.Web.Http;

namespace StreamGlass.Core.API.Overlay
{
    public abstract class AOverlayResource(bool isHTTPEndpoint, bool isWebsocketEndpoint)
    {
        private readonly bool m_IsHTTPEndpoint = isHTTPEndpoint;
        private readonly bool m_IsWebsocketEndpoint = isWebsocketEndpoint;

        public bool IsHTTPEndpoint => m_IsHTTPEndpoint;
        public bool IsWebsocketEndpoint => m_IsWebsocketEndpoint;

        //HTTP
        internal Response OnRequest(Path path, Request request) => request.Method switch
        {
            Request.MethodType.GET => OnGetRequest(path, request),
            Request.MethodType.HEAD => OnHeadRequest(path, request),
            Request.MethodType.POST => OnPostRequest(path, request),
            Request.MethodType.PUT => OnPutRequest(path, request),
            Request.MethodType.DELETE => OnDeleteRequest(path, request),
            Request.MethodType.CONNECT => OnConnectRequest(path, request),
            Request.MethodType.OPTIONS => OnOptionsRequest(path, request),
            Request.MethodType.TRACE => OnTraceRequest(path, request),
            Request.MethodType.PATCH => OnPatchRequest(path, request),
            _ => new(400, "Bad Request")
        };

        protected virtual Response OnGetRequest(Path path, Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnHeadRequest(Path path, Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnPostRequest(Path path, Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnPutRequest(Path path, Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnDeleteRequest(Path path, Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnConnectRequest(Path path, Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnOptionsRequest(Path path, Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnTraceRequest(Path path, Request request) => new(405, "Method Not Allowed");
        protected virtual Response OnPatchRequest(Path path, Request request) => new(405, "Method Not Allowed");

        //Websocket
        internal void RegisterClient(Path path, WebsocketReference wsReference) => OnClientRegistered(path, wsReference);
        protected virtual void OnClientRegistered(Path path, WebsocketReference wsReference) { }
        internal void ClientMessage(Path path, WebsocketReference wsReference, string message) => OnClientMessage(path, wsReference, message);
        protected virtual void OnClientMessage(Path path, WebsocketReference wsReference, string message) { }
        internal void ClientUnregistered(Path path, WebsocketReference wsReference) => OnClientUnregistered(path, wsReference);
        protected virtual void OnClientUnregistered(Path path, WebsocketReference wsReference) { }
    }
}
