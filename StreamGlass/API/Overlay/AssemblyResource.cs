using CorpseLib.Web;
using CorpseLib.Web.Http;
using System.IO;
using System.Reflection;

namespace StreamGlass.API.Overlay
{
    public class AssemblyResource(string assemblyPath, MIME? mime = null) : AOverlayResource(true, false)
    {
        private readonly MIME? m_MIME = mime;
        private readonly string m_AssemblyPath = assemblyPath;

        protected override Response OnGetRequest(Request request)
        {
            Stream? internalResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(m_AssemblyPath);
            if (internalResourceStream != null)
            {
                if (m_MIME != null)
                    return new Response(200, "Ok", new StreamReader(internalResourceStream).ReadToEnd(), m_MIME);
                return new Response(200, "Ok", new StreamReader(internalResourceStream).ReadToEnd());
            }
            return new(404, "Not Found", string.Format("{0} does not exist", request.Path));
        }
    }
}
