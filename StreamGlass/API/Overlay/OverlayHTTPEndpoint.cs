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
    public class OverlayHTTPEndpoint : AHTTPEndpoint
    {
        private static MIME? GetMIME(string path) => System.IO.Path.GetExtension(path).ToLower() switch
        {
            "html" => MIME.TEXT.HTML,
            "js" => MIME.TEXT.JAVASCRIPT,
            "css" => MIME.TEXT.CSS,
            "webp" => MIME.IMAGE.WEBP,
            "png" => MIME.IMAGE.PNG,
            "jpg" => MIME.IMAGE.JPEG,
            "jpeg" => MIME.IMAGE.JPEG,
            _ => null
        };

        private class Overlay(string root, string index)
        {
            private readonly string m_Root = root;
            private readonly string m_Index = index;

            public Response GetResource(Path resourcePath)
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
                    path = System.IO.Path.GetFullPath(System.IO.Path.Combine(m_Root, resourceBuilder.ToString()));
                }
                else if (paths.Length == 2)
                    path = System.IO.Path.GetFullPath(System.IO.Path.Combine(m_Root, m_Index));
                else //This case should never happen
                    return new(404, "Not Found", string.Format("{0} does not exist", resourcePath));
                if (File.Exists(path))
                {
                    MIME? mime = GetMIME(path);
                    if (mime != null)
                        return new(200, "Ok", File.ReadAllBytes(path), mime);
                    return new(200, "Ok", File.ReadAllBytes(path));
                }
                return new(404, "Not Found", string.Format("{0} does not exist", resourcePath));
            }
        }

        private readonly Dictionary<string, Overlay> m_Overlays = [];

        public OverlayHTTPEndpoint() : base("/overlay", false)
        {
            if (Directory.Exists("overlay/"))
            {
                string[] directories = Directory.GetDirectories("overlay/");
                foreach (string directory in directories)
                {
                    string overlayDirectory = (directory[^1] == '/') ? directory : string.Format("{0}/", directory);
                    string overlayPath = System.IO.Path.GetFileName(overlayDirectory[..^1]);
                    Overlay? overlay = LoadOverlay(string.Format("overlay/{0}", overlayPath));
                    if (overlay != null)
                        m_Overlays[overlayPath] = overlay;
                }
            }
        }

        private static Overlay? LoadOverlay(string path)
        {
            string overlayDirectory = (path[^1] == '/') ? path : string.Format("{0}/", path);
            if (!Directory.Exists(overlayDirectory))
                return null;
            Overlay overlay;
            string jsonFile = System.IO.Path.GetFullPath(System.IO.Path.Combine(overlayDirectory, "overlay.json"));
            if (File.Exists(jsonFile))
            {
                DataObject settings = JsonParser.LoadFromFile(jsonFile);
                overlay = new(settings.GetOrDefault("root", overlayDirectory), settings.GetOrDefault("index", "index.html"));
            }
            else
                overlay = new(overlayDirectory, "index.html");
            return overlay;
        }

        protected override Response OnGetRequest(Request request)
        {
            string[] paths = request.Path.Paths;
            if (paths.Length > 1)
            {
                string overlayName = paths[1];
                if (m_Overlays.TryGetValue(overlayName, out Overlay? overlay))
                    return overlay.GetResource(request.Path);
                else
                {
                    string overlayPath = string.Format("overlay/{0}", overlayName);
                    Overlay? loadedOverlay = LoadOverlay(overlayPath);
                    if (loadedOverlay != null)
                    {
                        m_Overlays[overlayName] = loadedOverlay;
                        return loadedOverlay.GetResource(request.Path);
                    }
                    else
                    {
                        if (paths.Length == 2)
                        {
                            Stream? internalResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("StreamGlass.API.Overlay.{0}", paths[1]));
                            if (internalResourceStream != null)
                            {
                                MIME? mime = GetMIME(paths[1]);
                                if (mime != null)
                                    return new Response(200, "Ok", new StreamReader(internalResourceStream).ReadToEnd(), mime);
                                return new Response(200, "Ok", new StreamReader(internalResourceStream).ReadToEnd());
                            }
                        }
                    }
                }
            }
            return new(404, "Not Found", string.Format("{0} does not exist", request.Path));
        }
    }
}
