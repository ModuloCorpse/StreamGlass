using CorpseLib.Web.Http;
using CorpseLib.Web;
using System.IO;

namespace StreamGlass.API.Overlay
{
    internal class LocalFileResource(string filePath, MIME? mime = null) : AOverlayResource(true, false)
    {
        private readonly MIME? m_MIME = mime;
        private readonly string m_FilePath = filePath;

        protected override Response OnGetRequest(Request request)
        {
            if (File.Exists(m_FilePath))
            {
                if (m_MIME != null)
                    return new Response(200, "Ok", File.ReadAllBytes(m_FilePath), m_MIME);
                return new Response(200, "Ok", File.ReadAllBytes(m_FilePath));
            }
            return new(404, "Not Found", string.Format("{0} does not exist", request.Path));
        }
    }
}
