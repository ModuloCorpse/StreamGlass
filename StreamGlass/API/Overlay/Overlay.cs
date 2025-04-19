using CorpseLib.Web;
using CorpseLib.Web.Http;
using static CorpseLib.Web.API.API;

namespace StreamGlass.API.Overlay
{
    public class Overlay
    {
        private readonly PathTreeNode<AOverlayResource> m_EndpointTreeNode = new();
        private readonly Path m_RootPath;
        private readonly string m_Name;

        public Path RootPath => m_RootPath;
        public string Name => m_Name;

        internal Overlay()
        {
            m_RootPath = new("/overlay");
            m_Name = string.Empty;
        }

        public Overlay(string name)
        {
            m_RootPath = new(string.Format("/overlay/{0}", name));
            m_Name = name;
        }

        private void AddResource(Path resourcePath, AOverlayResource resource, bool isRoot)
        {
            m_EndpointTreeNode.AddValue(Path.Append(m_RootPath, resourcePath), resource, true);
            if (isRoot)
                m_EndpointTreeNode.AddValue(m_RootPath, resource, true);
        }

        public void AddRootAssemblyResource(string resourcePath, string assemblyPath, MIME? mime = null) => AddResource(new Path(resourcePath), new AssemblyResource(assemblyPath, mime), true);
        public void AddRootAssemblyResource(Path resourcePath, string assemblyPath, MIME? mime = null) => AddResource(resourcePath, new AssemblyResource(assemblyPath, mime), true);
        public void AddAssemblyResource(string resourcePath, string assemblyPath, MIME? mime = null) => AddResource(new Path(resourcePath), new AssemblyResource(assemblyPath, mime), false);
        public void AddAssemblyResource(Path resourcePath, string assemblyPath, MIME? mime = null) => AddResource(resourcePath, new AssemblyResource(assemblyPath, mime), false);

        public void AddRootLocalFileResource(string resourcePath, string filePath, MIME? mime = null) => AddResource(new Path(resourcePath), new LocalFileResource(filePath, mime), true);
        public void AddRootLocalFileResource(Path resourcePath, string filePath, MIME? mime = null) => AddResource(resourcePath, new LocalFileResource(filePath, mime), true);
        public void AddLocalFileResource(string resourcePath, string filePath, MIME? mime = null) => AddResource(new Path(resourcePath), new LocalFileResource(filePath, mime), false);
        public void AddLocalFileResource(Path resourcePath, string filePath, MIME? mime = null) => AddResource(resourcePath, new LocalFileResource(filePath, mime), false);

        internal Response OnRequest(Request request)
        {
            AOverlayResource? endpoint = m_EndpointTreeNode.GetValue(request.Path);
            if (endpoint != null)
                return endpoint.OnRequest(request);
            return new(404, "Not Found", string.Format("{0} does not exist", request.Path));
        }

        //Websocket
        internal void OnClientRegistered(APIProtocol client, Path path) { }
        internal void OnClientMessage(string id, string message) { }
        internal void OnClientUnregistered(string id) { }
    }
}
