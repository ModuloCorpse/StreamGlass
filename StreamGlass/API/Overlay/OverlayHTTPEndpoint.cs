using CorpseLib.Web;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using System.IO;
using System.Reflection;

namespace StreamGlass.API.Overlay
{
    public class OverlayHTTPEndpoint() : AHTTPEndpoint("/overlay", false)
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
    }
}
