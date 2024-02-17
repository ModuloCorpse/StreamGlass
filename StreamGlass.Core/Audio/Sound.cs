namespace StreamGlass.Core.Audio
{
    public class Sound
    {
        private readonly string m_File;
        private readonly string m_Output;

        public string File => m_File;
        public string Output => m_Output;

        public Sound(string file, string output)
        {
            m_File = file;
            m_Output = output;
            if (!string.IsNullOrEmpty(m_File))
                SoundManager.LoadSound(m_File);
        }

        ~Sound()
        {
            if (!string.IsNullOrEmpty(m_File))
                SoundManager.UnloadSound(m_File);
        }
    }
}
