using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamGlass.API
{
    public class OverlayEndpoint : AHTTPEndpoint
    {
        public OverlayEndpoint() : base("/overlay", false) { }

        protected override Response OnGetRequest(Request request)
        {
            string path = "." + request.Path;
            if (Directory.Exists(path))
            {
                if (path[^1] != '/')
                    path += "/";
                path += "index.html";
            }
            if (File.Exists(path))
                return new(200, "Ok", File.ReadAllText(path));
            return new(404, "Not Found", string.Format("{0} does not exist", request.Path));
        }
    }
}
