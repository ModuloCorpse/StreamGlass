using CorpseLib.Ini;
using CorpseLib.Web.API;
using StreamGlass.Core.Profile;
using StreamGlass.Core.Settings;

namespace StreamGlass.Core.Plugin
{
    public abstract class APlugin(string name)
    {
        private readonly string m_Name = name;

        internal string Name => m_Name;


        internal void Init() => InitPlugin();
        internal void RegisterPlugin()
        {
            InitTranslation();
            InitSettings();
            InitCommands();
            InitCanals();
        }

        internal void RegisterPluginToAPI(API api) => RegisterAPI(api);
        internal void UnregisterPlugin() => Unregister();
        internal void Test() => TestPlugin();
        internal void Tick(long deltaTime) => Update(deltaTime);
        internal void FillSettings(Dialog settingsDialog)
        {
            TabItemContent[] settings = GetSettings();
            foreach (TabItemContent setting in settings)
                settingsDialog.AddTabItem(setting);
        }

        protected abstract void InitTranslation();
        protected abstract void InitSettings();
        protected abstract void InitCommands();
        protected abstract void InitCanals();
        protected abstract void InitPlugin();

        protected abstract void RegisterAPI(API api);
        protected abstract void Unregister();
        protected abstract void Update(long deltaTime);

        protected abstract TabItemContent[] GetSettings();

        protected abstract void TestPlugin();
    }
}
