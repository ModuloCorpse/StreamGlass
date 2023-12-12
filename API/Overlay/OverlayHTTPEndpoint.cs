using CorpseLib.Json;
using CorpseLib.Web;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using System.IO;
using System.Reflection;

namespace StreamGlass.API.Overlay
{
    public class OverlayHTTPEndpoint(OverlayWebsocketEndpoint websocketEndpoint) : AHTTPEndpoint("/overlay", false)
    {
        private readonly OverlayWebsocketEndpoint m_WebsocketEndpoint = websocketEndpoint;

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

        protected override Response OnGetRequest(Request request)
        {
            if (request.Path.FullPath == "/overlay/StreamGlass.js")
            {
                Stream? javascriptStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("StreamGlass.API.Overlay.StreamGlass.js");
                if (javascriptStream != null)
                    return new Response(200, "Ok", new StreamReader(javascriptStream).ReadToEnd(), MIME.TEXT.JAVASCRIPT);
            }

            string path = "." + request.Path.FullPath;
            if (Directory.Exists(path))
            {
                if (path[^1] != '/')
                    path += "/";
                path += "index.html";
            }
            if (File.Exists(path))
            {
                MIME? mime = GetMIME(path);
                if (mime != null)
                    return new(200, "Ok", File.ReadAllBytes(path), mime);
                return new(200, "Ok", File.ReadAllBytes(path));
            }
            return new(404, "Not Found", string.Format("{0} does not exist", request.Path));
        }

        protected override Response OnPostRequest(Request request)
        {
            try
            {
                JFile json = new(request.Body);
                if (json.TryGet("type", out string? type))
                {
                    switch (type)
                    {
                        case "event":
                        {
                            if (json.TryGet("event", out string? eventType))
                            {
                                if (json.TryGet("data", out JObject? data))
                                    return m_WebsocketEndpoint.Emit(request.Path, eventType!, data!);
                                return new(400, "Bad Request", "No event data given");
                            }
                            return new(400, "Bad Request", "No event type given");
                        }
                        default:
                        {
                            return new(400, "Bad Request", string.Format("Unknown type {0}", type));
                        }
                    }
                }
                return new(400, "Bad Request", "No type given");
            } catch
            {
                return new(400, "Bad Request", "Body is not a valid json");
            }
        }
    }
}
