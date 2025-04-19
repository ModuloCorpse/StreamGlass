using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Web;
using System.Collections.Generic;
using System.IO;

namespace StreamGlass.API.Overlay
{
    public class LocalOverlay : Overlay
    {
        public LocalOverlay(string overlayDirectory) : base(Path.GetFileName((overlayDirectory[^1] == '/') ? overlayDirectory[..^1] : overlayDirectory) ?? overlayDirectory)
        {
            string root = (overlayDirectory[^1] == '/') ? overlayDirectory : string.Format("{0}/", overlayDirectory);
            string jsonFile = Path.GetFullPath(Path.Combine(root, "overlay.json"));
            string indexFile = "index.html";
            List<string> excludedFile = [];
            if (File.Exists(jsonFile))
            {
                DataObject settings = JsonParser.LoadFromFile(jsonFile);
                root = settings.GetOrDefault("root", overlayDirectory);
                indexFile = settings.GetOrDefault("index", "index.html");
                excludedFile = settings.GetList<string>("exclude");
            }

            root = (root[^1] == '/') ? root : string.Format("{0}/", root);
            CorpseLib.Web.Http.Path path = RootPath.Duplicate();
            string indexFilePath = Path.GetFullPath(Path.Combine(root, indexFile));
            if (File.Exists(indexFilePath))
                AddRootLocalFileResource(path, indexFilePath, MIME.GetMIME(indexFilePath));
            //Iterate over root directory and add all files to the overlay
        }
    }
}
