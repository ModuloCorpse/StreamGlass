using CorpseLib.Web;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using System.Reflection;

namespace StreamGlass.Core.API.Overlay
{
    public class Overlay
    {
        private readonly PathTreeNode<AOverlayResource> m_EndpointTreeNode = new();
        private readonly Path m_RootPath;
        private readonly string m_Name;

        public Path RootPath => m_RootPath;
        public string Name => m_Name;

        public Overlay()
        {
            m_RootPath = new();
            m_Name = string.Empty;
        }

        public Overlay(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                m_RootPath = new();
                m_Name = string.Empty;
            }
            else
            {
                m_RootPath = new(name);
                m_Name = name;
            }
        }

        private void AddResource(Path resourcePath, AOverlayResource resource, bool isRoot)
        {
            m_EndpointTreeNode.AddValue(resourcePath, resource, true);
            if (isRoot)
                m_EndpointTreeNode.AddValue(new(), resource, true);
        }

        public void AddRootResource(string resourcePath, AOverlayResource resource) => AddResource(new Path(resourcePath), resource, true);
        public void AddRootResource(Path resourcePath, AOverlayResource resource) => AddResource(resourcePath, resource, true);
        public void AddResource(string resourcePath, AOverlayResource resource) => AddResource(new Path(resourcePath), resource, false);
        public void AddResource(Path resourcePath, AOverlayResource resource) => AddResource(resourcePath, resource, false);

        public void AddRootAssemblyResource(string resourcePath, string assemblyPath, MIME? mime = null) => AddResource(new Path(resourcePath), new AssemblyResource(Assembly.GetCallingAssembly(), assemblyPath, mime), true);
        public void AddRootAssemblyResource(Path resourcePath, string assemblyPath, MIME? mime = null) => AddResource(resourcePath, new AssemblyResource(Assembly.GetCallingAssembly(), assemblyPath, mime), true);
        public void AddAssemblyResource(string resourcePath, string assemblyPath, MIME? mime = null) => AddResource(new Path(resourcePath), new AssemblyResource(Assembly.GetCallingAssembly(), assemblyPath, mime), false);
        public void AddAssemblyResource(Path resourcePath, string assemblyPath, MIME? mime = null) => AddResource(resourcePath, new AssemblyResource(Assembly.GetCallingAssembly(), assemblyPath, mime), false);

        public void AddRootLocalFileResource(string resourcePath, string filePath, MIME? mime = null) => AddResource(new Path(resourcePath), new LocalFileResource(filePath, mime), true);
        public void AddRootLocalFileResource(Path resourcePath, string filePath, MIME? mime = null) => AddResource(resourcePath, new LocalFileResource(filePath, mime), true);
        public void AddLocalFileResource(string resourcePath, string filePath, MIME? mime = null) => AddResource(new Path(resourcePath), new LocalFileResource(filePath, mime), false);
        public void AddLocalFileResource(Path resourcePath, string filePath, MIME? mime = null) => AddResource(resourcePath, new LocalFileResource(filePath, mime), false);

        internal Response OnRequest(Path path, Request request)
        {
            Path resourcePath = (string.IsNullOrEmpty(m_Name)) ? path : path.NextPath() ?? new(); //Removing /<name> if needed
            AOverlayResource? endpoint = m_EndpointTreeNode.GetValue(resourcePath);
            if (endpoint != null)
                return endpoint.OnRequest(resourcePath, request);
            return new(404, "Not Found", string.Format("{0} does not exist", request.Path));
        }

        //Websocket
        internal void OnClientRegistered(Path path, WebsocketReference wsReference)
        {
            Path resourcePath = (string.IsNullOrEmpty(m_Name)) ? path : path.NextPath() ?? new(); //Removing /<name> if needed
            AOverlayResource? endpoint = m_EndpointTreeNode.GetValue(resourcePath);
            endpoint?.RegisterClient(resourcePath, wsReference);
        }

        internal void OnClientMessage(Path path, WebsocketReference wsReference, string message)
        {
            Path resourcePath = (string.IsNullOrEmpty(m_Name)) ? path : path.NextPath() ?? new(); //Removing /<name> if needed
            AOverlayResource? endpoint = m_EndpointTreeNode.GetValue(resourcePath);
            endpoint?.ClientMessage(resourcePath, wsReference, message);
        }

        internal void OnClientUnregistered(Path path, WebsocketReference wsReference)
        {
            Path resourcePath = (string.IsNullOrEmpty(m_Name)) ? path : path.NextPath() ?? new(); //Removing /<name> if needed
            AOverlayResource? endpoint = m_EndpointTreeNode.GetValue(resourcePath);
            endpoint?.ClientUnregistered(resourcePath, wsReference);
        }
    }
}
