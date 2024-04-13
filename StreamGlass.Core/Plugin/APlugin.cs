﻿namespace StreamGlass.Core.Plugin
{
    public abstract class APlugin(string name) : IPlugin
    {
        private Metadata? m_Metadata = null;
        private readonly string m_Name = name;

        public string Name => m_Name;

        public void InitMetadata(Metadata metadata) => m_Metadata = metadata;

        internal PluginInfo GetPluginInfo() => GeneratePluginInfo();
        protected abstract PluginInfo GeneratePluginInfo();

        protected string GetFilePath(string fileName) => string.Format("{0}{1}", m_Metadata?.Directory ?? string.Empty, fileName);

        public void RegisterPlugin() => OnLoad();
        protected abstract void OnLoad();

        public void Init() => OnInit();
        protected abstract void OnInit();

        public void UnregisterPlugin() => OnUnload();
        protected abstract void OnUnload();
    }
}
