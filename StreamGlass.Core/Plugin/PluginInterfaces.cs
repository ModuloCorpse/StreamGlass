using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core.API.Overlay;
using StreamGlass.Core.Settings;
using System.Windows.Controls;

namespace StreamGlass.Core.Plugin
{
    public interface IPlugin
    {
        public string Name { get; }
    }

    public interface ISettingsPlugin : IPlugin
    {
        public TabItemContent[] GetSettings();
    }

    public interface IOverlayPlugin : IPlugin
    {
        public Overlay[] GetOverlays();
    }

    public interface IAPIPlugin : IPlugin
    {
        public Dictionary<Path, AEndpoint> GetEndpoints();
    }

    public interface IUpdatablePlugin : IPlugin
    {
        public void Update(long deltaTime);
    }

    public interface ITestablePlugin : IPlugin
    {
        public void Test();
    }

    public interface IPanelPlugin : IPlugin
    {
        public Control? GetPanel(string panelID);
    }
}
