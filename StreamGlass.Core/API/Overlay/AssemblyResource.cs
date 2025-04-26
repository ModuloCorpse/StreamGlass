using CorpseLib.Web;
using CorpseLib.Web.Http;
using System.IO;
using System.Reflection;

namespace StreamGlass.Core.API.Overlay
{
    public class AssemblyResource(Assembly assembly, string assemblyPath, MIME? mime = null) : AOverlayResource(true, false)
    {
        private readonly Assembly m_Assembly = assembly;
        private readonly MIME? m_MIME = mime;
        private readonly string m_AssemblyPath = assemblyPath;

        private static byte[] ReadFully(Stream input)
        {
            using MemoryStream ms = new();
            input.CopyTo(ms);
            return ms.ToArray();
        }

        protected override Response OnGetRequest(CorpseLib.Web.Http.Path _, Request request)
        {
            Stream? internalResourceStream = m_Assembly.GetManifestResourceStream(m_AssemblyPath);
            if (internalResourceStream != null)
            {
                MIME? mime = m_MIME ?? MIME.GetMIME(m_AssemblyPath);
                if (mime != null)
                    return new Response(200, "Ok", ReadFully(internalResourceStream), mime);
                return new Response(200, "Ok", ReadFully(internalResourceStream));
            }
            return new(404, "Not Found", string.Format("{0} does not exist", request.Path));
        }
    }
}
