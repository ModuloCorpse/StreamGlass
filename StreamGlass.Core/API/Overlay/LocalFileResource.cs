using CorpseLib.Web.Http;
using CorpseLib.Web;
using System.IO;

namespace StreamGlass.Core.API.Overlay
{
    internal class LocalFileResource(string filePath, MIME? mime = null) : AOverlayResource(true, false)
    {
        private readonly MIME? m_MIME = mime;
        private readonly string m_FilePath = filePath;

        protected override Response OnGetRequest(CorpseLib.Web.Http.Path _, Request request)
        {
            if (File.Exists(m_FilePath))
            {
                MIME? mime = m_MIME ?? MIME.GetMIME(m_FilePath);
                if (mime != null)
                    return new Response(200, "Ok", File.ReadAllBytes(m_FilePath), mime);
                return new Response(200, "Ok", File.ReadAllBytes(m_FilePath));
            }
            return new(404, "Not Found", string.Format("{0} does not exist", request.Path));
        }
    }
}
