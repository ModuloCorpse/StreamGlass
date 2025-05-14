using CorpseLib.Web;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using System.Reflection;
using static CorpseLib.Web.Http.ResourceSystem;

namespace StreamGlass.Core.API.Overlay
{
    public class Overlay(string name) : Directory
    {
        private readonly string m_Name = name;

        public string Name => m_Name;

        private void AddResource(Path resourcePath, AEndpoint resource, bool isRoot)
        {
            Add(resourcePath, resource);
            if (isRoot)
                Add(new(), resource);
        }

        public void AddRootResource(string resourcePath, AEndpoint resource) => AddResource(new Path(resourcePath), resource, true);
        public void AddRootResource(Path resourcePath, AEndpoint resource) => AddResource(resourcePath, resource, true);
        public void AddResource(string resourcePath, AEndpoint resource) => AddResource(new Path(resourcePath), resource, false);
        public void AddResource(Path resourcePath, AEndpoint resource) => AddResource(resourcePath, resource, false);

        public void AddRootAssemblyResource(string resourcePath, string assemblyPath, MIME? mime = null) => AddResource(new Path(resourcePath), new AssemblyResource(false, Assembly.GetCallingAssembly(), assemblyPath, mime), true);
        public void AddRootAssemblyResource(Path resourcePath, string assemblyPath, MIME? mime = null) => AddResource(resourcePath, new AssemblyResource(false, Assembly.GetCallingAssembly(), assemblyPath, mime), true);
        public void AddAssemblyResource(string resourcePath, string assemblyPath, MIME? mime = null) => AddResource(new Path(resourcePath), new AssemblyResource(false, Assembly.GetCallingAssembly(), assemblyPath, mime), false);
        public void AddAssemblyResource(Path resourcePath, string assemblyPath, MIME? mime = null) => AddResource(resourcePath, new AssemblyResource(false, Assembly.GetCallingAssembly(), assemblyPath, mime), false);

        public void AddRootLocalFileResource(string resourcePath, string filePath, MIME? mime = null) => AddResource(new Path(resourcePath), new LocalFileResource(filePath, mime), true);
        public void AddRootLocalFileResource(Path resourcePath, string filePath, MIME? mime = null) => AddResource(resourcePath, new LocalFileResource(filePath, mime), true);
        public void AddLocalFileResource(string resourcePath, string filePath, MIME? mime = null) => AddResource(new Path(resourcePath), new LocalFileResource(filePath, mime), false);
        public void AddLocalFileResource(Path resourcePath, string filePath, MIME? mime = null) => AddResource(resourcePath, new LocalFileResource(filePath, mime), false);
    }
}
