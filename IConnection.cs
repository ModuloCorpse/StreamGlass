namespace StreamGlass
{
    public interface IConnection
    {
        public void Update(long deltaTime);
        public void Disconnect();
        public Settings.TabItem GetSettings();
    }
}
