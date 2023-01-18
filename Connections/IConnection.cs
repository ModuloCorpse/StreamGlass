namespace StreamGlass.Connections
{
    public interface IConnection
    {
        public void Update(long deltaTime);
        public void Disconnect();
        public Settings.TabItemContent[] GetSettings();
        public void Test();
    }
}
