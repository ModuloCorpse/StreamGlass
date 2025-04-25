using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Web;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Path = CorpseLib.Web.Http.Path;

namespace StreamGlass.API.Overlay
{
    public class OverlayEndpoint : AEndpoint
    {
        private abstract class AOverlay(string root, string index)
        {
            private readonly string m_Root = root;
            private readonly string m_Index = index;

            protected string Root => m_Root;
            protected string Index => m_Index;
            internal abstract Response GetResource(Path resourcePath);
        }

        private class FileOverlay(string root, string index) : AOverlay(root, index)
        {
            internal override Response GetResource(Path resourcePath)
            {
                string[] paths = resourcePath.Paths;
                string path;
                if (paths.Length > 2)
                {
                    StringBuilder resourceBuilder = new();
                    for (int i = 2; i < paths.Length; i++)
                    {
                        if (i != 2)
                            resourceBuilder.Append('/');
                        resourceBuilder.Append(paths[i]);
                    }
                    path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Root, resourceBuilder.ToString()));
                }
                else if (paths.Length == 2)
                    path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Root, Index));
                else //This case should never happen
                    return new(404, "Not Found", string.Format("{0} does not exist", resourcePath));
                if (File.Exists(path))
                {
                    MIME? mime = MIME.GetMIME(path);
                    if (mime != null)
                        return new(200, "Ok", File.ReadAllBytes(path), mime);
                    return new(200, "Ok", File.ReadAllBytes(path));
                }
                return new(404, "Not Found", string.Format("{0} does not exist", resourcePath));
            }
        }

        private class AssemblyOverlay(string assembly, string index) : AOverlay(assembly, index)
        {
            internal override Response GetResource(Path resourcePath)
            {
                string[] paths = resourcePath.Paths;
                string resourceName = string.Empty;
                Stream? internalResourceStream;
                if (paths.Length > 2)
                {
                    StringBuilder resourceBuilder = new(Root);
                    for (int i = 2; i < paths.Length; i++)
                    {
                        resourceName = paths[i];
                        resourceBuilder.Append('.');
                        resourceBuilder.Append(resourceName);
                    }
                    internalResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceBuilder.ToString());
                }
                else if (paths.Length == 2)
                {
                    resourceName = Index;
                    internalResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}.{1}", Root, resourceName));
                }
                else //This case should never happen
                    return new(404, "Not Found", string.Format("{0} does not exist", resourcePath));
                if (internalResourceStream != null)
                {
                    MIME? mime = MIME.GetMIME(resourceName);
                    if (mime != null)
                        return new Response(200, "Ok", new StreamReader(internalResourceStream).ReadToEnd(), mime);
                    return new Response(200, "Ok", new StreamReader(internalResourceStream).ReadToEnd());
                }
                return new(404, "Not Found", string.Format("{0} does not exist", resourcePath));
            }
        }

        private readonly Dictionary<string, AOverlay> m_Overlays = [];
        private readonly Dictionary<string, Overlay> m_WebSocketOverlays = [];
        private readonly PathTreeNode<Overlay> m_OverlayTree = new();

        public OverlayEndpoint() : base("/overlay", false, true, true)
        {
            Overlay defaultOverlay = new();
            defaultOverlay.AddAssemblyResource("StreamGlass.js", "StreamGlass.API.Overlay.StreamGlass.js");
            AddOverlay(defaultOverlay);

            Overlay chatOverlay = new("chat");
            chatOverlay.AddAssemblyResource("chat.css", "StreamGlass.API.Overlay.Chat.chat.css");
            chatOverlay.AddRootAssemblyResource("chat.html", "StreamGlass.API.Overlay.Chat.chat.html");
            chatOverlay.AddAssemblyResource("chat.js", "StreamGlass.API.Overlay.Chat.chat.js");
            AddOverlay(chatOverlay);

            //TODO Load LocalOverlay from the overlay directory
            if (Directory.Exists("overlay/"))
            {
                string[] directories = Directory.GetDirectories("overlay/");
                foreach (string directory in directories)
                {
                    LocalOverlay localOverlay = new(directory);
                    AddOverlay(localOverlay);
                }
            }
        }

        private void AddOverlay(Overlay overlay) => m_OverlayTree.AddValue(overlay.RootPath, overlay, false);

        protected override Response OnRequest(Request request)
        {
            Overlay? overlay = m_OverlayTree.GetValue(request.Path);
            if (overlay != null)
                return overlay.OnRequest(request);
            else
                return base.OnRequest(request);
        }

        protected override void OnClientRegistered(CorpseLib.Web.API.API.APIProtocol client, Path path)
        {
            Overlay? overlay = m_OverlayTree.GetValue(path);
            if (overlay != null)
            {
                overlay.OnClientRegistered(client, path);
                m_WebSocketOverlays[client.ID] = overlay;
            }
        }

        protected override void OnClientMessage(string id, string message)
        {
            if (m_WebSocketOverlays.TryGetValue(id, out Overlay? overlay))
                overlay.OnClientMessage(id, message);
        }

        protected override void OnClientUnregistered(string id)
        {
            if (m_WebSocketOverlays.TryGetValue(id, out Overlay? overlay))
            {
                overlay.OnClientUnregistered(id);
                m_WebSocketOverlays.Remove(id);
            }
        }
    }
}
