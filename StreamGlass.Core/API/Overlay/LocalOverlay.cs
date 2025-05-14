using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Web;
using System.IO;

namespace StreamGlass.Core.API.Overlay
{
    public class LocalOverlay : Overlay
    {
        private void LoadDirectory(string directoryPath, CorpseLib.Web.Http.Path path, List<string> excludedFiles)
        {
            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    CorpseLib.Web.Http.Path resourcePath = path.Append(name);
                    if (!excludedFiles.Contains(name))
                        AddLocalFileResource(resourcePath, Path.GetFullPath(file), MIME.GetMIME(name));
                }
                string[] directories = Directory.GetDirectories(directoryPath);
                foreach (string directory in directories)
                {
                    string name = Path.GetFileName(directory);
                    CorpseLib.Web.Http.Path resourcePath = path.Append(name);
                    if (!excludedFiles.Contains(string.Format("{0}/", name)))
                        LoadDirectory(directory, resourcePath, excludedFiles);
                }
            }
        }

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
            CorpseLib.Web.Http.Path path = new(Name);
            string indexFilePath = Path.GetFullPath(Path.Combine(root, indexFile));
            if (File.Exists(indexFilePath))
                AddRootLocalFileResource(path, indexFilePath, MIME.GetMIME(indexFilePath));
            if (Directory.Exists(root))
                LoadDirectory(root, new(), excludedFile);
        }
    }
}
