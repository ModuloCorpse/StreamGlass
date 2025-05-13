using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using Path = CorpseLib.Web.Http.Path;

namespace StreamGlass.Core.API.Overlay
{
    public class OverlayEndpoint : AEndpoint
    {
        private readonly PathTreeNode<Overlay> m_OverlayTree = new();

        public OverlayEndpoint() : base("/overlay", false, true, true) { }

        public void AddOverlay(Path path, Overlay overlay)
        {
            StreamGlassContext.LOGGER.Log("New overlay at path ${0}", path);
            m_OverlayTree.AddValue(path, overlay, false);
        }

        private Path CleanPath(Path path)
        {
            string currentPath = path.CurrentPath;
            if (currentPath == "plugin" || currentPath == "local")
            {
                path = path.NextPath() ?? new(); //Removing /plugin
                path = path.NextPath() ?? new(); //Removing /<plugin directory name>
            }
            else if (currentPath == "local")
                path = path.NextPath() ?? new(); //Removing /local
            return path;
        }

        protected override Response OnRequest(Request request)
        {
            Path path = request.Path.NextPath() ?? new(); //Removing /overlay
            Overlay? overlay = m_OverlayTree.GetValue(path);
            path = CleanPath(path);
            if (overlay != null)
                return overlay.OnRequest(path, request);
            return base.OnRequest(request);
        }

        protected override void OnClientRegistered(WebsocketReference wsReference)
        {
            Path path = wsReference.Path.NextPath() ?? new(); //Removing /overlay
            Overlay? overlay = m_OverlayTree.GetValue(path);
            path = CleanPath(path);
            overlay?.OnClientRegistered(path, wsReference);
        }

        protected override void OnClientMessage(WebsocketReference wsReference, string message)
        {
            Path path = wsReference.Path.NextPath() ?? new(); //Removing /overlay
            Overlay? overlay = m_OverlayTree.GetValue(path);
            path = CleanPath(path);
            overlay?.OnClientMessage(path, wsReference, message);
        }

        protected override void OnClientUnregistered(WebsocketReference wsReference)
        {
            Path path = wsReference.Path.NextPath() ?? new(); //Removing /overlay
            Overlay? overlay = m_OverlayTree.GetValue(path);
            path = CleanPath(path);
            overlay?.OnClientUnregistered(path, wsReference);
        }
    }
}
